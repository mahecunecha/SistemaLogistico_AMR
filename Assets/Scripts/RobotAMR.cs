using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando }
    
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
        
        /*Busca en el mundo 3D el GameObject que se llame exactamente como dice el JSON*/
        GameObject destino = GameObject.Find(pedidoActual.coordenada_bodega);

        if (destino != null)
        {
            destinoActual = destino.transform;
            estadoActual = EstadoRobot.En_Transito;
            agente.SetDestination(destinoActual.position); // Da la orden de moverse
            Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destino.name + " para buscar " + pedidoActual.codigo_sku);
        }
        else
        {
            Debug.LogError("Alerta Logística: No existe la coordenada " + pedidoActual.coordenada_bodega + " en la bodega física.");
        }
    }

    IEnumerator ProcesoExtraccion()
    {
        // Simulación de tiempo de extracción de carga
        yield return new WaitForSeconds(2f);
        
        if (zonaTransferencia != null)
        {
            estadoActual = EstadoRobot.Transportando;
            agente.SetDestination(zonaTransferencia.position);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a la Zona de Transferencia.");
        }
        else
        {
            Debug.LogError("AMR Error: Falta asignar la Zona de Transferencia en el Inspector.");
        }
    }

    void Update()
    {
        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!agente.pathPending && agente.remainingDistance <= 1.0f)
            {
                estadoActual = EstadoRobot.Extrayendo;
                Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                StartCoroutine(ProcesoExtraccion());
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // Chequeo de distancia cuando el robot está retornando
            if (zonaTransferencia != null && Vector3.Distance(transform.position, zonaTransferencia.position) < 0.5f)
            {
                estadoActual = EstadoRobot.Entregando;
                Debug.Log("<color=magenta>LIFO:</color> Entregando estiba en Zona de Transferencia.");
                
                if (managerEVE != null)
                {
                    managerEVE.ConfirmarEntregaExitosa();
                }

                estadoActual = EstadoRobot.Inactivo;
            }
        }
    }
}