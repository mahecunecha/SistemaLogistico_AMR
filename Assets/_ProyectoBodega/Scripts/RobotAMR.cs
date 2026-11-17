using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando, Recargando, Apagado }
    
    public event System.Action<EstadoRobot> OnEstadoCambiado;

    [Header("Identidad de Flota")]
    public string idRobot = "AMR-01";

    [Header("Telemetría Batería")]
    public float bateriaActual = 80.0f;
    public float coeficienteVacio = 0.00175f;
    public float coeficienteCarga = 0.00142f;
    public float consumoIdle = 0.0010f;

    [Header("Gestión de Pausa Preventiva y Relevo")]
    public EstadoRobot estadoPreInterrupcionBateria = EstadoRobot.Inactivo;
    public bool enPausaPreventivaBateria20 = false;
    public bool relevoDiferidoPostEntrega = false;

    [Header("Recarga Inductiva KUKA KMP 1500P (20% -> 80% en 1h = 1.0%/min)")]
    // KUKA KMP 1500P Datasheet: Charge from 20% to 80% within 1 hour (60% / 3600s = 0.0166667% / s)
    public float tasaRecargaPorSegundo = 60.0f / 3600.0f; // 0.0166667f %/s (1.0% por minuto real)
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
    private float distanciaDeteccionAlerta = 1.65f;
    private float distanciaFrenadoEmergencia = 1.15f;
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

    [Header("Control de Tráfico Industrial (Mutex ISO 3691-4 & VDA 5050)")]
    private static readonly List<RobotAMR> flotaActiva = new List<RobotAMR>(3);
    private static readonly Dictionary<Transform, RobotAMR> zonasReservadasMutex = new Dictionary<Transform, RobotAMR>();
    private const float DistanciaCompuertaMutex = 3.5f;
    private const float RadioOcupacionFisicaMutex = 2.5f;
    private const float RadioBurbujaEntradaVDA = 2.50f;
    private const float RadioBurbujaSalidaVDA = 2.80f;
    private bool enEsperaMutex = false;
    private bool cediendoPasoAMR = false;
    private float tiempoDetenidoCediendo = 0f;
    private float tiempoInmunidadDespeje = 0f;

    [Header("Enrutamiento Ortogonal (VDA 5050)")]
    public const float ZAvenidaOeste = -32.0f;
    public const float ZAvenidaEste = -34.0f;
    public const float XPorticoSalidaNorte = 34.40f;
    public const float XPorticoEntradaSur = 32.60f;
    public const float ZUmbralPortico = -48.0f;
    public const float ZColectorSalaCarga = -51.80f;
    public const float ZPieRampaCamion = -2.0f;
    public const float DesfaseCarrilRack = 1.75f;
    public const float XCentroASRS = -21.20f;
    public const float DesfaseCarrilASRSCentro = 3.84f;
    public const float DesfaseFrontalAcopleVLM = 0.90f;
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
    private int indiceFilaManifiesto = 0;

    public Pedido PedidoActual => pedidoActual;
    [HideInInspector]
    public bool colapsoBMSArmadoPostDescarga = false;

    public bool TieneCargaFisica =>
        palletActualGO != null ||
        estadoActual == EstadoRobot.Extrayendo ||
        estadoActual == EstadoRobot.Transportando ||
        estadoActual == EstadoRobot.Entregando;

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    public Transform puntoMontajeCarga;

    [Header("Activos 3D de Carga (Gemelo Físico)")]
    public GameObject prefabPalletCervezas;
    public GameObject prefabPalletBarriles;
    private Transform destinoActual;
    private GameObject palletActualGO;
    private GameObject stockOcultadoActual;

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
        if (stockOcultadoActual != null)
        {
            stockOcultadoActual.SetActive(true);
            stockOcultadoActual = null;
        }
    }

    void OnEnable()
    {
        if (!flotaActiva.Contains(this))
        {
            flotaActiva.Add(this);
        }
    }

    void OnDisable()
    {
        flotaActiva.Remove(this);
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        colaWaypoints.Clear();
        LimpiarPalletTemporal();
        LiberarMutexDeRobot(this);
    }

    void OnDestroy()
    {
        flotaActiva.Remove(this);
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        colaWaypoints.Clear();
        LimpiarPalletTemporal();
        LiberarMutexDeRobot(this);
    }

    private int ObtenerIndiceUnidad()
    {
        if (!string.IsNullOrEmpty(idRobot))
        {
            if (idRobot.EndsWith("03")) return 3;
            if (idRobot.EndsWith("02")) return 2;
            if (idRobot.EndsWith("01")) return 1;
        }
        if (gameObject.name.Contains("03")) return 3;
        if (gameObject.name.Contains("02")) return 2;
        return 1;
    }

    public void ActualizarJerarquiaNavegacion()
    {
        if (agente == null || !agente.enabled) return;

        int offsetDesempate = Mathf.Max(0, ObtenerIndiceUnidad() - 1);

        switch (estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
            case EstadoRobot.Apagado:
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

    public bool EstaEvacuandoZonaMuelle()
    {
        return (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Recargando) &&
               transform.position.z > -12.0f && transform.position.x > 18.0f;
    }

    private bool IntentarReservarZonaMutex(Transform zona)
    {
        if (zona == null) return true;

        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR propietario))
        {
            if (propietario == null || !propietario.gameObject.activeInHierarchy ||
                propietario.estadoActual == EstadoRobot.Inactivo ||
                propietario.estadoActual == EstadoRobot.Recargando ||
                propietario.estadoActual == EstadoRobot.Apagado)
            {
                zonasReservadasMutex.Remove(zona);
            }
            else if (propietario != this)
            {
                return false;
            }
        }

        Vector3 puntoRef = (agente != null && agente.enabled && agente.hasPath) ? agente.pathEndPosition : zona.position;
        Vector2 posRefXZ = new Vector2(puntoRef.x, puntoRef.z);
        bool reservandoMuelleEntrega = estadoActual == EstadoRobot.Transportando && zona == zonaTransferencia;

        for (int i = 0; i < flotaActiva.Count; i++)
        {
            RobotAMR otroRobot = flotaActiva[i];
            if (otroRobot == null || otroRobot == this || !otroRobot.gameObject.activeInHierarchy) continue;
            if (otroRobot.estadoActual == EstadoRobot.Inactivo || otroRobot.estadoActual == EstadoRobot.Apagado) continue;

            // Si estamos entrando a un muelle, esperar antes de la rampa mientras otro AMR ocupe la cama o rampa de ese muelle
            if (reservandoMuelleEntrega &&
                otroRobot.transform.position.z > -5.5f &&
                Mathf.Abs(otroRobot.transform.position.x - zona.position.x) < 3.2f)
            {
                return false;
            }

            Vector2 posOtroXZ = new Vector2(otroRobot.transform.position.x, otroRobot.transform.position.z);
            if (Vector2.Distance(posOtroXZ, posRefXZ) < RadioOcupacionFisicaMutex)
            {
                return false;
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
        if (agente == null || !agente.enabled || !agente.isOnNavMesh) return;
        if (enParadaSeguridadLiDAR || cediendoPasoAMR || estadoActual == EstadoRobot.Interrumpido || estadoActual == EstadoRobot.Apagado) return;

        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando || estadoActual == EstadoRobot.Recargando)
        {
            Transform objetivo = (estadoActual == EstadoRobot.En_Transito)
                ? destinoActual
                : (estadoActual == EstadoRobot.Transportando ? zonaTransferencia : null);

            // Liberar cualquier cerrojo previo (o de muelle al descender bajo Z < -5.5f) si ya no es el objetivo activo
            if (zonasReservadasMutex.Count > 0)
            {
                List<Transform> obsoletas = null;
                Vector2 posActualXZ = new Vector2(transform.position.x, transform.position.z);
                foreach (var kvp in zonasReservadasMutex)
                {
                    if (kvp.Value == this && kvp.Key != null && kvp.Key != objetivo)
                    {
                        bool esMuelleDesocupado = kvp.Key.position.z > -4.0f && transform.position.z < -5.5f;
                        Vector2 posZonaXZ = new Vector2(kvp.Key.position.x, kvp.Key.position.z);
                        if (esMuelleDesocupado || Vector2.Distance(posActualXZ, posZonaXZ) > RadioOcupacionFisicaMutex)
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

            if (objetivo != null && agente != null && !agente.pathPending)
            {
                bool enDistanciaFinal = colaWaypoints.Count == 0 &&
                                        agente.remainingDistance <= DistanciaCompuertaMutex &&
                                        DistanciaPlanaADestinoAgente() <= DistanciaCompuertaMutex + 0.25f;

                bool enAproximacionPreRampa = estadoActual == EstadoRobot.Transportando &&
                                              objetivo == zonaTransferencia &&
                                              colaWaypoints.Count <= 1 &&
                                              transform.position.z > -11.0f &&
                                              transform.position.z < -5.5f &&
                                              Mathf.Abs(transform.position.x - objetivo.position.x) < 2.5f;

                if (enDistanciaFinal || enAproximacionPreRampa)
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

    public static bool EsEstacionTransferenciaFrontalASRS(Transform rackReferencia, Vector3 posicion)
    {
        if (rackReferencia != null)
        {
            if (rackReferencia.name == "P1-C1-F1" ||
                rackReferencia.name == "P1-C2-F1" ||
                rackReferencia.name == "P2-C4-F1" ||
                rackReferencia.name == "Punto_Acople_AMR" ||
                rackReferencia.name == "Estacion_Entrega_Rodillos" ||
                rackReferencia.name == "Estacion_PD_Rodillos_01" ||
                rackReferencia.name == "Base_Metalica_01")
            {
                return true;
            }

            Transform cursor = rackReferencia;
            while (cursor != null)
            {
                if (cursor.name == "ASRS_Estante_Heroe" || cursor.name == "Estacion_PD_Rodillos_01")
                {
                    return true;
                }
                cursor = cursor.parent;
            }
        }

        return Mathf.Abs(posicion.x - XCentroASRS) < 3.0f &&
               posicion.z <= -4.0f &&
               posicion.z >= -31.8f;
    }

    private static Vector3 ResolverPuntoAcopleFrontalASRS(Transform rackReferencia)
    {
        if (rackReferencia != null)
        {
            Transform cursor = rackReferencia;
            while (cursor != null)
            {
                if (cursor.name == "ASRS_Estante_Heroe")
                {
                    Transform acopleHijo = cursor.Find("Ascensor_VLM/Punto_Acople_AMR");
                    if (acopleHijo != null)
                    {
                        return new Vector3(acopleHijo.position.x, 0.0f, acopleHijo.position.z);
                    }
                    break;
                }
                cursor = cursor.parent;
            }
        }

        GameObject goAcople = GameObject.Find("Punto_Acople_AMR");
        if (goAcople != null)
        {
            Vector3 pos = goAcople.transform.position;
            return new Vector3(pos.x, 0.0f, pos.z);
        }

        return new Vector3(XCentroASRS, 0.0f, -30.57f);
    }

    private static Vector3 MuestrearNavMeshOCandidato(Vector3 candidato, float radioMax)
    {
        if (NavMesh.SamplePosition(candidato, out NavMeshHit hit, radioMax, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return candidato;
    }

    public float ResolverEjeCarrilX(Vector3 posicion, out Vector3 puntoCarrilNavMesh, Transform rackReferencia = null)
    {
        // Si la coordenada está en la rampa o interior del Flatbed del camión (Z > -1.0m),
        // muestrear directamente sobre el NavMesh del camión sin aplicar DesfaseCarrilRack
        if (posicion.z > -1.0f)
        {
            puntoCarrilNavMesh = MuestrearNavMeshOCandidato(posicion, 2.5f);
            return puntoCarrilNavMesh.x;
        }

        if (rackReferencia == null && destinoActual != null &&
            Vector2.Distance(new Vector2(posicion.x, posicion.z), new Vector2(destinoActual.position.x, destinoActual.position.z)) < 0.25f)
        {
            rackReferencia = destinoActual;
        }

        if (EsEstacionTransferenciaFrontalASRS(rackReferencia, posicion))
        {
            Vector3 candidatoFrontalPD = ResolverPuntoAcopleFrontalASRS(rackReferencia);
            puntoCarrilNavMesh = MuestrearNavMeshOCandidato(candidatoFrontalPD, 2.5f);
            return puntoCarrilNavMesh.x;
        }

        if (rackReferencia != null)
        {
            Collider colliderRack = rackReferencia.GetComponent<Collider>();
            if (colliderRack != null)
            {
                bool esASRS = rackReferencia.name.ToUpperInvariant().Contains("ASRS") ||
                              Mathf.Abs(colliderRack.bounds.center.x - XCentroASRS) < 1.5f;

                float xRack;
                if (esASRS)
                {
                    bool robotAlOeste = transform.position.x <= XCentroASRS;
                    xRack = robotAlOeste
                        ? (XCentroASRS - DesfaseCarrilASRSCentro)
                        : (XCentroASRS + DesfaseCarrilASRSCentro);
                }
                else
                {
                    // Determina si la cara frontal apunta al Oeste (ej. P1-C1) o Este (ej. P1-C2)
                    if (colliderRack.bounds.center.x < -9.0f || colliderRack.bounds.center.x > 0.0f)
                    {
                        xRack = colliderRack.bounds.min.x - DesfaseCarrilRack; // Pasillo Oeste
                    }
                    else
                    {
                        xRack = colliderRack.bounds.max.x + DesfaseCarrilRack; // Pasillo Este
                    }
                }

                float zRack = esASRS
                    ? Mathf.Clamp(posicion.z, colliderRack.bounds.min.z, colliderRack.bounds.max.z)
                    : posicion.z;
                Vector3 candidatoRack = new Vector3(xRack, 0.0f, zRack);
                puntoCarrilNavMesh = MuestrearNavMeshOCandidato(candidatoRack, 2.5f);
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
        bool origenEnZonaMuelle = !origenEnSalaCarga && !destinoEnCamion && origen.z > -4.5f && origen.x > 18.0f;
        float xCarrilEvacuacion = origenEnZonaMuelle ? (xOrigen - 2.20f) : xOrigen;

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
            if ((origenEnCamion || origenEnZonaMuelle) && !destinoEnCamion)
            {
                if (origen.z > ZPieRampaCamion)
                {
                    AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
                }
                if (destino.z < -7.0f)
                {
                    AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, -4.80f)));
                    AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xCarrilEvacuacion, 0.0f, -6.50f)));
                }
            }
            else if (!origenEnCamion && destinoEnCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, ZPieRampaCamion)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
            return;
        }

        float xSalidaAvenida = origenEnSalaCarga ? XPorticoSalidaNorte : xCarrilEvacuacion;
        float xEntradaAvenida = destinoEnSalaCarga ? XPorticoEntradaSur : xDestino;

        // Selección de carril direccional en la Avenida Transversal:
        // -32.0f para flujos hacia el Oeste (Izquierda), -34.0f para flujos hacia el Este (Derecha)
        float zAvenida = (xEntradaAvenida < xSalidaAvenida) ? ZAvenidaOeste : ZAvenidaEste;

        AgregarNodoRuta(nodos, origenNav);

        // Si parte desde el camión o pie de rampa, bajar recto y desviarse por el carril lateral izquierdo de evacuación
        if ((origenEnCamion || origenEnZonaMuelle) && !destinoEnCamion)
        {
            if (origen.z > ZPieRampaCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
            }
            if (origenEnZonaMuelle)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, -4.80f)));
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xCarrilEvacuacion, 0.0f, -6.50f)));
            }
        }
        else if (origenEnCamion)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
        }

        // 1. Salida hacia la Avenida Principal (vía carril colector frontal Z = -51.80m en Sala de Carga)
        if (origenEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(origenNav.x, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, 0.0f, zAvenida)));
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xCarrilEvacuacion, 0.0f, zAvenida)));
        }

        // 2. Transición por la Avenida y Entrada al Destino (vía carril colector Z = -51.80m hacia Pad)
        if (destinoEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, 0.0f, zAvenida)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(destinoNav.x, 0.0f, ZColectorSalaCarga)));
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
        if (agente == null || !agente.enabled || !agente.isOnNavMesh || enParadaSeguridadLiDAR || enEsperaMutex || cediendoPasoAMR || estadoActual == EstadoRobot.Interrumpido || estadoActual == EstadoRobot.Apagado) return false;
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
    public int slotLIFOAsignado = 0;
    public const float OffsetParadaRodillosCamion = 1.30f;

    public Vector3 ObtenerPosicionDescargaActiva()
    {
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null)
        {
            int slotObjetivo = slotLIFOAsignado > 0 ? slotLIFOAsignado : tms.ObtenerSlotLIFOProyectado(numCamionAsignado);
            return tms.ObtenerCoordenadaSlotLIFO(numCamionAsignado, slotObjetivo);
        }
        return zonaTransferencia != null ? zonaTransferencia.position : transform.position;
    }

    public Vector3 ObtenerPosicionParadaDescargaActiva()
    {
        Vector3 posSlot = ObtenerPosicionDescargaActiva();
        return new Vector3(posSlot.x, posSlot.y, posSlot.z - OffsetParadaRodillosCamion);
    }

    private void LiberarReservaSlotTMS()
    {
        if (slotLIFOAsignado <= 0) return;

        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null)
        {
            tms.CancelarReservaSlot(numCamionAsignado, slotLIFOAsignado);
        }
        slotLIFOAsignado = 0;
    }

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        // Si la zona de transferencia o el slot LIFO no han sido resueltos aún, consultar al TMS
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if ((zonaTransferencia == null || slotLIFOAsignado <= 0) && tms != null)
        {
            zonaTransferencia = tms.ConsultarMuelleDestino(pedido, out numCamionAsignado, out slotLIFOAsignado);
        }

        float distanciaIda = CalcularDistanciaNavMesh(transform.position, destino.position, null, destino);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            Vector3 destinoSlotCamion = ObtenerPosicionParadaDescargaActiva();
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
        AsignarMision(nuevoPedido, destinoFisico, -1);
    }

    public void AsignarMision(Pedido nuevoPedido, Transform destinoFisico, int indiceFilaSecuencial)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual == EstadoRobot.Recargando || estadoActual == EstadoRobot.Apagado)
        {
            Debug.LogWarning($"[{idRobot}]: Llamada a AsignarMision rechazada — unidad en estado {estadoActual}.");
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
        indiceFilaManifiesto = indiceFilaSecuencial >= 0 ? indiceFilaSecuencial : (managerEVE != null ? managerEVE.indicePedidoActual : 0);

        if (!CalcularViabilidadOperativa(pedidoActual, destinoFisico, out float bateriaProyectada))
        {
            destinoActual = destinoFisico;
            if (managerEVE != null) managerEVE.amrEnAlertaBateria20 = this;
            estadoPreInterrupcionBateria = EstadoRobot.Inactivo;
            enPausaPreventivaBateria20 = false;
            Debug.Log("<color=#10B981>[FLUJO]: 6. Pedido 3 evaluado. Emergencia de Batería (Ind. 5.0) disparada. Esperando decisión del supervisor.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ALERTA PREDICTIVA] Misión inviable. Retorno proyectado: {bateriaProyectada:F1}% (Umbral LiFePO4: {UmbralSeguridadLiFePO4:F0}%). Detención preventiva.</color>");
            DashboardUI.Instance?.MostrarEmergencia(true, bateriaProyectada, 3.2f);
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            if (agente != null) agente.isStopped = true;
            return;
        }

        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "EN TRÁNSITO", Color.yellow);
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
        LiberarReservaSlotTMS();
        LimpiarPalletTemporal();
        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        estadoActual = EstadoRobot.Recargando;

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = true;

        if (agente != null)
        {
            if (!agente.enabled) agente.enabled = true;
            ActualizarJerarquiaNavegacion();
            agente.updateRotation = true;
            agente.isStopped = false;
            agente.ResetPath();
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = velocidadBase;
            agente.acceleration = aceleracionBase;
            TrazarRutaOrtogonal(posicionPad);
        }
        else
        {
            ActualizarJerarquiaNavegacion();
        }

        OnEstadoCambiado?.Invoke(estadoActual);
        Debug.Log($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de carga en {posicionPad}");
        DashboardUI.Instance?.RegistrarLog($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de recarga.");
    }

    public void InducirInterrupcionPreventivaBateria(float nuevaBateria)
    {
        estadoPreInterrupcionBateria = estadoActual;
        enPausaPreventivaBateria20 = true;
        bateriaActual = nuevaBateria;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        estadoActual = EstadoRobot.Interrumpido;
        ActualizarJerarquiaNavegacion();
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    public void ReanudarTrasRelevoDiferido()
    {
        relevoDiferidoPostEntrega = true;
        enPausaPreventivaBateria20 = false;
        estadoActual = EstadoRobot.Transportando;
        ActualizarJerarquiaNavegacion();
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = false;
            agente.speed = velocidadBase * 0.65f;
        }
        OnEstadoCambiado?.Invoke(estadoActual);
    }

    public Pedido ForzarApagadoCriticoBMS()
    {
        colapsoBMSArmadoPostDescarga = false;
        Pedido pedidoHuerfano = pedidoActual;
        bateriaActual = 2.4f;

        if (pedidoHuerfano != null)
        {
            DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REASIGNADO (<3%)", new Color(0.98f, 0.45f, 0.09f));
        }

        StopAllCoroutines();
        LiberarReservaSlotTMS();
        LimpiarPalletTemporal();
        if (palletActualGO != null)
        {
            Destroy(palletActualGO);
            palletActualGO = null;
        }
        LiberarMutexDeRobot(this);

        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        if (agente != null)
        {
            if (agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = true;
                agente.ResetPath();
                agente.velocity = Vector3.zero;
            }
            agente.enabled = false;
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = false;

        estadoActual = EstadoRobot.Apagado;
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);

        return pedidoHuerfano;
    }

    public void FinalizarRescateEnPad(Vector3 posPad)
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = true;

        transform.position = posPad;
        transform.rotation = Quaternion.identity;

        if (agente != null)
        {
            agente.enabled = true;
            agente.Warp(posPad);
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        estadoActual = EstadoRobot.Recargando;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    public void EjecutarOverride()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        DirectorCinematico.Instance?.LiberarFocoIncidente(this);
        enEsperaMutex = false;

        modoDegradacionActivo = true;
        coeficienteCarga *= 1.3f;

        if (enPausaPreventivaBateria20)
        {
            enPausaPreventivaBateria20 = false;
            if (agente != null)
            {
                agente.speed = velocidadBase * 0.5f;
                agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
            }

            EstadoRobot estadoRestaurado = (estadoPreInterrupcionBateria != EstadoRobot.Inactivo && estadoPreInterrupcionBateria != EstadoRobot.Interrumpido)
                ? estadoPreInterrupcionBateria
                : (TieneCargaFisica ? EstadoRobot.Transportando : EstadoRobot.En_Transito);

            estadoActual = estadoRestaurado;
            ActualizarJerarquiaNavegacion();

            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = false;
            }

            OnEstadoCambiado?.Invoke(estadoActual);
            DashboardUI.Instance?.RegistrarLog($"<color=orange>[OVERRIDE EJECUTADO] {idRobot} reanuda misión ({estadoActual}) en caliente. Degradación térmica activa: Velocidad -50%, Aceleración -50%, Consumo +30%.</color>");
            Debug.Log($"<color=orange>[OVERRIDE EJECUTADO] {idRobot} reanuda misión ({estadoActual}) en caliente. Degradación térmica activa.</color>");
            return;
        }

        // Flujo si estaba Inactivo antes de iniciar ruta
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        if (agente != null)
        {
            agente.isStopped = false;
            agente.speed = velocidadBase * 0.5f;
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
        }

        if (destinoActual != null)
        {
            TrazarRutaOrtogonal(destinoActual.position);
        }

        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.RegistrarLog($"<color=orange>[OVERRIDE EJECUTADO] {idRobot}: Protocolo de contingencia forzado. Modo de degradación térmica activo: Aceleración -50%, Consumo +30%.</color>");
    }

    public void AbortarMisionPorSeguridad()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        colapsoBMSArmadoPostDescarga = false;

        string idPallet = pedidoActual != null ? pedidoActual.id_pallet : "ORDEN";
        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REPROGRAMADO", new Color(1.0f, 0.55f, 0.1f));

        DashboardUI.Instance?.RegistrarLog($"<color=#FFA500>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Celda protegida al {bateriaActual:F1}%. Orden reprogramada.");
        Debug.Log($"<color=orange>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Batería conservada al {bateriaActual:F1}%.");

        StopAllCoroutines();
        LiberarReservaSlotTMS();
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

        // Fase 0 (0.25s): Micro-alineación ortogonal exacta al Punto_Acople_AMR (-21.20f, -30.57f, 0° Norte)
        Vector3 posAcopleObjetivo = new Vector3(-21.20f, transform.position.y, -30.57f);
        Vector3 posInicialAMR = transform.position;
        Quaternion rotInicialAMR = transform.rotation;
        const float durAlineacionASRS = 0.25f;
        float tAlineacion = 0f;
        while (tAlineacion < durAlineacionASRS)
        {
            tAlineacion += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tAlineacion / durAlineacionASRS));
            transform.position = Vector3.Lerp(posInicialAMR, posAcopleObjetivo, s);
            transform.rotation = Quaternion.Slerp(rotInicialAMR, Quaternion.identity, s);
            yield return null;
        }
        transform.position = posAcopleObjetivo;
        transform.rotation = Quaternion.identity;

        // Ocultar temporalmente el Pallet_Stock del slot en el estante jefe y posicionar el pallet dinámico en ese slot
        stockOcultadoActual = null;
        if (destinoActual != null)
        {
            Transform stockHijo = destinoActual.Find("Pallet_Stock");
            if (stockHijo != null && stockHijo.gameObject.activeSelf)
            {
                stockOcultadoActual = stockHijo.gameObject;
                stockOcultadoActual.SetActive(false);
            }
        }

        palletActualGO = ConstruirPalletVisual(pedidoActual);
        Transform padreMontaje = puntoMontajeCarga != null ? puntoMontajeCarga : transform;
        Vector3 metaLocal = puntoMontajeCarga != null ? Vector3.zero : new Vector3(0f, 0.6f, 0f);

        if (palletActualGO != null && destinoActual != null)
        {
            palletActualGO.transform.SetParent(null, true);
            Vector3 posSlot = destinoActual.position;
            palletActualGO.transform.position = posSlot;
            palletActualGO.transform.rotation = Quaternion.identity;

            GameObject goPlat = GameObject.Find("Plataforma_Ascensor");
            Transform platAscensor = goPlat != null ? goPlat.transform : null;

            const float xCintaCentral = -21.20f;
            const float zAscensor = -28.00f;
            const float yAcoplePalletAMR = 0.52186f;
            const float yBasePlataforma = 0.45186f;
            float yPlataformaNivel = Mathf.Max(yBasePlataforma, posSlot.y - 0.07f);

            // Etapa 1 (0.7s): Empuje lateral del pallet hacia la Cinta_Central_Rodillos de su nivel
            // mientras la Plataforma_Ascensor se posiciona verticalmente en ese nivel
            const float durEtapa1 = 0.7f;
            Vector3 posCinta = new Vector3(xCintaCentral, posSlot.y, posSlot.z);
            Vector3 posPlatInicio = platAscensor != null ? platAscensor.position : new Vector3(xCintaCentral, yBasePlataforma, zAscensor);
            Vector3 posPlatNivel = new Vector3(xCintaCentral, yPlataformaNivel, zAscensor);

            float t1 = 0f;
            while (t1 < durEtapa1)
            {
                t1 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t1 / durEtapa1));
                if (palletActualGO != null)
                {
                    palletActualGO.transform.position = Vector3.Lerp(posSlot, posCinta, s);
                }
                if (platAscensor != null)
                {
                    platAscensor.position = Vector3.Lerp(posPlatInicio, posPlatNivel, s);
                }
                yield return null;
            }

            // Etapa 2 (1.2s): Avance longitudinal por la Cinta_Central_Rodillos (-Z) hasta ingresar al Ascensor_VLM
            const float durEtapa2 = 1.2f;
            Vector3 posEnAscensor = new Vector3(xCintaCentral, posSlot.y, zAscensor);
            float t2 = 0f;
            while (t2 < durEtapa2)
            {
                t2 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t2 / durEtapa2));
                if (palletActualGO != null)
                {
                    palletActualGO.transform.position = Vector3.Lerp(posCinta, posEnAscensor, s);
                }
                yield return null;
            }

            // Etapa 3 (1.1s): Descenso vertical sincronizado del Ascensor_VLM y el pallet hasta la cota de acople del AMR
            const float durEtapa3 = 1.1f;
            Vector3 posAscensorBase = new Vector3(xCintaCentral, yAcoplePalletAMR, zAscensor);
            Vector3 posPlatBase = new Vector3(xCintaCentral, yBasePlataforma, zAscensor);
            float t3 = 0f;
            while (t3 < durEtapa3)
            {
                t3 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t3 / durEtapa3));
                if (palletActualGO != null)
                {
                    palletActualGO.transform.position = Vector3.Lerp(posEnAscensor, posAscensorBase, s);
                }
                if (platAscensor != null)
                {
                    platAscensor.position = Vector3.Lerp(posPlatNivel, posPlatBase, s);
                }
                yield return null;
            }

            // Etapa 4 (0.9s): Transferencia final por Estacion_Entrega_Rodillos hacia Punto_Montaje_Carga del AMR
            const float durEtapa4 = 0.9f;
            float t4 = 0f;
            while (t4 < durEtapa4)
            {
                t4 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t4 / durEtapa4));
                if (palletActualGO != null)
                {
                    Vector3 metaMundo = puntoMontajeCarga != null
                        ? puntoMontajeCarga.position
                        : (transform.position + new Vector3(0f, 0.52186f, 0f));
                    palletActualGO.transform.position = Vector3.Lerp(posAscensorBase, metaMundo, s);
                }
                yield return null;
            }

            if (palletActualGO != null)
            {
                palletActualGO.transform.SetParent(padreMontaje, false);
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

        // Consultar muelle de destino únicamente si aún no hay reserva activa para esta misión
        if (zonaTransferencia == null || slotLIFOAsignado <= 0)
        {
            if (managerEVE != null && managerEVE.gestorTMS != null)
            {
                Transform muelleDestino = managerEVE.gestorTMS.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out slotLIFOAsignado);
                if (muelleDestino != null) zonaTransferencia = muelleDestino;
            }
            else if (ModuloDespachoTMS.Instance != null)
            {
                Transform muelleDestino = ModuloDespachoTMS.Instance.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out slotLIFOAsignado);
                if (muelleDestino != null) zonaTransferencia = muelleDestino;
            }
        }
        
        if (zonaTransferencia != null)
        {
            enEsperaMutex = false;
            estadoActual = EstadoRobot.Transportando;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase - (pedidoActual.peso_kg * coeficientePeso * 2f));

            Vector3 destinoParadaCamion = ObtenerPosicionParadaDescargaActiva();
            TrazarRutaOrtogonal(destinoParadaCamion);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a posición de parada en Camión (" + zonaTransferencia.name + $" en {destinoParadaCamion}).");
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

        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "ENTREGADO", Color.green);
        Debug.Log("<color=magenta>LIFO:</color> Descargando estiba en Slot Flatbed del Camión por rodillos frontales.");
        DashboardUI.Instance?.RegistrarLog("LIFO: Descarga frontal por rodillos en camión.");

        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        // a) 0.20s de asentamiento del chasis en la posición de parada y alineación a 0° Norte (Quaternion.identity)
        Vector3 posParada = ObtenerPosicionParadaDescargaActiva();
        Vector3 posInicialAMR = transform.position;
        Quaternion rotInicialAMR = transform.rotation;
        Vector3 posFinalAMR = new Vector3(posParada.x, transform.position.y, posParada.z);
        const float durAsentamientoChasis = 0.20f;
        float tChasis = 0f;
        while (tChasis < durAsentamientoChasis)
        {
            tChasis += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tChasis / durAsentamientoChasis));
            transform.position = Vector3.Lerp(posInicialAMR, posFinalAMR, s);
            transform.rotation = Quaternion.Slerp(rotInicialAMR, Quaternion.identity, s);
            yield return null;
        }
        transform.position = posFinalAMR;
        transform.rotation = Quaternion.identity;

        // b) 0.85s: Deslizamiento frontal por rodillos del pallet hacia destinoSlotLIFO mientras el AMR permanece inmóvil
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        Vector3 destinoSlotLIFO = ObtenerPosicionDescargaActiva();
        Quaternion rotacionSlot = zonaTransferencia != null ? zonaTransferencia.rotation : Quaternion.identity;

        GameObject palletEntregado = palletActualGO;
        palletActualGO = null;
        stockOcultadoActual = null;

        if (palletEntregado != null)
        {
            palletEntregado.transform.SetParent(null, true);
            Vector3 posInicioDeslizamiento = palletEntregado.transform.position;
            Quaternion rotInicioDeslizamiento = palletEntregado.transform.rotation;

            const float duracionDeslizamiento = 0.85f;
            float tDesliz = 0f;
            while (tDesliz < duracionDeslizamiento)
            {
                tDesliz += Time.deltaTime;
                float factor = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tDesliz / duracionDeslizamiento));
                if (palletEntregado != null)
                {
                    palletEntregado.transform.position = Vector3.Lerp(posInicioDeslizamiento, destinoSlotLIFO, factor);
                    palletEntregado.transform.rotation = Quaternion.Slerp(rotInicioDeslizamiento, rotacionSlot, factor);
                }
                yield return null;
            }

            if (palletEntregado != null)
            {
                palletEntregado.transform.position = destinoSlotLIFO;
                palletEntregado.transform.rotation = rotacionSlot;
            }

            if (tms != null)
            {
                tms.RegistrarEstibaEnCamion(numCamionAsignado, pedidoActual, palletEntregado, slotLIFOAsignado);
            }
            else if (managerEVE != null)
            {
                managerEVE.RegistrarEntregaLIFO(pedidoActual, palletEntregado);
            }
            slotLIFOAsignado = 0;
        }
        else
        {
            yield return new WaitForSeconds(0.85f);
        }

        // c) 0.90s de retroceso recto de despegue con chasis vacío (1.20 m hacia -transform.forward con agente.updateRotation = false)
        const float duracionRetroceso = 0.90f;
        const float distanciaRetroceso = 1.20f;
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

        if (colapsoBMSArmadoPostDescarga)
        {
            colapsoBMSArmadoPostDescarga = false;
            LiberarMutexDeRobot(this);
            pedidoActual = null;
            destinoActual = null;
            zonaTransferencia = null;

            if (agente != null)
            {
                agente.speed = velocidadBase;
                agente.acceleration = modoDegradacionActivo ? Mathf.Max(1.0f, aceleracionBase * 0.5f) : aceleracionBase;
            }

            if (managerEVE != null)
            {
                managerEVE.FinalizarEntregaYEjecutarColapsoBMS(this);
                yield break;
            }
        }

        if (relevoDiferidoPostEntrega)
        {
            relevoDiferidoPostEntrega = false;
            LiberarMutexDeRobot(this);
            pedidoActual = null;
            destinoActual = null;
            zonaTransferencia = null;

            if (managerEVE != null)
            {
                managerEVE.palletsEnColaWMS = Mathf.Max(0, managerEVE.palletsEnColaWMS - 1);
                managerEVE.palletsCompletadosTurno++;
                FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(managerEVE.palletsEnColaWMS);
            }

            if (agente != null)
            {
                agente.speed = velocidadBase;
                agente.acceleration = modoDegradacionActivo ? Mathf.Max(1.0f, aceleracionBase * 0.5f) : aceleracionBase;
            }

            Vector3 posPad = (managerEVE != null) ? managerEVE.ObtenerPosicionPadPorUnidad(this) : transform.position;
            EnviarACargaInductiva(posPad);
            yield break;
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
            managerEVE.ConfirmarEntregaExitosa(this);
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

            if (EsEstacionTransferenciaFrontalASRS(destinoActual, destinoActual.position) &&
                EsEstacionTransferenciaFrontalASRS(hitTransform, hitTransform.position))
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
                if (hit.distance <= distanciaFrenadoEmergencia && !enParadaSeguridadLiDAR && amrEnMovimiento && EsObstaculoISORegistrado(hit.transform))
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

    private int ObtenerPrioridadVDA(RobotAMR r)
    {
        switch (r.estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
                return 4;
            case EstadoRobot.Transportando:
                return 3;
            case EstadoRobot.En_Transito:
                return 2;
            case EstadoRobot.Recargando:
                return 1;
            default:
                return 0;
        }
    }

    private bool DebeCederPasoA(RobotAMR otro)
    {
        if (otro == null) return false;

        // Regla 1: Nunca ceder el paso por burbuja a un AMR detenido esperando compuerta Mutex
        if (otro.enEsperaMutex) return false;

        // Regla 2: Quien evacúa la zona de muelle/rampas tiene prioridad absoluta ("Antes de entrar, deje salir")
        bool yoEvacuando = EstaEvacuandoZonaMuelle();
        bool otroEvacuando = otro.EstaEvacuandoZonaMuelle();
        if (yoEvacuando && !otroEvacuando) return false;
        if (!yoEvacuando && otroEvacuando) return true;

        int miPrio = ObtenerPrioridadVDA(this);
        int otroPrio = ObtenerPrioridadVDA(otro);
        if (otroPrio != miPrio) return otroPrio > miPrio;
        return ObtenerIndiceUnidad() > otro.ObtenerIndiceUnidad();
    }

    private void GestionarBurbujaProximidadVDA5050()
    {
        bool enMovimientoNavegable = estadoActual == EstadoRobot.En_Transito ||
                                     estadoActual == EstadoRobot.Transportando ||
                                     estadoActual == EstadoRobot.Recargando;

        if (!enMovimientoNavegable || agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            cediendoPasoAMR = false;
            tiempoDetenidoCediendo = 0f;
            return;
        }

        // Ventana de inmunidad temporal para despejar cruces tras activarse el watchdog anti-deadlock
        if (Time.time < tiempoInmunidadDespeje)
        {
            tiempoDetenidoCediendo = 0f;
            if (cediendoPasoAMR)
            {
                cediendoPasoAMR = false;
                if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
                {
                    agente.isStopped = false;
                }
            }
            return;
        }

        // Si ya está estacionado cargando en el Pad, no participa en evasión activa
        if (estadoActual == EstadoRobot.Recargando && colaWaypoints.Count == 0 && !agente.pathPending && agente.remainingDistance <= 0.60f)
        {
            cediendoPasoAMR = false;
            tiempoDetenidoCediendo = 0f;
            return;
        }

        Vector2 posMia = new Vector2(transform.position.x, transform.position.z);
        Vector2 dirMiMov = new Vector2(transform.forward.x, transform.forward.z);
        if (agente.desiredVelocity.sqrMagnitude > 0.01f)
        {
            dirMiMov = new Vector2(agente.desiredVelocity.x, agente.desiredVelocity.z);
        }
        if (dirMiMov.sqrMagnitude > 0.0001f) dirMiMov.Normalize();

        float umbral = cediendoPasoAMR ? RadioBurbujaSalidaVDA : RadioBurbujaEntradaVDA;
        bool debeCeder = false;
        RobotAMR otroEnConflicto = null;

        for (int i = 0; i < flotaActiva.Count; i++)
        {
            RobotAMR otro = flotaActiva[i];
            if (otro == null || otro == this || !otro.gameObject.activeInHierarchy) continue;
            if (otro.estadoActual == EstadoRobot.Inactivo || otro.estadoActual == EstadoRobot.Apagado) continue;

            // Ignorar a otro AMR si está estacionado en su Pad cargando (a menos que estemos justo en su área)
            if (otro.estadoActual == EstadoRobot.Recargando && otro.agente != null && otro.agente.enabled && otro.agente.isStopped &&
                Vector2.Distance(posMia, new Vector2(otro.transform.position.x, otro.transform.position.z)) >= 1.50f)
            {
                continue;
            }

            Vector2 posOtro = new Vector2(otro.transform.position.x, otro.transform.position.z);
            Vector2 haciaOtro = posOtro - posMia;
            float dist = haciaOtro.magnitude;

            if (dist < umbral)
            {
                Vector2 dirHaciaOtro = dist > 0.001f ? (haciaOtro / dist) : Vector2.zero;
                bool trayectoriasConvergen = Vector2.Dot(dirMiMov, dirHaciaOtro) > -0.15f || dist < 1.35f;

                if (trayectoriasConvergen && DebeCederPasoA(otro))
                {
                    debeCeder = true;
                    otroEnConflicto = otro;
                    break;
                }
            }
        }

        if (debeCeder)
        {
            if (!cediendoPasoAMR)
            {
                cediendoPasoAMR = true;
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }

            bool otroDetenido = otroEnConflicto != null &&
                                (otroEnConflicto.cediendoPasoAMR ||
                                 otroEnConflicto.enEsperaMutex ||
                                 (otroEnConflicto.agente != null && otroEnConflicto.agente.velocity.sqrMagnitude < 0.04f));

            if (otroDetenido)
            {
                tiempoDetenidoCediendo += Time.deltaTime;
                bool tienePrioridadDesempate = EstaEvacuandoZonaMuelle() ||
                                               (otroEnConflicto != null && ObtenerIndiceUnidad() < otroEnConflicto.ObtenerIndiceUnidad());
                float limiteWatchdog = tienePrioridadDesempate ? 1.2f : 1.8f;

                if (tiempoDetenidoCediendo >= limiteWatchdog)
                {
                    tiempoInmunidadDespeje = Time.time + 3.0f;
                    tiempoDetenidoCediendo = 0f;
                    cediendoPasoAMR = false;
                    if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
                    {
                        agente.isStopped = false;
                    }
                    Debug.Log($"<color=yellow>[VDA 5050 WATCHDOG]:</color> {idRobot} liberado por inmunidad de despeje (3.0s) ante bloqueo circular.");
                }
            }
            else
            {
                tiempoDetenidoCediendo = 0f;
            }
        }
        else
        {
            tiempoDetenidoCediendo = 0f;
            if (cediendoPasoAMR)
            {
                cediendoPasoAMR = false;
                if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
                {
                    agente.isStopped = false;
                }
            }
        }
    }

    void Update()
    {
        if (estadoActual == EstadoRobot.Apagado) return;

        if (colapsoBMSArmadoPostDescarga && !TieneCargaFisica && transform.position.z < -2.5f)
        {
            colapsoBMSArmadoPostDescarga = false;
            managerEVE?.EjecutarColapsoYRescateInmediato(this);
            return;
        }

        // Procesamiento activo del sensor LiDAR con Docking Mute (ISO 3691-4)
        ProcesarSensoresLiDAR();

        // Burbuja de proximidad inter-AMR VDA 5050 (2.5m Yield / 2.8m Resume)
        GestionarBurbujaProximidadVDA5050();

        // Transición fluida entre tramos ortogonales (VDA 5050) sin parada en esquinas intermedias
        bool cambioTramoWaypoint = AvanzarColaWaypoints();

        // Gestión de exclusión mutua (Mutex) de zona de acople a <= 3.5m en el tramo final
        if (!cambioTramoWaypoint)
        {
            GestionarCompuertaMutex();
        }

        // Consumo pasivo obligatorio por encendido de sistemas lógicos (blindado a 0% en parada de seguridad/interrupción/espera Mutex/cediendo paso)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && !cediendoPasoAMR && estadoActual != EstadoRobot.Interrumpido && estadoActual != EstadoRobot.Recargando)
        {
            bateriaActual -= consumoIdle * Time.deltaTime;
        }

        // Matemáticas de Batería (Termodinámica Inyectada)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && !cediendoPasoAMR && estadoActual != EstadoRobot.Interrumpido &&
            (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando) &&
            agente != null && agente.enabled)
        {
            float distanciaRecorrida = agente.velocity.magnitude * Time.deltaTime;
            if (distanciaRecorrida > 0)
            {
                float gasto = 0f;
                if (estadoActual == EstadoRobot.En_Transito)
                {
                    gasto = distanciaRecorrida * coeficienteVacio;
                }
                else if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
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
            if (agente != null && agente.enabled && agente.isOnNavMesh)
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

        if (agente == null || !agente.enabled || !agente.isOnNavMesh) return;
        if (cambioTramoWaypoint || colaWaypoints.Count > 0) return;

        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!enEsperaMutex && !cediendoPasoAMR && !agente.pathPending && agente.remainingDistance <= 0.15f && DistanciaPlanaADestinoAgente() <= 0.25f)
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
            // CRI-01 y MED-04: Detección de arribo exacto a la posición de parada previa al Slot LIFO (Offset 1.30m)
            if (!enEsperaMutex && !cediendoPasoAMR && zonaTransferencia != null && !agente.pathPending && agente.remainingDistance <= 0.15f && DistanciaPlanaADestinoAgente() <= 0.25f)
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
                // KUKA KMP 1500P Datasheet: Charge from 20% to 80% within 1 hour (60% / 3600s = 0.0166667% / s = 1.0% por minuto real)
                bateriaActual = Mathf.Min(80.0f, bateriaActual + (Time.deltaTime * tasaRecargaPorSegundo));
                DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);

                if (bateriaActual >= 80.0f)
                {
                    estadoActual = EstadoRobot.Inactivo;
                    ActualizarJerarquiaNavegacion();
                    OnEstadoCambiado?.Invoke(estadoActual);
                    DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
                }
            }
        }
    }
}
