using System;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.AI;
using Unity.Cinemachine;
using Debug = UnityEngine.Debug;

public class RobotManagerEVE : MonoBehaviour
{
    public static RobotManagerEVE Instance { get; private set; }

    public event System.Action OnMisionEvaluada;
    public event System.Action<bool> OnPausaCambiada;
    public event System.Action OnEstadoAdmisionJSONCambiado;
    public event System.Action<string> OnTurnoCerrado;
    public event System.Action OnTurnoReiniciado;

    private ModuloDespachoTMS tmsSuscritoEventos;

    private void NotificarCambioEstadoAdmisionJSON()
    {
        OnEstadoAdmisionJSONCambiado?.Invoke();
    }

    [Header("Waypoints de Vuelo")]
    public Transform[] waypoints;
    public float velocidadVuelo = 5f;
    private int indiceWaypointActual = 0;

    [Header("Arquitectura Energética WMS (IEC 61508)")]
    // EVE opera como Orquestador Maestro SIL-3 alimentado por Bus Aéreo PoE++ 48V DC continuo.
    // Disponibilidad 24/7 garantizada sin degradación química de celda, evitando Single Point of Failure.
    [Tooltip("Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada")]
    public string estadoAlimentacionWMS = "Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada";

    [Header("Confinamiento Aéreo Espacial (AABB)")]
    public const float LimiteXMin = -38.0f;
    public const float LimiteXMax = 52.0f;
    public const float AltitudMin = 9.2f;
    public const float AltitudMax = 10.5f;
    public const float LimiteZMin = -43.0f;
    public const float LimiteZMax = -7.5f;

    [Header("Integración TMS (Módulo 5)")]
    public ModuloDespachoTMS gestorTMS;

    [Header("Flota de Robots AMR")]
    public RobotAMR robotAMR_01;
    public RobotAMR robotAMR_02;
    public RobotAMR robotAMR_03;

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public RobotAMR amrEnAlertaBateria20;
    public int indicePedidoActual = 0;
    public bool turnoIniciado = false;
    private bool sesionOperativaIniciada = false;
    private bool relevo24_7Activo = false;
    private Dictionary<string, Transform> directorioBodega = new Dictionary<string, Transform>();

    [Header("Cola Acumulativa WMS en Vivo")]
    public int palletsEnColaWMS = 0;
    public int palletsCompletadosTurno = 0;
    public bool flotaEscaladaDual = false;
    private List<Pedido> plantillaPedidosFisicos = new List<Pedido>();
    private readonly Queue<Pedido> colaPrioritariaReasignacion = new Queue<Pedido>();
    private readonly Queue<RobotAMR.PalletEnPasillo> colaPalletsEnPasillo = new Queue<RobotAMR.PalletEnPasillo>();
    private int indiceFilaManifiestoGlobal = 0;
    private float tiempoProximoDespachoPermitido = 0f;
    private Coroutine rutinaHeadwayDespacho;
    private const float HeadwaySalidaSalaCargaSegundos = 5.0f;

    public int CantidadPalletsEnPasillo => colaPalletsEnPasillo.Count;

    // Seguridad Operativa, Ley de Continuidad y Bloqueo de Admisión
    public int contadorDescargasCriticasTurno { get; private set; } = 0;
    public int contadorFallasHardwareTurno { get; private set; } = 0;
    public int TotalIncidentesCriticosTurno => contadorDescargasCriticasTurno + contadorFallasHardwareTurno;
    public bool turnoEnFaseCritica { get; private set; } = false;
    public bool operacionDetenidaPorSeguridad { get; private set; } = false;
    public bool admisionNuevosPedidosBloqueada { get; private set; } = false;
    public bool turnoCerrado { get; private set; } = false;
    public string motivoFaseCritica { get; private set; } = string.Empty;
    public string motivoBloqueoAdmision { get; private set; } = string.Empty;
    public string motivoCierreTurno { get; private set; } = string.Empty;

    [Header("Zona Técnica / Mantenimiento (AMR Dañados)")]
    public Transform contenedorZonaTecnica;
    public Transform[] slotsZonaTecnica;
    private readonly HashSet<int> slotsTecnicosOcupados = new HashSet<int>();

    [Header("Trazabilidad LIFO Camión")]
    private Stack<Pedido> matrizLIFOCamion = new Stack<Pedido>();
    private List<GameObject> palletsEntregados = new List<GameObject>();
    private float pesoAcumuladoCamion = 0f;
    private const float CapacidadMaximaCamionKg = 12000f;

    public Stack<Pedido> MatrizLIFOCamion => matrizLIFOCamion;
    public float PesoAcumuladoCamion => pesoAcumuladoCamion;

    [Header("Simulacion de Incertidumbre ISO 3691-4")]
    public GameObject obstaculoPasilloISO;
    public DronMantenimiento dronMantenimiento;
    private bool despachandoDespejeISO = false;

    public enum EscenarioHorizonte { EscenarioA_InicioTurno, EscenarioB_Madrugada, EscenarioC_FinalTurno }
    public enum DecisionAdmisionSLA { Invalido, Aprobado1AMR, EscaladoDual2AMRs, Rechazado }

    public struct ResultadoEvaluacionSLA
    {
        public DecisionAdmisionSLA Decision;
        public int NuevosPallets;
        public int ProyeccionCola;
        public int CupoDisponibleActual;
        public float HorasRestantes;
        public float Capacidad1AMR;
        public float Capacidad2AMRs;
        public int AMRRequeridos;
        public float ETAHoras;
        public string FeedbackTecnico;
    }

    public struct ResultadoHorizonteO1
    {
        public EscenarioHorizonte Escenario;
        public float HoraTurnoActual;
        public float HorasRestantes;
        public int NuevosPedidos;
        public float EnergiaRequeridaSoC;
        public float EnergiaDisponibleSoC;
        public float DeficitSoC;
        public float PesoLoteKg;
        public int PedidosAbsorbibles;
        public int PedidosRezagados;
        public float PorcentajeCarryOver;
        public int AMRRequeridos;
        public string EtiquetaEscenario;
    }

    public ResultadoEvaluacionSLA UltimaEvaluacionSLA { get; private set; }

    // Simulador de Estrés de Flota (Industria 5.0 - O(1))
    public float horaTurnoActual
    {
        get => RelojSimuladorTurno.HorasTranscurridas;
        set => RelojSimuladorTurno.EstablecerHoraSimuladorInterno(value);
    }
    public bool EnCompresionTemporalActiva => false;
    private bool alertaDemandaEnCurso = false;
    private int indiceCicloDemoHorizonte = 0;

    // Control Operativo en Vivo (Módulo 5 — Tope físico KMF 1500P-CB 1.25 m/s)
    public const float VelocidadNominalFlota = RobotAMR.VelocidadMaximaNominalKMF;
    public float VelocidadFlotaActual => VelocidadNominalFlota;
    public bool SimulacionPausada { get; private set; } = false;

    // Varianza industrial de peso por pedido (850-1250 kg, referencia 1000 kg)
    private static readonly float[] EstratosPesoIndustrialKg = { 870f, 940f, 1000f, 1060f, 1150f, 1230f };
    private int semillaVariacionPeso = 0;

    // Eficiencia Unitaria (1 AMR) y Promedio Dual (2 AMRs)
    public const float UmbralEficienciaSegundoAMR = 85.0f;
    public const float UmbralEficienciaDualBuzon = 78.0f;
    public const float TasaNominalPalletHora1AMR = 40.0f;
    public const float DistanciaCicloReferenciaTramoM = 85.5f;
    public const float TiempoManiobraReferenciaSeg = 6.10f;

    public float EficienciaUnitariaAMR01 { get; private set; } = 100.0f;
    public float EficienciaUnitariaAMR02 { get; private set; } = 100.0f;
    public float EficienciaPromedioDual { get; private set; } = 100.0f;
    public bool AlertaEficienciaDualActiva { get; private set; } = false;
    public string UltimoDiagnosticoEficienciaDual { get; private set; } = string.Empty;
    private bool alertaEficienciaDualReportadaEnEpisodio = false;
    private float tiempoProximaEvaluacionEficiencia = 0f;

    public static float GenerarPesoIndustrialPedido(int indiceSecuencial, int variacionJitter = 0)
    {
        int idx = Mathf.Abs(indiceSecuencial) % EstratosPesoIndustrialKg.Length;
        float baseEstrato = EstratosPesoIndustrialKg[idx];
        int hash = ((indiceSecuencial * 73) ^ (variacionJitter * 31)) & 0x7FFFFFFF;
        float offset = ((hash % 9) - 4) * 5f; // -20 kg a +20 kg en pasos de 5 kg
        return Mathf.Clamp(Mathf.Round((baseEstrato + offset) / 5f) * 5f, Pedido.PesoMinimoIndustrialKg, Pedido.PesoMaximoIndustrialKg);
    }

    void Awake()
    {
        Instance = this;
        if (GetComponent<RelojSimuladorTurno>() == null)
        {
            gameObject.AddComponent<RelojSimuladorTurno>();
        }
        if (gestorTMS == null) gestorTMS = FindFirstObjectByType<ModuloDespachoTMS>();
        if (dronMantenimiento == null) dronMantenimiento = FindFirstObjectByType<DronMantenimiento>();
        if (robotAMR_01 == null)
        {
            GameObject r1 = GameObject.Find("Robot_AMR");
            if (r1 != null) robotAMR_01 = r1.GetComponent<RobotAMR>();
        }
        if (robotAMR_02 == null)
        {
            GameObject r2 = GameObject.Find("Robot_AMR_02");
            if (r2 != null) robotAMR_02 = r2.GetComponent<RobotAMR>();
        }
        if (robotAMR_03 == null)
        {
            GameObject r3 = GameObject.Find("Robot_AMR_03");
            if (r3 != null) robotAMR_03 = r3.GetComponent<RobotAMR>();
        }
        if (robotEjecutor == null) robotEjecutor = robotAMR_01;

        SuscribirEventosTMS();
        CargarManifiesto();
    }

    private void SuscribirEventosTMS()
    {
        if (gestorTMS == null) gestorTMS = ModuloDespachoTMS.Instance != null ? ModuloDespachoTMS.Instance : FindFirstObjectByType<ModuloDespachoTMS>();
        if (tmsSuscritoEventos != gestorTMS)
        {
            if (tmsSuscritoEventos != null)
            {
                tmsSuscritoEventos.OnMuellesDisponiblesNuevamente -= OnMuellesTMSDisponiblesNuevamente;
            }
            tmsSuscritoEventos = gestorTMS;
        }
        if (tmsSuscritoEventos != null)
        {
            tmsSuscritoEventos.OnMuellesDisponiblesNuevamente -= OnMuellesTMSDisponiblesNuevamente;
            tmsSuscritoEventos.OnMuellesDisponiblesNuevamente += OnMuellesTMSDisponiblesNuevamente;
        }
    }

    private void OnMuellesTMSDisponiblesNuevamente()
    {
        if (!turnoIniciado || turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica) return;

        ReevaluarMuellesEnFlotaActiva();
        if (palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && turnoIniciado && !SimulacionPausada)
        {
            InyectarObstaculoPasilloISO();
        }

        if (Input.GetKeyDown(KeyCode.I) && turnoIniciado && !SimulacionPausada)
        {
            bool shiftPresionado = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shiftPresionado)
            {
                float[] horasDemo = new float[] { 1.5f, 5.0f, 7.5f };
                RelojSimuladorTurno.EstablecerHoraSimuladorInterno(horasDemo[indiceCicloDemoHorizonte]);
                indiceCicloDemoHorizonte = (indiceCicloDemoHorizonte + 1) % horasDemo.Length;
                alertaDemandaEnCurso = false;
                ProyectarEstresDeFlota(50);
            }
            else if (!alertaDemandaEnCurso)
            {
                ProyectarEstresDeFlota(50);
            }
        }

        if (!turnoIniciado || SimulacionPausada) return;

        if (EvaluarCierreTurnoSiCorresponde())
        {
            return;
        }

        MoverPorWaypoints();

