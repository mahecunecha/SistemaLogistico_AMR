using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos estados.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando, Recargando, Apagado, FueraDeServicio_Danado }

    public enum TipoFallaAMR { Ninguna, DescargaCritica3, FallaHardwareIrrecuperable }

    [System.Serializable]
    public class PalletEnPasillo
    {
        public Pedido pedido;
        public GameObject palletGO;
        public Vector3 posicionSuelo;
        public int indiceFilaManifiesto;
        public GameObject stockOcultadoOriginal;
    }
    
    public event System.Action<EstadoRobot> OnEstadoCambiado;

    [Header("Identidad de Flota")]
    public string idRobot = "AMR-01";

    [Header("Telemetría Batería")]
    public float bateriaActual = 80.0f;
    public float coeficienteVacio = 0.00175f;
    public float coeficienteCarga = 0.00142f;
    public float consumoIdle = 0.0010f;

    [Header("Gestión de Pausa Preventiva, Escalones Críticos y Relevo")]
    public EstadoRobot estadoPreInterrupcionBateria = EstadoRobot.Inactivo;
    public bool enPausaPreventivaBateria20 = false;
    public bool relevoDiferidoPostEntrega = false;
    public TipoFallaAMR tipoFallaActual = TipoFallaAMR.Ninguna;
    public bool escalon6Evaluado = false;
    public bool retornoACargaPostEntrega6 = false;
    public bool acopladoEnPadInductivo = false;
    public bool solicitudReincorporacionPendiente = false;
    private bool avisoPotenciaReducida20Registrado = false;

    [Header("Recarga Inductiva KUKA KMP 1500P (20% -> 80% en 2h reales sin aceleración)")]
    public const float DuracionRecargaRealSegundos = 7200f; // 2 horas reales
    public const float VentanaRecargaNominalPct = 60.0f;   // Tramo 20% -> 80%
    public float tasaRecargaPorSegundo = VentanaRecargaNominalPct / DuracionRecargaRealSegundos; // 0.008333333f %/s (0.5% por minuto real)
    public const float FactorCompresionTurno = 28800f / 180f; // 160x (uso exclusivo de proyeccion O(1) de horizonte; NO afecta al reloj determinista 1:1 del turno ni a movimiento/ETA del AMR)
    public float tiempoRecargaRealSegundos = DuracionRecargaRealSegundos;

    public float ObtenerTasaRecargaRealPorSegundo()
    {
        float duracion = tiempoRecargaRealSegundos > 0f ? tiempoRecargaRealSegundos : DuracionRecargaRealSegundos;
        return VentanaRecargaNominalPct / duracion;
    }

    [Header("Throttling Telemetría")]
    private float tiempoProximoEnvioTelemetria = 0f;
    private const float IntervaloTelemetria = 0.15f; // ~6.6 Hz
    private float ultimaBateriaTransmitida = -1f;
    private EstadoRobot ultimoEstadoTransmitido = (EstadoRobot)(-1);

    [Header("Sensores LiDAR")]
    public LayerMask capaObstaculos = (1 << 0) | (1 << 8); // Default (0) + ObstaculoISO (8)
    private float distanciaDeteccionAlerta = 1.65f;
    private float distanciaFrenadoEmergencia = 1.15f;
    private float distanciaMaxSensor = 4.0f;
    private bool obstaculoDetectado = false;
    private bool enParadaSeguridadLiDAR = false;
    private EstadoRobot estadoPrevioBloqueo = EstadoRobot.Inactivo;

    public bool EnParadaSeguridadLiDAR => enParadaSeguridadLiDAR;
    public EstadoRobot EstadoPrevioBloqueo => estadoPrevioBloqueo;

    private float velocidadBase = 3.5f;
    private float aceleracionBase = 20.0f;
    private float coeficientePeso = 0.002f;

    public float VelocidadBase => velocidadBase;

    public void EstablecerVelocidadBase(float nuevaVelocidad)
    {
        velocidadBase = Mathf.Clamp(nuevaVelocidad, 2.0f, 5.5f);
        if (agente != null && agente.enabled)
        {
            if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
            {
                float velNominal = ObtenerVelocidadNominal();
                agente.speed = obstaculoDetectado ? (velNominal * 0.4f) : velNominal;
                agente.acceleration = ObtenerAceleracionNominal();
            }
            else if (estadoActual == EstadoRobot.Recargando && !acopladoEnPadInductivo && !agente.isStopped)
            {
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }
        }
    }

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
    private bool inmunidadDespejeActiva = false;

    [Header("Enrutamiento Ortogonal (VDA 5050)")]
    public const float ZAvenidaOeste = -32.0f;
    public const float ZAvenidaEste = -34.0f;
    public const float XPorticoSalidaNorte = 34.40f;
    public const float XPorticoEntradaSur = 32.60f;
    public const float ZUmbralPortico = -48.0f;
    public const float ZColectorSalaCarga = -51.80f;
    public const float ZPieRampaCamion = -2.0f;
    public const float ZFronteraRampaCamion = -3.50f;
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
    private float coeficienteCargaNominal = 0.00142f;
    private PalletEnPasillo palletObjetivoRecoleccionPasillo = null;

    public bool ModoDegradacionActivo => modoDegradacionActivo;
    public bool EnPotenciaReducidaBateria => modoDegradacionActivo || bateriaActual < UmbralSeguridadLiFePO4;
    public bool TieneRestriccionPotenciaOVelocidad => EnPotenciaReducidaBateria || relevoDiferidoPostEntrega || retornoACargaPostEntrega6;
    public bool EstaEnRampaActiva { get; private set; } = false;
    public bool SubiendoRampa { get; private set; } = false;
    public float AnguloPendienteGrados { get; private set; } = 0f;
    public const float PendienteNominalRampaGrados = 23.275f;
    public const float FactorVelocidadDescensoRampa = 0.65f;
    public Pedido PedidoActual => pedidoActual;
    public PalletEnPasillo PalletObjetivoRecoleccionPasillo => palletObjetivoRecoleccionPasillo;
    public bool EsRecuperableEnTurno => estadoActual != EstadoRobot.FueraDeServicio_Danado && tipoFallaActual != TipoFallaAMR.FallaHardwareIrrecuperable;
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
    private GameObject palletRetenidoEnBahia;
    private bool enEsperaMuelleAbierto = false;

    public bool EstaEnZonaCamionORampa()
    {
        return transform.position.z > ZFronteraRampaCamion;
    }

    public void LimpiarCargaRetenidaEnBahia()
    {
        if (palletRetenidoEnBahia != null)
        {
            if (Application.isPlaying) Destroy(palletRetenidoEnBahia);
            else DestroyImmediate(palletRetenidoEnBahia);
            palletRetenidoEnBahia = null;
        }
    }

    [Header("Alineación Visual en Rampas (Cinemática Pura)")]
    private Transform chasisVisual;
    private Vector3 baseChasisLocalPos;
    private Quaternion baseChasisLocalRot = Quaternion.identity;
    private Vector3 baseCargaLocalPos;
    private Quaternion baseCargaLocalRot = Quaternion.identity;
    private bool posesVisualesCacheadas = false;
    private float pitchVisualSuave = 0f;
    private float deltaYVisualSuave = 0f;

    public GameObject PalletActualGO => palletActualGO;

    void Awake()
    {
        coeficienteCargaNominal = coeficienteCarga;
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

        CachearNodosVisualesChasis();
        ActualizarJerarquiaNavegacion();
    }

    private void LimpiarPalletTemporal()
    {
        if (palletActualGO != null)
        {
            if (Application.isPlaying) Destroy(palletActualGO);
            else DestroyImmediate(palletActualGO);
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
        bool restringido = TieneRestriccionPotenciaOVelocidad;

        switch (estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
            case EstadoRobot.Apagado:
            case EstadoRobot.FueraDeServicio_Danado:
                agente.avoidancePriority = 10 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                return;
        }

        if (enEsperaMutex)
        {
            agente.avoidancePriority = Mathf.Min(99, 95 + offsetDesempate);
            agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            return;
        }

        if (cediendoPasoAMR)
        {
            agente.avoidancePriority = Mathf.Min(99, 94 + offsetDesempate);
            agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            return;
        }

        if (inmunidadDespejeActiva)
        {
            agente.avoidancePriority = 15 + offsetDesempate;
            agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            return;
        }

        switch (estadoActual)
        {
            case EstadoRobot.Transportando:
                agente.avoidancePriority = Mathf.Min(99, (restringido ? 65 : 20) + offsetDesempate);
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;

            case EstadoRobot.En_Transito:
                agente.avoidancePriority = Mathf.Min(99, (restringido ? 72 : 50) + offsetDesempate);
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;

            case EstadoRobot.Recargando:
                agente.avoidancePriority = Mathf.Min(99, (restringido ? 86 : 80) + offsetDesempate);
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

    public bool EstaEvacuandoEstacionASRS()
    {
        return estadoActual == EstadoRobot.Transportando &&
               Mathf.Abs(transform.position.x - XCentroASRS) < 2.2f &&
               transform.position.z > -32.5f &&
               transform.position.z < -27.5f;
    }

    private bool IntentarReservarZonaMutex(Transform zona)
    {
        if (zona == null) return true;

        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR propietario))
        {
            if (propietario == null || !propietario.gameObject.activeInHierarchy ||
                propietario.estadoActual == EstadoRobot.Inactivo ||
                propietario.estadoActual == EstadoRobot.Recargando ||
                propietario.estadoActual == EstadoRobot.Apagado ||
                propietario.estadoActual == EstadoRobot.FueraDeServicio_Danado)
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
            if (otroRobot.estadoActual == EstadoRobot.Inactivo || otroRobot.estadoActual == EstadoRobot.Apagado || otroRobot.estadoActual == EstadoRobot.FueraDeServicio_Danado) continue;

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
        if (enParadaSeguridadLiDAR || cediendoPasoAMR || estadoActual == EstadoRobot.Interrumpido || estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado) return;

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
                            ActualizarJerarquiaNavegacion();
                        }
                    }
                    else if (enEsperaMutex)
                    {
                        enEsperaMutex = false;
                        agente.isStopped = false;
                        ActualizarJerarquiaNavegacion();
                    }
                }
                else if (enEsperaMutex)
                {
                    enEsperaMutex = false;
                    agente.isStopped = false;
                    ActualizarJerarquiaNavegacion();
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
            if (agente.enabled && agente.isOnNavMesh)
            {
                agente.SetDestination(primerWaypoint);
            }
        }
        else
        {
            ResolverEjeCarrilX(destinoFinal, out Vector3 destinoNav, rackDestino);
            agente.autoBraking = true;
            if (agente.enabled && agente.isOnNavMesh)
            {
                agente.SetDestination(destinoNav);
            }
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
        if (agente == null || !agente.enabled || !agente.isOnNavMesh || enParadaSeguridadLiDAR || enEsperaMutex || cediendoPasoAMR || estadoActual == EstadoRobot.Interrumpido || estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado) return false;
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

    public float CalcularDistanciaRestanteRutaActual()
    {
        if (agente == null || !agente.enabled || !agente.isOnNavMesh || agente.pathPending)
        {
            return float.NaN;
        }

        float tramoActual = agente.remainingDistance;
        if (float.IsNaN(tramoActual) || float.IsInfinity(tramoActual) || tramoActual < 0f)
        {
            if (!agente.hasPath) return float.NaN;
            tramoActual = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(agente.destination.x, agente.destination.z));
        }

        float total = tramoActual;
        Vector3 puntoPrevio = agente.destination;
        foreach (Vector3 wp in colaWaypoints)
        {
            total += Vector2.Distance(
                new Vector2(puntoPrevio.x, puntoPrevio.z),
                new Vector2(wp.x, wp.z));
            puntoPrevio = wp;
        }

        return (float.IsNaN(total) || float.IsInfinity(total)) ? float.NaN : total;
    }

    [Header("Batería Industrial LiFePO4")]
    public const float UmbralSeguridadLiFePO4 = 20.0f;
    public const float UmbralAlertaCritica6 = 6.0f;
    public const float UmbralApagadoInminente3 = 3.0f;
    public const float ReservaSeguridadEscalon6 = 0.50f;
    public const float DistanciaSueltaFrontalPallet = 1.45f;

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

    public static float CalcularConsumoTramo(
        float distanciaMetros,
        float pesoKg,
        float coefVacio,
        float coefCarga,
        bool cargado,
        float factorRuidoTermico = 1.15f)
    {
        if (distanciaMetros <= 0f) return 0f;
        if (!cargado)
        {
            return distanciaMetros * coefVacio;
        }
        return distanciaMetros * coefCarga * (pesoKg / 100f) * factorRuidoTermico;
    }

    public static bool EvaluarViabilidadLiFePO4(
        float bateriaActual,
        float distanciaIda,
        float distanciaVuelta,
        float pesoKg,
        float coefVacio,
        float coefCarga,
        out float bateriaRestanteEstimada)
    {
        // Proyección del consumo: ida vacío + vuelta cargado con ruido térmico máximo (1.15f)
        float gastoIda = CalcularConsumoTramo(distanciaIda, pesoKg, coefVacio, coefCarga, false, 1.0f);
        float gastoVuelta = CalcularConsumoTramo(distanciaVuelta, pesoKg, coefVacio, coefCarga, true, 1.15f);
        float gastoProyectadoTotal = gastoIda + gastoVuelta;

        bateriaRestanteEstimada = bateriaActual - gastoProyectadoTotal;
        return bateriaRestanteEstimada >= UmbralSeguridadLiFePO4;
    }

    public static bool EvaluarViabilidadEntregaEscalon6(
        float bateriaActual,
        bool muelleAbierto,
        float distanciaRestanteMuelle,
        float pesoKg,
        float coefVacio,
        float coefCarga,
        out float bateriaRestanteTrasEntrega,
        out string motivoNoViable)
    {
        if (!muelleAbierto)
        {
            bateriaRestanteTrasEntrega = bateriaActual;
            motivoNoViable = "Muelle destino cerrado/sellado o en inspección humana";
            return false;
        }

        float gastoMuelle = CalcularConsumoTramo(distanciaRestanteMuelle, pesoKg, coefVacio, coefCarga, true, 1.15f);
        bateriaRestanteTrasEntrega = bateriaActual - gastoMuelle;
        float pisoMinimo = UmbralApagadoInminente3 + ReservaSeguridadEscalon6;
        if (bateriaRestanteTrasEntrega <= pisoMinimo)
        {
            motivoNoViable = $"Energía proyectada tras entrega ({bateriaRestanteTrasEntrega:F2}%) insuficiente (requiere > {pisoMinimo:F2}%)";
            return false;
        }

        motivoNoViable = string.Empty;
        return true;
    }

    public bool EvaluarYActuarEscalon6(out string diagnostico)
    {
        escalon6Evaluado = true;
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;

        if (TieneCargaFisica)
        {
            bool muelleAbierto = tms == null || tms.EstaMuelleAbiertoParaEntrega(numCamionAsignado);
            float distMuelle = 0f;
            if (zonaTransferencia != null)
            {
                Vector3 posParada = ObtenerPosicionParadaDescargaActiva();
                distMuelle = CalcularDistanciaNavMesh(transform.position, posParada, null, null);
            }
            float pesoKg = (pedidoActual != null && pedidoActual.peso_kg > 0f) ? pedidoActual.peso_kg : Pedido.PesoReferenciaBaseKg;

            if (EvaluarViabilidadEntregaEscalon6(
                bateriaActual,
                muelleAbierto,
                distMuelle,
                pesoKg,
                coeficienteVacio,
                coeficienteCarga,
                out float restante,
                out string motivo))
            {
                retornoACargaPostEntrega6 = true;
                colapsoBMSArmadoPostDescarga = false;
                if (agente != null && agente.enabled && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando))
                {
                    agente.speed = ObtenerVelocidadNominal();
                }
                diagnostico = $"Escalón 6%: Muelle 0{numCamionAsignado} abierto y reserva suficiente ({restante:F2}%). Completando entrega y retornando a Pad.";
                return true;
            }

            diagnostico = $"Escalón 6% inviable ({motivo}). Escalando a corte de 3%.";
            return false;
        }

        Vector3 posPad = managerEVE != null ? managerEVE.ObtenerPosicionPadPorUnidad(this) : transform.position;
        float distPad = CalcularDistanciaNavMesh(transform.position, posPad, null, null);
        float gastoPad = CalcularConsumoTramo(distPad, 0f, coeficienteVacio, coeficienteCarga, false, 1.0f);
        float restantePad = bateriaActual - gastoPad;

        if (restantePad > UmbralApagadoInminente3 + 0.30f)
        {
            if (managerEVE != null)
            {
                if (palletObjetivoRecoleccionPasillo != null)
                {
                    managerEVE.ReencolarPalletEnPasilloPorRetiro(palletObjetivoRecoleccionPasillo);
                    palletObjetivoRecoleccionPasillo = null;
                }
                else if (pedidoActual != null)
                {
                    managerEVE.ReencolarPedidoPorRetiroPreventivo(pedidoActual);
                }
            }
            diagnostico = $"Escalón 6% sin carga: retorno a Pad viable ({restantePad:F2}%). Retirando a carga inductiva.";
            EnviarACargaInductiva(posPad);
            return true;
        }

        diagnostico = $"Escalón 6% sin carga: energía insuficiente para Pad ({restantePad:F2}%). Escalando a corte de 3%.";
        return false;
    }

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        // Si la zona de transferencia o el slot LIFO no han sido resueltos aún, consultar al TMS
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if ((zonaTransferencia == null || slotLIFOAsignado <= 0) && tms != null)
        {
            zonaTransferencia = tms.ConsultarMuelleDestino(pedido, out numCamionAsignado, out slotLIFOAsignado);
            if (zonaTransferencia == null)
            {
                bateriaRestanteEstimada = bateriaActual;
                return false;
            }
        }

        float distanciaIda = CalcularDistanciaNavMesh(transform.position, destino.position, null, destino);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            Vector3 destinoSlotCamion = ObtenerPosicionParadaDescargaActiva();
            distanciaVuelta = CalcularDistanciaNavMesh(destino.position, destinoSlotCamion, destino, null);
        }

        float pesoKg = (pedido != null && pedido.peso_kg > 0f) ? pedido.peso_kg : Pedido.PesoReferenciaBaseKg;
        return EvaluarViabilidadLiFePO4(
            bateriaActual,
            distanciaIda,
            distanciaVuelta,
            pesoKg,
            coeficienteVacio,
            coeficienteCarga,
            out bateriaRestanteEstimada);
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
        if (estadoActual == EstadoRobot.Recargando || estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado)
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

        ModuloDespachoTMS tmsCheck = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tmsCheck != null && !tmsCheck.TieneAlgunMuelleAbierto())
        {
            Debug.LogWarning($"[{idRobot}]: Misión diferida — todos los muelles TMS están cerrados/sellados o en inspección.");
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[TMS BLOQUEO]:</color> Todos los muelles cerrados. {idRobot} permanece en espera.");
            return;
        }

        palletObjetivoRecoleccionPasillo = null;
        pedidoActual = nuevoPedido;
        indiceFilaManifiesto = indiceFilaSecuencial >= 0 ? indiceFilaSecuencial : (managerEVE != null ? managerEVE.indicePedidoActual : 0);

        if (!CalcularViabilidadOperativa(pedidoActual, destinoFisico, out float bateriaProyectada))
        {
            if (zonaTransferencia == null && tmsCheck != null && !tmsCheck.TieneAlgunMuelleAbierto())
            {
                pedidoActual = null;
                return;
            }
            destinoActual = destinoFisico;
            if (managerEVE != null) managerEVE.amrEnAlertaBateria20 = this;
            estadoPreInterrupcionBateria = EstadoRobot.Inactivo;
            enPausaPreventivaBateria20 = false;
            Debug.Log("<color=#10B981>[FLUJO]: 6. Pedido 3 evaluado. Emergencia de Batería (Ind. 5.0) disparada. Esperando decisión del supervisor.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ALERTA PREDICTIVA] Misión inviable. Retorno proyectado: {bateriaProyectada:F1}% (Umbral LiFePO4: {UmbralSeguridadLiFePO4:F0}%). Detención preventiva.</color>");
            DashboardUI.Instance?.MostrarEmergencia(true, bateriaProyectada, 3.2f);
            DashboardUI.Instance?.DestellarAlertaUnidad(idRobot, false);
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            if (agente != null && agente.enabled && agente.isOnNavMesh) agente.isStopped = true;
            return;
        }

        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "EN TRÁNSITO", Color.yellow);
        destinoActual = destinoFisico;
        enEsperaMutex = false;
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        if (agente != null)
        {
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = ObtenerAceleracionNominal();
        }

        TrazarRutaOrtogonal(destinoActual.position); // Da la orden de moverse por rieles VDA 5050
        Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destinoFisico.name + " para buscar " + pedidoActual.codigo_sku);
        DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destinoFisico.name);
    }

    public bool AsignarMisionRecoleccionPasillo(PalletEnPasillo paquete, int indiceFilaSecuencial)
    {
        if (estadoActual == EstadoRobot.Recargando || estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado)
        {
            Debug.LogWarning($"[{idRobot}]: Recolección en pasillo rechazada — unidad en estado {estadoActual}.");
            return false;
        }
        if (estadoActual != EstadoRobot.Inactivo || paquete == null || paquete.pedido == null) return false;

        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null && !tms.TieneAlgunMuelleAbierto())
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[TMS BLOQUEO]:</color> Todos los muelles cerrados; recolección de {paquete.pedido.id_pallet} en espera.");
            return false;
        }

        if ((zonaTransferencia == null || slotLIFOAsignado <= 0) && tms != null)
        {
            zonaTransferencia = tms.ConsultarMuelleDestino(paquete.pedido, out numCamionAsignado, out slotLIFOAsignado);
            if (zonaTransferencia == null)
            {
                return false;
            }
        }

        palletObjetivoRecoleccionPasillo = paquete;
        pedidoActual = paquete.pedido;
        destinoActual = null;
        indiceFilaManifiesto = indiceFilaSecuencial >= 0 ? indiceFilaSecuencial : paquete.indiceFilaManifiesto;

        enEsperaMutex = false;
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "RECOLECCIÓN PASILLO", Color.yellow);

        if (agente != null)
        {
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = ObtenerAceleracionNominal();
        }

        TrazarRutaOrtogonal(paquete.posicionSuelo);
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[RELEVO PASILLO]:</color> {idRobot} en camino a recoger {paquete.pedido.id_pallet} en pasillo ({paquete.posicionSuelo.x:F1}, {paquete.posicionSuelo.z:F1}).");
        return true;
    }

    public void EnviarACargaInductiva(Vector3 posicionPad)
    {
        if (estadoActual == EstadoRobot.FueraDeServicio_Danado)
        {
            Debug.LogWarning($"[{idRobot}]: EnviarACargaInductiva rechazado — unidad dañada fuera de servicio.");
            return;
        }

        DashboardUI.Instance?.MostrarEmergencia(false);

        // Limpieza estricta de corrutinas y memoria de la misión abortada para evitar extracciones fantasma
        StopAllCoroutines();
        LiberarReservaSlotTMS();
        if (palletActualGO != null)
        {
            Pedido pedidoReencolar = pedidoActual;
            PalletEnPasillo paqueteSuelto = DepositarPalletEnPasillo();
            managerEVE?.RegistrarPalletAbandonadoPorAborto(pedidoReencolar, paqueteSuelto);
        }
        else
        {
            LimpiarPalletTemporal();
        }
        enEsperaMuelleAbierto = false;
        palletObjetivoRecoleccionPasillo = null;
        acopladoEnPadInductivo = false;
        solicitudReincorporacionPendiente = false;
        retornoACargaPostEntrega6 = false;
        colapsoBMSArmadoPostDescarga = false;
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
            if (agente.isOnNavMesh)
            {
                agente.isStopped = false;
                agente.ResetPath();
            }
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = ObtenerAceleracionNominal();
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
        DashboardUI.Instance?.DestellarAlertaUnidad(idRobot, false);
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
            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = ObtenerAceleracionNominal();
        }
        OnEstadoCambiado?.Invoke(estadoActual);
    }

    private PalletEnPasillo DepositarPalletEnPasillo()
    {
        if (palletActualGO == null) return null;

        // Si el AMR falló dentro del camión o en la rampa (z > -3.50m), NO depositar el pallet como recolectable
        // en pasillo para que otro AMR no ingrese a la bahía clausurada; se conserva contenido junto al AMR.
        if (EstaEnZonaCamionORampa())
        {
            palletRetenidoEnBahia = palletActualGO;
            palletActualGO = null;
            stockOcultadoActual = null;
            return null;
        }

        Vector3 dirFrente = transform.forward;
        dirFrente.y = 0f;
        if (dirFrente.sqrMagnitude < 0.001f) dirFrente = Vector3.forward;
        dirFrente.Normalize();

        Vector3 posCandidata = transform.position + dirFrente * DistanciaSueltaFrontalPallet;
        Vector3 posNav = posCandidata;
        if (NavMesh.SamplePosition(posCandidata, out NavMeshHit hitNav, 1.8f, NavMesh.AllAreas))
        {
            posNav = hitNav.position;
        }

        float ySuelo = transform.position.y;
        if (Application.isPlaying && MuestrearSuperficieTransitable(posNav, out float yHitSuelo, out _))
        {
            ySuelo = yHitSuelo;
        }
        else if (!Mathf.Approximately(posNav.y, 0f) && Mathf.Abs(posNav.y - transform.position.y) < 1.5f)
        {
            ySuelo = posNav.y;
        }

        Vector3 posFinalSuelo = new Vector3(posNav.x, ySuelo, posNav.z);

        GameObject palletSuelto = palletActualGO;
        GameObject stockOriginal = stockOcultadoActual;
        palletActualGO = null;
        stockOcultadoActual = null;

        palletSuelto.transform.SetParent(null, true);
        palletSuelto.transform.position = posFinalSuelo;
        palletSuelto.transform.rotation = Quaternion.LookRotation(dirFrente, Vector3.up);
        palletSuelto.SetActive(true);

        int capaPallet = LayerMask.NameToLayer("Pallet");
        if (capaPallet >= 0)
        {
            AsignarCapaRecursiva(palletSuelto, capaPallet);
        }

        return new PalletEnPasillo
        {
            pedido = pedidoActual,
            palletGO = palletSuelto,
            posicionSuelo = posFinalSuelo,
            indiceFilaManifiesto = indiceFilaManifiesto,
            stockOcultadoOriginal = stockOriginal
        };
    }

    public Pedido ForzarApagadoCriticoBMS()
    {
        return ForzarApagadoCriticoBMS(out _);
    }

    public Pedido ForzarApagadoCriticoBMS(out PalletEnPasillo palletDepositado)
    {
        colapsoBMSArmadoPostDescarga = false;
        retornoACargaPostEntrega6 = false;
        escalon6Evaluado = false;
        acopladoEnPadInductivo = false;
        solicitudReincorporacionPendiente = false;
        Pedido pedidoHuerfano = pedidoActual;
        bateriaActual = Mathf.Min(bateriaActual > UmbralApagadoInminente3 ? 2.4f : bateriaActual, 2.4f);

        if (pedidoHuerfano != null)
        {
            DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REASIGNADO (<3%)", new Color(0.98f, 0.45f, 0.09f));
        }

        StopAllCoroutines();
        LiberarReservaSlotTMS();

        // Suelta física del pallet enfrente del AMR sin destruir el GameObject ni devolver stock al rack
        palletDepositado = DepositarPalletEnPasillo();
        if (palletDepositado == null && stockOcultadoActual != null)
        {
            stockOcultadoActual.SetActive(true);
            stockOcultadoActual = null;
        }

        LiberarMutexDeRobot(this);

        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        palletObjetivoRecoleccionPasillo = null;
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

        tipoFallaActual = TipoFallaAMR.DescargaCritica3;
        estadoActual = EstadoRobot.Apagado;
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
        GestorAudioSCADA.Instance?.ReproducirAlarmaCritica();
        DashboardUI.Instance?.DestellarAlertaUnidad(idRobot, true);

        return pedidoHuerfano;
    }

    public Pedido ForzarFallaHardwareIrrecuperable(out PalletEnPasillo palletDepositado)
    {
        colapsoBMSArmadoPostDescarga = false;
        retornoACargaPostEntrega6 = false;
        escalon6Evaluado = false;
        acopladoEnPadInductivo = false;
        solicitudReincorporacionPendiente = false;
        Pedido pedidoHuerfano = pedidoActual;

        if (pedidoHuerfano != null)
        {
            DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REASIGNADO (HW)", new Color(0.86f, 0.15f, 0.15f));
        }

        StopAllCoroutines();
        LiberarReservaSlotTMS();

        palletDepositado = DepositarPalletEnPasillo();
        if (palletDepositado == null && stockOcultadoActual != null)
        {
            stockOcultadoActual.SetActive(true);
            stockOcultadoActual = null;
        }

        LiberarMutexDeRobot(this);

        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        palletObjetivoRecoleccionPasillo = null;
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

        tipoFallaActual = TipoFallaAMR.FallaHardwareIrrecuperable;
        estadoActual = EstadoRobot.FueraDeServicio_Danado;
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
        GestorAudioSCADA.Instance?.ReproducirAlarmaCritica();
        DashboardUI.Instance?.DestellarAlertaUnidad(idRobot, true);

        return pedidoHuerfano;
    }

    public void DetenerPorParadaSeguridadTurno()
    {
        if (estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado) return;

        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        if (estadoActual == EstadoRobot.En_Transito ||
            estadoActual == EstadoRobot.Transportando ||
            estadoActual == EstadoRobot.Extrayendo ||
            estadoActual == EstadoRobot.Entregando ||
            (estadoActual == EstadoRobot.Recargando && !acopladoEnPadInductivo))
        {
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
        }

        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    public void FinalizarRescateEnPad(Vector3 posPad)
    {
        LimpiarCargaRetenidaEnBahia();
        enEsperaMuelleAbierto = false;

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = true;

        transform.position = posPad;
        transform.rotation = Quaternion.identity;

        acopladoEnPadInductivo = true;
        solicitudReincorporacionPendiente = false;

        if (agente != null)
        {
            agente.enabled = true;
            agente.Warp(posPad);
            if (agente.isOnNavMesh)
            {
                agente.ResetPath();
                agente.SetDestination(posPad);
            }
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        estadoActual = EstadoRobot.Recargando;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    public void FinalizarTrasladoAZonaTecnica(Vector3 posSlotTecnico)
    {
        LimpiarCargaRetenidaEnBahia();
        enEsperaMuelleAbierto = false;

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

        transform.position = posSlotTecnico;
        transform.rotation = Quaternion.identity;

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = false;

        acopladoEnPadInductivo = false;
        solicitudReincorporacionPendiente = false;
        tipoFallaActual = TipoFallaAMR.FallaHardwareIrrecuperable;
        estadoActual = EstadoRobot.FueraDeServicio_Danado;
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

            EstadoRobot estadoRestaurado = (estadoPreInterrupcionBateria != EstadoRobot.Inactivo && estadoPreInterrupcionBateria != EstadoRobot.Interrumpido)
                ? estadoPreInterrupcionBateria
                : (TieneCargaFisica ? EstadoRobot.Transportando : EstadoRobot.En_Transito);

            estadoActual = estadoRestaurado;
            ActualizarJerarquiaNavegacion();

            if (agente != null)
            {
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }

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
            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = ObtenerAceleracionNominal();
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

        Pedido pedidoAbortado = pedidoActual;
        string idPallet = pedidoAbortado != null ? pedidoAbortado.id_pallet : "ORDEN";
        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REPROGRAMADO", new Color(1.0f, 0.55f, 0.1f));

        StopAllCoroutines();
        LiberarReservaSlotTMS();

        PalletEnPasillo palletDepositado = null;
        if (palletActualGO != null)
        {
            palletDepositado = DepositarPalletEnPasillo();
        }
        else
        {
            LimpiarPalletTemporal();
        }

        if (palletDepositado != null)
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#FFA500>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente al {bateriaActual:F1}%. Pallet depositado en pasillo para relevo.");
        }
        else
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#FFA500>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Celda protegida al {bateriaActual:F1}%. Orden reprogramada.");
        }
        Debug.Log($"<color=orange>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Batería conservada al {bateriaActual:F1}%.");

        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        enEsperaMuelleAbierto = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        palletObjetivoRecoleccionPasillo = null;
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        if (agente != null)
        {
            agente.updateRotation = true;
            agente.autoBraking = true;
            if (agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = true;
                agente.ResetPath();
                agente.velocity = Vector3.zero;
            }
        }

        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        if (managerEVE != null)
        {
            bool reencolada = palletDepositado != null || pedidoAbortado != null;
            if (reencolada)
            {
                managerEVE.RegistrarPalletAbandonadoPorAborto(pedidoAbortado, palletDepositado);
            }
            if (bateriaActual <= UmbralSeguridadLiFePO4)
            {
                Vector3 posPad = managerEVE.ObtenerPosicionPadPorUnidad(this);
                EnviarACargaInductiva(posPad);
            }
            managerEVE.NotificarMisionAbortada(reencolada);
        }
    }

    public void ReanudarTrasDespejeLiDAR()
    {
        if (!enParadaSeguridadLiDAR) return;

        enParadaSeguridadLiDAR = false;
        obstaculoDetectado = false;
        estadoActual = estadoPrevioBloqueo;
        ActualizarJerarquiaNavegacion();

        if (agente != null && agente.enabled && agente.isOnNavMesh)
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

            // 1. Corrección matemática de Pivote (Centrado X/Z = 0 y Base Inferior Y_min = 0.000m)
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

            // 2. Garantizar Collider físico en capa "Pallet" tras normalizar el pivote (sin destruir colliders existentes)
            if (rootPallet.GetComponentInChildren<Collider>(true) == null)
            {
                BoxCollider boxCol = rootPallet.AddComponent<BoxCollider>();
                boxCol.isTrigger = false;
                if (CalcularBoundsLocalesRenderers(rootPallet.transform, renderers, out Bounds boundsLocales))
                {
                    boxCol.center = boundsLocales.center;
                    boxCol.size = boundsLocales.size;
                }
                else
                {
                    boxCol.center = new Vector3(0f, 0.75f, 0f);
                    boxCol.size = new Vector3(1.0f, 1.5f, 1.2f);
                }
            }

            int capaPalletPrefab = LayerMask.NameToLayer("Pallet");
            if (capaPalletPrefab >= 0)
            {
                AsignarCapaRecursiva(rootPallet, capaPalletPrefab);
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

        int capaPalletFallback = LayerMask.NameToLayer("Pallet");
        if (capaPalletFallback >= 0)
        {
            AsignarCapaRecursiva(rootPallet, capaPalletFallback);
        }

        return rootPallet;
    }

    private static void AsignarCapaRecursiva(GameObject objeto, int capa)
    {
        if (objeto == null || capa < 0) return;

        Transform[] jerarquia = objeto.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < jerarquia.Length; i++)
        {
            jerarquia[i].gameObject.layer = capa;
        }
    }

    private static bool CalcularBoundsLocalesRenderers(Transform raiz, Renderer[] renderers, out Bounds boundsLocales)
    {
        boundsLocales = default;
        if (raiz == null || renderers == null || renderers.Length == 0) return false;

        bool tieneBounds = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            if (r.TryGetComponent<MeshFilter>(out MeshFilter mf) && mf.sharedMesh != null)
            {
                EncapsularBoundsEnLocal(raiz, mf.transform, mf.sharedMesh.bounds, ref boundsLocales, ref tieneBounds);
            }
            else
            {
                EncapsularBoundsEnLocal(raiz, null, r.bounds, ref boundsLocales, ref tieneBounds);
            }
        }

        return tieneBounds;
    }

    private static void EncapsularBoundsEnLocal(Transform raiz, Transform espacioOrigen, Bounds boundsOrigen, ref Bounds boundsLocales, ref bool tieneBounds)
    {
        Vector3 min = boundsOrigen.min;
        Vector3 max = boundsOrigen.max;

        for (int ix = 0; ix < 2; ix++)
        {
            float x = ix == 0 ? min.x : max.x;
            for (int iy = 0; iy < 2; iy++)
            {
                float y = iy == 0 ? min.y : max.y;
                for (int iz = 0; iz < 2; iz++)
                {
                    float z = iz == 0 ? min.z : max.z;
                    Vector3 puntoMundo = espacioOrigen != null
                        ? espacioOrigen.TransformPoint(new Vector3(x, y, z))
                        : new Vector3(x, y, z);
                    Vector3 puntoLocal = raiz.InverseTransformPoint(puntoMundo);

                    if (!tieneBounds)
                    {
                        boundsLocales = new Bounds(puntoLocal, Vector3.zero);
                        tieneBounds = true;
                    }
                    else
                    {
                        boundsLocales.Encapsulate(puntoLocal);
                    }
                }
            }
        }
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
            agente.acceleration = ObtenerAceleracionNominal();

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

    IEnumerator ProcesoRecoleccionDesdePasillo(PalletEnPasillo paquete)
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        estadoActual = EstadoRobot.Extrayendo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        GameObject palletRecuperado = (paquete != null) ? paquete.palletGO : null;
        if (paquete != null)
        {
            stockOcultadoActual = paquete.stockOcultadoOriginal;
        }

        if (palletRecuperado != null)
        {
            Vector3 dirHaciaPallet = palletRecuperado.transform.position - transform.position;
            dirHaciaPallet.y = 0f;
            if (dirHaciaPallet.sqrMagnitude > 0.001f)
            {
                Quaternion rotInicial = transform.rotation;
                Quaternion rotObjetivo = Quaternion.LookRotation(dirHaciaPallet.normalized, Vector3.up);
                const float durGiro = 0.25f;
                float tGiro = 0f;
                while (tGiro < durGiro)
                {
                    tGiro += Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(rotInicial, rotObjetivo, Mathf.Clamp01(tGiro / durGiro));
                    yield return null;
                }
                transform.rotation = rotObjetivo;
            }

            Transform padreMontaje = puntoMontajeCarga != null ? puntoMontajeCarga : transform;
            Vector3 metaMundo = puntoMontajeCarga != null ? puntoMontajeCarga.position : (transform.position + new Vector3(0f, 0.60f, 0f));
            Vector3 posInicioPallet = palletRecuperado.transform.position;
            Quaternion rotInicioPallet = palletRecuperado.transform.rotation;

            const float durAcople = 0.60f;
            float tAcople = 0f;
            while (tAcople < durAcople)
            {
                tAcople += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tAcople / durAcople));
                if (palletRecuperado != null)
                {
                    palletRecuperado.transform.position = Vector3.Lerp(posInicioPallet, metaMundo, s);
                    palletRecuperado.transform.rotation = Quaternion.Slerp(rotInicioPallet, transform.rotation, s);
                }
                yield return null;
            }

            if (palletRecuperado != null)
            {
                palletRecuperado.transform.SetParent(padreMontaje, false);
                palletRecuperado.transform.localPosition = puntoMontajeCarga != null ? Vector3.zero : new Vector3(0f, 0.6f, 0f);
                palletRecuperado.transform.localRotation = Quaternion.identity;
            }

            palletActualGO = palletRecuperado;
        }
        else if (pedidoActual != null)
        {
            palletActualGO = ConstruirPalletVisual(pedidoActual);
            yield return new WaitForSeconds(0.50f);
        }

        palletObjetivoRecoleccionPasillo = null;

        if (agente != null)
        {
            agente.isStopped = false;
        }

        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null && (zonaTransferencia == null || slotLIFOAsignado <= 0 || !tms.EstaMuelleAbiertoParaEntrega(numCamionAsignado)))
        {
            Transform muelleDestino = tms.ReasignarMuelleAbierto(pedidoActual, numCamionAsignado, slotLIFOAsignado, out numCamionAsignado, out slotLIFOAsignado);
            if (muelleDestino != null) zonaTransferencia = muelleDestino;
        }

        if (zonaTransferencia != null)
        {
            enEsperaMutex = false;
            estadoActual = EstadoRobot.Transportando;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);

            DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "TRANSPORTANDO", new Color(0.75f, 0.52f, 0.99f));

            if (agente != null)
            {
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }

            Vector3 destinoParadaCamion = ObtenerPosicionParadaDescargaActiva();
            TrazarRutaOrtogonal(destinoParadaCamion);
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[RELEVO PASILLO]:</color> {idRobot} recogió estiba {(pedidoActual != null ? pedidoActual.id_pallet : "PAL")} del pasillo y continúa hacia {zonaTransferencia.name}.");
        }
        else
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[TMS BLOQUEO]:</color> {idRobot} recogió estiba en pasillo pero no hay muelles abiertos disponibles.");
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

        if ((retornoACargaPostEntrega6 || colapsoBMSArmadoPostDescarga || bateriaActual <= UmbralApagadoInminente3) &&
            agente != null && transform.position.z > -3.20f)
        {
            bool prevUpdateRotEvac = agente.updateRotation;
            agente.updateRotation = false;
            float tEvac = 0f;
            float velEvac = ObtenerVelocidadNominal();
            while (transform.position.z > -3.20f && tEvac < 3.5f)
            {
                float dtEvac = Time.deltaTime;
                tEvac += dtEvac;
                agente.Move(Vector3.back * (velEvac * dtEvac));
                yield return null;
            }
            agente.updateRotation = prevUpdateRotEvac;
        }

        if (colapsoBMSArmadoPostDescarga || bateriaActual <= UmbralApagadoInminente3)
        {
            colapsoBMSArmadoPostDescarga = false;
            retornoACargaPostEntrega6 = false;
            LiberarMutexDeRobot(this);
            pedidoActual = null;
            destinoActual = null;
            zonaTransferencia = null;

            if (agente != null)
            {
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }

            if (managerEVE != null)
            {
                managerEVE.FinalizarEntregaYEjecutarColapsoBMS(this);
                yield break;
            }
        }

        if (retornoACargaPostEntrega6)
        {
            retornoACargaPostEntrega6 = false;
            LiberarMutexDeRobot(this);
            pedidoActual = null;
            destinoActual = null;
            zonaTransferencia = null;

            if (managerEVE != null)
            {
                managerEVE.palletsEnColaWMS = Mathf.Max(0, managerEVE.palletsEnColaWMS - 1);
                managerEVE.palletsCompletadosTurno++;
                FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.ActualizarResumenColaEnVivo(managerEVE.palletsEnColaWMS);
            }

            if (agente != null)
            {
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }

            Vector3 posPad6 = (managerEVE != null) ? managerEVE.ObtenerPosicionPadPorUnidad(this) : transform.position;
            EnviarACargaInductiva(posPad6);
            managerEVE?.NotificarRetiroACargaPostEntrega(this);
            yield break;
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
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }

            Vector3 posPad = (managerEVE != null) ? managerEVE.ObtenerPosicionPadPorUnidad(this) : transform.position;
            EnviarACargaInductiva(posPad);
            yield break;
        }

        LiberarMutexDeRobot(this);
        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        agente.speed = ObtenerVelocidadNominal();
        agente.acceleration = ObtenerAceleracionNominal();

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

    public bool ActivarParadaSeguridadLiDAR()
    {
        bool amrEnMovimiento = estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando;
        if (enParadaSeguridadLiDAR || !amrEnMovimiento)
        {
            return false;
        }

        enParadaSeguridadLiDAR = true;
        estadoPrevioBloqueo = estadoActual;
        obstaculoDetectado = true;

        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        estadoActual = EstadoRobot.Interrumpido;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);
        GestorAudioSCADA.Instance?.ReproducirAlarmaCritica();
        DashboardUI.Instance?.DestellarAlertaUnidad(idRobot, true);
        return true;
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
                    if (ActivarParadaSeguridadLiDAR())
                    {
                        Debug.Log("<color=#10B981>[FLUJO]: 4. Emergencia LiDAR (Ind. 4.0) disparada correctamente.</color>");
                        Debug.DrawLine(origen, hit.point, Color.red);

                        if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();
                        managerEVE?.GestionarObstruccionAutonomaISO(this, hit.collider.gameObject);
                    }
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

        // Gestión activa de velocidad preventiva según LiDAR, potencia reducida y rampas
        bool navegandoEnPista = estadoActual == EstadoRobot.En_Transito ||
                                estadoActual == EstadoRobot.Transportando ||
                                (estadoActual == EstadoRobot.Recargando && !acopladoEnPadInductivo);
        if (agente != null && navegandoEnPista)
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
            agente.acceleration = ObtenerAceleracionNominal();
        }
    }

    public static float CalcularFactorVelocidadRampa(bool enRampa, bool subiendo, bool conCarga, float pesoKg)
    {
        if (!enRampa) return 1.0f;

        if (subiendo)
        {
            if (!conCarga) return 0.85f;
            float pesoClamped = Mathf.Clamp(pesoKg > 0f ? pesoKg : Pedido.PesoReferenciaBaseKg, Pedido.PesoMinimoIndustrialKg, Pedido.PesoMaximoIndustrialKg);
            float t = Mathf.InverseLerp(Pedido.PesoMinimoIndustrialKg, Pedido.PesoMaximoIndustrialKg, pesoClamped);
            return Mathf.Lerp(0.78f, 0.62f, t);
        }

        return conCarga ? 0.60f : FactorVelocidadDescensoRampa;
    }

    public static float CalcularFactorEsfuerzoConsumoRampa(bool enRampa, bool subiendo, bool conCarga, float pesoKg)
    {
        if (!enRampa) return 1.0f;

        if (subiendo)
        {
            if (!conCarga) return 1.35f;
            float pesoClamped = Mathf.Clamp(pesoKg > 0f ? pesoKg : Pedido.PesoReferenciaBaseKg, Pedido.PesoMinimoIndustrialKg, Pedido.PesoMaximoIndustrialKg);
            float t = Mathf.InverseLerp(Pedido.PesoMinimoIndustrialKg, Pedido.PesoMaximoIndustrialKg, pesoClamped);
            return Mathf.Lerp(1.45f, 1.85f, t);
        }

        return 0.80f;
    }

    public float ObtenerVelocidadTransporteParaPeso(float pesoKg)
    {
        float pesoAplicado = pesoKg > 0f ? pesoKg : Pedido.PesoReferenciaBaseKg;
        return Mathf.Max(0.5f, velocidadBase - (pesoAplicado * coeficientePeso));
    }

    public float ObtenerVelocidadNominal()
    {
        float vel = velocidadBase;
        if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
        {
            vel = ObtenerVelocidadTransporteParaPeso(pedidoActual.peso_kg);
        }

        if (EnPotenciaReducidaBateria)
        {
            vel *= 0.5f;
        }
        else if (relevoDiferidoPostEntrega || retornoACargaPostEntrega6)
        {
            vel *= 0.65f;
        }

        if (EstaEnRampaActiva)
        {
            float pesoActual = pedidoActual != null ? pedidoActual.peso_kg : Pedido.PesoReferenciaBaseKg;
            vel *= CalcularFactorVelocidadRampa(true, SubiendoRampa, TieneCargaFisica, pesoActual);
        }

        return Mathf.Max(0.5f, vel);
    }

    public float ObtenerAceleracionNominal()
    {
        float acc = aceleracionBase;
        if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
        {
            float peso = pedidoActual.peso_kg > 0f ? pedidoActual.peso_kg : Pedido.PesoReferenciaBaseKg;
            acc = Mathf.Max(1.0f, aceleracionBase - (peso * coeficientePeso * 2f));
        }

        if (EnPotenciaReducidaBateria)
        {
            acc *= 0.5f;
        }

        if (EstaEnRampaActiva)
        {
            float pesoActual = pedidoActual != null ? pedidoActual.peso_kg : Pedido.PesoReferenciaBaseKg;
            acc *= CalcularFactorVelocidadRampa(true, SubiendoRampa, TieneCargaFisica, pesoActual);
        }

        return Mathf.Max(1.0f, acc);
    }

    public int ObtenerPrioridadVDA(RobotAMR r)
    {
        if (r == null) return 0;

        bool restringido = r.TieneRestriccionPotenciaOVelocidad;
        switch (r.estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
                return 10;
            case EstadoRobot.Transportando:
                return restringido ? 3 : 8;
            case EstadoRobot.En_Transito:
                return restringido ? 2 : 7;
            case EstadoRobot.Recargando:
                return restringido ? 1 : 4;
            default:
                return 0;
        }
    }

    public bool DebeCederPasoA(RobotAMR otro)
    {
        if (otro == null) return false;

        // Regla 1: Nunca ceder el paso por burbuja a un AMR detenido por Mutex, LiDAR o espera de muelle
        if (otro.enEsperaMutex || otro.enParadaSeguridadLiDAR || otro.enEsperaMuelleAbierto) return false;

        // Regla 2: Quien evacúa la zona de muelle/rampas tiene prioridad absoluta ("Antes de entrar, deje salir")
        bool yoEvacuando = EstaEvacuandoZonaMuelle();
        bool otroEvacuando = otro.EstaEvacuandoZonaMuelle();
        if (yoEvacuando && !otroEvacuando) return false;
        if (!yoEvacuando && otroEvacuando) return true;

        // Regla 2b: Quien evacúa cargado el bolsillo frontal del ASRS tiene prioridad sobre quien intenta entrar al mismo ASRS
        bool yoEvacuandoASRS = EstaEvacuandoEstacionASRS();
        bool otroEvacuandoASRS = otro.EstaEvacuandoEstacionASRS();
        bool yoEntrandoASRS = estadoActual == EstadoRobot.En_Transito && destinoActual != null &&
                              EsEstacionTransferenciaFrontalASRS(destinoActual, destinoActual.position);
        bool otroEntrandoASRS = otro.estadoActual == EstadoRobot.En_Transito && otro.destinoActual != null &&
                                EsEstacionTransferenciaFrontalASRS(otro.destinoActual, otro.destinoActual.position);
        if (yoEvacuandoASRS && otroEntrandoASRS) return false;
        if (otroEvacuandoASRS && yoEntrandoASRS) return true;

        // Regla 3: Respetar maniobras fijas en estantería/muelle o unidades interrumpidas
        bool yoEnManiobraFija = estadoActual == EstadoRobot.Extrayendo ||
                                estadoActual == EstadoRobot.Entregando ||
                                estadoActual == EstadoRobot.Interrumpido;
        if (yoEnManiobraFija) return false;

        bool otroEnManiobraFija = otro.estadoActual == EstadoRobot.Extrayendo ||
                                  otro.estadoActual == EstadoRobot.Entregando ||
                                  otro.estadoActual == EstadoRobot.Interrumpido;
        if (otroEnManiobraFija) return true;

        // Regla 4: Prioridad energética — un AMR con batería < 20% o restricción de potencia/velocidad
        // cede el paso a otro AMR sano sin restricción y con mayor nivel de batería.
        bool yoRestringido = TieneRestriccionPotenciaOVelocidad;
        bool otroRestringido = otro.TieneRestriccionPotenciaOVelocidad;
        if (yoRestringido && !otroRestringido && otro.bateriaActual > bateriaActual) return true;
        if (!yoRestringido && otroRestringido && bateriaActual > otro.bateriaActual) return false;

        // Regla 5: En igual condición de restricción/batería, priorizar Transportando > En_Transito > Recargando
        int miPrio = ObtenerPrioridadVDA(this);
        int otroPrio = ObtenerPrioridadVDA(otro);
        if (otroPrio != miPrio) return otroPrio > miPrio;

        // Regla 6: Desempate determinista por índice de unidad
        return ObtenerIndiceUnidad() > otro.ObtenerIndiceUnidad();
    }

    public static bool ConvergeOAlcanzaPorDetras(
        Vector2 posMia,
        Vector2 dirMiMov,
        bool yoRestringido,
        float miBateria,
        Vector2 posOtro,
        Vector2 dirOtroMov,
        bool otroRestringido,
        float otroBateria,
        float umbralBurbuja)
    {
        Vector2 haciaOtro = posOtro - posMia;
        float dist = haciaOtro.magnitude;
        if (dist >= umbralBurbuja) return false;

        Vector2 dirHaciaOtro = dist > 0.001f ? (haciaOtro / dist) : Vector2.zero;
        bool otroSeAproxima = dirOtroMov.sqrMagnitude > 0.0001f &&
                              (Vector2.Dot(dirOtroMov.normalized, -dirHaciaOtro) > 0.15f ||
                               (dirMiMov.sqrMagnitude > 0.0001f && Vector2.Dot(dirMiMov.normalized, dirOtroMov.normalized) > 0.35f));
        bool otroAlcanzaPorDetras = yoRestringido &&
                                    !otroRestringido &&
                                    otroBateria > miBateria &&
                                    otroSeAproxima;

        return Vector2.Dot(dirMiMov, dirHaciaOtro) > -0.15f || dist < 1.35f || otroAlcanzaPorDetras;
    }

    private static Vector2 ObtenerDireccionMovimientoPlana(RobotAMR amr)
    {
        if (amr == null) return Vector2.zero;
        if (amr.agente != null && amr.agente.enabled)
        {
            if (amr.agente.desiredVelocity.sqrMagnitude > 0.01f)
            {
                return new Vector2(amr.agente.desiredVelocity.x, amr.agente.desiredVelocity.z).normalized;
            }
            if (amr.agente.velocity.sqrMagnitude > 0.01f)
            {
                return new Vector2(amr.agente.velocity.x, amr.agente.velocity.z).normalized;
            }
        }
        Vector2 fwd = new Vector2(amr.transform.forward.x, amr.transform.forward.z);
        return fwd.sqrMagnitude > 0.0001f ? fwd.normalized : Vector2.zero;
    }

    private void GestionarBurbujaProximidadVDA5050()
    {
        bool enMovimientoNavegable = estadoActual == EstadoRobot.En_Transito ||
                                     estadoActual == EstadoRobot.Transportando ||
                                     estadoActual == EstadoRobot.Recargando;

        if (!enMovimientoNavegable || agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            bool cambioFlag = cediendoPasoAMR || inmunidadDespejeActiva;
            cediendoPasoAMR = false;
            inmunidadDespejeActiva = false;
            tiempoDetenidoCediendo = 0f;
            if (cambioFlag) ActualizarJerarquiaNavegacion();
            return;
        }

        // Ventana de inmunidad temporal para despejar cruces/bloqueos tras activarse el watchdog anti-deadlock
        if (Time.time < tiempoInmunidadDespeje)
        {
            tiempoDetenidoCediendo = 0f;
            bool cambioEstadoDespeje = !inmunidadDespejeActiva || cediendoPasoAMR;
            inmunidadDespejeActiva = true;
            if (cediendoPasoAMR)
            {
                cediendoPasoAMR = false;
                if (!enParadaSeguridadLiDAR && !enEsperaMutex && !enEsperaMuelleAbierto && estadoActual != EstadoRobot.Interrumpido)
                {
                    agente.isStopped = false;
                }
            }
            if (cambioEstadoDespeje) ActualizarJerarquiaNavegacion();
            return;
        }
        else if (inmunidadDespejeActiva)
        {
            inmunidadDespejeActiva = false;
            ActualizarJerarquiaNavegacion();
        }

        // Si ya está estacionado cargando en el Pad, no participa en evasión activa
        if (estadoActual == EstadoRobot.Recargando && colaWaypoints.Count == 0 && !agente.pathPending && agente.remainingDistance <= 0.60f)
        {
            if (cediendoPasoAMR)
            {
                cediendoPasoAMR = false;
                ActualizarJerarquiaNavegacion();
            }
            tiempoDetenidoCediendo = 0f;
            return;
        }

        Vector2 posMia = new Vector2(transform.position.x, transform.position.z);
        Vector2 dirMiMov = ObtenerDireccionMovimientoPlana(this);

        float umbral = cediendoPasoAMR ? RadioBurbujaSalidaVDA : RadioBurbujaEntradaVDA;
        bool debeCeder = false;
        RobotAMR otroEnConflicto = null;
        bool yoRestringido = TieneRestriccionPotenciaOVelocidad;

        for (int i = 0; i < flotaActiva.Count; i++)
        {
            RobotAMR otro = flotaActiva[i];
            if (otro == null || otro == this || !otro.gameObject.activeInHierarchy) continue;
            if (otro.estadoActual == EstadoRobot.Inactivo || otro.estadoActual == EstadoRobot.Apagado || otro.estadoActual == EstadoRobot.FueraDeServicio_Danado) continue;

            // Ignorar a otro AMR si está estacionado en su Pad cargando (a menos que estemos justo en su área)
            if (otro.estadoActual == EstadoRobot.Recargando && otro.agente != null && otro.agente.enabled && otro.agente.isStopped &&
                Vector2.Distance(posMia, new Vector2(otro.transform.position.x, otro.transform.position.z)) >= 1.50f)
            {
                continue;
            }

            Vector2 posOtro = new Vector2(otro.transform.position.x, otro.transform.position.z);
            Vector2 dirOtroMov = ObtenerDireccionMovimientoPlana(otro);
            bool otroRestringido = otro.TieneRestriccionPotenciaOVelocidad;

            bool trayectoriasConvergen = ConvergeOAlcanzaPorDetras(
                posMia,
                dirMiMov,
                yoRestringido,
                bateriaActual,
                posOtro,
                dirOtroMov,
                otroRestringido,
                otro.bateriaActual,
                umbral);

            if (trayectoriasConvergen && DebeCederPasoA(otro))
            {
                debeCeder = true;
                otroEnConflicto = otro;
                break;
            }
        }

        if (debeCeder)
        {
            if (!cediendoPasoAMR)
            {
                cediendoPasoAMR = true;
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
                ActualizarJerarquiaNavegacion();
            }

            bool otroDetenido = otroEnConflicto != null &&
                                (otroEnConflicto.cediendoPasoAMR ||
                                 otroEnConflicto.enEsperaMutex ||
                                 otroEnConflicto.enParadaSeguridadLiDAR ||
                                 otroEnConflicto.enEsperaMuelleAbierto ||
                                 (otroEnConflicto.agente != null && (otroEnConflicto.agente.isStopped || otroEnConflicto.agente.velocity.sqrMagnitude < 0.04f)));

            if (otroDetenido)
            {
                tiempoDetenidoCediendo += Time.deltaTime;
                bool prioridadPorBateriaSana = otroEnConflicto != null &&
                                               !yoRestringido &&
                                               otroEnConflicto.TieneRestriccionPotenciaOVelocidad &&
                                               bateriaActual > otroEnConflicto.bateriaActual;
                bool empateMismoNivelRestriccion = otroEnConflicto != null &&
                                                   yoRestringido == otroEnConflicto.TieneRestriccionPotenciaOVelocidad &&
                                                   ObtenerIndiceUnidad() < otroEnConflicto.ObtenerIndiceUnidad();
                bool tienePrioridadDesempate = EstaEvacuandoZonaMuelle() || EstaEvacuandoEstacionASRS() || prioridadPorBateriaSana || empateMismoNivelRestriccion;
                float limiteWatchdog = tienePrioridadDesempate ? 1.2f : 1.8f;

                if (tiempoDetenidoCediendo >= limiteWatchdog)
                {
                    tiempoInmunidadDespeje = Time.time + 4.0f;
                    inmunidadDespejeActiva = true;
                    tiempoDetenidoCediendo = 0f;
                    cediendoPasoAMR = false;
                    ActualizarJerarquiaNavegacion();
                    if (!enParadaSeguridadLiDAR && !enEsperaMutex && !enEsperaMuelleAbierto && estadoActual != EstadoRobot.Interrumpido)
                    {
                        agente.isStopped = false;
                    }
                    Debug.Log($"<color=yellow>[VDA 5050 WATCHDOG]:</color> {idRobot} liberado por inmunidad de despeje (4.0s) ante bloqueo circular.");
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
                ActualizarJerarquiaNavegacion();
                if (!enParadaSeguridadLiDAR && !enEsperaMutex && !enEsperaMuelleAbierto && estadoActual != EstadoRobot.Interrumpido)
                {
                    agente.isStopped = false;
                }
            }
        }
    }

    public bool AplicarConsumoYVerificarCorteBMS(float gastoSoC)
    {
        if (gastoSoC > 0f)
        {
            bateriaActual -= gastoSoC;
        }

        // CRI-04: Acotamiento de batería y corte crítico al 0%
        bateriaActual = Mathf.Max(0f, bateriaActual);
        if (bateriaActual <= 0f && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando || estadoActual == EstadoRobot.Extrayendo))
        {
            Debug.LogError("[FALLO CATASTRÓFICO] Batería agotada al 0% en pasillo. NavMeshAgent inmovilizado.");
            if (Application.isPlaying && managerEVE != null && managerEVE.turnoIniciado)
            {
                managerEVE.EjecutarColapsoYRescateInmediato(this);
                return true;
            }

            if (palletActualGO != null)
            {
                DepositarPalletEnPasillo();
            }
            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            DashboardUI.Instance?.RegistrarLog("<color=red><b>[FALLO CATASTRÓFICO]:</b> Celda agotada al 0% en pasillo. Unidad inmovilizada.</color>");
            GestorAudioSCADA.Instance?.ReproducirAlarmaCritica();
            DashboardUI.Instance?.DestellarAlertaUnidad(idRobot, true);
            return true;
        }
        return false;
    }

    public bool ReevaluarMuelleDestinoSiClausurado()
    {
        if (pedidoActual == null) return false;
        if (estadoActual != EstadoRobot.Transportando && estadoActual != EstadoRobot.En_Transito) return false;
        if (transform.position.z > ZPieRampaCamion) return false;

        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms == null || numCamionAsignado < 1 || numCamionAsignado > 3) return false;

        if (tms.EstaMuelleAbiertoParaEntrega(numCamionAsignado) && !enEsperaMuelleAbierto)
        {
            return false;
        }

        Transform nuevoMuelle = tms.ReasignarMuelleAbierto(pedidoActual, numCamionAsignado, slotLIFOAsignado, out int nuevoCamion, out int nuevoSlot);
        if (nuevoMuelle != null)
        {
            enEsperaMuelleAbierto = false;
            enEsperaMutex = false;
            LiberarMutexDeRobot(this);
            zonaTransferencia = nuevoMuelle;
            numCamionAsignado = nuevoCamion;
            slotLIFOAsignado = nuevoSlot;

            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = false;
            }

            if (estadoActual == EstadoRobot.Transportando)
            {
                TrazarRutaOrtogonal(ObtenerPosicionParadaDescargaActiva());
            }

            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[TMS RE-RUTEO PRE-RAMPA]:</color> Muelle anterior clausurado. {idRobot} redirigido a Muelle_Camion_0{nuevoCamion} (Slot #{nuevoSlot}).");
            return true;
        }

        if (estadoActual == EstadoRobot.Transportando && agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }
        if (!enEsperaMuelleAbierto)
        {
            enEsperaMuelleAbierto = true;
            DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[TMS BLOQUEO]:</color> Todos los muelles cerrados; {idRobot} en espera segura antes de rampa.");
        }
        return false;
    }

    private void ActualizarDeteccionRampa()
    {
        bool enFranja = estadoActual != EstadoRobot.Extrayendo &&
                        estadoActual != EstadoRobot.Apagado &&
                        estadoActual != EstadoRobot.FueraDeServicio_Danado &&
                        transform.position.z >= -4.2f &&
                        transform.position.z <= 5.5f;

        if (!enFranja)
        {
            EstaEnRampaActiva = false;
            SubiendoRampa = false;
            AnguloPendienteGrados = 0f;
            return;
        }

        Vector3 frente = transform.forward;
        bool okFront = MuestrearSuperficieTransitable(transform.position + frente * 0.45f, out float yFront, out bool rampaFront);
        bool okBack = MuestrearSuperficieTransitable(transform.position - frente * 0.45f, out float yBack, out bool rampaBack);
        bool okCenter = MuestrearSuperficieTransitable(transform.position, out _, out bool rampaCenter);

        if (okFront && okBack && (rampaFront || rampaBack || rampaCenter || Mathf.Abs(yFront - yBack) > 0.03f))
        {
            EstaEnRampaActiva = true;
            AnguloPendienteGrados = Mathf.Clamp(-Mathf.Atan2(yFront - yBack, 0.90f) * Mathf.Rad2Deg, -28f, 28f);

            Vector3 dirMov;
            if (agente != null && agente.enabled && agente.velocity.sqrMagnitude > 0.01f)
            {
                dirMov = agente.velocity.normalized;
            }
            else if (estadoActual == EstadoRobot.Entregando)
            {
                dirMov = -transform.forward;
            }
            else
            {
                dirMov = transform.forward;
            }

            float signoSentido = Vector3.Dot(dirMov, transform.forward) >= 0f ? 1f : -1f;
            SubiendoRampa = ((yFront - yBack) * signoSentido) > 0.02f;
        }
        else
        {
            EstaEnRampaActiva = false;
            SubiendoRampa = false;
            AnguloPendienteGrados = 0f;
        }
    }

    private void EvaluarEscalonesCriticosEnRuta()
    {
        if (!Application.isPlaying || managerEVE == null || !managerEVE.turnoIniciado || managerEVE.operacionDetenidaPorSeguridad)
        {
            return;
        }

        bool enMovimientoOperativo = estadoActual == EstadoRobot.En_Transito ||
                                     estadoActual == EstadoRobot.Transportando ||
                                     (estadoActual == EstadoRobot.Recargando && !acopladoEnPadInductivo);
        if (!enMovimientoOperativo)
        {
            return;
        }

        if (bateriaActual < UmbralSeguridadLiFePO4 && !avisoPotenciaReducida20Registrado)
        {
            avisoPotenciaReducida20Registrado = true;
            ActualizarJerarquiaNavegacion();
            if (!modoDegradacionActivo)
            {
                string socTxt = bateriaActual.ToString("F1", CultureInfo.InvariantCulture);
                DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[BMS &lt;20% - {idRobot}]:</color> Batería bajo umbral seguro ({socTxt}%). Reducción automática de potencia al 50% activada.");
            }
        }

        if (estadoActual != EstadoRobot.En_Transito && estadoActual != EstadoRobot.Transportando)
        {
            return;
        }

        if (bateriaActual <= UmbralApagadoInminente3)
        {
            managerEVE.EjecutarColapsoYRescateInmediato(this);
        }
        else if (bateriaActual <= UmbralAlertaCritica6 && !escalon6Evaluado)
        {
            managerEVE.ProcesarEscalonCritico6(this);
        }
    }

    void Update()
    {
        if (estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado) return;

        if (colapsoBMSArmadoPostDescarga && !TieneCargaFisica && transform.position.z < -2.5f)
        {
            colapsoBMSArmadoPostDescarga = false;
            managerEVE?.EjecutarColapsoYRescateInmediato(this);
            return;
        }

        // Detección continua de rampa / pendiente antes de velocidad LiDAR y consumo
        ActualizarDeteccionRampa();

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
                    gasto = CalcularConsumoTramo(distanciaRecorrida, 0f, coeficienteVacio, coeficienteCarga, false, 1.0f);
                    gasto *= CalcularFactorEsfuerzoConsumoRampa(EstaEnRampaActiva, SubiendoRampa, false, 0f);
                }
                else if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
                {
                    // Inyección de Ruido Termodinámico: Oscilación orgánica (Perlin Noise) para fricción e ineficiencia de motor (0.98 a 1.15)
                    float factorDeRuido = Mathf.Lerp(0.98f, 1.15f, Mathf.PerlinNoise(Time.time * 0.5f, 0f));
                    gasto = CalcularConsumoTramo(distanciaRecorrida, pedidoActual.peso_kg, coeficienteVacio, coeficienteCarga, true, factorDeRuido);
                    gasto *= CalcularFactorEsfuerzoConsumoRampa(EstaEnRampaActiva, SubiendoRampa, true, pedidoActual.peso_kg);
                }
                
                bateriaActual -= gasto;
            }
        }

        // CRI-04: Acotamiento de batería y corte crítico al 0%
        AplicarConsumoYVerificarCorteBMS(0f);

        // Doble escalón energético en ruta (6% preventivo con validación de muelle vs. 3% apagado y suelta física)
        EvaluarEscalonesCriticosEnRuta();
        if (estadoActual == EstadoRobot.Apagado || estadoActual == EstadoRobot.FueraDeServicio_Danado) return;

        // Re-evaluación temprana de muelle antes de ascender por la rampa (hasta ZPieRampaCamion = -2.0m)
        if ((estadoActual == EstadoRobot.Transportando || estadoActual == EstadoRobot.En_Transito) && transform.position.z <= ZPieRampaCamion)
        {
            ReevaluarMuelleDestinoSiClausurado();
            if (enEsperaMuelleAbierto && estadoActual == EstadoRobot.Transportando) return;
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
                if (palletObjetivoRecoleccionPasillo != null)
                {
                    StartCoroutine(ProcesoRecoleccionDesdePasillo(palletObjetivoRecoleccionPasillo));
                }
                else
                {
                    estadoActual = EstadoRobot.Extrayendo;
                    ActualizarJerarquiaNavegacion();
                    OnEstadoCambiado?.Invoke(estadoActual);
                    Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                    DashboardUI.Instance?.RegistrarLog("Destino Alcanzado: Extrayendo estiba.");
                    StartCoroutine(ProcesoExtraccion());
                }
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // CRI-01 y MED-04: Detección de arribo exacto a la posición de parada previa al Slot LIFO (Offset 1.30m)
            if (!enEsperaMutex && !cediendoPasoAMR && zonaTransferencia != null && !agente.pathPending && agente.remainingDistance <= 0.15f && DistanciaPlanaADestinoAgente() <= 0.25f)
            {
                ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
                if (tms != null && !tms.EstaMuelleAbiertoParaEntrega(numCamionAsignado))
                {
                    if (bateriaActual <= UmbralAlertaCritica6)
                    {
                        managerEVE?.EjecutarColapsoYRescateInmediato(this);
                        return;
                    }

                    Transform nuevoMuelle = tms.ReasignarMuelleAbierto(pedidoActual, numCamionAsignado, slotLIFOAsignado, out int nuevoCamion, out int nuevoSlot);
                    if (nuevoMuelle != null)
                    {
                        zonaTransferencia = nuevoMuelle;
                        numCamionAsignado = nuevoCamion;
                        slotLIFOAsignado = nuevoSlot;
                        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[TMS RE-RUTEO]:</color> Muelle cerrado en ruta. {idRobot} redirigido a Muelle_Camion_0{nuevoCamion} (Slot #{nuevoSlot}).");
                        TrazarRutaOrtogonal(ObtenerPosicionParadaDescargaActiva());
                    }
                    else
                    {
                        agente.isStopped = true;
                        agente.velocity = Vector3.zero;
                        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[TMS BLOQUEO]:</color> Todos los muelles cerrados; {idRobot} en espera segura con carga.");
                    }
                    return;
                }
                StartCoroutine(ProcesoEntrega());
            }
        }
        else if (estadoActual == EstadoRobot.Recargando)
        {
            bool listoParaRecargar = acopladoEnPadInductivo ||
                                     (!agente.pathPending && agente.remainingDistance <= 0.5f && DistanciaPlanaADestinoAgente() <= 0.75f);
            if (listoParaRecargar)
            {
                acopladoEnPadInductivo = true;
                // Llegó al pad de inducción. Frenar y recargar progresivamente a tasa real de 2 horas (sin multiplicadores artificiales).
                agente.isStopped = true;
                float tasaAplicada = ObtenerTasaRecargaRealPorSegundo();
                bateriaActual = Mathf.Min(80.0f, bateriaActual + (Time.deltaTime * tasaAplicada));
                DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);

                if (bateriaActual >= 80.0f)
                {
                    modoDegradacionActivo = false;
                    avisoPotenciaReducida20Registrado = false;
                    coeficienteCarga = coeficienteCargaNominal;
                    tipoFallaActual = TipoFallaAMR.Ninguna;
                    escalon6Evaluado = false;
                    retornoACargaPostEntrega6 = false;
                    colapsoBMSArmadoPostDescarga = false;

                    if (managerEVE != null)
                    {
                        if (!solicitudReincorporacionPendiente)
                        {
                            solicitudReincorporacionPendiente = true;
                            bool autorizada = managerEVE.SolicitarAutorizacionReincorporacion(this);
                            if (autorizada)
                            {
                                acopleYReincorporacionCompleta();
                                managerEVE.ConfirmarReincorporacionAPista(this);
                            }
                        }
                    }
                    else
                    {
                        acopleYReincorporacionCompleta();
                    }
                }
            }
            else if (!agente.isStopped)
            {
                agente.speed = ObtenerVelocidadNominal();
                agente.acceleration = ObtenerAceleracionNominal();
            }
        }
    }

    private void acopleYReincorporacionCompleta()
    {
        acopladoEnPadInductivo = false;
        solicitudReincorporacionPendiente = false;
        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    void LateUpdate()
    {
        ActualizarAlineacionVisualRampa();
    }

    private void CachearNodosVisualesChasis()
    {
        if (puntoMontajeCarga == null)
        {
            puntoMontajeCarga = transform.Find("Punto_Montaje_Carga");
        }

        if (chasisVisual == null)
        {
            Transform candidatoNombre = transform.Find("AMR-01");
            if (candidatoNombre != null)
            {
                chasisVisual = candidatoNombre;
            }
            else
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform hijo = transform.GetChild(i);
                    if (hijo == null || hijo == puntoMontajeCarga || hijo.name.Contains("HUD")) continue;
                    if (hijo.GetComponentInChildren<MeshRenderer>(true) != null)
                    {
                        chasisVisual = hijo;
                        break;
                    }
                }
            }
        }

        if (chasisVisual != null)
        {
            baseChasisLocalPos = chasisVisual.localPosition;
            baseChasisLocalRot = chasisVisual.localRotation;
        }
        if (puntoMontajeCarga != null)
        {
            baseCargaLocalPos = puntoMontajeCarga.localPosition;
            baseCargaLocalRot = puntoMontajeCarga.localRotation;
        }
        posesVisualesCacheadas = true;
    }

    private bool MuestrearSuperficieTransitable(Vector3 puntoMundo, out float ySuperficie, out bool esRampa)
    {
        ySuperficie = transform.position.y;
        esRampa = false;

        Vector3 origen = new Vector3(puntoMundo.x, transform.position.y + 1.0f, puntoMundo.z);
        RaycastHit[] hits = Physics.RaycastAll(origen, Vector3.down, 2.5f, 1 << 0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        bool encontrado = false;
        float mejorY = float.MinValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i].collider;
            if (col == null) continue;
            if (col.transform == transform || col.transform.IsChildOf(transform)) continue;

            string nombre = col.transform.name;
            bool transitable = nombre.Contains("Suelo") || nombre.Contains("Rampa") || nombre.Contains("Floor");
            if (!transitable) continue;

            float yHit = hits[i].point.y;
            if (!encontrado || yHit > mejorY)
            {
                mejorY = yHit;
                esRampa = nombre.Contains("Rampa");
                encontrado = true;
            }
        }

        if (!encontrado) return false;
        ySuperficie = mejorY;
        return true;
    }

    private void ActualizarAlineacionVisualRampa()
    {
        if (!posesVisualesCacheadas)
        {
            CachearNodosVisualesChasis();
        }
        if (chasisVisual == null) return;

        float pitchObjetivo = 0f;
        float deltaYObjetivo = 0f;

        bool enFranjaRampa = estadoActual != EstadoRobot.Extrayendo &&
                             estadoActual != EstadoRobot.Apagado &&
                             estadoActual != EstadoRobot.FueraDeServicio_Danado &&
                             transform.position.z >= -4.2f &&
                             transform.position.z <= 5.5f;

        if (enFranjaRampa)
        {
            Vector3 frente = transform.forward;
            bool okFront = MuestrearSuperficieTransitable(transform.position + frente * 0.45f, out float yFront, out bool rampaFront);
            bool okBack = MuestrearSuperficieTransitable(transform.position - frente * 0.45f, out float yBack, out bool rampaBack);
            bool okCenter = MuestrearSuperficieTransitable(transform.position, out float yCenter, out bool rampaCenter);

            if (okFront && okBack && (rampaFront || rampaBack || rampaCenter || Mathf.Abs(yFront - yBack) > 0.02f))
            {
                pitchObjetivo = Mathf.Clamp(-Mathf.Atan2(yFront - yBack, 0.90f) * Mathf.Rad2Deg, -28f, 28f);
                float promedioExtremos = (yFront + yBack) * 0.5f;
                float ySuperficieRampa = okCenter ? Mathf.Max(promedioExtremos, yCenter) : promedioExtremos;
                deltaYObjetivo = Mathf.Clamp(ySuperficieRampa - transform.position.y + 0.02f, -0.15f, 0.45f);
            }
        }

        float factorLerp = Mathf.Clamp01(Time.deltaTime * 12f);
        pitchVisualSuave = Mathf.Lerp(pitchVisualSuave, pitchObjetivo, factorLerp);
        deltaYVisualSuave = Mathf.Lerp(deltaYVisualSuave, deltaYObjetivo, factorLerp);

        Quaternion rotPitch = Quaternion.Euler(pitchVisualSuave, 0f, 0f);
        Vector3 offsetVertical = new Vector3(0f, deltaYVisualSuave, 0f);

        chasisVisual.localPosition = offsetVertical + (rotPitch * baseChasisLocalPos);
        chasisVisual.localRotation = rotPitch * baseChasisLocalRot;

        if (puntoMontajeCarga != null)
        {
            puntoMontajeCarga.localPosition = offsetVertical + (rotPitch * baseCargaLocalPos);
            puntoMontajeCarga.localRotation = rotPitch * baseCargaLocalRot;
        }
    }
}
