using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando }
    
    public event System.Action<EstadoRobot> OnEstadoCambiado;

    [Header("Telemetría Batería")]
    public float bateriaActual = 100f;
    public float coeficienteVacio = 0.05f;
    public float coeficienteCarga = 0.1f;

    [Header("Panel de Control (Solo lectura)")]
    public EstadoRobot estadoActual = EstadoRobot.Inactivo;
    /*El NavMeshAgent es el motor físico de Unity. Pero nota la variable Pedido. Tu cilindro físico no conoce el archivo JSON completo ni le interesa.
    Solo conoce la estructura de un pedido aislado que le entrega el Gestor, manteniendo el código limpio y modular.*/
    private NavMeshAgent agente;
    private Pedido pedidoActual; // Conoce la estructura, pero no toda la base de datos

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    private Transform destinoActual;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    public void AsignarMision(Pedido nuevoPedido)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual != EstadoRobot.Inactivo) return;

        pedidoActual = nuevoPedido;
        
        if (pedidoActual.peso_kg > 900)
        {
            DashboardUI.Instance?.RegistrarLog("<color=red>ALERTA CRÍTICA: Batería insuficiente para pedido masivo.</color>");
            estadoActual = EstadoRobot.Interrumpido;
            OnEstadoCambiado?.Invoke(estadoActual);
            agente.isStopped = true;
            return;
        }

        /*Busca en el mundo 3D el GameObject que se llame exactamente como dice el JSON*/
        GameObject destino = GameObject.Find(pedidoActual.coordenada_bodega);

        if (destino != null)
        {
            DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "EN TRÁNSITO", Color.yellow);
            destinoActual = destino.transform;
            estadoActual = EstadoRobot.En_Transito;
            OnEstadoCambiado?.Invoke(estadoActual);
            agente.SetDestination(destinoActual.position); // Da la orden de moverse
            Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destino.name + " para buscar " + pedidoActual.codigo_sku);
            DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destino.name);
        }
        else
        {
            Debug.LogError("Alerta Logística: No existe la coordenada " + pedidoActual.coordenada_bodega + " en la bodega física.");
            DashboardUI.Instance?.RegistrarLog("Alerta: No existe la coordenada " + pedidoActual.coordenada_bodega);
        }
    }

    IEnumerator ProcesoExtraccion()
    {
        // Simulación de tiempo de extracción de carga
        yield return new WaitForSeconds(2f);
        
        if (zonaTransferencia != null)
        {
            estadoActual = EstadoRobot.Transportando;
            OnEstadoCambiado?.Invoke(estadoActual);
            agente.SetDestination(zonaTransferencia.position);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a la Zona de Transferencia.");
            DashboardUI.Instance?.RegistrarLog("AMR Retornando: Llevando estiba a Zona de Transferencia.");
        }
        else
        {
            Debug.LogError("AMR Error: Falta asignar la Zona de Transferencia en el Inspector.");
            DashboardUI.Instance?.RegistrarLog("AMR Error: Falta Zona de Transferencia.");
        }
    }

    void Update()
    {
        // Matemáticas de Batería (Happy Path)
        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            float distanciaRecorrida = agente.velocity.magnitude * Time.deltaTime;
            if (distanciaRecorrida > 0)
            {
                float gasto = estadoActual == EstadoRobot.En_Transito ? 
                              distanciaRecorrida * coeficienteVacio : 
                              distanciaRecorrida * coeficienteCarga * (pedidoActual.peso_kg / 100f);
                
                bateriaActual -= gasto;
                DashboardUI.Instance?.ActualizarBateria(bateriaActual / 100f);
            }
        }

        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!agente.pathPending && agente.remainingDistance <= 1.0f)
            {
                estadoActual = EstadoRobot.Extrayendo;
                OnEstadoCambiado?.Invoke(estadoActual);
                Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                DashboardUI.Instance?.RegistrarLog("Destino Alcanzado: Extrayendo estiba.");
                StartCoroutine(ProcesoExtraccion());
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // Chequeo de distancia cuando el robot está retornando
            if (zonaTransferencia != null && Vector3.Distance(transform.position, zonaTransferencia.position) < 0.5f)
            {
                DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "ENTREGADO", Color.green);
                estadoActual = EstadoRobot.Entregando;
                OnEstadoCambiado?.Invoke(estadoActual);
                Debug.Log("<color=magenta>LIFO:</color> Entregando estiba en Zona de Transferencia.");
                DashboardUI.Instance?.RegistrarLog("LIFO: Entregando estiba en Zona de Transferencia.");
                
                if (managerEVE != null)
                {
                    managerEVE.ConfirmarEntregaExitosa();
                }

                estadoActual = EstadoRobot.Inactivo;
                OnEstadoCambiado?.Invoke(estadoActual);
            }
        }
    }
}