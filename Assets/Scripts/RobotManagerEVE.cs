using UnityEngine;
using System.IO;
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

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public int indicePedidoActual = 0;
    public bool turnoIniciado = false;
    private Dictionary<string, Transform> directorioBodega = new Dictionary<string, Transform>();

    void Awake()
    {
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

    public void ConfirmarEntregaExitosa()
    {
        indicePedidoActual++;
        if (indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            var pedido = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);
            robotEjecutor.AsignarMision(pedido, destino);
            OnMisionEvaluada?.Invoke();
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado. No hay más pedidos.");
            DashboardUI.Instance?.RegistrarLog("Operación Finalizada: Manifiesto completado.");
        }
    }

    void MoverPorWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceWaypointActual];
        if (destino == null) return;
        /*En Unity, la palabra transform (con "t" minúscula) es una variable nativa e invisible
        que el motor le otorga automáticamente a todos los scripts que heredan de MonoBehaviour.
        No necesitas declararla en tu código porque ya existe en el núcleo del sistema.*/
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
