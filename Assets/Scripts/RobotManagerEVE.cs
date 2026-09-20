using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class RobotManagerEVE : MonoBehaviour
{
    [Header("Waypoints de Vuelo")]
    public Transform[] waypoints;
    public float velocidadVuelo = 5f;
    private int indiceWaypointActual = 0;

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public int indicePedidoActual = 0;

    void Awake()
    {
        CargarManifiesto();
    }

    void Start()
    {
        if (manifiestoActual != null && manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
        {
            robotEjecutor.AsignarMision(manifiestoActual.pedidos[indicePedidoActual]);
        }
    }

    void Update()
    {
        MoverPorWaypoints();
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            Debug.Log("<color=green>EVE:</color> Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("EVE: Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
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
            robotEjecutor.AsignarMision(manifiestoActual.pedidos[indicePedidoActual]);
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
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidadVuelo * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }
}
