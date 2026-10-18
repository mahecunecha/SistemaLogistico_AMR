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
    private Vector3 velocidadActualEVE;
    private float smoothTime = 0.6f;

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
    private Dictionary<string, Transform> directorioBodega = new Dictionary<string, Transform>();

    [Header("Trazabilidad LIFO Camión")]
    private Stack<Pedido> matrizLIFOCamion = new Stack<Pedido>();
    private List<GameObject> palletsEntregados = new List<GameObject>();
    private float pesoAcumuladoCamion = 0f;
    private const float CapacidadMaximaCamionKg = 12000f;

    public Stack<Pedido> MatrizLIFOCamion => matrizLIFOCamion;
    public float PesoAcumuladoCamion => pesoAcumuladoCamion;

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
        if (!turnoIniciado) return;
        MoverPorWaypoints();
    }

    public void IniciarTurno()
    {
        if (turnoIniciado) return;
        turnoIniciado = true;

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
            FindObjectOfType<GeneradorTablaManifiesto>()?.PoblarTabla(manifiestoActual.pedidos);
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

                // Cuadrícula LIFO: acomodar a lo largo del eje Z del camión
                int indiceLIFO = matrizLIFOCamion.Count; // 1, 2, 3...
                float zOffset = (indiceLIFO - 2) * 1.6f;
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

            DespacharPedido(indicePedidoActual);
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado. No hay más pedidos.");
            DashboardUI.Instance?.RegistrarLog("Operación Finalizada: Manifiesto completado.");
        }
    }

    private void DespacharPedido(int indice)
    {
        if (manifiestoActual == null || indice >= manifiestoActual.pedidos.Count || robotEjecutor == null) return;

        var pedido = manifiestoActual.pedidos[indice];
        directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

        if (gestorTMS != null)
        {
            Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
            robotEjecutor.zonaTransferencia = muelleDestino;
            robotEjecutor.numCamionAsignado = numCamion;
        }

        robotEjecutor.AsignarMision(pedido, destino);
        OnMisionEvaluada?.Invoke();
    }

    private IEnumerator SecuenciaSaltoTemporal()
    {
        DashboardUI.Instance?.RegistrarLog("<color=#F59E0B><b>[TIME-LAPSE LOGÍSTICO]:</b> Avanzando 7 horas de turno continuo. Simulando fatiga de flota...</color>");
        DashboardUI.Instance?.ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 7.5/8 (Bloque Final) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        RobotAMR amr = robotAMR_01 != null ? robotAMR_01 : robotEjecutor;
        if (amr != null)
        {
            float bateriaInicial = amr.bateriaActual;
            float bateriaObjetivo = 26.5f;
            float duracion = 2.0f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                amr.bateriaActual = Mathf.Lerp(bateriaInicial, bateriaObjetivo, t);
                DashboardUI.Instance?.ActualizarBateria(amr.idRobot, amr.bateriaActual / 100f, amr.estadoActual);
                yield return null;
            }

            amr.bateriaActual = bateriaObjetivo;
            DashboardUI.Instance?.ActualizarBateria(amr.idRobot, amr.bateriaActual / 100f, amr.estadoActual);
        }

        yield return new WaitForSeconds(0.5f);

        DespacharPedido(indicePedidoActual);
    }

    public void EjecutarRelevoFlota24_7()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        // 1. Comanda a robotAMR_01 (o robot actual) a cancelar ruta y navegar a la estación de recarga en (-10.0f, 0.25f, -10.0f) (Pad_Inductivo_01)
        RobotAMR robotSaliente = robotEjecutor != null ? robotEjecutor : robotAMR_01;
        if (robotSaliente != null)
        {
            Vector3 destinoPad01 = new Vector3(-10.0f, 0.25f, -10.0f);
            Transform pad1 = GameObject.Find("Pad_Inductivo_01")?.transform;
            if (pad1 != null) destinoPad01 = pad1.position;

            robotSaliente.EnviarACargaInductiva(destinoPad01);
        }

        // 2. Despierta a robotAMR_02 en Pad_Inductivo_02 (iniciado al 80%), nómbralo robotActivo y asígnale el Pedido 3 pendiente
        if (robotAMR_02 != null)
        {
            robotEjecutor = robotAMR_02;

            indicePedidoActual = 2; // Pedido 3 (índice 2)
            if (manifiestoActual != null && indicePedidoActual < manifiestoActual.pedidos.Count)
            {
                var pedido3 = manifiestoActual.pedidos[indicePedidoActual];
                directorioBodega.TryGetValue(pedido3.coordenada_bodega, out Transform destino);

                if (gestorTMS != null)
                {
                    Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido3, out int numCamion, out int slot);
                    robotAMR_02.zonaTransferencia = muelleDestino;
                    robotAMR_02.numCamionAsignado = numCamion;
                }

                robotAMR_02.AsignarMision(pedido3, destino);
            }

            // 3. Invoca DirectorCinematico.Instance?.RelevarRobotActivo(robotAMR_02)
            DirectorCinematico.Instance?.RelevarRobotActivo(robotAMR_02);
        }

        // 4. Registra en el Dashboard
        string mensajeRelevo = "[RELEVO 24/7]: AMR-01 acoplado a carga inductiva (Pad 01). AMR-02 desplegado para Pedido 3.";
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8><b>{mensajeRelevo}</b></color>");
        Debug.Log($"<color=cyan>{mensajeRelevo}</color>");
    }

    void MoverPorWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceWaypointActual];
        if (destino == null) return;
        transform.position = Vector3.SmoothDamp(transform.position, destino.position, ref velocidadActualEVE, smoothTime, velocidadVuelo);
        
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
}
