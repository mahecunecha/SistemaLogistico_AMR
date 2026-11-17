using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
        if (Input.GetKeyDown(KeyCode.O))
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
        FindFirstObjectByType<GeneradorTablaManifiesto>()?.LimpiarTablaStandby();
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

        FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(palletsEnColaWMS);

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
        FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(palletsEnColaWMS);

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
        var generadorTabla = FindFirstObjectByType<GeneradorTablaManifiesto>();
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
        FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(palletsEnColaWMS);
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
        if (obstaculoPasilloISO != null)
        {
            obstaculoPasilloISO.transform.SetParent(null, true);
            Collider col = obstaculoPasilloISO.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            RobotAMR robot = robotEjecutor != null ? robotEjecutor : robotAMR_01;
            if (robot != null && (robot.estadoActual == RobotAMR.EstadoRobot.En_Transito || robot.estadoActual == RobotAMR.EstadoRobot.Transportando))
            {
                Vector3 dirFrente = robot.transform.forward;
                Vector3 posObstaculo = robot.transform.position + dirFrente * 3.10f;
                posObstaculo.y = 0.202f;

                obstaculoPasilloISO.transform.position = posObstaculo;
                obstaculoPasilloISO.transform.rotation = Quaternion.identity;
            }

            obstaculoPasilloISO.SetActive(true);
            DashboardUI.Instance?.RegistrarLog("<color=#F59E0B>[STRESS TEST]:</color> Obstaculo imprevisto detectado en corredor principal.");
        }
        else
        {
            Debug.LogWarning("[EVE]: Referencia obstaculoPasilloISO no asignada para el Stress Test.");
        }
    }

    public void GestionarObstruccionAutonomaISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        if (despachandoDespejeISO) return;
        StartCoroutine(ProtocoloDespejeAutonomoISO(robotAfectado, obstaculoDetectado));
    }

    public void ProyectarEstresDeFlota(int nuevosPedidos = 50)
    {
        if (alertaDemandaEnCurso) return;
        alertaDemandaEnCurso = true;

        const float deltaSoCPedido = 3.85f;
        const float pisoSeguridadLiFePO4 = RobotAMR.UmbralSeguridadLiFePO4; // 20.0f
        const float ventanaNominalUnidad = 60.0f; // 80.0% -> 20.0%
        const float pesoEstibaEstandarKg = 850.0f;

        float energiaRequerida = nuevosPedidos * deltaSoCPedido; // 192.5% para 50 pedidos

        float bateriaAMR01 = robotAMR_01 != null ? robotAMR_01.bateriaActual : 80.0f;
        float bateriaAMR02 = robotAMR_02 != null ? robotAMR_02.bateriaActual : 80.0f;
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
        float porcentajeCarryOver = (pedidosRezagados / (float)nuevosPedidos) * 100f;

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

        Debug.Log($"<color=#F59E0B>[ANALIZADOR HORIZONTE O(1) - {etiquetaEscenario}]:</color> Hora {horaTurnoActual:F1}/8 (Restan {horasRestantes:F1}h). Lote: {nuevosPedidos} ped ({pesoLoteKg:F0} kg). Déficit LiFePO4: -{deficitSoC:F1}% SoC | Carry-over SLA: {porcentajeCarryOver:F0}% | Recomendación: {amrRequeridos} AMRs.");
        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>[WMS HORIZONTE - {etiquetaEscenario}]:</b></color> Ráfaga de {nuevosPedidos} pedidos ({pesoLoteKg:F0} kg) en Hora {horaTurnoActual:F1}/8 (Restan {horasRestantes:F1}h).");
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[DICTAMEN O(1)]:</b></color> Déficit LiFePO4: -{deficitSoC:F1}% SoC | Rezago SLA (Carry-over): {porcentajeCarryOver:F0}%. Flota recomendada: {amrRequeridos} AMRs.");

        DashboardUI.Instance?.MostrarAlertaIncertidumbreDemanda(
            escenario, horaTurnoActual, horasRestantes, nuevosPedidos,
            deficitSoC, amrRequeridos, porcentajeCarryOver, pesoLoteKg
        );
    }

    public void NotificarResolucionAlertaDemanda()
    {
        alertaDemandaEnCurso = false;
    }

    public void IngestarManifiestoDesdeSO()
    {
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
            if (string.IsNullOrWhiteSpace(json))
            {
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            Manifiesto lote = JsonUtility.FromJson<Manifiesto>(json);
            bool sinPedidos = lote == null || lote.pedidos == null || lote.pedidos.Count == 0;
            if (lote == null || (sinPedidos && lote.total_pallets <= 0))
            {
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            if (sinPedidos)
            {
                if (manifiestoActual != null && manifiestoActual.pedidos != null && manifiestoActual.pedidos.Count > 0)
                {
                    lote.pedidos = new List<Pedido>(manifiestoActual.pedidos);
                }
                else
                {
                    DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                    return;
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
                    DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                    return;
                }
            }

            EvaluarIngestaAutonoma(lote);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[EVE Ingesta JSON]: Error procesando '{rutaArchivo}': {ex.Message}");
            DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
        }
    }

    public void EvaluarIngestaAutonoma(Manifiesto nuevoLote)
    {
        if (nuevoLote == null) return;

        if ((plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) &&
            nuevoLote.pedidos != null && nuevoLote.pedidos.Count > 0)
        {
            plantillaPedidosFisicos = new List<Pedido>(nuevoLote.pedidos);
        }

        int nuevosPallets = (nuevoLote.total_pallets > 0)
            ? nuevoLote.total_pallets
            : (nuevoLote.pedidos != null ? nuevoLote.pedidos.Count : 0);

        int proyeccionCola = palletsEnColaWMS + nuevosPallets;

        float horasRestantesRaw = Mathf.Max(0.5f, 8.0f - horaTurnoActual);
        float horasRestantes = Mathf.Clamp(horasRestantesRaw, 4.5f, 5.0f);

        float capacidad1AMR = 40f * horasRestantes;
        float capacidad2AMRs = 80f * horasRestantes;

        if (proyeccionCola <= capacidad1AMR)
        {
            palletsEnColaWMS = proyeccionCola;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * 850f,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>()?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

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

            float tasaActual = flotaEscaladaDual ? 80f : 40f;
            float eta = proyeccionCola / tasaActual;
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#10B981>APROBADO (1 AMR)</color>",
                $"Lote: +{nuevosPallets} | Cola Total: {proyeccionCola} pallets (SLA: 40 pal/h)\nPiso: AMR-01 | Soporte: AMR-02 (Reserva 80%) | ETA: {eta:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[SLA ADMISIÓN]:</color> Aprobado +{nuevosPallets} pallets (Cola WMS: {proyeccionCola}). Operación 1 AMR (ETA: {eta:F2}h).");
        }
        else if (proyeccionCola <= capacidad2AMRs)
        {
            palletsEnColaWMS = proyeccionCola;
            flotaEscaladaDual = true;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * 850f,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>()?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

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

            float eta = proyeccionCola / 80f;
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#38BDF8>ESCALADO DUAL (2 AMRs + AMR-03)</color>",
                $"Cola Total: {proyeccionCola} pallets | SLA Dual: 80 pal/h\nPiso: AMR-01 + AMR-02 activos | Soporte: AMR-03 | ETA: {eta:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[SLA ESCALAMIENTO]:</color> Despacho dual activado (AMR-01 + AMR-02 en piso, AMR-03 soporte). Cola WMS: {proyeccionCola} pallets (ETA: {eta:F2}h).");
        }
        else
        {
            int amrRequeridos = Mathf.CeilToInt(proyeccionCola / capacidad1AMR);

            DashboardUI.Instance?.RegistrarLog("Pedido rechazado, feedback adjuntado.");

            string feedback = $"[REPORTE SLA] Lote rechazado (+{nuevosPallets} pal | Proyección: {proyeccionCola} pal).\n" +
                              $"Techo actual (2 AMRs @ 80 pal/h en {horasRestantes:F1}h): {capacidad2AMRs:F0} pallets.\n" +
                              $"Flota requerida: {amrRequeridos} AMRs en piso + 1 AMR soporte ({amrRequeridos + 1} totales).";
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(feedback);

            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#EF4444>RECHAZADO</color>",
                $"Proyección ({proyeccionCola} pallets) excede techo SLA (2 AMRs).\nFlota requerida: {amrRequeridos} AMRs en piso. Feedback en Buzón.",
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

    private IEnumerator ProtocoloDespejeAutonomoISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        despachandoDespejeISO = true;

        string idRobot = robotAfectado != null ? robotAfectado.idRobot : "AMR";
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[ISO 3691-4 CAT. 3]:</color> Bloqueo frontal LiDAR. {idRobot} en parada de proteccion. EVE gestionando despeje...");

        if (dronMantenimiento != null && obstaculoPasilloISO != null)
        {
            Action reanudar = () =>
            {
                Debug.Log("<color=#10B981>[FLUJO]: 5. Obstáculo despejado por EVE. Retomando Pedido 2.</color>");
                DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[WMS - VIA LIBRE]:</color> Obstaculo neutralizado por EVE. Autorizando reanudacion temprana de {idRobot}.");
                if (robotAfectado != null)
                {
                    robotAfectado.ReanudarTrasDespejeLiDAR();
                }
            };
            yield return StartCoroutine(dronMantenimiento.EjecutarMisionDespeje(obstaculoPasilloISO, reanudar));
        }
        else
        {
            yield return new WaitForSeconds(12.0f);
            if (obstaculoPasilloISO != null && obstaculoPasilloISO.activeSelf)
            {
                obstaculoPasilloISO.SetActive(false);
            }
            Debug.Log("<color=#10B981>[FLUJO]: 5. Obstáculo despejado por EVE. Retomando Pedido 2.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[WMS - VIA LIBRE]:</color> Obstaculo neutralizado por EVE. Autorizando reanudacion de {idRobot}.");
            if (robotAfectado != null)
            {
                robotAfectado.ReanudarTrasDespejeLiDAR();
            }
        }

        despachandoDespejeISO = false;
    }
}
