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
    public float consumoIdle = 0.01f;

    [Header("Sensores")]
    public LayerMask capaObstaculos;

    private float velocidadBase = 3.5f;
    private float aceleracionBase = 8.0f;
    private float coeficientePeso = 0.002f;

    [Header("Panel de Control (Solo lectura)")]
    public EstadoRobot estadoActual = EstadoRobot.Inactivo;
    /*El NavMeshAgent es el motor físico de Unity. Pero nota la variable Pedido. Tu cilindro físico no conoce el archivo JSON completo ni le interesa.
    Solo conoce la estructura de un pedido aislado que le entrega el Gestor, manteniendo el código limpio y modular.*/
    private NavMeshAgent agente;
    private Pedido pedidoActual; // Conoce la estructura, pero no toda la base de datos
    private bool modoDegradacionActivo = false;

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    private Transform destinoActual;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        float distanciaIda = Vector3.Distance(transform.position, destino.position);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            distanciaVuelta = Vector3.Distance(destino.position, zonaTransferencia.position);
        }

        // Proyección del consumo: ida vacío + vuelta cargado con ruido térmico máximo (1.15f)
        float gastoIda = distanciaIda * coeficienteVacio;
        float gastoVuelta = distanciaVuelta * coeficienteCarga * (pedido.peso_kg / 100f) * 1.15f;
        float gastoProyectadoTotal = gastoIda + gastoVuelta;

        bateriaRestanteEstimada = bateriaActual - gastoProyectadoTotal;

        return bateriaRestanteEstimada >= 15.0f;
    }

    public void AsignarMision(Pedido nuevoPedido, Transform destinoFisico)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual != EstadoRobot.Inactivo) return;

        pedidoActual = nuevoPedido;
        
        if (!CalcularViabilidadOperativa(pedidoActual, destinoFisico, out float bateriaProyectada))
        {
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ALERTA PREDICTIVA] Misión inviable. Retorno proyectado: {bateriaProyectada:F1}% (Umbral crítico: 15%). Detención preventiva.</color>");
            DashboardUI.Instance?.MostrarEmergencia(true, bateriaProyectada, 3.2f);
            estadoActual = EstadoRobot.Interrumpido;
            OnEstadoCambiado?.Invoke(estadoActual);
            if (agente != null) agente.isStopped = true;
            return;
        }

        if (destinoFisico != null)
        {
            DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "EN TRÁNSITO", Color.yellow);
            destinoActual = destinoFisico;
            estadoActual = EstadoRobot.En_Transito;
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = velocidadBase;
            agente.acceleration = aceleracionBase;

            agente.SetDestination(destinoActual.position); // Da la orden de moverse
            Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destinoFisico.name + " para buscar " + pedidoActual.codigo_sku);
            DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destinoFisico.name);
        }
        else
        {
            Debug.LogError("Alerta Logística: Coordenada nula recibida para " + pedidoActual.coordenada_bodega);
            DashboardUI.Instance?.RegistrarLog("Alerta: Coordenada nula recibida.");
        }
    }

    public void EjecutarOverride()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        estadoActual = EstadoRobot.En_Transito;
        if (agente != null) agente.isStopped = false;
        
        modoDegradacionActivo = true;
        if (agente != null) agente.acceleration = Mathf.Max(1.0f, agente.acceleration * 0.5f);
        coeficienteCarga *= 1.3f;

        if (destinoActual != null)
        {
            agente.SetDestination(destinoActual.position);
        }
        
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.RegistrarLog("<color=orange>[OVERRIDE EJECUTADO] Protocolo de contingencia forzado. Modo de degradación térmica activo: Aceleración -50%, Consumo +30%.</color>");
    }

    IEnumerator ProcesoExtraccion()
    {
        // Simulación de tiempo de extracción de carga calculada por hardware
        float tiempoExtraccion = Mathf.Lerp(1.2f, 3.8f, pedidoActual.peso_kg / 1000f) + Random.Range(-0.2f, 0.2f);
        yield return new WaitForSeconds(tiempoExtraccion);
        
        if (zonaTransferencia != null)
        {
            estadoActual = EstadoRobot.Transportando;
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = Mathf.Max(0.5f, velocidadBase - (pedidoActual.peso_kg * coeficientePeso));
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase - (pedidoActual.peso_kg * coeficientePeso * 2f));

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

    private void DibujarSensoresLiDAR()
    {
        Vector3 origen = transform.position + Vector3.up * 0.5f;
        float distancia = 4f;

        // Frente
        Vector3 dirFrente = transform.forward;
        if (Physics.Raycast(origen, dirFrente, out RaycastHit hitFrente, distancia, capaObstaculos))
            Debug.DrawLine(origen, hitFrente.point, Color.red);
        else
            Debug.DrawRay(origen, dirFrente * distancia, Color.cyan);

        // Izquierda (-15 grados)
        Vector3 dirIzquierda = Quaternion.Euler(0, -15, 0) * transform.forward;
        if (Physics.Raycast(origen, dirIzquierda, out RaycastHit hitIzquierda, distancia, capaObstaculos))
            Debug.DrawLine(origen, hitIzquierda.point, Color.red);
        else
            Debug.DrawRay(origen, dirIzquierda * distancia, Color.cyan);

        // Derecha (+15 grados)
        Vector3 dirDerecha = Quaternion.Euler(0, 15, 0) * transform.forward;
        if (Physics.Raycast(origen, dirDerecha, out RaycastHit hitDerecha, distancia, capaObstaculos))
            Debug.DrawLine(origen, hitDerecha.point, Color.red);
        else
            Debug.DrawRay(origen, dirDerecha * distancia, Color.cyan);
    }

    void Update()
    {
        if (Time.frameCount % 5 == 0)
        {
            DibujarSensoresLiDAR();
        }

        // Consumo pasivo obligatorio por encendido de sistemas lógicos
        bateriaActual -= consumoIdle * Time.deltaTime;

        // Matemáticas de Batería (Termodinámica Inyectada)
        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            float distanciaRecorrida = agente.velocity.magnitude * Time.deltaTime;
            if (distanciaRecorrida > 0)
            {
                float gasto = 0f;
                if (estadoActual == EstadoRobot.En_Transito)
                {
                    gasto = distanciaRecorrida * coeficienteVacio;
                }
                else if (estadoActual == EstadoRobot.Transportando)
                {
                    // Inyección de Ruido Termodinámico: Oscilación orgánica (Perlin Noise) para fricción e ineficiencia de motor (0.98 a 1.15)
                    float factorDeRuido = Mathf.Lerp(0.98f, 1.15f, Mathf.PerlinNoise(Time.time * 0.5f, 0f));
                    gasto = distanciaRecorrida * coeficienteCarga * (pedidoActual.peso_kg / 100f) * factorDeRuido;
                }
                
                bateriaActual -= gasto;
            }
        }
        DashboardUI.Instance?.ActualizarBateria(bateriaActual / 100f);

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
                
                estadoActual = EstadoRobot.Inactivo;
                OnEstadoCambiado?.Invoke(estadoActual);

                agente.speed = velocidadBase;
                if (modoDegradacionActivo)
                {
                    agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
                }
                else
                {
                    agente.acceleration = aceleracionBase;
                }

                if (managerEVE != null)
                {
                    managerEVE.ConfirmarEntregaExitosa();
                }
            }
        }
    }
}