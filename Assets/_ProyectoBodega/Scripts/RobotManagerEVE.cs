using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine.AI;
using Unity.Cinemachine;
using Debug = UnityEngine.Debug;

public class RobotManagerEVE : MonoBehaviour
{
    public event System.Action OnMisionEvaluada;

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
    private int indiceFilaManifiestoGlobal = 0;
    private float tiempoProximoDespachoPermitido = 0f;
    private Coroutine rutinaHeadwayDespacho;
    private const float HeadwaySalidaSalaCargaSegundos = 5.0f;

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

    [Header("Simulador de Estrés de Flota (Industria 5.0 - O(1))")]
    public float horaTurnoActual = 1.0f;
    private bool alertaDemandaEnCurso = false;
    private bool enSaltoTemporal = false;
    private int indiceCicloDemoHorizonte = 0;

    void Awake()
    {
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

        CargarManifiesto();
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && turnoIniciado)
        {
            InyectarObstaculoPasilloISO();
        }

        if (turnoIniciado && !enSaltoTemporal && horaTurnoActual < 7.5f)
        {
            float deltaHoras = (Time.deltaTime * RobotAMR.FactorCompresionTurno) / 3600f;
            horaTurnoActual = Mathf.Min(7.5f, horaTurnoActual + deltaHoras);
        }

