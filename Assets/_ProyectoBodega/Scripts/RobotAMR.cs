using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando, Recargando }
    
    public event System.Action<EstadoRobot> OnEstadoCambiado;

    [Header("Identidad de Flota")]
    public string idRobot = "AMR-01";

    [Header("Telemetría Batería")]
    public float bateriaActual = 80.0f;
    public float coeficienteVacio = 0.00175f;
    public float coeficienteCarga = 0.00142f;
    public float consumoIdle = 0.0010f;

    [Header("Recarga Inductiva (Compresión 8h -> 3min = 160x)")]
    public const float FactorCompresionTurno = 28800f / 180f; // 160x
    public float tiempoRecargaRealSegundos = 3840f;
    private float tasaRecargaSimulada => (80.0f - 20.0f) / (tiempoRecargaRealSegundos / FactorCompresionTurno); // 2.50f

    [Header("Throttling Telemetría")]
    private float tiempoProximoEnvioTelemetria = 0f;
    private const float IntervaloTelemetria = 0.15f; // ~6.6 Hz
    private float ultimaBateriaTransmitida = -1f;
    private EstadoRobot ultimoEstadoTransmitido = (EstadoRobot)(-1);

    [Header("Sensores LiDAR")]
    public LayerMask capaObstaculos = 1; // Default layer (bit 0) por defecto
    private float distanciaDeteccionAlerta = 1.2f;
    private float distanciaMaxSensor = 4.0f;
    private bool obstaculoDetectado = false;
    private bool enParadaSeguridadLiDAR = false;
    private EstadoRobot estadoPrevioBloqueo = EstadoRobot.Inactivo;

    private float velocidadBase = 3.5f;
    private float aceleracionBase = 20.0f;
    private float coeficientePeso = 0.002f;

    [Header("Cinemática Industrial (ISO 3691-4)")]
    public const float RadioAgenteIndustrial = 0.60f;
    public const float VelocidadAngularIndustrial = 720.0f;

    [Header("Control de Tráfico Industrial (Mutex ISO 3691-4)")]
    private static readonly Dictionary<Transform, RobotAMR> zonasReservadasMutex = new Dictionary<Transform, RobotAMR>();
    private const float DistanciaCompuertaMutex = 3.5f;
    private const float RadioOcupacionFisicaMutex = 2.5f;
    private bool enEsperaMutex = false;

    [Header("Enrutamiento Ortogonal (VDA 5050)")]
    public const float ZAvenidaOeste = -32.0f;
    public const float ZAvenidaEste = -34.0f;
    public const float XPorticoSalidaNorte = 34.40f;
    public const float XPorticoEntradaSur = 32.60f;
    public const float ZUmbralPortico = -48.0f;
    public const float ZPieRampaCamion = -2.0f;
    public const float DesfaseCarrilRack = 1.75f;
    public const float UmbralPasoWaypoint = 0.65f;
    private readonly Queue<Vector3> colaWaypoints = new Queue<Vector3>();
    private readonly List<Vector3> bufferNodosRuta = new List<Vector3>();

    [Header("Panel de Control (Solo lectura)")]
    public EstadoRobot estadoActual = EstadoRobot.Inactivo;
    /*El NavMeshAgent es el motor físico de Unity. Pero nota la variable Pedido. Tu cilindro físico no conoce el archivo JSON completo ni le interesa.
    Solo conoce la estructura de un pedido aislado que le entrega el Gestor, manteniendo el código limpio y modular.*/
    private NavMeshAgent agente;
    private Pedido pedidoActual; // Conoce la estructura, pero no toda la base de datos
    private bool modoDegradacionActivo = false;

    public Pedido PedidoActual => pedidoActual;

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    public Transform puntoMontajeCarga;

    [Header("Activos 3D de Carga (Gemelo Físico)")]
    public GameObject prefabPalletCervezas;
    public GameObject prefabPalletBarriles;
    private Transform destinoActual;
    private GameObject palletActualGO;

    public GameObject PalletActualGO => palletActualGO;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();

        if (agente != null)
        {
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.acceleration = aceleracionBase;
            agente.stoppingDistance = 0.0f;
            agente.autoBraking = true;
        }

        ActualizarJerarquiaNavegacion();
    }

    private void LimpiarPalletTemporal()
    {
        if (palletActualGO != null)
        {
            Destroy(palletActualGO);
            palletActualGO = null;
        }
    }

    void OnDisable()
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        LimpiarPalletTemporal();
        LiberarMutexDeRobot(this);
    }

    void OnDestroy()
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        LimpiarPalletTemporal();
        LiberarMutexDeRobot(this);
    }

    public void ActualizarJerarquiaNavegacion()
    {
        if (agente == null) return;

        int offsetDesempate = ((!string.IsNullOrEmpty(idRobot) && idRobot.EndsWith("02")) || gameObject.name.Contains("02")) ? 1 : 0;

        switch (estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
                agente.avoidancePriority = 10 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                break;

            case EstadoRobot.Transportando:
                agente.avoidancePriority = 20 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;

            case EstadoRobot.En_Transito:
                agente.avoidancePriority = 50 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;

            case EstadoRobot.Recargando:
                agente.avoidancePriority = 80 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                break;

            case EstadoRobot.Inactivo:
            default:
                agente.avoidancePriority = 90 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
                break;
        }
    }

    private bool IntentarReservarZonaMutex(Transform zona)
    {
        if (zona == null) return true;

        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR propietario))
        {
            if (propietario == null || !propietario.gameObject.activeInHierarchy ||
                propietario.estadoActual == EstadoRobot.Inactivo || propietario.estadoActual == EstadoRobot.Recargando)
            {
                zonasReservadasMutex.Remove(zona);
            }
            else if (propietario != this)
            {
                return false;
            }
        }

        if (managerEVE != null)
        {
            RobotAMR otroRobot = (managerEVE.robotAMR_01 != null && managerEVE.robotAMR_01 != this) ? managerEVE.robotAMR_01 :
                                 ((managerEVE.robotAMR_02 != null && managerEVE.robotAMR_02 != this) ? managerEVE.robotAMR_02 : null);

            if (otroRobot != null && otroRobot.gameObject.activeInHierarchy && otroRobot.estadoActual != EstadoRobot.Inactivo)
            {
                Vector3 puntoRef = (agente != null && agente.hasPath) ? agente.pathEndPosition : zona.position;
                Vector2 posOtroXZ = new Vector2(otroRobot.transform.position.x, otroRobot.transform.position.z);
                Vector2 posRefXZ = new Vector2(puntoRef.x, puntoRef.z);

                if (Vector2.Distance(posOtroXZ, posRefXZ) < RadioOcupacionFisicaMutex)
                {
                    return false;
                }
            }
        }

        zonasReservadasMutex[zona] = this;
        return true;
    }

    private void LiberarZonaMutex(Transform zona, RobotAMR propietario)
    {
        if (zona == null) return;
        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR actual) && actual == propietario)
        {
            zonasReservadasMutex.Remove(zona);
        }
    }

    private static void LiberarMutexDeRobot(RobotAMR robot)
    {
        if (robot == null || zonasReservadasMutex.Count == 0) return;

        List<Transform> clavesALiberar = null;
        foreach (var kvp in zonasReservadasMutex)
        {
            if (kvp.Value == null || kvp.Value == robot)
            {
                if (clavesALiberar == null) clavesALiberar = new List<Transform>();
                clavesALiberar.Add(kvp.Key);
            }
        }

        if (clavesALiberar != null)
        {
            for (int i = 0; i < clavesALiberar.Count; i++)
            {
                zonasReservadasMutex.Remove(clavesALiberar[i]);
            }
        }
    }

    private void GestionarCompuertaMutex()
    {
        if (enParadaSeguridadLiDAR || estadoActual == EstadoRobot.Interrumpido) return;

        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            Transform objetivo = (estadoActual == EstadoRobot.En_Transito) ? destinoActual : zonaTransferencia;

            // Liberar cualquier cerrojo previo que ya no sea el objetivo activo si el robot ya salió del radio físico
            if (zonasReservadasMutex.Count > 0)
            {
                List<Transform> obsoletas = null;
                Vector2 posActualXZ = new Vector2(transform.position.x, transform.position.z);
                foreach (var kvp in zonasReservadasMutex)
                {
                    if (kvp.Value == this && kvp.Key != null && kvp.Key != objetivo)
                    {
                        Vector2 posZonaXZ = new Vector2(kvp.Key.position.x, kvp.Key.position.z);
                        if (Vector2.Distance(posActualXZ, posZonaXZ) > RadioOcupacionFisicaMutex)
                        {
                            if (obsoletas == null) obsoletas = new List<Transform>();
                            obsoletas.Add(kvp.Key);
                        }
                    }
                }
                if (obsoletas != null)
                {
                    for (int i = 0; i < obsoletas.Count; i++) zonasReservadasMutex.Remove(obsoletas[i]);
                }
            }

            if (objetivo != null && agente != null && colaWaypoints.Count == 0 && !agente.pathPending)
            {
                if (agente.remainingDistance <= DistanciaCompuertaMutex &&
                    DistanciaPlanaADestinoAgente() <= DistanciaCompuertaMutex + 0.25f)
                {
                    if (!IntentarReservarZonaMutex(objetivo))
                    {
                        if (!enEsperaMutex)
                        {
                            enEsperaMutex = true;
                            agente.isStopped = true;
                            agente.velocity = Vector3.zero;
                        }
                    }
                    else if (enEsperaMutex)
                    {
                        enEsperaMutex = false;
                        agente.isStopped = false;
                    }
                }
                else if (enEsperaMutex)
                {
                    enEsperaMutex = false;
                    agente.isStopped = false;
                }
            }
        }
        else
        {
            enEsperaMutex = false;
        }
    }

    private Vector3 AjustarPuntoNavMesh(Vector3 punto)
    {
        if (NavMesh.SamplePosition(punto, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return punto;
    }

    public float ResolverEjeCarrilX(Vector3 posicion, out Vector3 puntoCarrilNavMesh, Transform rackReferencia = null)
    {
        // Si la coordenada está en la rampa o interior del Flatbed del camión (Z > -1.0m),
        // muestrear directamente sobre el NavMesh del camión sin aplicar DesfaseCarrilRack
        if (posicion.z > -1.0f)
        {
            if (NavMesh.SamplePosition(posicion, out NavMeshHit hitCamion, 2.5f, NavMesh.AllAreas))
            {
                puntoCarrilNavMesh = hitCamion.position;
            }
            else
            {
                puntoCarrilNavMesh = posicion;
            }
            return puntoCarrilNavMesh.x;
        }

        if (rackReferencia == null && destinoActual != null &&
            Vector2.Distance(new Vector2(posicion.x, posicion.z), new Vector2(destinoActual.position.x, destinoActual.position.z)) < 0.25f)
        {
            rackReferencia = destinoActual;
        }

        if (rackReferencia != null)
        {
            Collider colliderRack = rackReferencia.GetComponent<Collider>();
            if (colliderRack != null)
            {
                float xRack;
                // Determina si la cara frontal apunta al Oeste (ej. P1-C1) o Este (ej. P1-C2)
                if (colliderRack.bounds.center.x < -9.0f || colliderRack.bounds.center.x > 0.0f)
                {
                    xRack = colliderRack.bounds.min.x - 1.75f; // Pasillo Oeste
                }
                else
                {
                    xRack = colliderRack.bounds.max.x + 1.75f; // Pasillo Este
                }

                Vector3 candidatoRack = new Vector3(xRack, 0.0f, posicion.z);
                if (NavMesh.SamplePosition(candidatoRack, out NavMeshHit hitRack, 2.5f, NavMesh.AllAreas))
                {
                    puntoCarrilNavMesh = hitRack.position;
                }
                else
                {
                    puntoCarrilNavMesh = candidatoRack;
                }

                return puntoCarrilNavMesh.x;
            }
        }

        Vector3 baseNav = posicion;
        if (NavMesh.SamplePosition(posicion, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            baseNav = hit.position;
        }

        float dx = baseNav.x - posicion.x;
        float xCarril = baseNav.x;
        if (Mathf.Abs(dx) > 0.15f)
        {
            xCarril = baseNav.x + Mathf.Sign(dx) * DesfaseCarrilRack;
        }

        Vector3 candidato = new Vector3(xCarril, baseNav.y, baseNav.z);
        if (NavMesh.SamplePosition(candidato, out NavMeshHit hitCarril, 2.0f, NavMesh.AllAreas))
        {
            puntoCarrilNavMesh = hitCarril.position;
        }
        else
        {
            puntoCarrilNavMesh = baseNav;
        }

        return puntoCarrilNavMesh.x;
    }

    private static void AgregarNodoRuta(List<Vector3> nodos, Vector3 candidato, bool esDestinoFinal = false)
    {
        if (nodos.Count == 0)
        {
            nodos.Add(candidato);
            return;
        }

        Vector3 ultimo = nodos[nodos.Count - 1];
        Vector2 ultimoXZ = new Vector2(ultimo.x, ultimo.z);
        Vector2 candXZ = new Vector2(candidato.x, candidato.z);

        if (Vector2.Distance(ultimoXZ, candXZ) > 0.25f)
        {
            nodos.Add(candidato);
        }
        else if (esDestinoFinal && nodos.Count > 1)
        {
            nodos[nodos.Count - 1] = candidato;
        }
    }

    private void ConstruirNodosManhattan(Vector3 origen, Vector3 destino, List<Vector3> nodos, Transform rackOrigen = null, Transform rackDestino = null)
    {
        nodos.Clear();

        float xOrigen = ResolverEjeCarrilX(origen, out Vector3 origenNav, rackOrigen);
        float xDestino = ResolverEjeCarrilX(destino, out Vector3 destinoNav, rackDestino);

        bool origenEnSalaCarga = origen.z < ZUmbralPortico;
        bool destinoEnSalaCarga = destino.z < ZUmbralPortico;
        bool origenEnCamion = origen.z > -1.0f;
        bool destinoEnCamion = destino.z > -1.0f;

        // Caso 1: Ambos puntos dentro de la Sala de Carga
        if (origenEnSalaCarga && destinoEnSalaCarga)
        {
            AgregarNodoRuta(nodos, origenNav);
            if (Mathf.Abs(origenNav.x - destinoNav.x) > 0.25f && Mathf.Abs(origenNav.z - destinoNav.z) > 0.25f)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(destinoNav.x, destinoNav.y, origenNav.z)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
            return;
        }

        // Caso 2: Ambos puntos en el mismo pasillo / muelle de la nave principal (sin cruzar pórtico)
        if (!origenEnSalaCarga && !destinoEnSalaCarga && Mathf.Abs(xOrigen - xDestino) <= 0.5f)
        {
            AgregarNodoRuta(nodos, origenNav);
            if (origenEnCamion && !destinoEnCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
            }
            else if (!origenEnCamion && destinoEnCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, ZPieRampaCamion)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
            return;
        }

        float xSalidaAvenida = origenEnSalaCarga ? XPorticoSalidaNorte : xOrigen;
        float xEntradaAvenida = destinoEnSalaCarga ? XPorticoEntradaSur : xDestino;

        // Selección de carril direccional en la Avenida Transversal:
        // -32.0f para flujos hacia el Oeste (Izquierda), -34.0f para flujos hacia el Este (Derecha)
        float zAvenida = (xEntradaAvenida < xSalidaAvenida) ? ZAvenidaOeste : ZAvenidaEste;

        AgregarNodoRuta(nodos, origenNav);

        // Si parte desde el interior del camión / rampa, alinear primero con el pie de rampa en suelo firme
        if (origenEnCamion)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
        }

        // 1. Salida hacia la Avenida Principal
        if (origenEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, 0.0f, origenNav.z)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, 0.0f, zAvenida)));
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, zAvenida)));
        }

        // 2. Transición por la Avenida y Entrada al Destino
        if (destinoEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, 0.0f, zAvenida)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, 0.0f, destinoNav.z)));
            AgregarNodoRuta(nodos, destinoNav, true);
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, zAvenida)));
            // Waypoint de alineación perpendicular al pie de la rampa antes de ingresar al slot LIFO del camión
            if (destinoEnCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, ZPieRampaCamion)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
        }
    }

    private void TrazarRutaOrtogonal(Vector3 destinoFinal, Transform rackDestino = null)
    {
        if (agente == null) return;

        colaWaypoints.Clear();
        if (rackDestino == null && destinoActual != null &&
            Vector2.Distance(new Vector2(destinoFinal.x, destinoFinal.z), new Vector2(destinoActual.position.x, destinoActual.position.z)) < 0.25f)
        {
            rackDestino = destinoActual;
        }
        ConstruirNodosManhattan(transform.position, destinoFinal, bufferNodosRuta, null, rackDestino);

        for (int i = 1; i < bufferNodosRuta.Count; i++)
        {
            colaWaypoints.Enqueue(bufferNodosRuta[i]);
        }

        if (colaWaypoints.Count > 0)
        {
            Vector3 primerWaypoint = colaWaypoints.Dequeue();
            agente.autoBraking = (colaWaypoints.Count == 0);
            agente.SetDestination(primerWaypoint);
        }
        else
        {
            ResolverEjeCarrilX(destinoFinal, out Vector3 destinoNav, rackDestino);
            agente.autoBraking = true;
            agente.SetDestination(destinoNav);
        }
    }

    private float DistanciaPlanaADestinoAgente()
    {
        if (agente == null) return float.MaxValue;
        Vector2 posXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 destXZ = new Vector2(agente.destination.x, agente.destination.z);
        return Vector2.Distance(posXZ, destXZ);
    }

    private bool AvanzarColaWaypoints()
    {
        if (agente == null || enParadaSeguridadLiDAR || enEsperaMutex || estadoActual == EstadoRobot.Interrumpido) return false;
        if (colaWaypoints.Count == 0) return false;

        if (!agente.pathPending &&
            agente.remainingDistance <= UmbralPasoWaypoint &&
            DistanciaPlanaADestinoAgente() <= UmbralPasoWaypoint + 0.25f)
        {
            Vector3 siguienteWaypoint = colaWaypoints.Dequeue();
            agente.autoBraking = (colaWaypoints.Count == 0);
            agente.SetDestination(siguienteWaypoint);
            return true;
        }
        return false;
    }

    private float CalcularDistanciaNavMesh(Vector3 origen, Vector3 destino, Transform rackOrigen = null, Transform rackDestino = null)
    {
        List<Vector3> nodos = new List<Vector3>(8);
        ConstruirNodosManhattan(origen, destino, nodos, rackOrigen, rackDestino);

        if (nodos.Count > 1)
        {
            float distanciaTotal = 0f;
            for (int i = 0; i < nodos.Count - 1; i++)
            {
                Vector2 a = new Vector2(nodos[i].x, nodos[i].z);
                Vector2 b = new Vector2(nodos[i + 1].x, nodos[i + 1].z);
                distanciaTotal += Vector2.Distance(a, b);
            }
            return distanciaTotal;
        }

        Vector3 dif = destino - origen;
        dif.y = 0f;
        return dif.magnitude;
    }

    [Header("Batería Industrial LiFePO4")]
    public const float UmbralSeguridadLiFePO4 = 20.0f;

    [Header("Ruteo TMS")]
    public int numCamionAsignado = 1;

    public Vector3 ObtenerPosicionDescargaActiva()
    {
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null)
        {
            return tms.ObtenerCoordenadaSlotActivo(numCamionAsignado);
        }
        return zonaTransferencia != null ? zonaTransferencia.position : transform.position;
    }

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        // Si la zona de transferencia no ha sido resuelta para este pedido, consultar al TMS
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (zonaTransferencia == null && tms != null)
        {
            zonaTransferencia = tms.ConsultarMuelleDestino(pedido, out numCamionAsignado, out _);
        }

        float distanciaIda = CalcularDistanciaNavMesh(transform.position, destino.position, null, destino);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            Vector3 destinoSlotCamion = ObtenerPosicionDescargaActiva();
            distanciaVuelta = CalcularDistanciaNavMesh(destino.position, destinoSlotCamion, destino, null);
        }

        // Proyección del consumo: ida vacío + vuelta cargado con ruido térmico máximo (1.15f)
        float gastoIda = distanciaIda * coeficienteVacio;
        float gastoVuelta = distanciaVuelta * coeficienteCarga * (pedido.peso_kg / 100f) * 1.15f;
        float gastoProyectadoTotal = gastoIda + gastoVuelta;

        bateriaRestanteEstimada = bateriaActual - gastoProyectadoTotal;

        return bateriaRestanteEstimada >= UmbralSeguridadLiFePO4;
    }

    public void AsignarMision(Pedido nuevoPedido)
    {
        Transform destino = null;
        if (nuevoPedido != null && !string.IsNullOrEmpty(nuevoPedido.coordenada_bodega))
        {
            GameObject go = GameObject.Find(nuevoPedido.coordenada_bodega);
            if (go != null) destino = go.transform;
        }
        AsignarMision(nuevoPedido, destino);
    }

    public void AsignarMision(Pedido nuevoPedido, Transform destinoFisico)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual == EstadoRobot.Recargando)
        {
            Debug.LogWarning($"[{idRobot}]: Llamada a AsignarMision rechazada — unidad acoplada en estado Recargando.");
            return;
        }
        if (estadoActual != EstadoRobot.Inactivo) return;

        // CRI-05: Guardia de Nulidad estricta para evitar NullReferenceException
        if (destinoFisico == null)
        {
            string idPallet = nuevoPedido != null ? nuevoPedido.id_pallet : "NULO";
            string coord = nuevoPedido != null ? nuevoPedido.coordenada_bodega : "DESCONOCIDA";
            Debug.LogError($"[ALERTA WMS] Destino físico nulo recibido para pedido {idPallet} ({coord}). Misión abortada.");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ERROR WMS] Ubicación {coord} no localizada en bodega. Misión abortada.</color>");
            return;
        }

        pedidoActual = nuevoPedido;
        
        if (!CalcularViabilidadOperativa(pedidoActual, destinoFisico, out float bateriaProyectada))
        {
            destinoActual = destinoFisico;
            Debug.Log("<color=#10B981>[FLUJO]: 6. Pedido 3 evaluado. Emergencia de Batería (Ind. 5.0) disparada. Esperando decisión del supervisor.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ALERTA PREDICTIVA] Misión inviable. Retorno proyectado: {bateriaProyectada:F1}% (Umbral LiFePO4: {UmbralSeguridadLiFePO4:F0}%). Detención preventiva.</color>");
            DashboardUI.Instance?.MostrarEmergencia(true, bateriaProyectada, 3.2f);
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            if (agente != null) agente.isStopped = true;
            return;
        }

        DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "EN TRÁNSITO", Color.yellow);
        destinoActual = destinoFisico;
        enEsperaMutex = false;
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        agente.radius = RadioAgenteIndustrial;
        agente.angularSpeed = VelocidadAngularIndustrial;
        agente.speed = velocidadBase;
        agente.acceleration = aceleracionBase;

        TrazarRutaOrtogonal(destinoActual.position); // Da la orden de moverse por rieles VDA 5050
        Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destinoFisico.name + " para buscar " + pedidoActual.codigo_sku);
        DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destinoFisico.name);
    }

    public void EnviarACargaInductiva(Vector3 posicionPad)
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        // Limpieza estricta de corrutinas y memoria de la misión abortada para evitar extracciones fantasma
        StopAllCoroutines();
        LimpiarPalletTemporal();
        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        estadoActual = EstadoRobot.Recargando;
        ActualizarJerarquiaNavegacion();

        if (agente != null)
        {
            agente.updateRotation = true;
            agente.isStopped = false;
            agente.ResetPath();
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = velocidadBase;
            agente.acceleration = aceleracionBase;
            TrazarRutaOrtogonal(posicionPad);
        }

        OnEstadoCambiado?.Invoke(estadoActual);
        Debug.Log($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de carga en {posicionPad}");
        DashboardUI.Instance?.RegistrarLog($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de recarga.");
    }

    public void EjecutarOverride()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        enEsperaMutex = false;
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        if (agente != null) agente.isStopped = false;
        
        modoDegradacionActivo = true;
        if (agente != null) agente.acceleration = Mathf.Max(1.0f, agente.acceleration * 0.5f);
        coeficienteCarga *= 1.3f;

        if (destinoActual != null)
        {
            TrazarRutaOrtogonal(destinoActual.position);
        }
        
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.RegistrarLog("<color=orange>[OVERRIDE EJECUTADO] Protocolo de contingencia forzado. Modo de degradación térmica activo: Aceleración -50%, Consumo +30%.</color>");
    }

    public void AbortarMisionPorSeguridad()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        string idPallet = pedidoActual != null ? pedidoActual.id_pallet : "ORDEN";
        int indicePedido = managerEVE != null ? managerEVE.indicePedidoActual : 0;
        DashboardUI.Instance?.ActualizarEstadoManifiesto(indicePedido, "REPROGRAMADO", new Color(1.0f, 0.55f, 0.1f));

        DashboardUI.Instance?.RegistrarLog($"<color=#FFA500>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Celda protegida al {bateriaActual:F1}%. Orden reprogramada.");
        Debug.Log($"<color=orange>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Batería conservada al {bateriaActual:F1}%.");

        StopAllCoroutines();
        LimpiarPalletTemporal();
        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        if (agente != null)
        {
            agente.updateRotation = true;
            agente.autoBraking = true;
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        if (managerEVE != null)
        {
            managerEVE.NotificarMisionAbortada();
        }
    }

    public void ReanudarTrasDespejeLiDAR()
    {
        if (!enParadaSeguridadLiDAR) return;

        enParadaSeguridadLiDAR = false;
        obstaculoDetectado = false;
        estadoActual = estadoPrevioBloqueo;
        ActualizarJerarquiaNavegacion();

        if (agente != null)
        {
            agente.isStopped = enEsperaMutex;
        }

        OnEstadoCambiado?.Invoke(estadoActual);
    }

    private GameObject ConstruirPalletVisual(Pedido pedido)
    {
        GameObject rootPallet = new GameObject($"Pallet_{pedido.id_pallet}_{pedido.codigo_sku}");
        if (puntoMontajeCarga != null)
        {
            rootPallet.transform.SetParent(puntoMontajeCarga, false);
            rootPallet.transform.localPosition = Vector3.zero;
            rootPallet.transform.localRotation = Quaternion.identity;
        }
        else
        {
            rootPallet.transform.SetParent(transform, false);
            rootPallet.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            rootPallet.transform.localRotation = Quaternion.identity;
        }

        Transform padreMontaje = puntoMontajeCarga != null ? puntoMontajeCarga : transform;
        rootPallet.transform.SetParent(padreMontaje, false);
        rootPallet.transform.localScale = Vector3.one;

        // Selección dinámica por SKU / Peso (920 kg o barril 50L -> PalletBarriles; 850 kg -> PalletCervezas)
        bool esBarrilCritico = pedido != null &&
            (pedido.peso_kg >= 900f ||
             (!string.IsNullOrEmpty(pedido.codigo_sku) &&
              (pedido.codigo_sku.Contains("50L") || pedido.codigo_sku.Contains("BAR"))));

        GameObject prefabElegido = esBarrilCritico ? prefabPalletBarriles : prefabPalletCervezas;

        if (prefabElegido != null)
        {
            GameObject modeloInstanciado = Instantiate(prefabElegido, rootPallet.transform, false);
            modeloInstanciado.name = prefabElegido.name;
            modeloInstanciado.transform.localPosition = Vector3.zero;
            modeloInstanciado.transform.localRotation = Quaternion.identity;
            modeloInstanciado.transform.localScale = Vector3.one;

            // 1. REGLA CRÍTICA LiDAR: Deshabilitar y destruir cualquier Collider en raíz o hijos
            Collider[] colliders = modeloInstanciado.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                    Destroy(colliders[i]);
                }
            }

            // 2. Corrección matemática de Pivote (Centrado X/Z = 0 y Base Inferior Y_min = 0.000m)
            //    + Optimización GPU para mallas densas (ej. PalletBarriles 1.37M tris -> sin ShadowCaster)
            Renderer[] renderers = modeloInstanciado.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds boundsMundo = renderers[0].bounds;
                for (int i = 0; i < renderers.Length; i++)
                {
                    boundsMundo.Encapsulate(renderers[i].bounds);

                    MeshFilter mf = renderers[i].GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 50000)
                    {
                        renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderers[i].receiveShadows = false;
                    }
                }

                Vector3 puntoBaseCentroMundo = new Vector3(boundsMundo.center.x, boundsMundo.min.y, boundsMundo.center.z);
                Vector3 desfaseLocal = rootPallet.transform.InverseTransformPoint(puntoBaseCentroMundo);
                modeloInstanciado.transform.localPosition -= desfaseLocal;
            }

            return rootPallet;
        }

        // Fallback seguro (primitivas) si algún prefab no estuviera asignado en el Inspector
        // 1. Base de madera oscura (Estiba oficial: 1.00m x 0.15m x 1.20m)
        GameObject baseMadera = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseMadera.name = "Base_Madera";
        baseMadera.transform.SetParent(rootPallet.transform, false);
        baseMadera.transform.localPosition = new Vector3(0f, 0.075f, 0f);
        baseMadera.transform.localScale = new Vector3(1.00f, 0.15f, 1.20f);

        // REGLA CRÍTICA DE FÍSICA: Eliminar cualquier Collider para no interferir con LiDAR ni NavMesh
        Collider colBase = baseMadera.GetComponent<Collider>();
        if (colBase != null) Destroy(colBase);

        Renderer rendBase = baseMadera.GetComponent<Renderer>();
        if (rendBase != null)
        {
            rendBase.material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            rendBase.material.color = new Color(0.35f, 0.20f, 0.10f); // Tono madera oscura
        }

        // 2. Bloque de carga
        GameObject bloqueCarga = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bloqueCarga.name = "Bloque_Carga";
        bloqueCarga.transform.SetParent(rootPallet.transform, false);
        bloqueCarga.transform.localPosition = new Vector3(0f, 0.80f, 0f);
        bloqueCarga.transform.localScale = new Vector3(0.96f, 1.45f, 1.16f);

        // REGLA CRÍTICA DE FÍSICA: Eliminar Collider del bloque de carga
        Collider colCarga = bloqueCarga.GetComponent<Collider>();
        if (colCarga != null) Destroy(colCarga);

        Renderer rendCarga = bloqueCarga.GetComponent<Renderer>();
        if (rendCarga != null)
        {
            rendCarga.material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            if (pedido.peso_kg >= 900f || pedido.codigo_sku.Contains("50L"))
            {
                // Pedido crítico 920 kg: Rojo industrial distintivo
                rendCarga.material.color = new Color(0.85f, 0.15f, 0.10f);
            }
            else
            {
                // Pedidos estándar 850 kg: Color ámbar cervecero dorado
                rendCarga.material.color = new Color(0.85f, 0.55f, 0.12f);
            }
        }

        return rootPallet;
    }

    IEnumerator ProcesoExtraccion()
    {
        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        // Simulación de tiempo de extracción de carga dividida en Sub-fase 1 (Acople) y Sub-fase 2 (Rodillos 1.5s)
        float tiempoExtraccion = Mathf.Lerp(1.2f, 3.8f, pedidoActual.peso_kg / 1000f) + Random.Range(-0.2f, 0.2f);
        const float duracionRodillos = 1.5f;
        float duracionSubFase1 = Mathf.Max(0.2f, tiempoExtraccion - duracionRodillos);
        yield return new WaitForSeconds(duracionSubFase1);

        // Sub-fase 2 (1.5s): Instanciación en el hueco del rack y deslizamiento SmoothStep hacia puntoMontajeCarga
        palletActualGO = ConstruirPalletVisual(pedidoActual);
        Transform padreMontaje = puntoMontajeCarga != null ? puntoMontajeCarga : transform;
        Vector3 metaLocal = puntoMontajeCarga != null ? Vector3.zero : new Vector3(0f, 0.6f, 0f);

        if (palletActualGO != null && destinoActual != null)
        {
            Vector3 inicioLocal = padreMontaje.InverseTransformPoint(destinoActual.position);
            palletActualGO.transform.localPosition = inicioLocal;
            palletActualGO.transform.localRotation = Quaternion.identity;

            float transcurrido = 0f;
            while (transcurrido < duracionRodillos)
            {
                transcurrido += Time.deltaTime;
                float t = Mathf.Clamp01(transcurrido / duracionRodillos);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                if (palletActualGO != null)
                {
                    palletActualGO.transform.localPosition = Vector3.Lerp(inicioLocal, metaLocal, smoothT);
                }
                yield return null;
            }

            if (palletActualGO != null)
            {
                palletActualGO.transform.localPosition = metaLocal;
                palletActualGO.transform.localRotation = Quaternion.identity;
            }
        }

        if (agente != null)
        {
            agente.isStopped = false;
        }

        // Liberar reserva del rack extraído al iniciar retorno hacia muelle
        LiberarZonaMutex(destinoActual, this);

        // Consultar o validar muelle de destino en ModuloDespachoTMS al iniciar retorno
        if (managerEVE != null && managerEVE.gestorTMS != null)
        {
            Transform muelleDestino = managerEVE.gestorTMS.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out _);
            if (muelleDestino != null) zonaTransferencia = muelleDestino;
        }
        else if (ModuloDespachoTMS.Instance != null)
        {
            Transform muelleDestino = ModuloDespachoTMS.Instance.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out _);
            if (muelleDestino != null) zonaTransferencia = muelleDestino;
        }
        
        if (zonaTransferencia != null)
        {
            enEsperaMutex = false;
            estadoActual = EstadoRobot.Transportando;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase - (pedidoActual.peso_kg * coeficientePeso * 2f));

            Vector3 destinoSlotLIFO = ObtenerPosicionDescargaActiva();
            TrazarRutaOrtogonal(destinoSlotLIFO);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba al Slot LIFO del Camión (" + zonaTransferencia.name + $" en {destinoSlotLIFO}).");
            DashboardUI.Instance?.RegistrarLog("AMR Retornando: Llevando estiba a " + zonaTransferencia.name);
        }
        else
        {
            Debug.LogError("AMR Error: Falta asignar la Zona de Transferencia en el Inspector.");
            DashboardUI.Instance?.RegistrarLog("AMR Error: Falta Zona de Transferencia.");
        }
    }

    IEnumerator ProcesoEntrega()
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        estadoActual = EstadoRobot.Entregando;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "ENTREGADO", Color.green);
        Debug.Log("<color=magenta>LIFO:</color> Descargando estiba en Slot Flatbed del Camión.");
        DashboardUI.Instance?.RegistrarLog("LIFO: Descargando estiba en Zona de Transferencia.");

        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        // a) 0.5 s para desacoplar y asentar suavemente la estiba a ySuperficie (1.813 m) registrándola en el TMS
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        Vector3 destinoSlotLIFO = ObtenerPosicionDescargaActiva();
        Quaternion rotacionSlot = zonaTransferencia != null ? zonaTransferencia.rotation : Quaternion.identity;

        GameObject palletEntregado = palletActualGO;
        palletActualGO = null;

        if (palletEntregado != null)
        {
            palletEntregado.transform.SetParent(null, true);
            Vector3 posInicioDescenso = palletEntregado.transform.position;
            Quaternion rotInicioDescenso = palletEntregado.transform.rotation;

            const float duracionAsentamiento = 0.5f;
            float tAsentamiento = 0f;
            while (tAsentamiento < duracionAsentamiento)
            {
                tAsentamiento += Time.deltaTime;
                float factor = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tAsentamiento / duracionAsentamiento));
                if (palletEntregado != null)
                {
                    palletEntregado.transform.position = Vector3.Lerp(posInicioDescenso, destinoSlotLIFO, factor);
                    palletEntregado.transform.rotation = Quaternion.Slerp(rotInicioDescenso, rotacionSlot, factor);
                }
                yield return null;
            }

            if (tms != null)
            {
                tms.RegistrarEstibaEnCamion(numCamionAsignado, pedidoActual, palletEntregado);
            }
            else if (managerEVE != null)
            {
                managerEVE.RegistrarEntregaLIFO(pedidoActual, palletEntregado);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // b) 1.0 s de retroceso recto de despegue (1.5 m hacia -transform.forward con agente.updateRotation = false)
        const float duracionRetroceso = 1.0f;
        const float distanciaRetroceso = 1.5f;
        if (agente != null)
        {
            bool prevUpdateRotation = agente.updateRotation;
            agente.updateRotation = false;

            Vector3 dirRetroceso = -transform.forward;
            dirRetroceso.y = 0f;
            if (dirRetroceso.sqrMagnitude < 0.001f) dirRetroceso = Vector3.back;
            dirRetroceso.Normalize();

            float velocidadRetroceso = distanciaRetroceso / duracionRetroceso;
            float tRetroceso = 0f;
            while (tRetroceso < duracionRetroceso)
            {
                float dt = Time.deltaTime;
                tRetroceso += dt;
                agente.Move(dirRetroceso * (velocidadRetroceso * dt));
                yield return null;
            }

            agente.updateRotation = prevUpdateRotation;
            agente.isStopped = false;
        }
        else
        {
            yield return new WaitForSeconds(duracionRetroceso);
        }

        LiberarMutexDeRobot(this);
        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        agente.speed = velocidadBase;
        if (modoDegradacionActivo)
        {
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
        }
        else
        {
            agente.acceleration = aceleracionBase;
        }

        if (managerEVE != null)
        {
            managerEVE.ConfirmarEntregaExitosa();
        }
    }

    private bool EsObjetivoDeAcoplamiento(Transform hitTransform)
    {
        if (hitTransform == null) return false;

        // Si viaja hacia una estantería en En_Transito
        if (estadoActual == EstadoRobot.En_Transito && destinoActual != null)
        {
            if (hitTransform == destinoActual || 
                hitTransform.IsChildOf(destinoActual) || 
                destinoActual.IsChildOf(hitTransform))
            {
                return true;
            }
        }
        // Si retorna hacia la zona de transferencia / camión en Transportando
        else if (estadoActual == EstadoRobot.Transportando && zonaTransferencia != null)
        {
            if (hitTransform == zonaTransferencia || 
                hitTransform.IsChildOf(zonaTransferencia) || 
                zonaTransferencia.IsChildOf(hitTransform))
            {
                return true;
            }
        }

        return false;
    }

    private bool EsObstaculoISORegistrado(Transform hitTransform)
    {
        if (hitTransform == null || managerEVE == null || managerEVE.obstaculoPasilloISO == null) return false;
        Transform raizISO = managerEVE.obstaculoPasilloISO.transform;
        return hitTransform == raizISO || hitTransform.IsChildOf(raizISO);
    }

    private void ProcesarSensoresLiDAR()
    {
        if (enParadaSeguridadLiDAR) return;

        Vector3 origen = transform.position + Vector3.up * 0.5f;
        obstaculoDetectado = false;

        Vector3[] direcciones = new Vector3[]
        {
            transform.forward,
            Quaternion.Euler(0, -15, 0) * transform.forward,
            Quaternion.Euler(0, 15, 0) * transform.forward
        };

        foreach (var dir in direcciones)
        {
            if (Physics.Raycast(origen, dir, out RaycastHit hit, distanciaMaxSensor, capaObstaculos))
            {
                // 1. Filtrar propio chasis y componentes hijos (punto de carga, telemetría, estibas montadas)
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
                    continue;
                }

                // 2. Filtrar suelo de la bodega
                if (hit.transform.name.Contains("Suelo") || hit.point.y <= 0.35f)
                {
                    Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
                    continue;
                }

                // 3. Protocolo Docking Mute (ISO 3691-4): Acoplamiento con objetivo legítimo
                if (EsObjetivoDeAcoplamiento(hit.transform))
                {
                    // Visualización en amarillo: Acoplamiento legítimo en curso sin penalizar velocidad
                    Debug.DrawLine(origen, hit.point, Color.yellow);
                    continue;
                }

                // 4. Obstáculo imprevisto
                // Parada Cat. 3 solo con el AMR en movimiento y ante el obstáculo ISO registrado por EVE.
                // Evita disparos en reposo (spawn frente a P1-C1-F2) y que EVE desactive infraestructura estática (racks).
                bool amrEnMovimiento = estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando;
                if (hit.distance <= 0.85f && !enParadaSeguridadLiDAR && amrEnMovimiento && EsObstaculoISORegistrado(hit.transform))
                {
                    Debug.Log("<color=#10B981>[FLUJO]: 4. Emergencia LiDAR (Ind. 4.0) disparada correctamente.</color>");
                    enParadaSeguridadLiDAR = true;
                    estadoPrevioBloqueo = estadoActual;
                    obstaculoDetectado = true;
                    Debug.DrawLine(origen, hit.point, Color.red);

                    if (agente != null)
                    {
                        agente.isStopped = true;
                        agente.velocity = Vector3.zero;
                    }

                    estadoActual = EstadoRobot.Interrumpido;
                    ActualizarJerarquiaNavegacion();
                    OnEstadoCambiado?.Invoke(estadoActual);

                    if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();
                    managerEVE?.GestionarObstruccionAutonomaISO(this, hit.collider.gameObject);
                    break;
                }
                else if (hit.distance < distanciaDeteccionAlerta)
                {
                    // Zona de advertencia crítica: reducción preventiva de velocidad
                    obstaculoDetectado = true;
                    Debug.DrawLine(origen, hit.point, Color.red);
                }
                else
                {
                    // Obstáculo detectado en zona lejana (amarillo-naranja de aviso)
                    Debug.DrawLine(origen, hit.point, new Color(1f, 0.6f, 0f));
                }
            }
            else
            {
                Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
            }
        }

        // Gestión activa de velocidad preventiva según LiDAR
        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            float velocidadNominal = ObtenerVelocidadNominal();
            if (obstaculoDetectado)
            {
                // Campo de protección LiDAR activo: 40% de velocidad nominal
                agente.speed = velocidadNominal * 0.4f;
            }
            else
            {
                agente.speed = velocidadNominal;
            }
        }
    }

    private float ObtenerVelocidadNominal()
    {
        if (estadoActual == EstadoRobot.En_Transito)
        {
            return velocidadBase;
        }
        else if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
        {
            return Mathf.Max(0.5f, velocidadBase - (pedidoActual.peso_kg * coeficientePeso));
        }
        return velocidadBase;
    }

    void Update()
    {
        // Procesamiento activo del sensor LiDAR con Docking Mute (ISO 3691-4)
        ProcesarSensoresLiDAR();

        // Transición fluida entre tramos ortogonales (VDA 5050) sin parada en esquinas intermedias
        bool cambioTramoWaypoint = AvanzarColaWaypoints();

        // Gestión de exclusión mutua (Mutex) de zona de acople a <= 3.5m en el tramo final
        if (!cambioTramoWaypoint)
        {
            GestionarCompuertaMutex();
        }

        // Consumo pasivo obligatorio por encendido de sistemas lógicos (blindado a 0% en parada de seguridad/interrupción/espera Mutex)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido && estadoActual != EstadoRobot.Recargando)
        {
            bateriaActual -= consumoIdle * Time.deltaTime;
        }

        // Matemáticas de Batería (Termodinámica Inyectada)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando))
        {
            float distanciaRecorrida = agente.velocity.magnitude * Time.deltaTime;
            if (distanciaRecorrida > 0)
            {
                float gasto = 0f;
                if (estadoActual == EstadoRobot.En_Transito)
                {
                    gasto = distanciaRecorrida * coeficienteVacio;
                }
                else if (estadoActual == EstadoRobot.Transportando)
                {
                    // Inyección de Ruido Termodinámico: Oscilación orgánica (Perlin Noise) para fricción e ineficiencia de motor (0.98 a 1.15)
                    float factorDeRuido = Mathf.Lerp(0.98f, 1.15f, Mathf.PerlinNoise(Time.time * 0.5f, 0f));
                    gasto = distanciaRecorrida * coeficienteCarga * (pedidoActual.peso_kg / 100f) * factorDeRuido;
                }
                
                bateriaActual -= gasto;
            }
        }

        // CRI-04: Acotamiento de batería y corte crítico al 0%
        bateriaActual = Mathf.Max(0f, bateriaActual);
        if (bateriaActual <= 0f && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando))
        {
            if (agente != null)
            {
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            DashboardUI.Instance?.RegistrarLog("<color=red><b>[FALLO CATASTRÓFICO]:</b> Celda agotada al 0% en pasillo. Unidad inmovilizada.</color>");
            Debug.LogError("[FALLO CATASTRÓFICO] Batería agotada al 0% en pasillo. NavMeshAgent inmovilizado.");
        }

        // Telemetría con throttling (6.6 Hz / 0.15s) y compuerta por delta (> 0.1% o cambio de estado)
        if (Time.time >= tiempoProximoEnvioTelemetria || Mathf.Abs(bateriaActual - ultimaBateriaTransmitida) > 0.1f || estadoActual != ultimoEstadoTransmitido)
        {
            tiempoProximoEnvioTelemetria = Time.time + IntervaloTelemetria;
            ultimaBateriaTransmitida = bateriaActual;
            ultimoEstadoTransmitido = estadoActual;
            DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
        }

        if (cambioTramoWaypoint || colaWaypoints.Count > 0) return;

        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!enEsperaMutex && !agente.pathPending && agente.remainingDistance <= 1.0f && DistanciaPlanaADestinoAgente() <= 1.25f)
            {
                estadoActual = EstadoRobot.Extrayendo;
                ActualizarJerarquiaNavegacion();
                OnEstadoCambiado?.Invoke(estadoActual);
                Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                DashboardUI.Instance?.RegistrarLog("Destino Alcanzado: Extrayendo estiba.");
                StartCoroutine(ProcesoExtraccion());
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // CRI-01 y MED-04: Detección de arribo exacto al Slot LIFO en el Flatbed y desacople Drop & Reverse
            if (!enEsperaMutex && zonaTransferencia != null && !agente.pathPending && agente.remainingDistance <= 0.30f && DistanciaPlanaADestinoAgente() <= 0.40f)
            {
                StartCoroutine(ProcesoEntrega());
            }
        }
        else if (estadoActual == EstadoRobot.Recargando)
        {
            if (!agente.pathPending && agente.remainingDistance <= 0.5f && DistanciaPlanaADestinoAgente() <= 0.75f)
            {
                // Llegó al pad de inducción. Frenar y recargar progresivamente.
                agente.isStopped = true;
                // Tasa de recarga calibrada por compresión temporal 160x (20% -> 80% en ~24s)
                bateriaActual = Mathf.Min(80.0f, bateriaActual + (Time.deltaTime * tasaRecargaSimulada));
                DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
            }
        }
    }
}
