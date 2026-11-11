using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public int indicePedidoActual = 0;
    public bool turnoIniciado = false;
    private bool relevo24_7Activo = false;
    private Dictionary<string, Transform> directorioBodega = new Dictionary<string, Transform>();

    [Header("Trazabilidad LIFO Camión")]
    private Stack<Pedido> matrizLIFOCamion = new Stack<Pedido>();
    private List<GameObject> palletsEntregados = new List<GameObject>();
    private float pesoAcumuladoCamion = 0f;
    private const float CapacidadMaximaCamionKg = 12000f;

    public Stack<Pedido> MatrizLIFOCamion => matrizLIFOCamion;
    public float PesoAcumuladoCamion => pesoAcumuladoCamion;

    [Header("Simulacion de Incertidumbre ISO 3691-4")]
    public GameObject obstaculoPasilloISO;
    private bool despachandoDespejeISO = false;

    void Awake()
    {
        if (gestorTMS == null) gestorTMS = FindFirstObjectByType<ModuloDespachoTMS>();
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

        if (!turnoIniciado) return;
        MoverPorWaypoints();
    }

    public void IniciarTurno()
    {
        if (turnoIniciado) return;
        turnoIniciado = true;
        Debug.Log("<color=#10B981>[FLUJO]: 1. Botón 'Iniciar Turno' presionado. Despachando Pedido 1.</color>");

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

        // Población sincronizada de la tabla del manifiesto tras activar el HUD
        FindObjectOfType<GeneradorTablaManifiesto>()?.PoblarTabla(manifiestoActual.pedidos);

        if (manifiestoActual != null && manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
        {
            var pedido = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

            if (gestorTMS != null)
            {
                Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
                robotEjecutor.zonaTransferencia = muelleDestino;
                robotEjecutor.numCamionAsignado = numCamion;
            }

            robotEjecutor.AsignarMision(pedido, destino);
        }
        DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Turno de despacho nocturno iniciado.");
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[WMS ENERGÍA]:</color> Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada.");
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            directorioBodega.Clear();
            foreach(var pedido in manifiestoActual.pedidos)
            {
                if (!directorioBodega.ContainsKey(pedido.coordenada_bodega))
                {
                    GameObject destino = GameObject.Find(pedido.coordenada_bodega);
                    if (destino != null) directorioBodega.Add(pedido.coordenada_bodega, destino.transform);
                }
            }

            Debug.Log("<color=green>EVE:</color> Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("EVE: Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
        }
        else
        {
            Debug.LogError("EVE Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("EVE Error: No se encontró el Manifiesto de Carga.");
        }
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

    public void ConfirmarEntregaExitosa()
    {
        indicePedidoActual++;
        if (indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            if (indicePedidoActual == 1)
            {
                StartCoroutine(SecuenciaSaltoTemporal());
                return;
            }

            if (relevo24_7Activo && robotAMR_02 != null)
            {
                robotEjecutor = robotAMR_02;
            }
            else if (robotEjecutor != null && robotEjecutor.estadoActual == RobotAMR.EstadoRobot.Recargando)
            {
                if (robotAMR_02 != null && robotAMR_02.estadoActual != RobotAMR.EstadoRobot.Recargando)
                {
                    robotEjecutor = robotAMR_02;
                }
                else
                {
                    Debug.LogWarning($"[EVE]: Despacho bloqueado — {robotEjecutor.idRobot} se encuentra en estado Recargando.");
                    return;
                }
            }

            DespacharPedido(indicePedidoActual);
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado. No hay más pedidos.");
            DashboardUI.Instance?.RegistrarLog("Operación Finalizada: Manifiesto completado.");
        }
    }

    public void NotificarMisionAbortada()
    {
        DashboardUI.Instance?.RegistrarLog("<color=orange>[WMS]:</color> Misión abortada. Avanzando cola de manifiesto.");
        indicePedidoActual++;
        if (manifiestoActual != null && indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            var pedido = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

            if (gestorTMS != null && robotEjecutor != null)
            {
                Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
                robotEjecutor.zonaTransferencia = muelleDestino;
                robotEjecutor.numCamionAsignado = numCamion;
            }

            OnMisionEvaluada?.Invoke();
            if (robotEjecutor != null)
            {
                robotEjecutor.AsignarMision(pedido, destino);
            }
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado (Con Reprogramaciones).");
            DashboardUI.Instance?.RegistrarLog("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado (Con Reprogramaciones).");
        }
    }

    private void DespacharPedido(int indice)
    {
        if (relevo24_7Activo && robotAMR_02 != null)
        {
            robotEjecutor = robotAMR_02;
        }
        if (manifiestoActual == null || indice >= manifiestoActual.pedidos.Count || robotEjecutor == null) return;
        if (robotEjecutor.estadoActual == RobotAMR.EstadoRobot.Recargando)
        {
            Debug.LogWarning($"[EVE]: Bloqueado despacho hacia {robotEjecutor.idRobot} porque se encuentra en estado Recargando.");
            return;
        }

        var pedido = manifiestoActual.pedidos[indice];
        directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

        if (gestorTMS != null)
        {
            Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
            robotEjecutor.zonaTransferencia = muelleDestino;
            robotEjecutor.numCamionAsignado = numCamion;
        }

        OnMisionEvaluada?.Invoke();
        robotEjecutor.AsignarMision(pedido, destino);
    }

    private IEnumerator SecuenciaSaltoTemporal()
    {
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
            float bateriaObjetivo = 22.20f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
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
                DashboardUI.Instance?.ActualizarProgresoTimeLapse(t);
                yield return null;
            }
        }

        DashboardUI.Instance?.ActualizarProgresoTimeLapse(1f);
        DashboardUI.Instance?.ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 7.5/8 (Bloque Final) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        yield return new WaitForSeconds(0.5f);
        DashboardUI.Instance?.MostrarOverlayTimeLapse(false);

        Debug.Log("<color=#10B981>[FLUJO]: 3. Time-Lapse completado. Despachando Pedido 2. (PRESIONA 'O' AHORA PARA EMERGENCIA LiDAR)</color>");
        DespacharPedido(indicePedidoActual);
    }

    public void EjecutarRelevoFlota24_7()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        // 1. Enviar evento de relevo al DirectorCinematico antes de alterar estados para desuscribir AMR-01 inmediatamente
        if (robotAMR_02 != null)
        {
            DirectorCinematico.Instance?.RelevarRobotActivo(robotAMR_02);
        }

        // 2. Desvincular AMR-01 nombrando a AMR-02 como nuevo robotEjecutor
        RobotAMR robotSaliente = robotAMR_01 != null ? robotAMR_01 : robotEjecutor;
        relevo24_7Activo = true;
        if (robotAMR_02 != null)
        {
            robotEjecutor = robotAMR_02;
        }

        // 3. Enviar AMR-01 a estación de carga inductiva (Pad_Inductivo_01) limpiando su estado
        if (robotSaliente != null && robotSaliente != robotAMR_02)
        {
            Vector3 destinoPad01 = new Vector3(-10.0f, 0.25f, -10.0f);
            Transform pad1 = GameObject.Find("Pad_Inductivo_01")?.transform;
            if (pad1 != null) destinoPad01 = pad1.position;

            robotSaliente.EnviarACargaInductiva(destinoPad01);
        }

        // 4. Asignar la misión pendiente al nuevo robotEjecutor (AMR-02)
        if (robotEjecutor != null && manifiestoActual != null && indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            var pedidoPendiente = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedidoPendiente.coordenada_bodega, out Transform destino);

            if (gestorTMS != null)
            {
                Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedidoPendiente, out int numCamion, out int slot);
                robotEjecutor.zonaTransferencia = muelleDestino;
                robotEjecutor.numCamionAsignado = numCamion;
            }

            robotEjecutor.AsignarMision(pedidoPendiente, destino);
        }

        // 5. Registrar en el Dashboard
        string mensajeRelevo = "[RELEVO 24/7]: AMR-01 acoplado a carga inductiva (Pad 01). AMR-02 desplegado para Pedido 3.";
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8><b>{mensajeRelevo}</b></color>");
        Debug.Log($"<color=cyan>{mensajeRelevo}</color>");
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
            RobotAMR robot = robotEjecutor != null ? robotEjecutor : robotAMR_01;
            if (robot != null && (robot.estadoActual == RobotAMR.EstadoRobot.En_Transito || robot.estadoActual == RobotAMR.EstadoRobot.Transportando))
            {
                Vector3 dirFrente = robot.transform.forward;
                Vector3 posObstaculo = robot.transform.position + dirFrente * 2.2f;
                posObstaculo.y = robot.transform.position.y;

                obstaculoPasilloISO.transform.position = posObstaculo;
                if (dirFrente != Vector3.zero)
                {
                    obstaculoPasilloISO.transform.rotation = Quaternion.LookRotation(-dirFrente);
                }
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

    private IEnumerator ProtocoloDespejeAutonomoISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        despachandoDespejeISO = true;

        string idRobot = robotAfectado != null ? robotAfectado.idRobot : "AMR";
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[ISO 3691-4 CAT. 3]:</color> Bloqueo frontal a <0.85m. {idRobot} en parada de proteccion. EVE gestionando despeje...");

        yield return new WaitForSeconds(3.5f);

        // Solo se neutraliza el obstáculo ISO registrado (nunca racks u otra infraestructura estática)
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

        despachandoDespejeISO = false;
    }
}