        if (Input.GetKeyDown(KeyCode.I) && turnoIniciado)
        {
            bool shiftPresionado = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shiftPresionado)
            {
                float[] horasDemo = new float[] { 1.5f, 5.0f, 7.5f };
                horaTurnoActual = horasDemo[indiceCicloDemoHorizonte];
                indiceCicloDemoHorizonte = (indiceCicloDemoHorizonte + 1) % horasDemo.Length;
                alertaDemandaEnCurso = false;
                ProyectarEstresDeFlota(50);
            }
            else if (!alertaDemandaEnCurso)
            {
                ProyectarEstresDeFlota(50);
            }
        }

        if (!turnoIniciado) return;
        MoverPorWaypoints();
    }

    public void IniciarTurno()
    {
        if (turnoIniciado) return;
        turnoIniciado = true;
        sesionOperativaIniciada = true;
        palletsEnColaWMS = 0;
        palletsCompletadosTurno = 0;
        flotaEscaladaDual = false;
        indicePedidoActual = 0;
        indiceFilaManifiestoGlobal = 0;
        colaPrioritariaReasignacion.Clear();
        relevo24_7Activo = false;
        tiempoProximoDespachoPermitido = 0f;
        if (rutinaHeadwayDespacho != null)
        {
            StopCoroutine(rutinaHeadwayDespacho);
            rutinaHeadwayDespacho = null;
        }

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
            if (!enSaltoTemporal)
            {
                DespacharColaDisponible(robotEntregador);
            }
        }
        else if (ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
            Debug.Log("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
        }
    }

    public void NotificarMisionAbortada()
    {
        DashboardUI.Instance?.RegistrarLog("<color=orange>[WMS]:</color> Misión abortada. Avanzando cola WMS.");
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.ActualizarResumenColaEnVivo(palletsEnColaWMS);

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
        }
    }

    private void FinalizarTurnoYRetornarACarga(string mensajeCierre)
    {
        turnoIniciado = false;
        Debug.Log(mensajeCierre);
        DashboardUI.Instance?.RegistrarLog(mensajeCierre);

        if (robotEjecutor != null && robotEjecutor.estadoActual != RobotAMR.EstadoRobot.Recargando)
        {
            bool esUnidad02 = (robotEjecutor == robotAMR_02) ||
                              (!string.IsNullOrEmpty(robotEjecutor.idRobot) && robotEjecutor.idRobot.EndsWith("02"));
            string nombrePad = esUnidad02 ? "Pad_Inductivo_02" : "Pad_Inductivo_01";
            Vector3 posPadFallback = esUnidad02 ? new Vector3(38.13f, 0.25886f, -54.66f) : new Vector3(28.86f, 0.25886f, -54.91f);

            GameObject goPad = GameObject.Find(nombrePad);
            Vector3 destinoPad = goPad != null ? goPad.transform.position : posPadFallback;
            robotEjecutor.EnviarACargaInductiva(destinoPad);
        }

        DirectorCinematico.Instance?.ActivarVistaGeneralFinDeTurno();
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
        return flotaEscaladaDual ? 2 : 1;
    }

    private void DespacharColaDisponible(RobotAMR robotPreferido = null)
    {
        if (plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) return;
        if (palletsEnColaWMS <= 0) return;

        int iteracionesSeguridad = 4;
        int cuposMaximos = ObtenerCuposMaximosPiso();
        while (palletsEnColaWMS > ContarMisionesEnVuelo() && ContarMisionesEnVuelo() < cuposMaximos && iteracionesSeguridad-- > 0)
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
        if (!enSaltoTemporal && palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
    }

    private RobotAMR SeleccionarSiguienteRobotLibre(RobotAMR preferido)
    {
        if (preferido != null && preferido.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            if (flotaEscaladaDual) return preferido;
            if (relevo24_7Activo && preferido == robotAMR_02) return preferido;
            if (!relevo24_7Activo && preferido == robotAMR_01) return preferido;
        }

        if (flotaEscaladaDual)
        {
            if (robotAMR_01 != null && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo) return robotAMR_01;
            if (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo) return robotAMR_02;
            if (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
                return robotAMR_03;
            }
            return null;
        }

        RobotAMR ejecutorActual = (relevo24_7Activo && robotAMR_02 != null)
            ? robotAMR_02
            : (robotAMR_01 != null ? robotAMR_01 : robotEjecutor);

        if (ejecutorActual != null && ejecutorActual.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            return ejecutorActual;
        }

        if (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo &&
            (robotAMR_01 == null || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Recargando || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Apagado))
        {
            return robotAMR_02;
        }

        if (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo &&
            (robotAMR_01 == null || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Recargando || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Apagado || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido))
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
            return robotAMR_03;
        }

        return null;
    }

    private bool DespacharPedidoARobot(RobotAMR robot)
    {
        if (robot == null || plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) return false;
        if (robot.estadoActual != RobotAMR.EstadoRobot.Inactivo) return false;

        bool desdeColaReasignacion = colaPrioritariaReasignacion.Count > 0;
        Pedido pedido = desdeColaReasignacion
            ? colaPrioritariaReasignacion.Dequeue()
            : plantillaPedidosFisicos[Mathf.Abs(indicePedidoActual) % plantillaPedidosFisicos.Count];

        bool ok = DespacharPedidoEspecificoARobot(robot, pedido);
        if (ok && !desdeColaReasignacion)
        {
            indicePedidoActual++;
        }
        return ok;
    }

    private bool DespacharPedidoEspecificoARobot(RobotAMR robot, Pedido pedido)
    {
        if (robot == null || pedido == null) return false;
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

    private IEnumerator SecuenciaSaltoTemporal()
    {
        enSaltoTemporal = true;
        OnMisionEvaluada?.Invoke();
        Debug.Log("<color=#10B981>[FLUJO]: 2. Pedido 1 Entregado. Iniciando Time-Lapse Logístico (7 horas).</color>");
        DashboardUI.Instance?.RegistrarLog("<color=#F59E0B><b>[TIME-LAPSE LOGÍSTICO]:</b> Avanzando 7 horas de turno continuo (00:30 AM -> 05:30 AM). Simulando fatiga de flota...</color>");

        float duracion = 3.5f;
        DirectorCinematico.Instance?.EjecutarBarridoTimeLapse(duracion);
        DashboardUI.Instance?.MostrarOverlayTimeLapse(true);
        DashboardUI.Instance?.ActualizarProgresoTimeLapse(0f);

        RobotAMR amr = robotAMR_01 != null ? robotAMR_01 : robotEjecutor;
        if (amr != null)
        {
            float bateriaInicial = amr.bateriaActual;
            float bateriaObjetivo = 21.93f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                horaTurnoActual = Mathf.Lerp(1.0f, 7.5f, t);
                amr.bateriaActual = Mathf.Lerp(bateriaInicial, bateriaObjetivo, t);
                DashboardUI.Instance?.ActualizarBateria(amr.idRobot, amr.bateriaActual / 100f, amr.estadoActual);
                DashboardUI.Instance?.ActualizarProgresoTimeLapse(t);
                yield return null;
            }

            amr.bateriaActual = bateriaObjetivo;
            DashboardUI.Instance?.ActualizarBateria(amr.idRobot, amr.bateriaActual / 100f, amr.estadoActual);
        }
        else
        {
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                horaTurnoActual = Mathf.Lerp(1.0f, 7.5f, t);
                DashboardUI.Instance?.ActualizarProgresoTimeLapse(t);
                yield return null;
            }
        }

        horaTurnoActual = 7.5f;
        enSaltoTemporal = false;
        DashboardUI.Instance?.ActualizarProgresoTimeLapse(1f);
        DashboardUI.Instance?.ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 7.5/8 (Bloque Final) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        yield return new WaitForSeconds(0.5f);
        DashboardUI.Instance?.MostrarOverlayTimeLapse(false);

        Debug.Log("<color=#10B981>[FLUJO]: 3. Time-Lapse completado. Continuando despacho de Cola WMS.</color>");
        DespacharColaDisponible();
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
        if (robotAMR_02 != null && robotAMR_02 != robotSaliente && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_02;
        }
        else if (robotAMR_03 != null && robotAMR_03 != robotSaliente && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_03;
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
        }
        else if (robotAMR_01 != null && robotAMR_01 != robotSaliente && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo)
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

            if (robotSaliente != null && robotSaliente != robotRelevo)
            {
                robotSaliente.enPausaPreventivaBateria20 = false;
                Vector3 destinoPad = ObtenerPosicionPadPorUnidad(robotSaliente);
                robotSaliente.EnviarACargaInductiva(destinoPad);
            }

            if (robotRelevo != null)
            {
                DirectorCinematico.Instance?.RelevarRobotActivo(robotRelevo);
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotRelevo);
                relevo24_7Activo = (robotRelevo == robotAMR_02);
                robotEjecutor = robotRelevo;

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

    public void SimularColapsoBateria3()
    {
        if (!turnoIniciado) return;

        RobotAMR[] candidatos = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        RobotAMR objetivoSinCarga = null;
        RobotAMR objetivoCargado = null;

        // Caso A (Inmediato): AMR activo en tránsito sin estiba a bordo
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null) continue;
            if (EstaEnMisionActiva(r) && !r.TieneCargaFisica && !r.colapsoBMSArmadoPostDescarga)
            {
                objetivoSinCarga = r;
                break;
            }
        }

        if (objetivoSinCarga != null)
        {
            EjecutarColapsoYRescateInmediato(objetivoSinCarga);
            return;
        }

        // Caso B (Diferido seguro): AMR con estiba física completa descarga antes del corte <3%
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null) continue;
            if (EstaEnMisionActiva(r) && r.TieneCargaFisica && !r.colapsoBMSArmadoPostDescarga)
            {
                objetivoCargado = r;
                break;
            }
        }

        if (objetivoCargado != null)
        {
            objetivoCargado.colapsoBMSArmadoPostDescarga = true;
            objetivoCargado.bateriaActual = 4.2f;
            DashboardUI.Instance?.ActualizarBateria(objetivoCargado.idRobot, objetivoCargado.bateriaActual / 100f, objetivoCargado.estadoActual);

            string msgDiferido = $"[BMS CRÍTICO]: Caída de tensión en {objetivoCargado.idRobot} (4.2%). Completando descarga de estiba en muelle antes de corte total (<3%) y extracción aérea.";
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>{msgDiferido}</b></color>");
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA("<color=#EF4444>BMS CRÍTICO (4.2%)</color>", msgDiferido, 4.5f);
            Debug.Log($"<color=orange>{msgDiferido}</color>");
            return;
        }

        RobotAMR fallback = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: true);
        if (fallback == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA <3%]:</color> No hay AMRs disponibles en escena.");
            return;
        }

        if (fallback.colapsoBMSArmadoPostDescarga)
        {
            DashboardUI.Instance?.RegistrarLog($"<color=orange>[BMS CRÍTICO]:</color> {fallback.idRobot} ya tiene corte diferido armado tras descarga.");
            return;
        }

        EjecutarColapsoYRescateInmediato(fallback);
    }

    public void FinalizarEntregaYEjecutarColapsoBMS(RobotAMR objetivo)
    {
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        palletsCompletadosTurno++;
        FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.ActualizarResumenColaEnVivo(palletsEnColaWMS);
        EjecutarColapsoYRescateInmediato(objetivo);
    }

    public void EjecutarColapsoYRescateInmediato(RobotAMR objetivo)
    {
        if (objetivo == null) return;

        Pedido pedidoHuerfano = objetivo.ForzarApagadoCriticoBMS();
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[COLAPSO CRÍTICO <3% BMS]:</b></color> {objetivo.idRobot} apagado en pasillo (2.4% SoC). Desplegando Dron de Mantenimiento y relevo físico AMR-03.");

        // Bloquea VCam4_Emergencia sobre el AMR colapsado desde el segundo 0 hasta completar izaje vertical (Etapa 2)
        DirectorCinematico.Instance?.EnfocarIncidenteCritico(objetivo.transform);

        RobotAMR relevo = (robotAMR_03 != null && robotAMR_03 != objetivo && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            ? robotAMR_03
            : SeleccionarSiguienteRobotLibre(null);

        if (relevo == robotAMR_03)
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#FACC15>EN RELEVO ACTIVO</color>");
        }

        if (pedidoHuerfano != null)
        {
            palletsEnColaWMS = Mathf.Max(palletsEnColaWMS, 1);
            if (relevo != null && relevo.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                StartCoroutine(DespacharRelevoConHeadway(relevo, pedidoHuerfano));
            }
            else
            {
                colaPrioritariaReasignacion.Enqueue(pedidoHuerfano);
            }
        }
        else if (palletsEnColaWMS > 0 && relevo != null)
        {
            DespacharColaDisponible(relevo);
        }
        else if (palletsEnColaWMS == 0 && ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
        }

        if (dronMantenimiento != null)
        {
            Vector3 posPadDestino = ObtenerPosicionPadPorUnidad(objetivo);
            StartCoroutine(dronMantenimiento.EjecutarRescateAMRAbortado(
                objetivo,
                posPadDestino,
                onRescateCompletado: () =>
                {
                    DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[RESCATE AÉREO COMPLETADO]:</color> {objetivo.idRobot} acoplado en Pad Inductivo (Recarga 1C iniciada).");
                },
                onElevacionCompletada: () =>
                {
                    RobotAMR seguimientoRelevo = (relevo != null && relevo.estadoActual != RobotAMR.EstadoRobot.Apagado)
                        ? relevo
                        : (EstaEnMisionActiva(robotAMR_03) ? robotAMR_03 : null);
                    DirectorCinematico.Instance?.LiberarFocoIncidente(seguimientoRelevo);
                }
            ));
        }
        else
        {
            DirectorCinematico.Instance?.LiberarFocoIncidente(relevo);
        }
    }

    private IEnumerator DespacharRelevoConHeadway(RobotAMR relevo, Pedido pedido)
    {
        if (relevo != null && relevo.transform.position.z < RobotAMR.ZUmbralPortico)
        {
            while (Time.time < tiempoProximoDespachoPermitido)
            {
                yield return null;
            }
        }
        DespacharPedidoEspecificoARobot(relevo, pedido);
    }

    private RobotAMR BuscarAMRActivoOPisoParaSimulacion(bool soloEnMovimiento)
    {
        RobotAMR[] candidatos = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null) continue;
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
                if (r != null && r.estadoActual != RobotAMR.EstadoRobot.Apagado && r.estadoActual != RobotAMR.EstadoRobot.Recargando)
                {
                    return r;
                }
            }
        }

        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r != null && r.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                return r;
            }
        }

        return robotAMR_01 != null ? robotAMR_01 : robotAMR_02;
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
        DirectorCinematico director = DirectorCinematico.Instance;
        if (director == null || director.vcam3_AMR_Hombro == null || director.vcam3_AMR_Hombro.Target.TrackingTarget == null)
        {
            return null;
        }

        CinemachineCamera cam3 = director.vcam3_AMR_Hombro;
        int pCam3 = cam3.Priority.Value;
        int pCam1 = director.vcam1_Menu != null ? director.vcam1_Menu.Priority.Value : int.MinValue;
        int pCam2 = director.vcam2_EVE != null ? director.vcam2_EVE.Priority.Value : int.MinValue;

        if (pCam3 >= 10 && pCam3 >= pCam1 && pCam3 >= pCam2)
        {
            return cam3.Target.TrackingTarget.GetComponent<RobotAMR>();
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

    private void EjecutarProtocoloInspeccionHumanaMuelle(RobotAMR amrEnCamion)
    {
        int camionAfectado = DeterminarNumeroCamionPorAMR(amrEnCamion);
        ModuloDespachoTMS tms = gestorTMS != null ? gestorTMS : ModuloDespachoTMS.Instance;
        int siguienteCamion = (camionAfectado % 3) + 1;

        if (tms != null)
        {
            tms.ReportarInspeccionHumanaMuelle(camionAfectado, out siguienteCamion);
        }

        string titulo = "INSPECCIÓN HUMANA EN MUELLE 0" + camionAfectado;
        string detalle = "Zona de camión restringida para dron de soporte aéreo. Solicitud enviada al supervisor/operario en piso; nuevos pedidos redirigidos a Muelle_Camion_0" + siguienteCamion + ".";

        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>[{titulo}]:</b></color> {detalle}");
        DashboardUI.Instance?.MostrarPopUpEjecutivoSLA($"<color=#F59E0B>{titulo}</color>", detalle, 5.0f);
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
        float horaTurnoActual)
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
        float tasaAbsorcionFlotaActual = 2f * tasaAbsorcionPorAMR; // 10.0 ped/h

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
            amrRequeridos = 3; // 2 activos + 1 relevo en carga inductiva 1C
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

        float bateriaAMR01 = robotAMR_01 != null ? robotAMR_01.bateriaActual : 80.0f;
        float bateriaAMR02 = robotAMR_02 != null ? robotAMR_02.bateriaActual : 80.0f;

        ResultadoHorizonteO1 r = CalcularProyeccionHorizonte(nuevosPedidos, bateriaAMR01, bateriaAMR02, horaTurnoActual);

        Debug.Log($"<color=#F59E0B>[ANALIZADOR HORIZONTE O(1) - {r.EtiquetaEscenario}]:</color> Hora {r.HoraTurnoActual:F1}/8 (Restan {r.HorasRestantes:F1}h). Lote: {r.NuevosPedidos} ped ({r.PesoLoteKg:F0} kg). Déficit LiFePO4: -{r.DeficitSoC:F1}% SoC | Carry-over SLA: {r.PorcentajeCarryOver:F0}% | Recomendación: {r.AMRRequeridos} AMRs.");
        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>[WMS HORIZONTE - {r.EtiquetaEscenario}]:</b></color> Ráfaga de {r.NuevosPedidos} pedidos ({r.PesoLoteKg:F0} kg) en Hora {r.HoraTurnoActual:F1}/8 (Restan {r.HorasRestantes:F1}h).");
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
        if (!turnoIniciado) return;

        string dirInicial = Directory.Exists(Application.streamingAssetsPath)
            ? Application.streamingAssetsPath
            : Environment.GetFolderPath(Environment.SpecialFolder.Personal);

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
        StartCoroutine(EjecutarSelectorArchivoSOAsync(() =>
        {
            string dirEscapado = dirInicial.Replace("'", "''");
            string psScript = $"Add-Type -AssemblyName System.Windows.Forms; $d = New-Object System.Windows.Forms.OpenFileDialog; $d.InitialDirectory = '{dirEscapado}'; $d.Filter = 'JSON (*.json)|*.json'; if ($d.ShowDialog() -eq 'OK') {{ Write-Output $d.FileName }}";
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -STA -Command \"{psScript}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
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
#else
        DashboardUI.Instance?.MostrarPopUpRechazo("Selector de archivos no soportado en esta plataforma.");
#endif
    }

#if !UNITY_EDITOR && (UNITY_STANDALONE_LINUX || UNITY_STANDALONE_WIN)
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

        try
        {
            Manifiesto lote = JsonUtility.FromJson<Manifiesto>(json);
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
            }

            loteValido = lote;
            return true;
        }
        catch (Exception ex)
        {
            motivoError = ex.Message;
            return false;
        }
    }

    public void ProcesarArchivoManifiesto(string rutaArchivo)
    {
        try
        {
            if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
            {
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            string json = File.ReadAllText(rutaArchivo);
            List<Pedido> fallback = (manifiestoActual != null) ? manifiestoActual.pedidos : null;
            if (!TryParsearYValidarManifiesto(json, fallback, out Manifiesto lote, out string motivoError))
            {
                if (!string.IsNullOrEmpty(motivoError) && motivoError != "Esquema inválido")
                {
                    Debug.LogWarning($"[EVE Ingesta JSON]: Error procesando '{rutaArchivo}': {motivoError}");
                }
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            EvaluarIngestaAutonoma(lote);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[EVE Ingesta JSON]: Error procesando '{rutaArchivo}': {ex.Message}");
            DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
        }
    }

    public static ResultadoEvaluacionSLA CalcularEvaluacionSLA(
        int palletsEnColaActual,
        int nuevosPallets,
        float horaTurnoActual,
        bool flotaEscaladaDualActual = false)
    {
        float horasRestantesRaw = Mathf.Max(0.5f, 8.0f - horaTurnoActual);
        float horasRestantes = Mathf.Clamp(horasRestantesRaw, 4.5f, 5.0f);

        float capacidad1AMR = 40f * horasRestantes;
        float capacidad2AMRs = 80f * horasRestantes;

        if (nuevosPallets <= 0)
        {
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Invalido,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = palletsEnColaActual,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = 0,
                ETAHoras = 0f,
                FeedbackTecnico = string.Empty
            };
        }

        int proyeccionCola = palletsEnColaActual + nuevosPallets;

        if (proyeccionCola <= capacidad1AMR)
        {
            float tasaActual = flotaEscaladaDualActual ? 80f : 40f;
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Aprobado1AMR,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = flotaEscaladaDualActual ? 2 : 1,
                ETAHoras = proyeccionCola / tasaActual,
                FeedbackTecnico = string.Empty
            };
        }
        else if (proyeccionCola <= capacidad2AMRs)
        {
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.EscaladoDual2AMRs,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
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
            int amrRequeridos = Mathf.CeilToInt(proyeccionCola / capacidad1AMR);
            string feedback = $"[REPORTE SLA] Lote rechazado (+{nuevosPallets} pal | Proyección: {proyeccionCola} pal).\n" +
                              $"Techo actual (2 AMRs @ 80 pal/h en {horasRestantes:F1}h): {capacidad2AMRs:F0} pallets.\n" +
                              $"Flota requerida: {amrRequeridos} AMRs en piso + 1 AMR soporte ({amrRequeridos + 1} totales).";
            return new ResultadoEvaluacionSLA
            {
                Decision = DecisionAdmisionSLA.Rechazado,
                NuevosPallets = nuevosPallets,
                ProyeccionCola = proyeccionCola,
                HorasRestantes = horasRestantes,
                Capacidad1AMR = capacidad1AMR,
                Capacidad2AMRs = capacidad2AMRs,
                AMRRequeridos = amrRequeridos,
                ETAHoras = 0f,
                FeedbackTecnico = feedback
            };
        }
    }

    public void EvaluarIngestaAutonoma(Manifiesto nuevoLote)
    {
        if (nuevoLote == null)
        {
            UltimaEvaluacionSLA = default;
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

        ResultadoEvaluacionSLA eval = CalcularEvaluacionSLA(palletsEnColaWMS, nuevosPallets, horaTurnoActual, flotaEscaladaDual);
        UltimaEvaluacionSLA = eval;

        if (eval.Decision == DecisionAdmisionSLA.Invalido) return;

        if (eval.Decision == DecisionAdmisionSLA.Aprobado1AMR)
        {
            palletsEnColaWMS = eval.ProyeccionCola;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * 850f,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

            if (!flotaEscaladaDual)
            {
                DashboardUI.Instance?.ConfigurarReservaAMR02(true);
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            }

            if (sesionOperativaIniciada || turnoIniciado)
            {
                turnoIniciado = true;
                if (!enSaltoTemporal)
                {
                    DespacharColaDisponible();
                }
            }

            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#10B981>APROBADO (1 AMR)</color>",
                $"Lote: +{nuevosPallets} | Cola Total: {eval.ProyeccionCola} pallets (SLA: 40 pal/h)\nPiso: AMR-01 | Soporte: AMR-02 (Reserva 80%) | ETA: {eval.ETAHoras:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[SLA ADMISIÓN]:</color> Aprobado +{nuevosPallets} pallets (Cola WMS: {eval.ProyeccionCola}). Operación 1 AMR (ETA: {eval.ETAHoras:F2}h).");
        }
        else if (eval.Decision == DecisionAdmisionSLA.EscaladoDual2AMRs)
        {
            palletsEnColaWMS = eval.ProyeccionCola;
            flotaEscaladaDual = true;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * 850f,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#38BDF8>SOPORTE / RELEVO 24/7 (80% SoC)</color>");

            if (sesionOperativaIniciada || turnoIniciado)
            {
                turnoIniciado = true;
                if (!enSaltoTemporal)
                {
                    DespacharColaDisponible();
                }
            }

            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#38BDF8>ESCALADO DUAL (2 AMRs + AMR-03)</color>",
                $"Cola Total: {eval.ProyeccionCola} pallets | SLA Dual: 80 pal/h\nPiso: AMR-01 + AMR-02 activos | Soporte: AMR-03 | ETA: {eval.ETAHoras:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[SLA ESCALAMIENTO]:</color> Despacho dual activado (AMR-01 + AMR-02 en piso, AMR-03 soporte). Cola WMS: {eval.ProyeccionCola} pallets (ETA: {eval.ETAHoras:F2}h).");
        }
        else
        {
            DashboardUI.Instance?.RegistrarLog("Pedido rechazado, feedback adjuntado.");
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(eval.FeedbackTecnico);

            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#EF4444>RECHAZADO</color>",
                $"Proyección ({eval.ProyeccionCola} pallets) excede techo SLA (2 AMRs).\nFlota requerida: {eval.AMRRequeridos} AMRs en piso. Feedback en Buzón.",
                4.5f
            );
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
