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
    public float coeficienteVacio = 0.06f;
    public float coeficienteCarga = 0.125f;
    public float consumoIdle = 0.01f;

    [Header("Sensores LiDAR")]
    public LayerMask capaObstaculos = 1; // Default layer (bit 0) por defecto
    private float distanciaDeteccionAlerta = 1.2f;
    private float distanciaMaxSensor = 4.0f;
    private bool obstaculoDetectado = false;

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

    public Pedido PedidoActual => pedidoActual;

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    public Transform puntoMontajeCarga;
    private Transform destinoActual;
    private GameObject palletActualGO;

    public GameObject PalletActualGO => palletActualGO;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    private float CalcularDistanciaNavMesh(Vector3 origen, Vector3 destino)
    {
        Vector3 origenNav = origen;
        Vector3 destinoNav = destino;

        if (NavMesh.SamplePosition(origen, out NavMeshHit hitOrigen, 5.0f, NavMesh.AllAreas))
        {
            origenNav = hitOrigen.position;
        }
        if (NavMesh.SamplePosition(destino, out NavMeshHit hitDestino, 5.0f, NavMesh.AllAreas))
        {
            destinoNav = hitDestino.position;
        }

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(origenNav, destinoNav, NavMesh.AllAreas, path) && path.corners.Length > 1)
        {
            float distanciaTotal = 0f;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                distanciaTotal += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return distanciaTotal;
        }
        else
        {
            // Fallback topológico en plano XZ si el agente o destino no están resueltos sobre la malla
            Vector3 dif = destino - origen;
            dif.y = 0f;
            return dif.magnitude;
        }
    }

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        float distanciaIda = CalcularDistanciaNavMesh(transform.position, destino.position);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            distanciaVuelta = CalcularDistanciaNavMesh(destino.position, zonaTransferencia.position);
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

        // CRI-05: Guardia de Nulidad estricta para evitar NullReferenceException
        if (destinoFisico == null)
        {
            string idPallet = nuevoPedido != null ? nuevoPedido.id_pallet : "NULO";
            string coord = nuevoPedido != null ? nuevoPedido.coordenada_bodega : "DESCONOCIDA";
            Debug.LogError($"[ALERTA WMS] Destino físico nulo recibido para pedido {idPallet} ({coord}). Misión abortada.");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ERROR WMS] Ubicación {coord} no localizada en bodega. Misión abortada.</color>");
            return;
        }

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

    private GameObject ConstruirPalletVisual(Pedido pedido)
    {
        GameObject rootPallet = new GameObject($"Pallet_{pedido.id_pallet}_{pedido.codigo_sku}");
        if (puntoMontajeCarga != null)
        {
            rootPallet.transform.SetParent(puntoMontajeCarga, false);
            rootPallet.transform.localPosition = Vector3.zero;
            rootPallet.transform.localRotation = Quaternion.identity;
        }
        else
        {
            rootPallet.transform.SetParent(transform, false);
            rootPallet.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            rootPallet.transform.localRotation = Quaternion.identity;
        }

        // 1. Base de madera oscura (Estiba estándar: 1.1m x 0.15m x 0.9m)
        GameObject baseMadera = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseMadera.name = "Base_Madera";
        baseMadera.transform.SetParent(rootPallet.transform, false);
        baseMadera.transform.localPosition = new Vector3(0f, 0.075f, 0f);
        baseMadera.transform.localScale = new Vector3(1.1f, 0.15f, 0.9f);

        // REGLA CRÍTICA DE FÍSICA: Eliminar cualquier Collider para no interferir con LiDAR ni NavMesh
        Collider colBase = baseMadera.GetComponent<Collider>();
        if (colBase != null) Destroy(colBase);

        Renderer rendBase = baseMadera.GetComponent<Renderer>();
        if (rendBase != null)
        {
            rendBase.material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            rendBase.material.color = new Color(0.35f, 0.20f, 0.10f); // Tono madera oscura
        }

        // 2. Bloque de carga
        GameObject bloqueCarga = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bloqueCarga.name = "Bloque_Carga";
        bloqueCarga.transform.SetParent(rootPallet.transform, false);
        bloqueCarga.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        bloqueCarga.transform.localScale = new Vector3(1.0f, 0.6f, 0.85f);

        // REGLA CRÍTICA DE FÍSICA: Eliminar Collider del bloque de carga
        Collider colCarga = bloqueCarga.GetComponent<Collider>();
        if (colCarga != null) Destroy(colCarga);

        Renderer rendCarga = bloqueCarga.GetComponent<Renderer>();
        if (rendCarga != null)
        {
            rendCarga.material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            if (pedido.peso_kg >= 900f || pedido.codigo_sku.Contains("50L"))
            {
                // Pedido crítico 920 kg: Rojo industrial distintivo
                rendCarga.material.color = new Color(0.85f, 0.15f, 0.10f);
            }
            else
            {
                // Pedidos estándar 850 kg: Color ámbar cervecero dorado
                rendCarga.material.color = new Color(0.85f, 0.55f, 0.12f);
            }
        }

        return rootPallet;
    }

    IEnumerator ProcesoExtraccion()
    {
        if (agente != null)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        // Simulación de tiempo de extracción de carga calculada por hardware
        float tiempoExtraccion = Mathf.Lerp(1.2f, 3.8f, pedidoActual.peso_kg / 1000f) + Random.Range(-0.2f, 0.2f);
        yield return new WaitForSeconds(tiempoExtraccion);

        // CRI-03: Construcción e instanciación del gemelo físico del pallet sobre el punto de montaje
        palletActualGO = ConstruirPalletVisual(pedidoActual);

        if (agente != null)
        {
            agente.isStopped = false;
        }
        
        if (zonaTransferencia != null)
        {
            estadoActual = EstadoRobot.Transportando;
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = ObtenerVelocidadNominal();
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

    IEnumerator ProcesoEntrega()
    {
        estadoActual = EstadoRobot.Entregando;
        OnEstadoCambiado?.Invoke(estadoActual);

        DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "ENTREGADO", Color.green);
        Debug.Log("<color=magenta>LIFO:</color> Descargando estiba en Zona de Transferencia.");
        DashboardUI.Instance?.RegistrarLog("LIFO: Descargando estiba en Zona de Transferencia.");

        if (agente != null)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        // CRI-03: Desacoplar pallet del AMR y transferirlo a la Matriz LIFO del Camión
        if (managerEVE != null && palletActualGO != null)
        {
            managerEVE.RegistrarEntregaLIFO(pedidoActual, palletActualGO);
            palletActualGO = null;
        }

        // Simulación temporal de desacople de carga
        yield return new WaitForSeconds(1.5f);

        if (agente != null)
        {
            agente.isStopped = false;
        }

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

    private bool EsObjetivoDeAcoplamiento(Transform hitTransform)
    {
        if (hitTransform == null) return false;

        // Si viaja hacia una estantería en En_Transito
        if (estadoActual == EstadoRobot.En_Transito && destinoActual != null)
        {
            if (hitTransform == destinoActual || 
                hitTransform.IsChildOf(destinoActual) || 
                destinoActual.IsChildOf(hitTransform))
            {
                return true;
            }
        }
        // Si retorna hacia la zona de transferencia / camión en Transportando
        else if (estadoActual == EstadoRobot.Transportando && zonaTransferencia != null)
        {
            if (hitTransform == zonaTransferencia || 
                hitTransform.IsChildOf(zonaTransferencia) || 
                zonaTransferencia.IsChildOf(hitTransform))
            {
                return true;
            }
        }

        return false;
    }

    private void ProcesarSensoresLiDAR()
    {
        Vector3 origen = transform.position + Vector3.up * 0.5f;
        obstaculoDetectado = false;

        Vector3[] direcciones = new Vector3[]
        {
            transform.forward,
            Quaternion.Euler(0, -15, 0) * transform.forward,
            Quaternion.Euler(0, 15, 0) * transform.forward
        };

        foreach (var dir in direcciones)
        {
            if (Physics.Raycast(origen, dir, out RaycastHit hit, distanciaMaxSensor, capaObstaculos))
            {
                // 1. Filtrar propio chasis y componentes hijos (punto de carga, telemetría, estibas montadas)
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
                    continue;
                }

                // 2. Filtrar suelo de la bodega
                if (hit.transform.name.Contains("Suelo") || hit.point.y <= 0.35f)
                {
                    Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
                    continue;
                }

                // 3. Protocolo Docking Mute (ISO 3691-4): Acoplamiento con objetivo legítimo
                if (EsObjetivoDeAcoplamiento(hit.transform))
                {
                    // Visualización en amarillo: Acoplamiento legítimo en curso sin penalizar velocidad
                    Debug.DrawLine(origen, hit.point, Color.yellow);
                    continue;
                }

                // 4. Obstáculo imprevisto
                if (hit.distance < distanciaDeteccionAlerta)
                {
                    // Zona de advertencia crítica: reducción preventiva de velocidad
                    obstaculoDetectado = true;
                    Debug.DrawLine(origen, hit.point, Color.red);
                }
                else
                {
                    // Obstáculo detectado en zona lejana (amarillo-naranja de aviso)
                    Debug.DrawLine(origen, hit.point, new Color(1f, 0.6f, 0f));
                }
            }
            else
            {
                Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
            }
        }

        // Gestión activa de velocidad preventiva según LiDAR
        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            float velocidadNominal = ObtenerVelocidadNominal();
            if (obstaculoDetectado)
            {
                // Campo de protección LiDAR activo: 40% de velocidad nominal
                agente.speed = velocidadNominal * 0.4f;
            }
            else
            {
                agente.speed = velocidadNominal;
            }
        }
    }

    private float ObtenerVelocidadNominal()
    {
        if (estadoActual == EstadoRobot.En_Transito)
        {
            return velocidadBase;
        }
        else if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
        {
            return Mathf.Max(0.5f, velocidadBase - (pedidoActual.peso_kg * coeficientePeso));
        }
        return velocidadBase;
    }

    void Update()
    {
        // Procesamiento activo del sensor LiDAR con Docking Mute (ISO 3691-4)
        ProcesarSensoresLiDAR();

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

        // CRI-04: Acotamiento de batería y corte crítico al 0%
        bateriaActual = Mathf.Max(0f, bateriaActual);
        if (bateriaActual <= 0f && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando))
        {
            if (agente != null)
            {
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }
            estadoActual = EstadoRobot.Interrumpido;
            OnEstadoCambiado?.Invoke(estadoActual);
            DashboardUI.Instance?.RegistrarLog("<color=red><b>[FALLO CATASTRÓFICO]:</b> Celda agotada al 0% en pasillo. Unidad inmovilizada.</color>");
            Debug.LogError("[FALLO CATASTRÓFICO] Batería agotada al 0% en pasillo. NavMeshAgent inmovilizado.");
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
            // CRI-01 y MED-04: Detección estandarizada NavMesh y desacople temporal en corrutina
            if (zonaTransferencia != null && !agente.pathPending && agente.remainingDistance <= 1.0f)
            {
                StartCoroutine(ProcesoEntrega());
            }
        }
    }
}