        if (palletsEnColaWMS > 0 && Time.unscaledTime >= tiempoProximaEvaluacionEficiencia)
        {
            tiempoProximaEvaluacionEficiencia = Time.unscaledTime + 1.0f;
            EvaluarEficienciaOperativaYEscalamiento();

            if (!turnoCerrado && !operacionDetenidaPorSeguridad && !turnoEnFaseCritica &&
                palletsEnColaWMS > ContarMisionesEnVuelo() &&
                (gestorTMS == null || gestorTMS.TieneAlgunMuelleAbierto()))
            {
                ReevaluarMuellesEnFlotaActiva();
                DespacharColaDisponible();
            }
        }
    }

    public bool AlternarPausaSimulacion()
    {
        EstablecerPausaSimulacion(!SimulacionPausada);
        return SimulacionPausada;
    }

    public void EstablecerPausaSimulacion(bool pausar)
    {
        if (!turnoIniciado) return;
        if (DashboardUI.Instance != null && DashboardUI.Instance.EsModalEmergenciaActivo()) return;
        if (SimulacionPausada == pausar && Mathf.Approximately(Time.timeScale, pausar ? 0f : 1f)) return;

        SimulacionPausada = pausar;
        Time.timeScale = pausar ? 0f : 1f;

        if (pausar)
        {
            DashboardUI.Instance?.RegistrarLog("<color=#F59E0B>[SCADA] Simulación PAUSADA por supervisor.</color>");
        }
        else
        {
            DashboardUI.Instance?.RegistrarLog("<color=#10B981>[SCADA] Simulación REANUDADA.</color>");
        }

        OnPausaCambiada?.Invoke(SimulacionPausada);
        DashboardUI.Instance?.ActualizarMarcadorNBAHud(true);
    }

    void OnDestroy()
    {
        if (tmsSuscritoEventos != null)
        {
            tmsSuscritoEventos.OnMuellesDisponiblesNuevamente -= OnMuellesTMSDisponiblesNuevamente;
            tmsSuscritoEventos = null;
        }
        if (Instance == this)
        {
            Instance = null;
        }
        Time.timeScale = 1f;
    }

    void OnApplicationQuit()
    {
        Time.timeScale = 1f;
    }

    public void ReiniciarContadoresIncidentesTurno()
    {
        colaPrioritariaReasignacion.Clear();
        colaPalletsEnPasillo.Clear();
        slotsTecnicosOcupados.Clear();
        contadorDescargasCriticasTurno = 0;
        contadorFallasHardwareTurno = 0;
        turnoEnFaseCritica = false;
        operacionDetenidaPorSeguridad = false;
        admisionNuevosPedidosBloqueada = false;
        turnoCerrado = false;
        motivoFaseCritica = string.Empty;
        motivoBloqueoAdmision = string.Empty;
        motivoCierreTurno = string.Empty;
        EficienciaUnitariaAMR01 = 100.0f;
        EficienciaUnitariaAMR02 = 100.0f;
        EficienciaPromedioDual = 100.0f;
        AlertaEficienciaDualActiva = false;
        alertaEficienciaDualReportadaEnEpisodio = false;
        UltimoDiagnosticoEficienciaDual = string.Empty;
    }

    public void IniciarTurno()
    {
        if (turnoIniciado) return;
        SimulacionPausada = false;
        Time.timeScale = 1f;
        turnoIniciado = true;
        semillaVariacionPeso = UnityEngine.Random.Range(1, 997);
        sesionOperativaIniciada = true;
        palletsEnColaWMS = 0;
        palletsCompletadosTurno = 0;
        flotaEscaladaDual = false;
        indicePedidoActual = 0;
        indiceFilaManifiestoGlobal = 0;
        ReiniciarContadoresIncidentesTurno();
        RelojSimuladorTurno.ReiniciarRelojTurno(0.0f);
        relevo24_7Activo = false;
        tiempoProximoDespachoPermitido = 0f;
        if (rutinaHeadwayDespacho != null)
        {
            StopCoroutine(rutinaHeadwayDespacho);
            rutinaHeadwayDespacho = null;
        }

        DashboardUI.Instance?.ActualizarBannerTurno($"<b><color=#38BDF8>TURNO NOCTURNO:</color> {RelojSimuladorTurno.HoraActualFormateada} (Restan {RelojSimuladorTurno.TiempoRestanteFormateado} | Cierre {RelojSimuladorTurno.HoraCierreTurnoTexto}) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        Debug.Log("<color=#10B981>[FLUJO]: 1. Turno iniciado en STANDBY. Esperando ingesta JSON ERP.</color>");

        Transform hudTransform = DashboardUI.Instance != null ? DashboardUI.Instance.transform.Find("HUD_Operativo") : null;
        if (hudTransform != null)
        {
            hudTransform.gameObject.SetActive(true);
        }
        else
        {
            GameObject hud = GameObject.Find("HUD_Operativo");
            if (hud != null) hud.SetActive(true);
        }

        // Arranque en blanco / Standby real
        SuscribirEventosTMS();
        gestorTMS?.ReiniciarEstadoMuelles();
        FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.LimpiarTablaStandby();
        DashboardUI.Instance?.ConfigurarReservaAMR02(false);
        DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);

        DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
            "<color=#38BDF8>TURNO INICIADO EN STANDBY</color>",
            "Use [+] Cargar JSON ERP para inyectar órdenes a la cola WMS.",
            4.5f
        );

        DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Turno iniciado en STANDBY. Esperando ingesta de lotes JSON ERP.");
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[WMS ENERGÍA]:</color> Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada.");
        NotificarCambioEstadoAdmisionJSON();
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        plantillaPedidosFisicos.Clear();
        directorioBodega.Clear();

        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            Manifiesto baseJson = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            if (baseJson != null && baseJson.pedidos != null)
            {
                foreach (var pedido in baseJson.pedidos)
                {
                    if (pedido == null) continue;
                    plantillaPedidosFisicos.Add(pedido);
                    if (!string.IsNullOrEmpty(pedido.coordenada_bodega) && !directorioBodega.ContainsKey(pedido.coordenada_bodega))
                    {
                        GameObject destino = GameObject.Find(pedido.coordenada_bodega);
                        if (destino != null) directorioBodega.Add(pedido.coordenada_bodega, destino.transform);
                    }
                }
            }

            Debug.Log($"<color=green>EVE:</color> Directorio físico cacheado ({plantillaPedidosFisicos.Count} puntos). Sistema listo en Standby.");
        }
        else
        {
            Debug.LogError("EVE Error crítico: No se encontró el Manifiesto base en la ruta: " + ruta);
        }

        // Arranque sin lote activo cargado
        manifiestoActual = new Manifiesto { pedidos = new List<Pedido>() };
        palletsEnColaWMS = 0;
        flotaEscaladaDual = false;
    }

    public void RegistrarEntregaLIFO(Pedido pedidoEntregado, GameObject palletFisico)
    {
        if (pedidoEntregado == null) return;

        matrizLIFOCamion.Push(pedidoEntregado);
        pesoAcumuladoCamion += pedidoEntregado.peso_kg;

        Transform zonaCamionTransform = null;
        GameObject zonaObj = GameObject.Find("Zona_Camion");
        if (zonaObj != null) zonaCamionTransform = zonaObj.transform;

        if (palletFisico != null)
        {
            palletsEntregados.Add(palletFisico);

            if (zonaCamionTransform != null)
            {
                // Desemparentar del AMR preservando escala de mundo
                palletFisico.transform.SetParent(null, true);

                // Cuadrícula LIFO: acomodar a lo largo del eje Z del camión (de fondo a frente)
                int indiceLIFO = matrizLIFOCamion.Count; // 1, 2, 3...
                float zOffset = (2 - indiceLIFO) * 1.6f;
                float ySuperficie = zonaCamionTransform.position.y + 1.55f;

                Vector3 posicionLIFO = new Vector3(
                    zonaCamionTransform.position.x,
                    ySuperficie,
                    zonaCamionTransform.position.z + zOffset
                );

                palletFisico.transform.position = posicionLIFO;
                palletFisico.transform.rotation = zonaCamionTransform.rotation;
            }
        }

        DashboardUI.Instance?.RegistrarLog($"<color=cyan>[MATRIZ LIFO]:</color> Estiba {pedidoEntregado.id_pallet} apilada en Posición #{matrizLIFOCamion.Count} (Peso acumulado: {pesoAcumuladoCamion:F0} kg / {CapacidadMaximaCamionKg:F0} kg)");
        Debug.Log($"<color=cyan>[MATRIZ LIFO]:</color> Estiba {pedidoEntregado.id_pallet} apilada en Posición #{matrizLIFOCamion.Count}. Peso acumulado: {pesoAcumuladoCamion:F0} kg.");
    }

    public void ConfirmarEntregaExitosa(RobotAMR robotEntregador = null)
    {
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        palletsCompletadosTurno++;

        FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.ActualizarResumenColaEnVivo(palletsEnColaWMS);

        if (palletsEnColaWMS > 0)
        {
            DespacharColaDisponible(robotEntregador);
        }
        else if (ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            string msgColaFin = admisionNuevosPedidosBloqueada
                ? "<color=#F59E0B>[WMS CONTINUIDAD]:</color> Cola asignada completada. Admisión de pedidos nuevos bloqueada por pérdida de soporte/flota degradada."
                : "<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.";
            DashboardUI.Instance?.RegistrarLog(msgColaFin);
            Debug.Log(msgColaFin);
            EvaluarCierreTurnoSiCorresponde();
        }

        EvaluarEficienciaOperativaYEscalamiento(null, permitirDespachoInmediato: false);
        NotificarCambioEstadoAdmisionJSON();
    }

    public void RegistrarPalletAbandonadoPorAborto(Pedido pedido, RobotAMR.PalletEnPasillo paquete)
    {
        EncolarCargaDeUnidadCaida(pedido, paquete);
    }

    public void NotificarMisionAbortada()
    {
        NotificarMisionAbortada(false);
    }

    public void NotificarMisionAbortada(bool conservarEnCola)
    {
        DashboardUI.Instance?.RegistrarLog("<color=orange>[WMS]:</color> Misión abortada. Avanzando cola WMS.");
        if (!conservarEnCola)
        {
            palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
            FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.ActualizarResumenColaEnVivo(palletsEnColaWMS);
        }

        if (palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
        else if (ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada (Con Reprogramaciones). Unidades en espera de nuevas órdenes ERP.");
            EvaluarCierreTurnoSiCorresponde();
        }

        NotificarCambioEstadoAdmisionJSON();
    }

    public bool EvaluarCierreTurnoSiCorresponde()
    {
        if (turnoCerrado) return true;
        if (!turnoIniciado && !sesionOperativaIniciada) return false;

        if (RelojSimuladorTurno.TurnoSuperoCierre)
        {
            CerrarTurnoPorEVE($"Cumplimiento de jornada de 8 horas ({RelojSimuladorTurno.HoraCierreTurnoTexto}).");
            return true;
        }

        if (admisionNuevosPedidosBloqueada && palletsEnColaWMS == 0 && ContarMisionesEnVuelo() == 0)
        {
            string causa = !string.IsNullOrEmpty(motivoBloqueoAdmision)
                ? motivoBloqueoAdmision
                : "Cola WMS completada con admisión bloqueada.";
            CerrarTurnoPorEVE($"Cola asignada completada con admisión bloqueada ({causa}).");
            return true;
        }

        if (TotalIncidentesCriticosTurno > 0 && ContarAMRsRecuperablesEnTurno() == 0)
        {
            string causa = !string.IsNullOrEmpty(motivoFaseCritica)
                ? motivoFaseCritica
                : "Cero unidades AMR operativas o recuperables.";
            CerrarTurnoPorEVE(causa, mantenerModalFaseCritica: true);
            return true;
        }

        return false;
    }

    public void CerrarTurnoPorEVE(string motivoCierre, bool mantenerModalFaseCritica = false)
    {
        if (turnoCerrado) return;

        if (SimulacionPausada)
        {
            SimulacionPausada = false;
            Time.timeScale = 1f;
            OnPausaCambiada?.Invoke(false);
        }

        if (rutinaHeadwayDespacho != null)
        {
            StopCoroutine(rutinaHeadwayDespacho);
            rutinaHeadwayDespacho = null;
        }

        turnoCerrado = true;
        turnoIniciado = false;
        admisionNuevosPedidosBloqueada = true;
        motivoCierreTurno = !string.IsNullOrEmpty(motivoCierre) ? motivoCierre : "Se cierra turno operativo.";
        if (string.IsNullOrEmpty(motivoBloqueoAdmision))
        {
            motivoBloqueoAdmision = motivoCierreTurno;
        }

        string horaCierre = RelojSimuladorTurno.HoraActualFormateada;
        string msgLog = $"<color=#F59E0B><b>[SE CIERRA TURNO — {horaCierre}]:</b></color> {motivoCierreTurno} Nuevos despachos bloqueados hasta reiniciar.";
        Debug.Log(msgLog);
        DashboardUI.Instance?.RegistrarLog(msgLog);
        DashboardUI.Instance?.RegistrarFeedbackSupervisor($"[SE CIERRA TURNO — {horaCierre}]: {motivoCierreTurno}");
        DashboardUI.Instance?.ActualizarBannerTurno($"<b><color=#EF4444>SE CIERRA TURNO ({horaCierre}):</color> DESPACHOS DETENIDOS — PULSE REINICIAR TURNO</b>");

        if (!mantenerModalFaseCritica)
        {
            RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
            for (int i = 0; i < flota.Length; i++)
            {
                RobotAMR r = flota[i];
                if (r != null && r.EsRecuperableEnTurno && r.estadoActual != RobotAMR.EstadoRobot.Recargando && r.estadoActual != RobotAMR.EstadoRobot.Apagado)
                {
                    r.EnviarACargaInductiva(ObtenerPosicionPadPorUnidad(r));
                }
            }
            DirectorCinematico.Instance?.ActivarVistaGeneralFinDeTurno();
        }

        DashboardUI.Instance?.NotificarCierreTurnoEVE(motivoCierreTurno, mantenerModalFaseCritica);
        NotificarCambioEstadoAdmisionJSON();
        OnTurnoCerrado?.Invoke(motivoCierreTurno);
    }

    private void FinalizarTurnoYRetornarACarga(string mensajeCierre)
    {
        CerrarTurnoPorEVE(mensajeCierre);
    }

    public void ReiniciarEstadoPorCompilacion()
    {
        StopAllCoroutines();
        rutinaHeadwayDespacho = null;
        despachandoDespejeISO = false;
        alertaDemandaEnCurso = false;
        Time.timeScale = 1f;
        SimulacionPausada = false;

        LimpiarResiduosEscenaTurno();

        turnoCerrado = false;
        turnoIniciado = false;
        sesionOperativaIniciada = false;
        motivoCierreTurno = string.Empty;
        palletsEnColaWMS = 0;
        palletsCompletadosTurno = 0;
        flotaEscaladaDual = false;
        indicePedidoActual = 0;
        indiceFilaManifiestoGlobal = 0;
        relevo24_7Activo = false;
        tiempoProximoDespachoPermitido = 0f;
        ReiniciarContadoresIncidentesTurno();
        RelojSimuladorTurno.ReiniciarRelojTurno(0.0f);
        RobotAMR.LimpiarEstaticosTrafico();
    }

    public void ReiniciarTurnoCompleto()
    {
        StopAllCoroutines();
        rutinaHeadwayDespacho = null;
        despachandoDespejeISO = false;
        alertaDemandaEnCurso = false;
        SimulacionPausada = false;
        Time.timeScale = 1f;
        OnPausaCambiada?.Invoke(false);

        LimpiarResiduosEscenaTurno();

        turnoCerrado = false;
        turnoIniciado = false;
        motivoCierreTurno = string.Empty;
        palletsEnColaWMS = 0;
        palletsCompletadosTurno = 0;
        flotaEscaladaDual = false;
        indicePedidoActual = 0;
        indiceFilaManifiestoGlobal = 0;
        relevo24_7Activo = false;
        tiempoProximoDespachoPermitido = 0f;
        amrEnAlertaBateria20 = null;
        robotEjecutor = robotAMR_01;
        ReiniciarContadoresIncidentesTurno();
        RelojSimuladorTurno.ReiniciarRelojTurno(0.0f);
        RobotAMR.LimpiarEstaticosTrafico();

        manifiestoActual = new Manifiesto { pedidos = new List<Pedido>() };
        matrizLIFOCamion.Clear();
        pesoAcumuladoCamion = 0f;

        if (gestorTMS == null) gestorTMS = ModuloDespachoTMS.Instance != null ? ModuloDespachoTMS.Instance : FindFirstObjectByType<ModuloDespachoTMS>();
        gestorTMS?.ReiniciarEstadoMuelles();
        dronMantenimiento?.ReiniciarBaseYEstado();

        GestorAudioSCADA audio = GestorAudioSCADA.Instance != null ? GestorAudioSCADA.Instance : FindFirstObjectByType<GestorAudioSCADA>(FindObjectsInactive.Include);
        audio?.SilenciarAlarmaFabrica();

        RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR r = flota[i];
            if (r != null)
            {
                r.ReiniciarEstadoUnidadTurno(ObtenerPosicionPadPorUnidad(r));
            }
        }

        DashboardUI.Instance?.LimpiarUIAlReiniciarTurno();
        IniciarTurno();
        OnTurnoReiniciado?.Invoke();
    }

    private void LimpiarResiduosEscenaTurno()
    {
        while (colaPalletsEnPasillo.Count > 0)
        {
            RobotAMR.PalletEnPasillo p = colaPalletsEnPasillo.Dequeue();
            if (p != null)
            {
                if (p.stockOcultadoOriginal != null)
                {
                    p.stockOcultadoOriginal.SetActive(true);
                }
                if (p.palletGO != null)
                {
                    if (Application.isPlaying) Destroy(p.palletGO);
                    else DestroyImmediate(p.palletGO);
                }
            }
        }

        for (int i = 0; i < palletsEntregados.Count; i++)
        {
            if (palletsEntregados[i] != null)
            {
                if (Application.isPlaying) Destroy(palletsEntregados[i]);
                else DestroyImmediate(palletsEntregados[i]);
            }
        }
        palletsEntregados.Clear();

        foreach (var kvp in directorioBodega)
        {
            if (kvp.Value == null) continue;
            Transform stock = kvp.Value.Find("Pallet_Stock");
            if (stock != null && !stock.gameObject.activeSelf)
            {
                stock.gameObject.SetActive(true);
            }
        }

        if (obstaculoPasilloISO != null && obstaculoPasilloISO.activeSelf)
        {
            obstaculoPasilloISO.transform.SetParent(null, true);
            obstaculoPasilloISO.SetActive(false);
        }
    }

    private int ContarMisionesEnVuelo()
    {
        int enVuelo = 0;
        if (EstaEnMisionActiva(robotAMR_01)) enVuelo++;
        if (EstaEnMisionActiva(robotAMR_02)) enVuelo++;
        if (EstaEnMisionActiva(robotAMR_03)) enVuelo++;
        return enVuelo;
    }

    private static bool EstaEnMisionActiva(RobotAMR robot)
    {
        if (robot == null) return false;
        return robot.estadoActual == RobotAMR.EstadoRobot.En_Transito ||
               robot.estadoActual == RobotAMR.EstadoRobot.Extrayendo ||
               robot.estadoActual == RobotAMR.EstadoRobot.Transportando ||
               robot.estadoActual == RobotAMR.EstadoRobot.Entregando ||
               robot.estadoActual == RobotAMR.EstadoRobot.Interrumpido;
    }

    private int ObtenerCuposMaximosPiso()
    {
        ObtenerEstadoFlotaReal(out int operativosPiso, out bool tieneSoporteDedicado);
        if (operativosPiso <= 0) return 0;
        int deseados = (flotaEscaladaDual || (!tieneSoporteDedicado && palletsEnColaWMS > 1)) ? 2 : 1;
        return Mathf.Clamp(deseados, 1, operativosPiso);
    }

    private void DespacharColaDisponible(RobotAMR robotPreferido = null)
    {
        if (turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica) return;
        if (palletsEnColaWMS <= 0) return;
        if (colaPalletsEnPasillo.Count == 0 && colaPrioritariaReasignacion.Count == 0 &&
            (plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0))
        {
            return;
        }

        int iteracionesSeguridad = 4;
        while (palletsEnColaWMS > ContarMisionesEnVuelo() && ContarMisionesEnVuelo() < ObtenerCuposMaximosPiso() && iteracionesSeguridad-- > 0)
        {
            RobotAMR objetivo = SeleccionarSiguienteRobotLibre(robotPreferido);
            robotPreferido = null;

            if (objetivo == null) break;

            bool enSalaCarga = objetivo.transform.position.z < RobotAMR.ZUmbralPortico;
            if (enSalaCarga && Time.time < tiempoProximoDespachoPermitido)
            {
                if (rutinaHeadwayDespacho == null)
                {
                    rutinaHeadwayDespacho = StartCoroutine(EsperarHeadwayYDespachar());
                }
                break;
            }

            if (!DespacharPedidoARobot(objetivo))
            {
                break;
            }
        }
    }

    private IEnumerator EsperarHeadwayYDespachar()
    {
        while (Time.time < tiempoProximoDespachoPermitido)
        {
            yield return null;
        }
        rutinaHeadwayDespacho = null;
        if (!turnoCerrado && !operacionDetenidaPorSeguridad && !turnoEnFaseCritica && palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
    }

    private RobotAMR SeleccionarSiguienteRobotLibre(RobotAMR preferido)
    {
        if (preferido != null && preferido.EsRecuperableEnTurno && preferido.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            if (flotaEscaladaDual) return preferido;
            if (relevo24_7Activo && preferido == robotAMR_02) return preferido;
            if (!relevo24_7Activo && preferido == robotAMR_01) return preferido;
        }

        if (flotaEscaladaDual)
        {
            if (robotAMR_01 != null && robotAMR_01.EsRecuperableEnTurno && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo) return robotAMR_01;
            if (robotAMR_02 != null && robotAMR_02.EsRecuperableEnTurno && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo) return robotAMR_02;
            if (robotAMR_03 != null && robotAMR_03.EsRecuperableEnTurno && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
                return robotAMR_03;
            }
            return null;
        }

        RobotAMR ejecutorActual = (relevo24_7Activo && robotAMR_02 != null && robotAMR_02.EsRecuperableEnTurno)
            ? robotAMR_02
            : ((robotAMR_01 != null && robotAMR_01.EsRecuperableEnTurno) ? robotAMR_01 : robotEjecutor);

        if (ejecutorActual != null && ejecutorActual.EsRecuperableEnTurno && ejecutorActual.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            return ejecutorActual;
        }

        if (robotAMR_02 != null && robotAMR_02.EsRecuperableEnTurno && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo &&
            (robotAMR_01 == null || !robotAMR_01.EsRecuperableEnTurno || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Recargando || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Apagado || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido))
        {
            return robotAMR_02;
        }

        if (robotAMR_03 != null && robotAMR_03.EsRecuperableEnTurno && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo &&
            (robotAMR_01 == null || !robotAMR_01.EsRecuperableEnTurno || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Recargando || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Apagado || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido))
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
            return robotAMR_03;
        }

        if (robotAMR_01 != null && robotAMR_01.EsRecuperableEnTurno && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            return robotAMR_01;
        }

        return null;
    }

    private bool DespacharPedidoARobot(RobotAMR robot)
    {
        if (turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica) return false;
        if (robot == null || !robot.EsRecuperableEnTurno || robot.estadoActual != RobotAMR.EstadoRobot.Inactivo) return false;

        if (colaPalletsEnPasillo.Count > 0)
        {
            RobotAMR.PalletEnPasillo paquete = colaPalletsEnPasillo.Peek();
            if (DespacharRecoleccionPalletEnPasillo(robot, paquete))
            {
                colaPalletsEnPasillo.Dequeue();
                return true;
            }
            return false;
        }

        bool desdeColaReasignacion = colaPrioritariaReasignacion.Count > 0;
        if (!desdeColaReasignacion && (plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0))
        {
            return false;
        }

        Pedido pedido;
        if (desdeColaReasignacion)
        {
            pedido = colaPrioritariaReasignacion.Peek();
            if (pedido != null && pedido.peso_kg <= 0f)
            {
                pedido.peso_kg = Pedido.PesoReferenciaBaseKg;
            }
        }
        else
        {
            Pedido plantilla = plantillaPedidosFisicos[Mathf.Abs(indicePedidoActual) % plantillaPedidosFisicos.Count];
            float pesoAsignado = GenerarPesoIndustrialPedido(indicePedidoActual, semillaVariacionPeso);
            string idPallet = $"PAL-{8491 + indicePedidoActual}";
            pedido = plantilla != null
                ? plantilla.ClonarConPeso(pesoAsignado, idPallet)
                : new Pedido
                {
                    id_pallet = idPallet,
                    nombre_producto = "Carga Industrial",
                    codigo_sku = "SKU-STD",
                    peso_kg = pesoAsignado,
                    coordenada_bodega = "P1-C1-F1"
                };
        }

        bool ok = DespacharPedidoEspecificoARobot(robot, pedido);
        if (ok)
        {
            if (desdeColaReasignacion)
            {
                colaPrioritariaReasignacion.Dequeue();
            }
            else
            {
                indicePedidoActual++;
            }

            EvaluarEficienciaOperativaYEscalamiento(pedido, permitirDespachoInmediato: false);
        }
        return ok;
    }

    private bool DespacharRecoleccionPalletEnPasillo(RobotAMR robot, RobotAMR.PalletEnPasillo paquete)
    {
        if (turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica) return false;
        if (robot == null || paquete == null || paquete.pedido == null) return false;
        if (!robot.EsRecuperableEnTurno || robot.estadoActual != RobotAMR.EstadoRobot.Inactivo) return false;

        if (robot.transform.position.z < RobotAMR.ZUmbralPortico)
        {
            tiempoProximoDespachoPermitido = Time.time + HeadwaySalidaSalaCargaSegundos;
        }

        if (robot == robotAMR_03)
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
        }

        int filaSecuencial = paquete.indiceFilaManifiesto;
        if (filaSecuencial < 0)
        {
            filaSecuencial = indiceFilaManifiestoGlobal++;
            var generadorTabla = FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include);
            if (generadorTabla != null)
            {
                filaSecuencial = generadorTabla.AsegurarFilaOperativaParaMision(filaSecuencial, paquete.pedido, palletsEnColaWMS);
            }
        }

        bool ok = robot.AsignarMisionRecoleccionPasillo(paquete, filaSecuencial);
        if (ok)
        {
            robotEjecutor = robot;
            OnMisionEvaluada?.Invoke();
        }
        return ok;
    }

    private bool DespacharPedidoEspecificoARobot(RobotAMR robot, Pedido pedido)
    {
        if (turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica) return false;
        if (robot == null || pedido == null || !robot.EsRecuperableEnTurno) return false;
        if (robot.estadoActual != RobotAMR.EstadoRobot.Inactivo) return false;

        if (robot.transform.position.z < RobotAMR.ZUmbralPortico)
        {
            tiempoProximoDespachoPermitido = Time.time + HeadwaySalidaSalaCargaSegundos;
        }

        if (robot == robotAMR_03)
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
        }

        directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);
        if (destino == null && !string.IsNullOrEmpty(pedido.coordenada_bodega))
        {
            GameObject go = GameObject.Find(pedido.coordenada_bodega);
            if (go != null)
            {
                destino = go.transform;
                directorioBodega[pedido.coordenada_bodega] = destino;
            }
        }

        int numCamion = 1;
        int slot = 0;
        if (gestorTMS != null)
        {
            Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out numCamion, out slot);
            if (muelleDestino == null)
            {
                return false;
            }
            robot.zonaTransferencia = muelleDestino;
            robot.numCamionAsignado = numCamion;
            robot.slotLIFOAsignado = slot;
        }

        if (destino == null)
        {
            robot.slotLIFOAsignado = 0;
            if (gestorTMS != null && slot > 0) gestorTMS.CancelarReservaSlot(numCamion, slot);
            robot.AsignarMision(pedido, null, -1);
            return false;
        }

        int filaSecuencial = indiceFilaManifiestoGlobal++;
        var generadorTabla = FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include);
        if (generadorTabla != null)
        {
            filaSecuencial = generadorTabla.AsegurarFilaOperativaParaMision(filaSecuencial, pedido, palletsEnColaWMS);
        }

        robotEjecutor = robot;
        OnMisionEvaluada?.Invoke();
        robot.AsignarMision(pedido, destino, filaSecuencial);
        return true;
    }

    public Vector3 ObtenerSiguienteSlotZonaTecnica(RobotAMR amr)
    {
        Vector3[] fallbacks = new Vector3[]
        {
            new Vector3(-34.50f, 0.25886f, -40.50f),
            new Vector3(-32.00f, 0.25886f, -40.50f),
            new Vector3(-29.50f, 0.25886f, -40.50f)
        };

        int indicePreferido = 0;
        if (amr != null)
        {
            if (amr == robotAMR_03 || (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("03")) || amr.gameObject.name.Contains("03"))
            {
                indicePreferido = 2;
            }
            else if (amr == robotAMR_02 || (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("02")) || amr.gameObject.name.Contains("02"))
            {
                indicePreferido = 1;
            }
        }

        int indiceElegido = indicePreferido;
        if (slotsTecnicosOcupados.Contains(indiceElegido))
        {
            for (int i = 0; i < 3; i++)
            {
                if (!slotsTecnicosOcupados.Contains(i))
                {
                    indiceElegido = i;
                    break;
                }
            }
        }
        slotsTecnicosOcupados.Add(indiceElegido);

        if (slotsZonaTecnica != null && indiceElegido >= 0 && indiceElegido < slotsZonaTecnica.Length && slotsZonaTecnica[indiceElegido] != null)
        {
            Vector3 p = slotsZonaTecnica[indiceElegido].position;
            return new Vector3(p.x, 0.25886f, p.z);
        }

        GameObject goSlot = GameObject.Find($"Slot_Tecnico_0{indiceElegido + 1}");
        if (goSlot != null)
        {
            Vector3 p = goSlot.transform.position;
            return new Vector3(p.x, 0.25886f, p.z);
        }

        return fallbacks[Mathf.Clamp(indiceElegido, 0, fallbacks.Length - 1)];
    }

    public Vector3 ObtenerPosicionPadPorUnidad(RobotAMR amr)
    {
        string nombrePad = "Pad_Inductivo_01";
        Vector3 fallback = new Vector3(28.00f, 0.25886f, -54.80f);

        if (amr != null)
        {
            bool es03 = (amr == robotAMR_03) ||
                        (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("03")) ||
                        amr.gameObject.name.Contains("03");
            bool es02 = !es03 && ((amr == robotAMR_02) ||
                        (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("02")) ||
                        amr.gameObject.name.Contains("02"));

            if (es03)
            {
                nombrePad = "Pad_Inductivo_03";
                fallback = new Vector3(40.00f, 0.25886f, -54.80f);
            }
            else if (es02)
            {
                nombrePad = "Pad_Inductivo_02";
                fallback = new Vector3(34.00f, 0.25886f, -57.40f);
            }
        }

        GameObject goPad = GameObject.Find(nombrePad);
        if (goPad != null)
        {
            Vector3 p = goPad.transform.position;
            return new Vector3(p.x, 0.25886f, p.z);
        }
        return fallback;
    }

    public void AutorizarOverrideBateria()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        RobotAMR objetivo = (amrEnAlertaBateria20 != null && amrEnAlertaBateria20.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
            ? amrEnAlertaBateria20
            : ((robotEjecutor != null && robotEjecutor.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
                ? robotEjecutor
                : (robotAMR_01 != null && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                    ? robotAMR_01
                    : (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                        ? robotAMR_02
                        : (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                            ? robotAMR_03
                            : amrEnAlertaBateria20))));

        if (objetivo != null)
        {
            objetivo.EjecutarOverride();
        }
        else if (robotEjecutor != null)
        {
            robotEjecutor.EjecutarOverride();
        }

        amrEnAlertaBateria20 = null;
    }

    public void EjecutarRelevoFlota24_7()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        RobotAMR robotSaliente = (amrEnAlertaBateria20 != null && amrEnAlertaBateria20.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
            ? amrEnAlertaBateria20
            : ((robotEjecutor != null && robotEjecutor.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
                ? robotEjecutor
                : (robotAMR_01 != null && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                    ? robotAMR_01
                    : (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                        ? robotAMR_02
                        : (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                            ? robotAMR_03
                            : (robotAMR_01 != null ? robotAMR_01 : robotEjecutor)))));

        RobotAMR robotRelevo = null;
        if (robotAMR_02 != null && robotAMR_02 != robotSaliente && robotAMR_02.EsRecuperableEnTurno && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_02;
        }
        else if (robotAMR_03 != null && robotAMR_03 != robotSaliente && robotAMR_03.EsRecuperableEnTurno && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_03;
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
        }
        else if (robotAMR_01 != null && robotAMR_01 != robotSaliente && robotAMR_01.EsRecuperableEnTurno && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_01;
        }

        bool tieneCarga = robotSaliente != null && (robotSaliente.TieneCargaFisica || robotSaliente.estadoPreInterrupcionBateria == RobotAMR.EstadoRobot.Transportando);

        if (tieneCarga)
        {
            // Caso B: El AMR YA tiene el pallet encima -> Reanudar para terminar entrega y diferir recarga
            robotSaliente.ReanudarTrasRelevoDiferido();
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[RELEVO 24/7]:</color> {robotSaliente.idRobot} transportando estiba. Completará entrega en muelle antes de acoplarse al Pad.");

            // Desplegar inmediatamente la unidad de soporte libre para el siguiente pedido de la cola WMS
            if (robotRelevo != null)
            {
                DirectorCinematico.Instance?.RelevarRobotActivo(robotRelevo);
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotRelevo);
                relevo24_7Activo = (robotRelevo == robotAMR_02);
                robotEjecutor = robotRelevo;
                DespacharColaDisponible(robotRelevo);
            }
            else
            {
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotSaliente);
            }
        }
        else
        {
            // Caso A: El AMR aún NO tiene pallet físico encima -> Desviar inmediatamente a recargar
            Pedido pedidoInterrumpido = robotSaliente != null ? robotSaliente.PedidoActual : null;

            if (robotRelevo != null)
            {
                DirectorCinematico.Instance?.RelevarRobotActivo(robotRelevo);
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotRelevo);
                relevo24_7Activo = (robotRelevo == robotAMR_02);
                robotEjecutor = robotRelevo;

                if (robotSaliente != null && robotSaliente != robotRelevo)
                {
                    robotSaliente.enPausaPreventivaBateria20 = false;
                    Vector3 destinoPad = ObtenerPosicionPadPorUnidad(robotSaliente);
                    robotSaliente.EnviarACargaInductiva(destinoPad);
                }

                Pedido pedidoPendiente = pedidoInterrumpido;
                if (pedidoPendiente == null && plantillaPedidosFisicos != null && plantillaPedidosFisicos.Count > 0)
                {
                    pedidoPendiente = plantillaPedidosFisicos[Mathf.Abs(indicePedidoActual) % plantillaPedidosFisicos.Count];
                    indicePedidoActual++;
                }

                if (pedidoPendiente != null)
                {
                    DespacharPedidoEspecificoARobot(robotRelevo, pedidoPendiente);
                }
            }
            else if (robotSaliente != null)
            {
                // Sin soporte disponible: los AMRs restantes continúan operando bajo el 20% en modo continuidad
                if (!admisionNuevosPedidosBloqueada)
                {
                    ActivarBloqueoAdmisionNuevosPedidos($"Sin unidad de soporte libre durante alerta <20% de {robotSaliente.idRobot}");
                }
                robotSaliente.EjecutarOverride();
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotSaliente);
                DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[MODO CONTINUIDAD — SIN SOPORTE]:</color> No hay relevo libre. {robotSaliente.idRobot} continúa misión por debajo del 20% (hasta escalón 6%/3%).");
            }
            else
            {
                DirectorCinematico.Instance?.LiberarFocoIncidente(null);
            }
        }

        amrEnAlertaBateria20 = null;

        string idSaliente = robotSaliente != null ? robotSaliente.idRobot : "AMR";
        string idRelevo = robotRelevo != null ? robotRelevo.idRobot : "SOPORTE";
        string mensajeRelevo = $"[RELEVO 24/7]: Gestión completada para {idSaliente}. {idRelevo} activo en cola WMS.";
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8><b>{mensajeRelevo}</b></color>");
        Debug.Log($"<color=cyan>{mensajeRelevo}</color>");
    }

    public void SimularAlertaBateria20()
    {
        if (!turnoIniciado) return;

        RobotAMR objetivo = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: false);
        if (objetivo == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA <20%]:</color> No hay AMRs disponibles en escena.");
            return;
        }

        amrEnAlertaBateria20 = objetivo;
        objetivo.InducirInterrupcionPreventivaBateria(20.8f);
        robotEjecutor = objetivo;
        DirectorCinematico.Instance?.EnfocarIncidenteCritico(objetivo.transform);
        DashboardUI.Instance?.RegistrarLog($"<color=red>[SIMULACIÓN SCADA <20%]:</color> Descarga preventiva inducida en {objetivo.idRobot} (20.8% SoC). Evaluando Override vs. Relevo 24/7.");
        DashboardUI.Instance?.MostrarEmergencia(true, 18.9f, 3.2f);
    }

    public void SimularAlertaCritica6()
    {
        if (!turnoIniciado) return;
        if (operacionDetenidaPorSeguridad || turnoEnFaseCritica)
        {
            DashboardUI.Instance?.RegistrarLog("<color=#EF4444>[FASE CRÍTICA]:</color> Turno detenido por seguridad. Simulación de escalón 6% bloqueada.");
            return;
        }

        RobotAMR objetivo = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: true, preferirEnCorredor: true);
        if (objetivo == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA <=6%]:</color> No hay AMRs operativos disponibles en escena.");
            return;
        }

        objetivo.bateriaActual = 5.4f;
        objetivo.escalon6Evaluado = false;
        DashboardUI.Instance?.ActualizarBateria(objetivo.idRobot, objetivo.bateriaActual / 100f, objetivo.estadoActual);
        ProcesarEscalonCritico6(objetivo);
    }

    public void ProcesarEscalonCritico6(RobotAMR objetivo)
    {
        if (objetivo == null || !objetivo.EsRecuperableEnTurno || objetivo.estadoActual == RobotAMR.EstadoRobot.Apagado)
        {
            return;
        }

        bool viable = objetivo.EvaluarYActuarEscalon6(out string diagnostico);
        if (viable)
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>[ESCALÓN 6% BMS - {objetivo.idRobot}]:</b></color> {diagnostico}");
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA($"<color=#F59E0B>ESCALÓN 6% ({objetivo.idRobot})</color>", diagnostico, 4.0f);
            DashboardUI.Instance?.DestellarAlertaUnidad(objetivo.idRobot, false);
            if (!objetivo.TieneCargaFisica && !operacionDetenidaPorSeguridad && !turnoEnFaseCritica && palletsEnColaWMS > 0)
            {
                DespacharColaDisponible();
            }
        }
        else
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[ESCALÓN 6% -> CORTE 3% ({objetivo.idRobot})]:</b></color> {diagnostico}");
            EjecutarColapsoYRescateInmediato(objetivo);
        }
    }

    public void SimularColapsoBateria3()
    {
        if (!turnoIniciado) return;
        if (operacionDetenidaPorSeguridad || turnoEnFaseCritica)
        {
            DashboardUI.Instance?.RegistrarLog("<color=#EF4444>[FASE CRÍTICA]:</color> Turno detenido por seguridad. Despachos bloqueados.");
            return;
        }

        RobotAMR objetivo = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: true, preferirEnCorredor: true);
        if (objetivo == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA <=3%]:</color> No hay AMRs disponibles en escena.");
            return;
        }

        EjecutarColapsoYRescateInmediato(objetivo);
    }

    public void SimularFallaHardwareAMR()
    {
        if (!turnoIniciado) return;
        if (operacionDetenidaPorSeguridad || turnoEnFaseCritica)
        {
            DashboardUI.Instance?.RegistrarLog("<color=#EF4444>[FASE CRÍTICA]:</color> Turno detenido por seguridad.");
            return;
        }

        RobotAMR objetivo = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: false, preferirEnCorredor: true);
        if (objetivo == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA HW]:</color> No hay AMRs recuperables disponibles en escena.");
            return;
        }

        ReportarFallaHardwareAMR(objetivo);
    }

    public void ReportarFallaHardwareAMR(RobotAMR objetivo)
    {
        if (objetivo == null || objetivo.estadoActual == RobotAMR.EstadoRobot.FueraDeServicio_Danado) return;

        bool enZonaCamion = objetivo.EstaEnZonaCamionORampa();
        int camionAfectado = enZonaCamion ? DeterminarNumeroCamionPorAMR(objetivo) : 0;

        Pedido pedidoHuerfano = objetivo.ForzarFallaHardwareIrrecuperable(out RobotAMR.PalletEnPasillo palletDepositado);
        EncolarCargaDeUnidadCaida(pedidoHuerfano, enZonaCamion ? null : palletDepositado);

        string contextoZona = enZonaCamion
            ? $"dentro de Muelle_Camion_0{camionAfectado} (bahía clausurada; intervención humana requerida)"
            : (objetivo.transform.position.z <= -2.64f
                ? "en pasillo. Desplegando Dron de Mantenimiento hacia Zona Técnica"
                : "en zona de transición; requiere despeje humano");

        string msgHardware = $"[RAMA B - AMR DAÑADO]: {objetivo.idRobot} presenta falla de hardware irrecuperable {contextoZona}.";
        DashboardUI.Instance?.RegistrarLog($"<color=#DC2626><b>{msgHardware}</b></color>");
        DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
            "<color=#DC2626>AMR DAÑADO — FUERA DE SERVICIO</color>",
            $"{objetivo.idRobot} fuera de servicio durante el turno ({contextoZona}).",
            5.0f
        );

        GestorAudioSCADA.Instance?.ActivarAlarmaSonoraFabrica($"Falla HW de {objetivo.idRobot}" + (enZonaCamion ? $" en Muelle 0{camionAfectado}" : " — retiro a Zona Técnica"));

        if (enZonaCamion)
        {
            ModuloDespachoTMS tms = gestorTMS != null ? gestorTMS : ModuloDespachoTMS.Instance;
            tms?.ClausurarMuellePorIncidenteAMR(camionAfectado, $"Falla HW de {objetivo.idRobot} en bahía", out _);
            ReevaluarMuellesEnFlotaActiva();
        }

        DirectorCinematico.Instance?.EnfocarIncidenteCritico(objetivo.transform);

        bool bloqueoPrevioHW = admisionNuevosPedidosBloqueada;
        bool paradaActivada = RegistrarIncidenteYEvaluarReincidencia(objetivo, RobotAMR.TipoFallaAMR.FallaHardwareIrrecuperable);
        RobotAMR relevo = (!paradaActivada && !operacionDetenidaPorSeguridad) ? SeleccionarRelevoParaIncidente(objetivo) : null;
        if (bloqueoPrevioHW && !paradaActivada)
        {
            string idRel = relevo != null ? relevo.idRobot : "flota viva";
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                $"[INCIDENTE HW ADICIONAL — {RelojSimuladorTurno.HoraActualFormateada}]: {objetivo.idRobot} fuera de servicio ({contextoZona}). Admisión permanece cerrada; continuidad sostenida con {idRel}.");
        }

        if (!paradaActivada && !operacionDetenidaPorSeguridad)
        {
            if (relevo != null && (colaPalletsEnPasillo.Count > 0 || colaPrioritariaReasignacion.Count > 0 || palletsEnColaWMS > 0))
            {
                StartCoroutine(DespacharRelevoPendienteConHeadway(relevo));
            }
        }

        if (!enZonaCamion && objetivo.transform.position.z <= -2.64f && dronMantenimiento != null)
        {
            Vector3 posSlotTecnico = ObtenerSiguienteSlotZonaTecnica(objetivo);
            StartCoroutine(dronMantenimiento.EjecutarRescateAMRAbortado(
                objetivo,
                posSlotTecnico,
                onRescateCompletado: () =>
                {
                    DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[TRASLADO ZONA TÉCNICA COMPLETADO]:</color> {objetivo.idRobot} depositado en Zona Técnica de Mantenimiento (Fuera de Servicio).");
                },
                onElevacionCompletada: () =>
                {
                    if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica)
                    {
                        RobotAMR seguimientoRelevo = (relevo != null && relevo.EsRecuperableEnTurno && relevo.estadoActual != RobotAMR.EstadoRobot.Apagado)
                            ? relevo
                            : (EstaEnMisionActiva(robotAMR_03) ? robotAMR_03 : (EstaEnMisionActiva(robotAMR_02) ? robotAMR_02 : null));
                        DirectorCinematico.Instance?.LiberarFocoIncidente(seguimientoRelevo);
                    }
                },
                esTrasladoZonaTecnica: true
            ));
        }
        else if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica)
        {
            DirectorCinematico.Instance?.LiberarFocoIncidente(relevo);
        }
    }

    public void FinalizarEntregaYEjecutarColapsoBMS(RobotAMR objetivo)
    {
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        palletsCompletadosTurno++;
        FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.ActualizarResumenColaEnVivo(palletsEnColaWMS);
        EjecutarColapsoYRescateInmediato(objetivo);
    }

    public void NotificarRetiroACargaPostEntrega(RobotAMR robotRetirado)
    {
        if (!turnoCerrado && !operacionDetenidaPorSeguridad && !turnoEnFaseCritica && palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
        else if (palletsEnColaWMS == 0 && ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
            EvaluarCierreTurnoSiCorresponde();
        }
    }

    public void ReencolarPedidoPorRetiroPreventivo(Pedido pedido)
    {
        if (pedido == null) return;
        colaPrioritariaReasignacion.Enqueue(pedido);
        palletsEnColaWMS = Mathf.Max(palletsEnColaWMS, 1);
    }

    public void ReencolarPalletEnPasilloPorRetiro(RobotAMR.PalletEnPasillo paquete)
    {
        if (paquete == null || paquete.pedido == null) return;
        colaPalletsEnPasillo.Enqueue(paquete);
        palletsEnColaWMS = Mathf.Max(palletsEnColaWMS, 1);
    }

    private void EncolarCargaDeUnidadCaida(Pedido pedidoHuerfano, RobotAMR.PalletEnPasillo palletDepositado)
    {
        if (palletDepositado != null && palletDepositado.palletGO != null && palletDepositado.pedido != null)
        {
            colaPalletsEnPasillo.Enqueue(palletDepositado);
            palletsEnColaWMS = Mathf.Max(palletsEnColaWMS, 1);
        }
        else if (pedidoHuerfano != null)
        {
            colaPrioritariaReasignacion.Enqueue(pedidoHuerfano);
            palletsEnColaWMS = Mathf.Max(palletsEnColaWMS, 1);
        }
    }

    private RobotAMR SeleccionarRelevoParaIncidente(RobotAMR objetivo)
    {
        if (robotAMR_03 != null && robotAMR_03 != objetivo && robotAMR_03.EsRecuperableEnTurno && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#FACC15>EN RELEVO ACTIVO</color>");
            return robotAMR_03;
        }

        RobotAMR candidato = SeleccionarSiguienteRobotLibre(null);
        if (candidato != null && candidato != objetivo)
        {
            if (candidato == robotAMR_03)
            {
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#FACC15>EN RELEVO ACTIVO</color>");
            }
            return candidato;
        }
        return null;
    }

    public bool TieneFlotaAsignadaEnEscena()
    {
        return robotAMR_01 != null || robotAMR_02 != null || robotAMR_03 != null;
    }

    public int ContarAMRsRecuperablesEnTurno()
    {
        if (!TieneFlotaAsignadaEnEscena()) return 3;

        RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        int recuperables = 0;
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR r = flota[i];
            if (r == null || !r.gameObject.activeInHierarchy) continue;
            bool varadoEnCamion = r.estadoActual == RobotAMR.EstadoRobot.Apagado && r.EstaEnZonaCamionORampa();
            if (r.EsRecuperableEnTurno && !varadoEnCamion)
            {
                recuperables++;
            }
        }
        return recuperables;
    }

    public int ContarAMRsOperativosInmediatos()
    {
        if (!TieneFlotaAsignadaEnEscena()) return 3;

        RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        int operativos = 0;
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR r = flota[i];
            if (r == null || !r.gameObject.activeInHierarchy) continue;
            if (r.EsRecuperableEnTurno && r.estadoActual != RobotAMR.EstadoRobot.Apagado)
            {
                operativos++;
            }
        }
        return operativos;
    }

    private int ContarFlotaTotalConfigurada()
    {
        if (!TieneFlotaAsignadaEnEscena()) return 3;
        int total = 0;
        if (robotAMR_01 != null && robotAMR_01.gameObject.activeInHierarchy) total++;
        if (robotAMR_02 != null && robotAMR_02.gameObject.activeInHierarchy) total++;
        if (robotAMR_03 != null && robotAMR_03.gameObject.activeInHierarchy) total++;
        return Mathf.Max(1, total);
    }

    public void ObtenerEstadoFlotaReal(out int amrsOperativosPiso, out bool tieneSoporteDedicadoDisponible)
    {
        if (!TieneFlotaAsignadaEnEscena())
        {
            amrsOperativosPiso = 2;
            tieneSoporteDedicadoDisponible = true;
            return;
        }

        int recuperables = ContarAMRsRecuperablesEnTurno();
        int operativosInmediatos = ContarAMRsOperativosInmediatos();
        amrsOperativosPiso = Mathf.Clamp(recuperables, 0, 2);
        tieneSoporteDedicadoDisponible = robotAMR_03 != null &&
                                         robotAMR_03.gameObject.activeInHierarchy &&
                                         robotAMR_03.EsRecuperableEnTurno &&
                                         recuperables >= 3 &&
                                         operativosInmediatos >= 3;
    }

    public bool HayUnidadSoporteDisponible()
    {
        if (!TieneFlotaAsignadaEnEscena()) return true;

        int recuperables = ContarAMRsRecuperablesEnTurno();
        int operativosInmediatos = ContarAMRsOperativosInmediatos();

        if (flotaEscaladaDual)
        {
            return recuperables >= 3 && operativosInmediatos >= 3 &&
                   robotAMR_03 != null && robotAMR_03.gameObject.activeInHierarchy &&
                   robotAMR_03.EsRecuperableEnTurno && robotAMR_03.estadoActual != RobotAMR.EstadoRobot.Apagado;
        }

        if (recuperables < 2) return false;

        if (TotalIncidentesCriticosTurno > 0 && operativosInmediatos <= ObtenerCuposMaximosPiso())
        {
            return false;
        }

        return true;
    }

    private int ContarAMRsOperativosDisponibles()
    {
        return ContarAMRsOperativosInmediatos();
    }

    public void ActivarBloqueoAdmisionNuevosPedidos(string motivo)
    {
        motivoBloqueoAdmision = string.IsNullOrEmpty(motivo)
            ? "Unidad de soporte/reserva perdida o flota degradada en el turno"
            : motivo;
        bool flancoNuevo = !admisionNuevosPedidosBloqueada;
        admisionNuevosPedidosBloqueada = true;

        int recuperables = ContarAMRsRecuperablesEnTurno();
        int totalFlota = ContarFlotaTotalConfigurada();
        string horaActualTexto = RelojSimuladorTurno.HoraActualFormateada;
        DashboardUI.Instance?.ActualizarBannerTurno($"<b><color=#F59E0B>MODO CONTINUIDAD ({recuperables}/{totalFlota} AMRs | {horaActualTexto}):</color> ADMISIÓN NUEVOS PEDIDOS BLOQUEADA — DRENANDO COLA ACTIVA</b>");

        if (flancoNuevo)
        {
            string avisoBuzon = $"[BLOQUEO DE ADMISIÓN NUEVOS PEDIDOS — {horaActualTexto} (Restan {RelojSimuladorTurno.TiempoRestanteFormateado} hasta {RelojSimuladorTurno.HoraCierreTurnoTexto})]: {motivoBloqueoAdmision}. El turno continúa con {recuperables} unidad(es) para completar la cola ya asignada.";
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>{avisoBuzon}</b></color>");
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(avisoBuzon);
        }

        NotificarCambioEstadoAdmisionJSON();
    }

    public bool RegistrarIncidenteYEvaluarReincidencia(RobotAMR unidadAfectada, RobotAMR.TipoFallaAMR tipoIncidente)
    {
        if (tipoIncidente == RobotAMR.TipoFallaAMR.DescargaCritica3)
        {
            contadorDescargasCriticasTurno++;
        }
        else if (tipoIncidente == RobotAMR.TipoFallaAMR.FallaHardwareIrrecuperable)
        {
            contadorFallasHardwareTurno++;
        }

        int recuperablesRestantes = ContarAMRsRecuperablesEnTurno();
        int operativosRestantes = ContarAMRsOperativosInmediatos();
        string idUnidad = unidadAfectada != null ? unidadAfectada.idRobot : "AMR";

        // Cierre total del turno SOLO cuando 0 AMRs quedan operativos o recuperables
        if (TotalIncidentesCriticosTurno > 0 && recuperablesRestantes == 0)
        {
            string motivo = $"Cero unidades AMR operativas o recuperables tras incidente en {idUnidad} ({contadorDescargasCriticasTurno} descargas / {contadorFallasHardwareTurno} fallas HW).";
            ActivarFaseCriticaYDetenerOperacion(unidadAfectada, motivo);
            return true;
        }

        // Ley de Continuidad: con >= 1 AMR recuperable u operativo, nunca se detiene el turno activo;
        // si se pierde el soporte o la flota queda degradada, se bloquea únicamente la admisión de nuevos pedidos.
        if (contadorFallasHardwareTurno > 0 || !HayUnidadSoporteDisponible())
        {
            string motivoSoporte = contadorFallasHardwareTurno > 0
                ? $"Flota degradada tras falla HW de {idUnidad} ({recuperablesRestantes} unidad(es) recuperables, {operativosRestantes} operativas en pista)"
                : $"Pérdida de unidad de soporte/reserva tras incidente en {idUnidad} ({operativosRestantes} unidad(es) operativas inmediatas)";
            ActivarBloqueoAdmisionNuevosPedidos(motivoSoporte);
        }
        else
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[CONTINUIDAD OPERATIVA]:</color> Incidente en {idUnidad} contenido con unidad de soporte activa ({operativosRestantes} AMRs operativos). Cola WMS continúa.");
        }

        return false;
    }

    public void ActivarFaseCriticaYDetenerOperacion(RobotAMR unidadDisparadora, string motivo)
    {
        turnoEnFaseCritica = true;
        operacionDetenidaPorSeguridad = true;
        admisionNuevosPedidosBloqueada = true;
        motivoFaseCritica = !string.IsNullOrEmpty(motivo) ? motivo : "Cero unidades AMR operativas o recuperables en el turno.";
        motivoBloqueoAdmision = motivoFaseCritica;

        if (rutinaHeadwayDespacho != null)
        {
            StopCoroutine(rutinaHeadwayDespacho);
            rutinaHeadwayDespacho = null;
        }

        RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < flota.Length; i++)
        {
            if (flota[i] != null && flota[i] != unidadDisparadora)
            {
                flota[i].DetenerPorParadaSeguridadTurno();
            }
        }

        string msgCritico = $"[CIERRE TOTAL DE TURNO — 0 AMRs OPERATIVOS]: {motivoFaseCritica} Despachos detenidos.";
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>{msgCritico}</b></color>");
        GestorAudioSCADA.Instance?.ActivarAlarmaSonoraFabrica(msgCritico);
        DashboardUI.Instance?.MostrarAlertaFaseCriticaSupervisor(motivoFaseCritica, contadorDescargasCriticasTurno, contadorFallasHardwareTurno);
        CerrarTurnoPorEVE(motivoFaseCritica, mantenerModalFaseCritica: true);
    }

    public bool SolicitarAutorizacionReincorporacion(RobotAMR amr)
    {
        if (amr == null) return false;

        if (!amr.EsRecuperableEnTurno)
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#DC2626>[EVE REINCORPORACIÓN DENEGADA]:</color> {amr.idRobot} presenta falla de hardware irrecuperable y permanece fuera de servicio.");
            return false;
        }

        if (operacionDetenidaPorSeguridad || turnoEnFaseCritica)
        {
            DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[EVE REINCORPORACIÓN BLOQUEADA]:</color> {amr.idRobot} alcanzó 80% SoC, pero el turno está en FASE CRÍTICA. Requiere autorización del supervisor en piso.");
            return false;
        }

        return true;
    }

    public void ConfirmarReincorporacionAPista(RobotAMR amr)
    {
        if (amr == null) return;

        if (amr == robotAMR_01 && (robotAMR_02 == null || !robotAMR_02.EsRecuperableEnTurno))
        {
            relevo24_7Activo = false;
        }

        string msg = $"<color=#10B981>[EVE AUTORIZACIÓN REINCORPORACIÓN]:</color> {amr.idRobot} recargado al {amr.bateriaActual:F1}% en Pad Inductivo. Autorizado para volver a operar en pista.";
        DashboardUI.Instance?.RegistrarLog(msg);
        Debug.Log(msg);

        if (admisionNuevosPedidosBloqueada && contadorFallasHardwareTurno == 0 && HayUnidadSoporteDisponible())
        {
            admisionNuevosPedidosBloqueada = false;
            motivoBloqueoAdmision = string.Empty;
            DashboardUI.Instance?.ActualizarBannerTurno($"<b><color=#38BDF8>TURNO NOCTURNO:</color> {RelojSimuladorTurno.HoraActualFormateada} (Flota Restablecida {ContarAMRsRecuperablesEnTurno()}/{ContarFlotaTotalConfigurada()}) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");
        }

        if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica && palletsEnColaWMS > 0)
        {
            DespacharColaDisponible(amr);
        }

        NotificarCambioEstadoAdmisionJSON();
    }

    public void EjecutarColapsoYRescateInmediato(RobotAMR objetivo)
    {
        if (objetivo == null || objetivo.estadoActual == RobotAMR.EstadoRobot.Apagado || objetivo.estadoActual == RobotAMR.EstadoRobot.FueraDeServicio_Danado) return;

        bool enZonaCamion = objetivo.EstaEnZonaCamionORampa();
        int camionAfectado = enZonaCamion ? DeterminarNumeroCamionPorAMR(objetivo) : 0;

        Pedido pedidoHuerfano = objetivo.ForzarApagadoCriticoBMS(out RobotAMR.PalletEnPasillo palletDepositado);
        EncolarCargaDeUnidadCaida(pedidoHuerfano, enZonaCamion ? null : palletDepositado);

        string detallePallet = (!enZonaCamion && palletDepositado != null && palletDepositado.palletGO != null)
            ? $" Estiba {(pedidoHuerfano != null ? pedidoHuerfano.id_pallet : string.Empty)} depositada en pasillo para recolección."
            : (enZonaCamion && pedidoHuerfano != null ? $" Pedido {pedidoHuerfano.id_pallet} reencolado sin reingresar a bahía clausurada." : string.Empty);

        string detalleRescate = enZonaCamion
            ? $"Varado en Muelle_Camion_0{camionAfectado} (vuelo dron restringido); bahía clausurada para despeje humano."
            : "Desplegando Dron de Mantenimiento.";

        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[RAMA A - COLAPSO <=3% BMS]:</b></color> {objetivo.idRobot} apagado ({objetivo.bateriaActual:F1}% SoC).{detallePallet} {detalleRescate}");

        if (enZonaCamion)
        {
            ModuloDespachoTMS tms = gestorTMS != null ? gestorTMS : ModuloDespachoTMS.Instance;
            tms?.ClausurarMuellePorIncidenteAMR(camionAfectado, $"Descarga <=3% de {objetivo.idRobot} en bahía/rampa", out _);
            ReevaluarMuellesEnFlotaActiva();
            GestorAudioSCADA.Instance?.ActivarAlarmaSonoraFabrica($"AMR {objetivo.idRobot} varado en Muelle 0{camionAfectado} (despeje humano requerido)");
        }

        // Bloquea VCam4_Emergencia sobre el AMR colapsado desde el segundo 0 hasta completar izaje vertical
        DirectorCinematico.Instance?.EnfocarIncidenteCritico(objetivo.transform);

        bool bloqueoPrevioBMS = admisionNuevosPedidosBloqueada;
        bool paradaActivada = RegistrarIncidenteYEvaluarReincidencia(objetivo, RobotAMR.TipoFallaAMR.DescargaCritica3);
        RobotAMR relevo = (!paradaActivada && !operacionDetenidaPorSeguridad) ? SeleccionarRelevoParaIncidente(objetivo) : null;
        if (bloqueoPrevioBMS && !paradaActivada)
        {
            string idRel = relevo != null ? relevo.idRobot : "flota viva";
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(
                $"[COLAPSO BMS <=3% — {RelojSimuladorTurno.HoraActualFormateada}]: {objetivo.idRobot} apagado. Admisión permanece cerrada; continuidad sostenida con {idRel}.");
        }

        if (!paradaActivada && !operacionDetenidaPorSeguridad)
        {
            if (relevo != null && (colaPalletsEnPasillo.Count > 0 || colaPrioritariaReasignacion.Count > 0 || palletsEnColaWMS > 0))
            {
                StartCoroutine(DespacharRelevoPendienteConHeadway(relevo));
            }
            else if (palletsEnColaWMS == 0 && ContarMisionesEnVuelo() == 0)
            {
                flotaEscaladaDual = false;
                DashboardUI.Instance?.ConfigurarReservaAMR02(false);
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
                DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
                EvaluarCierreTurnoSiCorresponde();
            }
        }

        if (!enZonaCamion && objetivo.transform.position.z <= -2.64f && dronMantenimiento != null)
        {
            Vector3 posPadDestino = ObtenerPosicionPadPorUnidad(objetivo);
            StartCoroutine(dronMantenimiento.EjecutarRescateAMRAbortado(
                objetivo,
                posPadDestino,
                onRescateCompletado: () =>
                {
                    DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[RESCATE AÉREO COMPLETADO]:</color> {objetivo.idRobot} acoplado en Pad Inductivo (Recarga inductiva 2h iniciada hasta >=80%).");
                },
                onElevacionCompletada: () =>
                {
                    if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica)
                    {
                        RobotAMR seguimientoRelevo = (relevo != null && relevo.EsRecuperableEnTurno && relevo.estadoActual != RobotAMR.EstadoRobot.Apagado)
                            ? relevo
                            : (EstaEnMisionActiva(robotAMR_03) ? robotAMR_03 : (EstaEnMisionActiva(robotAMR_02) ? robotAMR_02 : null));
                        DirectorCinematico.Instance?.LiberarFocoIncidente(seguimientoRelevo);
                    }
                },
                esTrasladoZonaTecnica: false
            ));
        }
        else if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica)
        {
            DirectorCinematico.Instance?.LiberarFocoIncidente(relevo);
        }
    }

    private IEnumerator DespacharRelevoPendienteConHeadway(RobotAMR relevo)
    {
        if (relevo != null && relevo.transform.position.z < RobotAMR.ZUmbralPortico)
        {
            while (Time.time < tiempoProximoDespachoPermitido)
            {
                yield return null;
            }
        }
        if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica && relevo != null)
        {
            DespacharColaDisponible(relevo);
        }
    }

    private RobotAMR BuscarAMRActivoOPisoParaSimulacion(bool soloEnMovimiento, bool preferirEnCorredor = false)
    {
        RobotAMR[] candidatos = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        if (preferirEnCorredor)
        {
            for (int i = 0; i < candidatos.Length; i++)
            {
                RobotAMR r = candidatos[i];
                if (r == null || !r.EsRecuperableEnTurno) continue;
                if ((r.estadoActual == RobotAMR.EstadoRobot.En_Transito || r.estadoActual == RobotAMR.EstadoRobot.Transportando) &&
                    r.transform.position.z <= -2.64f)
                {
                    return r;
                }
            }
        }

        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null || !r.EsRecuperableEnTurno) continue;
            if (r.estadoActual == RobotAMR.EstadoRobot.En_Transito || r.estadoActual == RobotAMR.EstadoRobot.Transportando)
            {
                return r;
            }
        }

        if (soloEnMovimiento)
        {
            for (int i = 0; i < candidatos.Length; i++)
            {
                RobotAMR r = candidatos[i];
                if (r != null && r.EsRecuperableEnTurno && r.estadoActual != RobotAMR.EstadoRobot.Apagado && r.estadoActual != RobotAMR.EstadoRobot.Recargando)
                {
                    return r;
                }
            }
        }

        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r != null && r.EsRecuperableEnTurno && r.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                return r;
            }
        }

        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r != null && r.EsRecuperableEnTurno && r.estadoActual != RobotAMR.EstadoRobot.Apagado)
            {
                return r;
            }
        }

        return null;
    }

    void MoverPorWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceWaypointActual];
        if (destino == null) return;

        // Vuelo matemático determinista acotado por AABB industrial (Zero-Physics)
        Vector3 siguientePos = Vector3.MoveTowards(transform.position, destino.position, velocidadVuelo * Time.deltaTime);
        siguientePos.x = Mathf.Clamp(siguientePos.x, LimiteXMin, LimiteXMax);
        siguientePos.y = Mathf.Clamp(siguientePos.y, AltitudMin, AltitudMax);
        siguientePos.z = Mathf.Clamp(siguientePos.z, LimiteZMin, LimiteZMax);
        transform.position = siguientePos;
        
        Vector3 direccion = (destino.position - transform.position).normalized;
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 3f);
        }

        if (Vector3.Distance(transform.position, destino.position) < 0.5f)
        {
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }

    public void InyectarObstaculoPasilloISO()
    {
        if (!turnoIniciado) return;

        if (obstaculoPasilloISO == null)
        {
            Debug.LogWarning("[EVE]: Referencia obstaculoPasilloISO no asignada para el Stress Test.");
            return;
        }

        if ((dronMantenimiento != null && dronMantenimiento.ocupado) || despachandoDespejeISO)
        {
            DashboardUI.Instance?.RegistrarLog("<color=#F59E0B>[STRESS TEST ISO]:</color> Maniobra de despeje aéreo en curso. Espere a que Dron_Mantenimiento finalice.");
            return;
        }

        RobotAMR amrEnfocadoVCam3 = ObtenerAMREnfocadoEnVCam3();
        if (EstaEnZonaCamionRestringida(amrEnfocadoVCam3))
        {
            EjecutarProtocoloInspeccionHumanaMuelle(amrEnfocadoVCam3);
            return;
        }

        RobotAMR amrObjetivo = ResolverAMRObjetivoObstaculo(out Vector3 posObstaculo, out Vector3 _);
        if (amrObjetivo == null)
        {
            RobotAMR amrEnMuelle = ObtenerAMREnZonaCamionRestringida();
            if (amrEnMuelle != null)
            {
                EjecutarProtocoloInspeccionHumanaMuelle(amrEnMuelle);
                return;
            }

            string msgProtegido = "Zona de muelle o maniobra protegida — espere a que un AMR circule por el corredor principal.";
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[STRESS TEST ISO]:</color> {msgProtegido}");
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA("<color=#F59E0B>ZONA PROTEGIDA ISO</color>", msgProtegido, 3.5f);
            return;
        }

        obstaculoPasilloISO.transform.SetParent(null, true);
        Collider col = obstaculoPasilloISO.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        obstaculoPasilloISO.transform.position = posObstaculo;
        obstaculoPasilloISO.transform.rotation = Quaternion.identity;
        obstaculoPasilloISO.SetActive(true);

        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[STRESS TEST]:</color> Obstaculo imprevisto detectado frente a {amrObjetivo.idRobot} en corredor principal ({posObstaculo.x:F1}, {posObstaculo.z:F1}).");
    }

    private RobotAMR ObtenerAMREnfocadoEnVCam3()
    {
        if (DirectorCinematico.Instance != null)
        {
            return DirectorCinematico.Instance.ObtenerAMREnfocadoActual();
        }
        return null;
    }

    private static bool EstaEnZonaCamionRestringida(RobotAMR amr)
    {
        if (amr == null || !amr.gameObject.activeInHierarchy) return false;
        bool enMisionMuelle = amr.estadoActual == RobotAMR.EstadoRobot.En_Transito ||
                              amr.estadoActual == RobotAMR.EstadoRobot.Transportando ||
                              amr.estadoActual == RobotAMR.EstadoRobot.Entregando;
        if (!enMisionMuelle) return false;

        Vector3 dirFrente = amr.transform.forward.sqrMagnitude > 0.001f ? amr.transform.forward.normalized : Vector3.forward;
        Vector3 posCandidata = amr.transform.position + dirFrente * 3.10f;
        return amr.transform.position.z > -3.50f || posCandidata.z > -3.50f;
    }

    private RobotAMR ObtenerAMREnZonaCamionRestringida()
    {
        RobotAMR amrFoco = ObtenerAMREnfocadoEnVCam3();
        if (EstaEnZonaCamionRestringida(amrFoco)) return amrFoco;

        RobotAMR[] candidatos = new RobotAMR[] { robotEjecutor, robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < candidatos.Length; i++)
        {
            if (EstaEnZonaCamionRestringida(candidatos[i]))
            {
                return candidatos[i];
            }
        }
        return null;
    }

    private static int DeterminarNumeroCamionPorAMR(RobotAMR amr)
    {
        if (amr != null && amr.transform.position.x > 14.0f)
        {
            float x = amr.transform.position.x;
            float d1 = Mathf.Abs(x - 22.68f);
            float d2 = Mathf.Abs(x - 37.00f);
            float d3 = Mathf.Abs(x - 51.60f);

            if (d1 <= d2 && d1 <= d3) return 1;
            if (d2 <= d3) return 2;
            return 3;
        }

        return Mathf.Clamp(amr != null ? amr.numCamionAsignado : 1, 1, 3);
    }

    public void ReevaluarMuellesEnFlotaActiva()
    {
        RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < flota.Length; i++)
        {
            if (flota[i] != null && flota[i].gameObject.activeInHierarchy)
            {
                flota[i].ReevaluarMuelleDestinoSiClausurado();
            }
        }
    }

    public bool ConfirmarDespejeHumanoMuelleDesdeHUD(int numeroCamion = 0)
    {
        ModuloDespachoTMS tms = gestorTMS != null ? gestorTMS : ModuloDespachoTMS.Instance;
        if (tms == null) return false;

        int camionObjetivo = numeroCamion;
        if (camionObjetivo <= 0)
        {
            tms.TieneAlgunMuelleClausuradoOEnInspeccion(out camionObjetivo);
        }
        if (camionObjetivo < 1 || camionObjetivo > 3) return false;

        bool despejado = tms.ConfirmarDespejeHumanoMuelle(camionObjetivo);
        if (!despejado) return false;

        RobotAMR[] flota = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR r = flota[i];
            if (r == null || !r.gameObject.activeInHierarchy) continue;
            if (!r.EstaEnZonaCamionORampa()) continue;
            if (DeterminarNumeroCamionPorAMR(r) != camionObjetivo) continue;

            r.LimpiarCargaRetenidaEnBahia();
            if (r.estadoActual == RobotAMR.EstadoRobot.FueraDeServicio_Danado)
            {
                Vector3 slotTecnico = ObtenerSiguienteSlotZonaTecnica(r);
                r.FinalizarTrasladoAZonaTecnica(slotTecnico);
                DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[DESPEJE MUELLE 0{camionObjetivo}]:</color> Chasis dañado {r.idRobot} trasladado por personal a Zona Técnica.");
            }
            else if (r.estadoActual == RobotAMR.EstadoRobot.Apagado)
            {
                Vector3 pad = ObtenerPosicionPadPorUnidad(r);
                r.FinalizarRescateEnPad(pad);
                DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[DESPEJE MUELLE 0{camionObjetivo}]:</color> {r.idRobot} retirado hacia Pad Inductivo para recarga inductiva (2h).");
            }
        }

        if (GestorAudioSCADA.Instance != null && GestorAudioSCADA.Instance.alarmaFabricaActiva &&
            !tms.TieneAlgunMuelleClausuradoOEnInspeccion(out _) && !operacionDetenidaPorSeguridad)
        {
            GestorAudioSCADA.Instance.SilenciarAlarmaFabrica();
        }

        ReevaluarMuellesEnFlotaActiva();
        if (!operacionDetenidaPorSeguridad && !turnoEnFaseCritica && palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
        return true;
    }

    private void EjecutarProtocoloInspeccionHumanaMuelle(RobotAMR amrEnCamion)
    {
        int camionAfectado = DeterminarNumeroCamionPorAMR(amrEnCamion);
        ModuloDespachoTMS tms = gestorTMS != null ? gestorTMS : ModuloDespachoTMS.Instance;
        int siguienteCamion = (camionAfectado % 3) + 1;

        if (tms != null)
        {
            tms.ClausurarMuellePorIncidenteAMR(camionAfectado, "Obstrucción / inspección humana obligatoria en bahía", out siguienteCamion);
        }

        ReevaluarMuellesEnFlotaActiva();

        string titulo = "CLAUSURA MUELLE 0" + camionAfectado + " — INTERVENCIÓN HUMANA";
        string detalle = siguienteCamion > 0
            ? $"Zona de camión bloqueada (vuelo dron restringido). Alarma sonora de fábrica activada; pedidos redirigidos a Muelle_Camion_0{siguienteCamion} hasta confirmar despeje humano."
            : "Zona de camión bloqueada y sin otros muelles abiertos. Alarma sonora de fábrica activada hasta confirmación humana en piso.";

        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[{titulo}]:</b></color> {detalle}");
        DashboardUI.Instance?.MostrarPopUpEjecutivoSLA($"<color=#EF4444>{titulo}</color>", detalle, 5.0f);
        GestorAudioSCADA.Instance?.ActivarAlarmaSonoraFabrica($"Bloqueo en Muelle_Camion_0{camionAfectado} — despeje humano requerido");
        Debug.LogWarning($"[{titulo}] {detalle}");
    }

    public RobotAMR ResolverAMRObjetivoObstaculo(out Vector3 posValida, out Vector3 dirFrente)
    {
        posValida = Vector3.zero;
        dirFrente = Vector3.forward;

        // Prioridad 1: AMR enfocado actualmente en VCam3_AMR_Hombro si está en navegación activa
        RobotAMR amrEnfocado = ObtenerAMREnfocadoEnVCam3();
        if (EsPosicionSpawnObstaculoValida(amrEnfocado, out posValida, out dirFrente))
        {
            return amrEnfocado;
        }

        // Prioridad 2: Primer AMR activo en corredor principal [robotEjecutor, robotAMR_01, robotAMR_02, robotAMR_03]
        RobotAMR[] candidatos = new RobotAMR[] { robotEjecutor, robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR candidato = candidatos[i];
            if (EsPosicionSpawnObstaculoValida(candidato, out posValida, out dirFrente))
            {
                return candidato;
            }
        }

        return null;
    }

    public static bool CumpleReglasGeometricasSpawnISO(
        Vector3 posAMR,
        Vector3 dirFrente,
        Vector3 posBaseCarga,
        out Vector3 posCandidata)
    {
        Vector3 dirNorm = dirFrente.sqrMagnitude > 0.001f ? dirFrente.normalized : Vector3.forward;
        posCandidata = posAMR + dirNorm * 3.10f;

        // Regla 1: Rechazar zona de rampas, muelles y tráilers (y sala de carga trasera)
        if (posCandidata.z > -3.50f || posAMR.z > -3.50f || posCandidata.z < RobotAMR.ZUmbralPortico)
        {
            return false;
        }

        // Regla 2: Rechazar proximidad a ASRS_Estante_Heroe y Base_Carga_Principal
        if (posCandidata.x < -16.8f)
        {
            return false;
        }

        float distHorizontalBase = Vector2.Distance(
            new Vector2(posCandidata.x, posCandidata.z),
            new Vector2(posBaseCarga.x, posBaseCarga.z)
        );
        if (distHorizontalBase < 1.80f)
        {
            return false;
        }

        return true;
    }

    private bool EsPosicionSpawnObstaculoValida(RobotAMR amr, out Vector3 posValida, out Vector3 dirFrente)
    {
        posValida = Vector3.zero;
        dirFrente = Vector3.forward;

        if (amr == null || !amr.gameObject.activeInHierarchy) return false;
        if (amr.estadoActual != RobotAMR.EstadoRobot.En_Transito && amr.estadoActual != RobotAMR.EstadoRobot.Transportando)
        {
            return false;
        }

        dirFrente = amr.transform.forward.sqrMagnitude > 0.001f ? amr.transform.forward.normalized : Vector3.forward;
        GameObject baseCargaObj = GameObject.Find("Base_Carga_Principal");
        Vector3 posBaseCarga = baseCargaObj != null ? baseCargaObj.transform.position : new Vector3(-19.50f, 0f, -0.42f);

        if (!CumpleReglasGeometricasSpawnISO(amr.transform.position, dirFrente, posBaseCarga, out Vector3 posCandidata))
        {
            return false;
        }

        // Regla 3: Validación en NavMesh y volumen físico libre (Default | AMR | Pallet)
        if (!NavMesh.SamplePosition(posCandidata, out NavMeshHit hit, 0.8f, NavMesh.AllAreas))
        {
            return false;
        }

        Collider[] solapados = Physics.OverlapBox(
            hit.position + Vector3.up * 0.45f,
            new Vector3(0.65f, 0.45f, 0.75f),
            Quaternion.identity,
            (1 << 0) | (1 << 6) | (1 << 7),
            QueryTriggerInteraction.Ignore
        );

        if (solapados != null)
        {
            for (int i = 0; i < solapados.Length; i++)
            {
                Collider c = solapados[i];
                if (c == null) continue;
                if (obstaculoPasilloISO != null && (c.gameObject == obstaculoPasilloISO || c.transform.IsChildOf(obstaculoPasilloISO.transform)))
                {
                    continue;
                }
                if (c.name.Contains("Suelo") || c.name.Contains("Floor") || c.bounds.max.y <= hit.position.y + 0.08f)
                {
                    continue;
                }
                return false;
            }
        }

        posValida = new Vector3(hit.position.x, 0.202f, hit.position.z);
        return true;
    }

    public void GestionarObstruccionAutonomaISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        if (despachandoDespejeISO) return;
        StartCoroutine(ProtocoloDespejeAutonomoISO(robotAfectado, obstaculoDetectado));
    }

    public static ResultadoHorizonteO1 CalcularProyeccionHorizonte(
        int nuevosPedidos,
        float bateriaAMR01,
        float bateriaAMR02,
        float horaTurnoActual,
        int amrsActivosFlota = 2)
    {
        const float deltaSoCPedido = 3.85f;
        const float pisoSeguridadLiFePO4 = RobotAMR.UmbralSeguridadLiFePO4; // 20.0f
        const float ventanaNominalUnidad = 60.0f; // 80.0% -> 20.0%
        const float pesoEstibaEstandarKg = 850.0f;

        float energiaRequerida = nuevosPedidos * deltaSoCPedido; // 192.5% para 50 pedidos
        float energiaDisponible = Mathf.Max(0f, bateriaAMR01 - pisoSeguridadLiFePO4) +
                                  Mathf.Max(0f, bateriaAMR02 - pisoSeguridadLiFePO4);

        float deficitSoC = Mathf.Max(0f, energiaRequerida - energiaDisponible);
        float pesoLoteKg = nuevosPedidos * pesoEstibaEstandarKg; // 42,500 kg

        const float duracionTurnoHoras = 8.0f;
        const float tiempoCicloSimSegundos = 4.50f;
        float tiempoCicloHorasReales = (tiempoCicloSimSegundos * RobotAMR.FactorCompresionTurno) / 3600f; // 0.20 h/pedido
        float tasaAbsorcionPorAMR = 1f / tiempoCicloHorasReales; // 5.0 ped/h
        int amrsVivos = Mathf.Clamp(amrsActivosFlota, 0, 3);
        float tasaAbsorcionFlotaActual = amrsVivos * tasaAbsorcionPorAMR; // 10.0 ped/h para 2 AMRs

        float horasRestantes = Mathf.Max(0.5f, duracionTurnoHoras - horaTurnoActual);
        int pedidosAbsorbibles = Mathf.Clamp(Mathf.FloorToInt(horasRestantes * tasaAbsorcionFlotaActual), 0, nuevosPedidos);
        int pedidosRezagados = Mathf.Max(0, nuevosPedidos - pedidosAbsorbibles);
        float porcentajeCarryOver = nuevosPedidos > 0 ? (pedidosRezagados / (float)nuevosPedidos) * 100f : 0f;

        EscenarioHorizonte escenario;
        int amrRequeridos;
        string etiquetaEscenario;

        if (horaTurnoActual <= 3.0f)
        {
            escenario = EscenarioHorizonte.EscenarioA_InicioTurno;
            amrRequeridos = 3; // 2 activos + 1 relevo en carga inductiva 2h
            porcentajeCarryOver = 0f;
            etiquetaEscenario = "ESCENARIO A (INICIO TURNO)";
        }
        else if (horaTurnoActual <= 6.0f)
        {
            escenario = EscenarioHorizonte.EscenarioB_Madrugada;
            amrRequeridos = Mathf.Max(4, Mathf.CeilToInt(energiaRequerida / ventanaNominalUnidad));
            etiquetaEscenario = "ESCENARIO B (MADRUGADA)";
        }
        else
        {
            escenario = EscenarioHorizonte.EscenarioC_FinalTurno;
            amrRequeridos = Mathf.CeilToInt(energiaRequerida / ventanaNominalUnidad); // 4 unidades
            etiquetaEscenario = "ESCENARIO C (FINAL TURNO)";
        }

        return new ResultadoHorizonteO1
        {
            Escenario = escenario,
            HoraTurnoActual = horaTurnoActual,
            HorasRestantes = horasRestantes,
            NuevosPedidos = nuevosPedidos,
            EnergiaRequeridaSoC = energiaRequerida,
            EnergiaDisponibleSoC = energiaDisponible,
            DeficitSoC = deficitSoC,
            PesoLoteKg = pesoLoteKg,
            PedidosAbsorbibles = pedidosAbsorbibles,
            PedidosRezagados = pedidosRezagados,
            PorcentajeCarryOver = porcentajeCarryOver,
            AMRRequeridos = amrRequeridos,
            EtiquetaEscenario = etiquetaEscenario
        };
    }

    public void ProyectarEstresDeFlota(int nuevosPedidos = 50)
    {
        if (!turnoIniciado) return;
        if (alertaDemandaEnCurso) return;
        alertaDemandaEnCurso = true;

        float bateriaAMR01 = (robotAMR_01 != null && robotAMR_01.EsRecuperableEnTurno) ? robotAMR_01.bateriaActual : 20.0f;
        float bateriaAMR02 = (robotAMR_02 != null && robotAMR_02.EsRecuperableEnTurno) ? robotAMR_02.bateriaActual : 20.0f;
        ObtenerEstadoFlotaReal(out int amrsActivosReales, out _);
        float horaConsulta = RelojSimuladorTurno.HorasTranscurridas;
        string relojTexto = RelojSimuladorTurno.HoraActualFormateada;

        ResultadoHorizonteO1 r = CalcularProyeccionHorizonte(nuevosPedidos, bateriaAMR01, bateriaAMR02, horaConsulta, amrsActivosReales);

        Debug.Log($"<color=#F59E0B>[ANALIZADOR HORIZONTE O(1) - {r.EtiquetaEscenario}]:</color> Reloj {relojTexto} (Restan {r.HorasRestantes:F1}h). Lote: {r.NuevosPedidos} ped ({r.PesoLoteKg:F0} kg). Déficit LiFePO4: -{r.DeficitSoC:F1}% SoC | Carry-over SLA: {r.PorcentajeCarryOver:F0}% | Recomendación: {r.AMRRequeridos} AMRs.");
        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>[WMS HORIZONTE - {r.EtiquetaEscenario}]:</b></color> Ráfaga de {r.NuevosPedidos} pedidos ({r.PesoLoteKg:F0} kg) a las {relojTexto} (Restan {RelojSimuladorTurno.FormatearDuracionHMS(r.HorasRestantes)} hasta {RelojSimuladorTurno.HoraCierreTurnoTexto}).");
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[DICTAMEN O(1)]:</b></color> Déficit LiFePO4: -{r.DeficitSoC:F1}% SoC | Rezago SLA (Carry-over): {r.PorcentajeCarryOver:F0}%. Flota recomendada: {r.AMRRequeridos} AMRs.");

        DashboardUI.Instance?.MostrarAlertaIncertidumbreDemanda(
            r.Escenario, r.HoraTurnoActual, r.HorasRestantes, r.NuevosPedidos,
            r.DeficitSoC, r.AMRRequeridos, r.PorcentajeCarryOver, r.PesoLoteKg
        );
    }

    public void NotificarResolucionAlertaDemanda()
    {
        alertaDemandaEnCurso = false;
    }

    public void IngestarManifiestoDesdeSO()
    {
        if (!turnoIniciado)
        {
            Debug.LogWarning($"[EVE Ingesta JSON]: IngestarManifiestoDesdeSO ignorado porque turnoIniciado == false en '{gameObject.name}' (InstanceID={GetInstanceID()}).");
            return;
        }

        if (!PuedeRecibirNuevosPedidos(out _, out string motivoBloqueo))
        {
            GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
            DashboardUI.Instance?.RegistrarLog("<color=#EF4444>[INGESTA BLOQUEADA]:</color> Sin capacidad/admisión bloqueada para nuevos pedidos. Detalle en Buzón.");
            DashboardUI.Instance?.MostrarPopUpExcedeCapacidadActual(0, 0, "Admisión bloqueada");
            if (!string.IsNullOrEmpty(motivoBloqueo))
            {
                DashboardUI.Instance?.RegistrarFeedbackSupervisor($"[BLOQUEO DE INGESTA JSON — {RelojSimuladorTurno.HoraActualFormateada}]: {motivoBloqueo}");
            }
            NotificarCambioEstadoAdmisionJSON();
            return;
        }

        string dirInicial = SelectorArchivosWindows.ResolverDirectorioInicialBandejaERP();

#if UNITY_EDITOR
        string rutaSeleccionada = UnityEditor.EditorUtility.OpenFilePanel("Cargar Lote ERP", dirInicial, "json");
        if (!string.IsNullOrEmpty(rutaSeleccionada))
        {
            ProcesarArchivoManifiesto(rutaSeleccionada);
        }
#elif UNITY_STANDALONE_LINUX
        StartCoroutine(EjecutarSelectorArchivoSOAsync(() =>
        {
            string archivoInicial = dirInicial.EndsWith("/") ? dirInicial : dirInicial + "/";
            var psi = new ProcessStartInfo
            {
                FileName = "zenity",
                Arguments = $"--file-selection --title=\"Cargar Lote ERP\" --filename=\"{archivoInicial}\" --file-filter=\"Archivos JSON (*.json) | *.json\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process proc = Process.Start(psi))
            {
                if (proc == null) return string.Empty;
                string salida = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return proc.ExitCode == 0 ? salida.Trim() : string.Empty;
            }
        }));
#elif UNITY_STANDALONE_WIN
        string dirWindows = SelectorArchivosWindows.NormalizarRutaWindows(dirInicial);
        string rutaSeleccionada = SelectorArchivosWindows.AbrirDialogoJson("Cargar Lote ERP", dirWindows);
        if (!string.IsNullOrEmpty(rutaSeleccionada))
        {
            ProcesarArchivoManifiesto(rutaSeleccionada);
        }
#else
        DashboardUI.Instance?.MostrarPopUpRechazo("Selector de archivos no soportado en esta plataforma.");
#endif
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_LINUX
    private IEnumerator EjecutarSelectorArchivoSOAsync(Func<string> accionSelector)
    {
        Task<string> tarea = Task.Run(() =>
        {
            try
            {
                return accionSelector();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EVE Selector SO]: {ex.Message}");
                return string.Empty;
            }
        });

        while (!tarea.IsCompleted)
        {
            yield return null;
        }

        string rutaSeleccionada = tarea.Result;
        if (!string.IsNullOrEmpty(rutaSeleccionada))
        {
            ProcesarArchivoManifiesto(rutaSeleccionada);
        }
    }
#endif

    public static bool TryParsearYValidarManifiesto(
        string json,
        List<Pedido> pedidosFallback,
        out Manifiesto loteValido,
        out string motivoError)
    {
        loteValido = null;
        motivoError = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            motivoError = "Esquema inválido";
            return false;
        }

        string jsonLimpio = json.TrimStart('\uFEFF').Trim();
        if (string.IsNullOrWhiteSpace(jsonLimpio))
        {
            motivoError = "Esquema inválido";
            return false;
        }

        bool eraArregloRaiz = jsonLimpio.StartsWith("[");
        if (eraArregloRaiz)
        {
            if (!jsonLimpio.EndsWith("]"))
            {
                motivoError = "JSON mal formado: arreglo raíz sin cierre ']'";
                return false;
            }
            jsonLimpio = "{\"pedidos\":" + jsonLimpio + "}";
        }

        try
        {
            Manifiesto lote = JsonUtility.FromJson<Manifiesto>(jsonLimpio);
            bool sinPedidos = lote == null || lote.pedidos == null || lote.pedidos.Count == 0;
            if (lote == null || (sinPedidos && lote.total_pallets <= 0))
            {
                motivoError = "Esquema inválido";
                return false;
            }

            if (sinPedidos)
            {
                if (pedidosFallback != null && pedidosFallback.Count > 0)
                {
                    lote.pedidos = new List<Pedido>(pedidosFallback);
                }
                else
                {
                    motivoError = "Esquema inválido";
                    return false;
                }
            }
            else
            {
                bool tieneCampoValido = false;
                for (int i = 0; i < lote.pedidos.Count; i++)
                {
                    if (lote.pedidos[i] != null && !string.IsNullOrEmpty(lote.pedidos[i].id_pallet))
                    {
                        tieneCampoValido = true;
                        break;
                    }
                }
                if (!tieneCampoValido && lote.total_pallets <= 0)
                {
                    motivoError = "Esquema inválido";
                    return false;
                }

                if (eraArregloRaiz && lote.total_pallets <= 0 && tieneCampoValido)
                {
                    lote.total_pallets = lote.pedidos.Count;
                }
            }

            loteValido = lote;
            return true;
        }
        catch (Exception ex)
        {
            motivoError = "JSON mal formado: " + ex.Message;
            return false;
        }
    }

    public void ProcesarArchivoManifiesto(string rutaArchivo)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                const string msgRutaVacia = "Ruta inválida: no se especificó ningún archivo JSON.";
                Debug.LogWarning($"[EVE Ingesta JSON]: {msgRutaVacia}");
                GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
                DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[INGESTA ERP]:</color> {msgRutaVacia}");
                DashboardUI.Instance?.MostrarPopUpRechazo(msgRutaVacia);
                return;
            }

            if (!File.Exists(rutaArchivo))
            {
                string msgNoExiste = $"Ruta inválida: archivo no encontrado ({Path.GetFileName(rutaArchivo)}).";
                Debug.LogWarning($"[EVE Ingesta JSON]: {msgNoExiste} Ruta='{rutaArchivo}'");
                GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
                DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[INGESTA ERP]:</color> {msgNoExiste}");
                DashboardUI.Instance?.MostrarPopUpRechazo(msgNoExiste);
                return;
            }

            string json;
            using (var reader = new StreamReader(rutaArchivo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                json = reader.ReadToEnd();
            }
            json = string.IsNullOrEmpty(json) ? string.Empty : json.TrimStart('\uFEFF').Trim();

            List<Pedido> fallback = (plantillaPedidosFisicos != null && plantillaPedidosFisicos.Count > 0)
                ? plantillaPedidosFisicos
                : (manifiestoActual != null ? manifiestoActual.pedidos : null);

            if (!TryParsearYValidarManifiesto(json, fallback, out Manifiesto lote, out string motivoError))
            {
                string diagnostico = !string.IsNullOrEmpty(motivoError) ? motivoError : "Esquema inválido";
                string etiquetaPopup = diagnostico.StartsWith("JSON mal formado", StringComparison.OrdinalIgnoreCase)
                    ? "JSON mal formado"
                    : "Esquema inválido";

                Debug.LogWarning($"[EVE Ingesta JSON]: Error procesando '{rutaArchivo}': {diagnostico}");
                GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
                DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[INGESTA ERP]:</color> Rechazo de '{Path.GetFileName(rutaArchivo)}' — {diagnostico}");
                DashboardUI.Instance?.MostrarPopUpRechazo(etiquetaPopup);
                return;
            }

            EvaluarIngestaAutonoma(lote);
        }
        catch (Exception ex)
        {
            string detalleEx = $"Error leyendo archivo: {ex.Message}";
            Debug.LogWarning($"[EVE Ingesta JSON]: Error procesando '{rutaArchivo}': {detalleEx}");
            GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
            DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[INGESTA ERP]:</color> {detalleEx}");
            DashboardUI.Instance?.MostrarPopUpRechazo("Ruta inválida");
        }
    }

    public static ResultadoEvaluacionSLA CalcularEvaluacionSLA(
        int palletsEnColaActual,
        int nuevosPallets,
        float horaTurnoActual,
        bool flotaEscaladaDualActual = false,
        int amrsOperativosDisponibles = 2,
        bool bloqueadoPorFaltaSoporte = false,
        string motivoBloqueoSoporte = "")
    {
        float horasRestantes = RelojSimuladorTurno.CalcularHorasRestantesDesde(horaTurnoActual);
        string horaFormateada = RelojSimuladorTurno.FormatearHoraRelojNocturno(horaTurnoActual);
        int cuposPisoViva = Mathf.Clamp(amrsOperativosDisponibles, 0, 2);

        float capacidad1AMR = (cuposPisoViva >= 1 ? 40f : 0f) * horasRestantes;
        float capacidad2AMRs = cuposPisoViva >= 2 ? (80f * horasRestantes) : capacidad1AMR;
        int cupoDisponible = (bloqueadoPorFaltaSoporte || cuposPisoViva == 0)
            ? 0
            : Mathf.Max(0, Mathf.FloorToInt(capacidad2AMRs) - palletsEnColaActual);

        if (nuevosPallets <= 0)
        {
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Invalido,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = palletsEnColaActual,
                CupoDisponibleActual = cupoDisponible,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = 0,
                ETAHoras = 0f,
                FeedbackTecnico = string.Empty
            };
        }

        int proyeccionCola = palletsEnColaActual + nuevosPallets;

        if (bloqueadoPorFaltaSoporte || cuposPisoViva == 0)
        {
            string causa = !string.IsNullOrEmpty(motivoBloqueoSoporte)
                ? motivoBloqueoSoporte
                : "Unidad de soporte/reserva perdida o flota degradada en turno";
            string feedbackBloqueo = $"[BLOQUEO DE ADMISIÓN — MODO CONTINUIDAD] Lote rechazado (+{nuevosPallets} pal | Cola activa: {palletsEnColaActual} pal).\n" +
                                     $"Cupo máximo disponible actual: 0 pallets.\n" +
                                     $"Política de tiempo: Reloj oficial {horaFormateada} (restan {RelojSimuladorTurno.FormatearDuracionHMS(horasRestantes)} hasta {RelojSimuladorTurno.HoraCierreTurnoTexto} | recarga inductiva 2h).\n" +
                                     $"Política de peso: Rango industrial 850–1250 kg (base {Pedido.PesoReferenciaBaseKg:F0} kg/pallet).\n" +
                                     $"Causa: {causa}.\n" +
                                     $"Flota viva concluye cola asignada sin admitir nuevos pedidos.";
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Rechazado,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
                CupoDisponibleActual = 0,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = 3,
                ETAHoras = 0f,
                FeedbackTecnico = feedbackBloqueo
            };
        }

        if (proyeccionCola <= capacidad1AMR)
        {
            float tasaActual = (flotaEscaladaDualActual && cuposPisoViva >= 2) ? 80f : 40f;
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Aprobado1AMR,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
                CupoDisponibleActual = cupoDisponible,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = (flotaEscaladaDualActual && cuposPisoViva >= 2) ? 2 : 1,
                ETAHoras = proyeccionCola / tasaActual,
                FeedbackTecnico = string.Empty
            };
        }
        else if (cuposPisoViva >= 2 && proyeccionCola <= capacidad2AMRs)
        {
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.EscaladoDual2AMRs,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
                CupoDisponibleActual = cupoDisponible,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = 2,
                ETAHoras = proyeccionCola / 80f,
                FeedbackTecnico = string.Empty
            };
        }
        else
        {
            float capacidadUnitariaReal = 40f * Mathf.Max(0.1f, horasRestantes);
            int amrRequeridos = Mathf.Max(cuposPisoViva + 1, Mathf.CeilToInt(proyeccionCola / capacidadUnitariaReal));
            float cargaNominalToneladas = (nuevosPallets * Pedido.PesoReferenciaBaseKg) / 1000f;
            float cargaMinToneladas = (nuevosPallets * Pedido.PesoMinimoIndustrialKg) / 1000f;
            float cargaMaxToneladas = (nuevosPallets * Pedido.PesoMaximoIndustrialKg) / 1000f;
            string feedback = $"[REPORTE SLA — EXCEDE CAPACIDAD ACTUAL] Lote rechazado (+{nuevosPallets} pal | Cola actual: {palletsEnColaActual} pal | Proyección: {proyeccionCola} pal).\n" +
                              $"Cupo máximo disponible actual: {cupoDisponible} pallets.\n" +
                              $"Política de tiempo: Reloj oficial {horaFormateada} (restan {RelojSimuladorTurno.FormatearDuracionHMS(horasRestantes)} hasta {RelojSimuladorTurno.HoraCierreTurnoTexto} | techo operativo: {capacidad2AMRs:F0} pallets | recarga inductiva 2h).\n" +
                              $"Política de peso: Rango industrial 850–1250 kg (base {Pedido.PesoReferenciaBaseKg:F0} kg/pallet | lote ≈ {cargaNominalToneladas:F1} t, rango {cargaMinToneladas:F1}–{cargaMaxToneladas:F1} t).\n" +
                              $"Flota requerida: {amrRequeridos} AMRs en piso + 1 AMR soporte ({amrRequeridos + 1} totales).";
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Rechazado,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
                CupoDisponibleActual = cupoDisponible,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = amrRequeridos,
                ETAHoras = 0f,
                FeedbackTecnico = feedback
            };
        }
    }

    public int CalcularCupoMaximoDisponibleActual(
        out float capacidadTechoTotal,
        out int amrsPisoDisponibles,
        out bool bloqueoAdmision,
        out string motivoBloqueo)
    {
        ObtenerEstadoFlotaReal(out amrsPisoDisponibles, out _);
        float horasRestantes = RelojSimuladorTurno.HorasRestantesHastaCierre;
        int cuposPisoViva = Mathf.Clamp(amrsPisoDisponibles, 0, 2);
        capacidadTechoTotal = cuposPisoViva >= 2 ? (80f * horasRestantes) : ((cuposPisoViva >= 1 ? 40f : 0f) * horasRestantes);

        bool bloqueoSoporte = admisionNuevosPedidosBloqueada ||
                              (TieneFlotaAsignadaEnEscena() && TotalIncidentesCriticosTurno > 0 &&
                               (contadorFallasHardwareTurno > 0 || !HayUnidadSoporteDisponible()));

        if (turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica)
        {
            bloqueoAdmision = true;
            motivoBloqueo = turnoCerrado && !string.IsNullOrEmpty(motivoCierreTurno)
                ? motivoCierreTurno
                : (!string.IsNullOrEmpty(motivoFaseCritica) ? motivoFaseCritica : "Cierre total de turno (0 AMRs operativos).");
            return 0;
        }

        if (bloqueoSoporte || cuposPisoViva == 0)
        {
            bloqueoAdmision = true;
            motivoBloqueo = !string.IsNullOrEmpty(motivoBloqueoAdmision) ? motivoBloqueoAdmision : "Soporte/reserva perdido o flota degradada.";
            return 0;
        }

        int cupo = Mathf.Max(0, Mathf.FloorToInt(capacidadTechoTotal) - palletsEnColaWMS);
        if (RelojSimuladorTurno.TurnoSuperoCierre || cupo <= 0)
        {
            bloqueoAdmision = true;
            motivoBloqueo = "Capacidad horaria del turno agotada (cupo disponible: 0 pallets).";
            return 0;
        }

        bloqueoAdmision = false;
        motivoBloqueo = string.Empty;
        return cupo;
    }

    public bool PuedeRecibirNuevosPedidos(out int cupoMaximoDisponible, out string motivoBloqueo)
    {
        cupoMaximoDisponible = CalcularCupoMaximoDisponibleActual(out _, out _, out bool bloqueo, out motivoBloqueo);
        return !bloqueo && cupoMaximoDisponible > 0;
    }

    public static float CalcularTiempoCicloReferenciaSeg(float pesoKg)
    {
        float pesoAplicado = pesoKg > 0f ? pesoKg : Pedido.PesoReferenciaBaseKg;
        float vIda = VelocidadNominalFlota;
        float vCarga = Mathf.Max(0.35f, vIda - (pesoAplicado * RobotAMR.CoeficientePesoNominal));
        return (DistanciaCicloReferenciaTramoM / vIda) + (DistanciaCicloReferenciaTramoM / vCarga) + TiempoManiobraReferenciaSeg;
    }

    public static float CalcularFactorMasaCiclo(float pesoKg, bool modoDegradacion = false)
    {
        float cicloBase = CalcularTiempoCicloReferenciaSeg(Pedido.PesoReferenciaBaseKg);
        float cicloActual = CalcularTiempoCicloReferenciaSeg(pesoKg);
        float factor = cicloActual > 0f ? Mathf.Min(1.0f, cicloBase / cicloActual) : 1.0f;
        if (modoDegradacion)
        {
            factor *= 0.50f;
        }
        return Mathf.Clamp01(factor);
    }

    public static float CalcularFactorEnergiaLiFePO4(float bateriaPct, RobotAMR.EstadoRobot estado = RobotAMR.EstadoRobot.Inactivo)
    {
        if (estado == RobotAMR.EstadoRobot.Apagado ||
            estado == RobotAMR.EstadoRobot.FueraDeServicio_Danado ||
            estado == RobotAMR.EstadoRobot.Interrumpido)
        {
            return 0f;
        }

        float u = Mathf.Clamp01((bateriaPct - RobotAMR.UmbralSeguridadLiFePO4) / RobotAMR.VentanaRecargaNominalPct);
        return Mathf.Clamp(0.70f + (0.60f * u), 0.50f, 1.00f);
    }

    public static float CalcularFactorHolguraTemporalSLA(int palletsCola, float horasRestantes, int amrsActivos = 1)
    {
        if (palletsCola <= 0) return 1.0f;
        int n = Mathf.Clamp(amrsActivos, 1, 2);
        float capacidadNominal = TasaNominalPalletHora1AMR * n * Mathf.Max(0f, horasRestantes);
        return capacidadNominal <= 0f ? 0f : Mathf.Min(1.0f, capacidadNominal / palletsCola);
    }

    public static float CalcularEficienciaUnitariaAMR(
        float pesoKg,
        float bateriaPct,
        int palletsCola,
        float horasRestantes,
        int amrsActivos = 1,
        RobotAMR.EstadoRobot estado = RobotAMR.EstadoRobot.Inactivo,
        bool modoDegradacion = false)
    {
        float eficiencia = 100.0f
            * CalcularFactorMasaCiclo(pesoKg, modoDegradacion)
            * CalcularFactorEnergiaLiFePO4(bateriaPct, estado)
            * CalcularFactorHolguraTemporalSLA(palletsCola, horasRestantes, amrsActivos);
        return Mathf.Clamp(eficiencia, 0.0f, 100.0f);
    }

    public static float CalcularEficienciaPromedioDual(float eficienciaAMR01, float eficienciaAMR02)
    {
        return (eficienciaAMR01 + eficienciaAMR02) * 0.5f;
    }

    private static float CalcularPesoEvaluacionLote(Manifiesto lote)
    {
        if (lote == null || lote.pedidos == null || lote.pedidos.Count == 0)
        {
            return Pedido.PesoReferenciaBaseKg;
        }

        float suma = 0f;
        int validos = 0;
        bool todosPesados = true;
        for (int i = 0; i < lote.pedidos.Count; i++)
        {
            Pedido p = lote.pedidos[i];
            if (p == null || p.peso_kg <= 0f) continue;
            suma += p.peso_kg;
            validos++;
            if (p.peso_kg < 1150f)
            {
                todosPesados = false;
            }
        }

        if (validos == 0) return Pedido.PesoReferenciaBaseKg;
        return todosPesados ? (suma / validos) : Pedido.PesoReferenciaBaseKg;
    }

    public void EvaluarEficienciaOperativaYEscalamiento(Pedido pedidoRecienAsignado = null, bool permitirDespachoInmediato = true)
    {
        float horasRestantes = RelojSimuladorTurno.HorasRestantesHastaCierre;

        float peso1 = (robotAMR_01 != null && robotAMR_01.PedidoActual != null && robotAMR_01.PedidoActual.peso_kg > 0f)
            ? robotAMR_01.PedidoActual.peso_kg
            : ((pedidoRecienAsignado != null && pedidoRecienAsignado.peso_kg > 0f)
                ? pedidoRecienAsignado.peso_kg
                : Pedido.PesoReferenciaBaseKg);
        float soc1 = robotAMR_01 != null ? robotAMR_01.bateriaActual : 80.0f;
        RobotAMR.EstadoRobot est1 = robotAMR_01 != null ? robotAMR_01.estadoActual : RobotAMR.EstadoRobot.Inactivo;
        bool deg1 = robotAMR_01 != null && robotAMR_01.EnPotenciaReducidaBateria;

        float peso2 = (robotAMR_02 != null && robotAMR_02.PedidoActual != null && robotAMR_02.PedidoActual.peso_kg > 0f)
            ? robotAMR_02.PedidoActual.peso_kg
            : ((pedidoRecienAsignado != null && pedidoRecienAsignado.peso_kg >= 1150f)
                ? pedidoRecienAsignado.peso_kg
                : Pedido.PesoReferenciaBaseKg);
        float soc2 = robotAMR_02 != null ? robotAMR_02.bateriaActual : 80.0f;
        RobotAMR.EstadoRobot est2 = robotAMR_02 != null ? robotAMR_02.estadoActual : RobotAMR.EstadoRobot.Inactivo;
        bool deg2 = robotAMR_02 != null && robotAMR_02.EnPotenciaReducidaBateria;

        float eta1Single = CalcularEficienciaUnitariaAMR(peso1, soc1, palletsEnColaWMS, horasRestantes, 1, est1, deg1);
        ObtenerEstadoFlotaReal(out int amrsPisoDisponibles, out _);

        if (!flotaEscaladaDual && !operacionDetenidaPorSeguridad && !turnoEnFaseCritica &&
            palletsEnColaWMS > 0 && amrsPisoDisponibles >= 2 && eta1Single < UmbralEficienciaSegundoAMR)
        {
            flotaEscaladaDual = true;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#38BDF8>SOPORTE / RELEVO 24/7 (80% SoC)</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[EFICIENCIA 1 AMR]:</color> η₁ = {eta1Single:F1}% (<{UmbralEficienciaSegundoAMR:F0}%). Activando AMR-02 en apoyo de piso.");

            if (permitirDespachoInmediato && turnoIniciado && palletsEnColaWMS > ContarMisionesEnVuelo())
            {
                DespacharColaDisponible(robotAMR_02);
            }
        }

        if (flotaEscaladaDual)
        {
            EficienciaUnitariaAMR01 = CalcularEficienciaUnitariaAMR(peso1, soc1, palletsEnColaWMS, horasRestantes, 2, est1, deg1);
            EficienciaUnitariaAMR02 = CalcularEficienciaUnitariaAMR(peso2, soc2, palletsEnColaWMS, horasRestantes, 2, est2, deg2);
            EficienciaPromedioDual = CalcularEficienciaPromedioDual(EficienciaUnitariaAMR01, EficienciaUnitariaAMR02);

            if (palletsEnColaWMS > 0 && EficienciaPromedioDual < UmbralEficienciaDualBuzon)
            {
                AlertaEficienciaDualActiva = true;
                string relojTexto = RelojSimuladorTurno.HoraActualFormateada;
                UltimoDiagnosticoEficienciaDual =
                    $"[EFICIENCIA DUAL BAJA — {relojTexto}] Promedio conjunto 2 AMRs: {EficienciaPromedioDual:F1}% (umbral < {UmbralEficienciaDualBuzon:F1}%).\n" +
                    $"AMR-01: {EficienciaUnitariaAMR01:F1}% ({peso1:F0} kg | {soc1:F1}% SoC) | AMR-02: {EficienciaUnitariaAMR02:F1}% ({peso2:F0} kg | {soc2:F1}% SoC).\n" +
                    $"Cola activa: {palletsEnColaWMS} pal | Restan {RelojSimuladorTurno.FormatearDuracionHMS(horasRestantes)} hasta {RelojSimuladorTurno.HoraCierreTurnoTexto}.\n" +
                    "Acción: Operación continúa normalmente con la flota activa en piso manteniendo a AMR-03 en soporte/relevo.";

                if (!alertaEficienciaDualReportadaEnEpisodio)
                {
                    alertaEficienciaDualReportadaEnEpisodio = true;
                    DashboardUI.Instance?.RegistrarFeedbackSupervisor(UltimoDiagnosticoEficienciaDual);
                    DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B>[EFICIENCIA DUAL]:</color> Promedio 2 AMRs en {EficienciaPromedioDual:F1}% (<{UmbralEficienciaDualBuzon:F0}%). Diagnóstico enviado al Buzón; operación continúa.");
                }
            }
            else if (EficienciaPromedioDual >= UmbralEficienciaDualBuzon + 3.0f || palletsEnColaWMS == 0)
            {
                AlertaEficienciaDualActiva = false;
                alertaEficienciaDualReportadaEnEpisodio = false;
            }
        }
        else
        {
            EficienciaUnitariaAMR01 = eta1Single;
            EficienciaUnitariaAMR02 = CalcularEficienciaUnitariaAMR(Pedido.PesoReferenciaBaseKg, soc2, 0, horasRestantes, 1, est2, deg2);
            EficienciaPromedioDual = EficienciaUnitariaAMR01;
            AlertaEficienciaDualActiva = false;
            alertaEficienciaDualReportadaEnEpisodio = false;
        }
    }

    public void EvaluarIngestaAutonoma(Manifiesto nuevoLote)
    {
        if (nuevoLote == null)
        {
            UltimaEvaluacionSLA = default;
            return;
        }

        if (turnoCerrado || operacionDetenidaPorSeguridad || turnoEnFaseCritica)
        {
            GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
            DashboardUI.Instance?.RegistrarLog("<color=#EF4444>[INGESTA BLOQUEADA]:</color> Turno cerrado o detenido. Pulse Reiniciar Turno para operar nuevamente.");
            DashboardUI.Instance?.MostrarPopUpExcedeCapacidadActual(0, 0, "Admisión bloqueada");
            NotificarCambioEstadoAdmisionJSON();
            return;
        }

        if ((plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) &&
            nuevoLote.pedidos != null && nuevoLote.pedidos.Count > 0)
        {
            plantillaPedidosFisicos = new List<Pedido>(nuevoLote.pedidos);
        }

        int nuevosPallets = (nuevoLote.total_pallets > 0)
            ? nuevoLote.total_pallets
            : (nuevoLote.pedidos != null ? nuevoLote.pedidos.Count : 0);

        ObtenerEstadoFlotaReal(out int amrsPisoDisponibles, out bool tieneSoporteDedicado);
        bool bloqueoSoporte = admisionNuevosPedidosBloqueada ||
                              (TieneFlotaAsignadaEnEscena() && TotalIncidentesCriticosTurno > 0 &&
                               (contadorFallasHardwareTurno > 0 || !HayUnidadSoporteDisponible()));
        float horasRestantesReloj = RelojSimuladorTurno.HorasRestantesHastaCierre;
        float horaOperativaConsulta = RelojSimuladorTurno.DuracionOperativaTurnoHoras - horasRestantesReloj;
        string relojTexto = RelojSimuladorTurno.HoraActualFormateada;

        ResultadoEvaluacionSLA eval = CalcularEvaluacionSLA(
            palletsEnColaWMS,
            nuevosPallets,
            horaOperativaConsulta,
            flotaEscaladaDual,
            amrsPisoDisponibles,
            bloqueoSoporte,
            motivoBloqueoAdmision);

        float pesoEval = CalcularPesoEvaluacionLote(nuevoLote);
        float soc1 = robotAMR_01 != null ? robotAMR_01.bateriaActual : 80.0f;
        RobotAMR.EstadoRobot est1 = robotAMR_01 != null ? robotAMR_01.estadoActual : RobotAMR.EstadoRobot.Inactivo;
        bool deg1 = robotAMR_01 != null && robotAMR_01.EnPotenciaReducidaBateria;

        if (eval.Decision == DecisionAdmisionSLA.Aprobado1AMR && amrsPisoDisponibles >= 2)
        {
            float eta1Lote = CalcularEficienciaUnitariaAMR(pesoEval, soc1, eval.ProyeccionCola, eval.HorasRestantes, 1, est1, deg1);
            if (eta1Lote < UmbralEficienciaSegundoAMR)
            {
                eval.Decision = DecisionAdmisionSLA.EscaladoDual2AMRs;
                eval.AMRRequeridos = 2;
                eval.ETAHoras = eval.ProyeccionCola / 80f;
            }
        }

        UltimaEvaluacionSLA = eval;

        if (eval.Decision == DecisionAdmisionSLA.Invalido) return;

        if (eval.Decision == DecisionAdmisionSLA.Aprobado1AMR)
        {
            palletsEnColaWMS = eval.ProyeccionCola;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * Pedido.PesoReferenciaBaseKg,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

            if (!flotaEscaladaDual)
            {
                DashboardUI.Instance?.ConfigurarReservaAMR02(tieneSoporteDedicado);
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            }

            if (sesionOperativaIniciada || turnoIniciado)
            {
                turnoIniciado = true;
                DespacharColaDisponible();
            }

            EvaluarEficienciaOperativaYEscalamiento(null, permitirDespachoInmediato: false);

            string detalleSoporte = tieneSoporteDedicado ? "Soporte: AMR-02 (Reserva 80%)" : "Flota activa: 2 AMRs en piso";
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#10B981>APROBADO (1 AMR)</color>",
                $"Lote: +{nuevosPallets} | Cola: {eval.ProyeccionCola} pal (SLA: 40 pal/h | {relojTexto}, {RelojSimuladorTurno.FormatearDuracionHMS(eval.HorasRestantes)} rest.)\nPiso: AMR-01 | {detalleSoporte} | ETA: {eval.ETAHoras:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[SLA ADMISIÓN]:</color> Aprobado +{nuevosPallets} pallets a las {relojTexto} (Cola WMS: {eval.ProyeccionCola}). Operación 1 AMR (ETA: {eval.ETAHoras:F2}h).");
            GestorAudioSCADA.Instance?.ReproducirIngresoPermitidoJSON();
            NotificarCambioEstadoAdmisionJSON();
        }
        else if (eval.Decision == DecisionAdmisionSLA.EscaladoDual2AMRs)
        {
            palletsEnColaWMS = eval.ProyeccionCola;
            flotaEscaladaDual = true;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * Pedido.PesoReferenciaBaseKg,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(tieneSoporteDedicado, "<color=#38BDF8>SOPORTE / RELEVO 24/7 (80% SoC)</color>");

            if (sesionOperativaIniciada || turnoIniciado)
            {
                turnoIniciado = true;
                DespacharColaDisponible();
            }

            Pedido referenciaLote = pesoEval >= 1150f ? new Pedido { peso_kg = pesoEval } : null;
            EvaluarEficienciaOperativaYEscalamiento(referenciaLote, permitirDespachoInmediato: false);

            string tituloDual = tieneSoporteDedicado
                ? "<color=#38BDF8>ESCALADO DUAL (2 AMRs + AMR-03)</color>"
                : "<color=#38BDF8>FLOTA DUAL ACTIVA (2 AMRs REALES)</color>";
            string detalleSoporteDual = tieneSoporteDedicado ? " | Soporte: AMR-03" : " (Sin 3.er AMR de reserva)";
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                tituloDual,
                $"Cola: {eval.ProyeccionCola} pal | SLA Dual: 80 pal/h ({relojTexto}, {RelojSimuladorTurno.FormatearDuracionHMS(eval.HorasRestantes)} rest.)\nPiso: AMR-01 + AMR-02 activos{detalleSoporteDual} | ETA: {eval.ETAHoras:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[SLA ESCALAMIENTO]:</color> Despacho dual activado a las {relojTexto} (AMR-01 + AMR-02 en piso{(tieneSoporteDedicado ? ", AMR-03 soporte" : string.Empty)}). Cola WMS: {eval.ProyeccionCola} pallets (ETA: {eval.ETAHoras:F2}h).");
            GestorAudioSCADA.Instance?.ReproducirIngresoPermitidoJSON();
            NotificarCambioEstadoAdmisionJSON();
        }
        else
        {
            GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
            bool esBloqueo = admisionNuevosPedidosBloqueada || amrsPisoDisponibles == 0 || eval.CupoDisponibleActual <= 0;
            string avisoTerminal = esBloqueo
                ? "<color=#F59E0B>[INGESTA BLOQUEADA]:</color> Sin margen disponible o soporte bloqueado. Detalle en Buzón."
                : $"<color=#EF4444>[INGESTA RECHAZADA]:</color> Lote +{nuevosPallets} pal excede capacidad actual (cupo: {eval.CupoDisponibleActual} pal). Detalle en Buzón.";

            DashboardUI.Instance?.RegistrarLog(avisoTerminal);
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(eval.FeedbackTecnico);

            if (eval.CupoDisponibleActual > 0)
            {
                DashboardUI.Instance?.MostrarPopUpExcedeCapacidadActual(eval.CupoDisponibleActual, nuevosPallets);
            }
            else
            {
                DashboardUI.Instance?.MostrarPopUpExcedeCapacidadActual(0, nuevosPallets, "Admisión bloqueada");
            }

            NotificarCambioEstadoAdmisionJSON();
        }
    }

    private void RegistrarCoordenadasBodega(List<Pedido> pedidos)
    {
        if (pedidos == null) return;
        foreach (var pedido in pedidos)
        {
            if (pedido != null && !string.IsNullOrEmpty(pedido.coordenada_bodega) && !directorioBodega.ContainsKey(pedido.coordenada_bodega))
            {
                GameObject destino = GameObject.Find(pedido.coordenada_bodega);
                if (destino != null) directorioBodega.Add(pedido.coordenada_bodega, destino.transform);
            }
        }
    }

    public bool ValidarYAutorizarReanudacionISO(RobotAMR robotAfectado, GameObject obstaculo)
    {
        if (robotAfectado == null) return false;

        bool galiboLibre = (obstaculo == null || !obstaculo.activeInHierarchy || obstaculo.transform.position.y >= 3.00f);
        if (!galiboLibre) return false;

        Vector3 origenCast = robotAfectado.transform.position + Vector3.up * 0.55f;
        Vector3 halfExtents = new Vector3(0.42f, 0.35f, 0.15f);
        Vector3 dirFrente = robotAfectado.transform.forward;
        bool carrilBloqueado = false;

        RaycastHit[] hits = Physics.BoxCastAll(
            origenCast,
            halfExtents,
            dirFrente,
            robotAfectado.transform.rotation,
            2.8f,
            robotAfectado.capaObstaculos,
            QueryTriggerInteraction.Ignore
        );

        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Collider c = hits[i].collider;
                if (c == null) continue;
                if (c.transform.IsChildOf(robotAfectado.transform)) continue;
                if (obstaculo != null && (c.gameObject == obstaculo || c.transform.IsChildOf(obstaculo.transform)))
                {
                    if (obstaculo.transform.position.y >= 3.00f) continue;
                }
                if (c.name.Contains("Suelo") || c.name.Contains("Floor") || c.bounds.max.y <= robotAfectado.transform.position.y + 0.05f)
                {
                    continue;
                }
                carrilBloqueado = true;
                break;
            }
        }

        if (carrilBloqueado) return false;

        Debug.Log("<color=#10B981>[FLUJO]: 5. Gálibo y carril verificados por EVE. Autorizando reanudación.</color>");
        DashboardUI.Instance?.RegistrarLog("<color=#10B981>[EVE -> " + robotAfectado.idRobot + "]</color> Gálibo vertical despejado (>3.00m). Vía libre verificada.");
        robotAfectado.ReanudarTrasDespejeLiDAR();
        return true;
    }

    private IEnumerator ProtocoloDespejeAutonomoISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        despachandoDespejeISO = true;

        string idRobot = robotAfectado != null ? robotAfectado.idRobot : "AMR";
        GameObject obstaculoObjetivo = obstaculoPasilloISO != null ? obstaculoPasilloISO : obstaculoDetectado;
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[ISO 3691-4 CAT. 3]:</color> Bloqueo frontal LiDAR. {idRobot} en parada de proteccion. EVE gestionando despeje...");

        if (dronMantenimiento != null && obstaculoObjetivo != null)
        {
            yield return StartCoroutine(dronMantenimiento.EjecutarMisionDespeje(
                obstaculoObjetivo,
                () => ValidarYAutorizarReanudacionISO(robotAfectado, obstaculoObjetivo)
            ));

            if (robotAfectado != null && robotAfectado.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
            {
                ValidarYAutorizarReanudacionISO(robotAfectado, obstaculoObjetivo);
            }
        }
        else
        {
            if (obstaculoObjetivo != null && obstaculoObjetivo.activeSelf)
            {
                obstaculoObjetivo.SetActive(false);
            }
            ValidarYAutorizarReanudacionISO(robotAfectado, obstaculoObjetivo);
        }

        despachandoDespejeISO = false;
    }
}
