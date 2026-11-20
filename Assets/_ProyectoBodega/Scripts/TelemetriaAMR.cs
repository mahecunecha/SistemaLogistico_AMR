using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class TelemetriaAMR : MonoBehaviour
{
    private RobotAMR robot;
    private NavMeshAgent agente;

    [Header("UI Text References")]
    public TextMeshProUGUI txtEstado;
    public TextMeshProUGUI txtPayload;
    public TextMeshProUGUI txtETA;

    [Header("Elevación Dinámica Píldora")]
    public float alturaLocalVacio = 1.05f;
    public float alturaLocalConCarga = 2.18f;

    [Header("Throttling Settings")]
    private float tiempoProximoRefrescoETA = 0f;
    private const float IntervaloRefrescoETA = 0.15f; // ~6.6 Hz de refresco
    private float ultimoETAValor = -1f;
    private bool ultimoEstadoETAPendiente = false;
    private string textoETACache = "ETA: --";
    private int ultimoPorcentajeRecargaMostrado = -1;

    void Awake()
    {
        robot = GetComponentInParent<RobotAMR>();
        agente = GetComponentInParent<NavMeshAgent>();
        AplicarAlturaInicial();
    }

    void OnEnable()
    {
        if (robot != null)
        {
            robot.OnEstadoCambiado += ActualizarTelemetriaEstadoYPayload;
        }
        RefrescarTextoPildora();
    }

    void OnDisable()
    {
        if (robot != null)
        {
            robot.OnEstadoCambiado -= ActualizarTelemetriaEstadoYPayload;
        }
    }

    void Start()
    {
        AplicarAlturaInicial();
        if (txtPayload != null) txtPayload.gameObject.SetActive(false);
        if (txtETA != null) txtETA.gameObject.SetActive(false);
        RefrescarTextoPildora();
    }

    private void AplicarAlturaInicial()
    {
        Vector3 pos = transform.localPosition;
        pos.y = alturaLocalVacio;
        transform.localPosition = pos;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        bool conCarga = EsConCargaFisica();
        float yMeta = conCarga ? alturaLocalConCarga : alturaLocalVacio;
        Vector3 pos = transform.localPosition;
        pos.y = Mathf.Lerp(pos.y, yMeta, Time.deltaTime * 8f);
        transform.localPosition = pos;
    }

    void Update()
    {
        if (robot == null) return;

        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            if (textoETACache != "ETA: --" || ultimoETAValor != 0f || ultimoEstadoETAPendiente)
            {
                textoETACache = "ETA: --";
                ultimoETAValor = 0f;
                ultimoEstadoETAPendiente = false;
                RefrescarTextoPildora();
            }
            return;
        }

        // Throttling para cálculo y renderizado de ETA (~6-7 Hz para eliminar GC pressure a 60 FPS)
        if (Time.time >= tiempoProximoRefrescoETA)
        {
            tiempoProximoRefrescoETA = Time.time + IntervaloRefrescoETA;
            ActualizarETAThrottled();
        }
    }

    private bool EsConCargaFisica()
    {
        return robot != null && (robot.TieneCargaFisica || robot.estadoActual == RobotAMR.EstadoRobot.Transportando || robot.estadoActual == RobotAMR.EstadoRobot.Entregando);
    }

    private void ActualizarTelemetriaEstadoYPayload(RobotAMR.EstadoRobot nuevoEstado)
    {
        ultimoPorcentajeRecargaMostrado = -1;
        RefrescarTextoPildora();
    }

    private void ActualizarETAThrottled()
    {
        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            if (textoETACache != "ETA: --")
            {
                textoETACache = "ETA: --";
                RefrescarTextoPildora();
            }
            ultimoETAValor = 0f;
            ultimoEstadoETAPendiente = false;
            return;
        }

        bool requiereRefresco = false;

        if (agente.pathPending)
        {
            if (!ultimoEstadoETAPendiente || textoETACache != "ETA: --")
            {
                textoETACache = "ETA: --";
                ultimoEstadoETAPendiente = true;
                ultimoETAValor = -1f;
                requiereRefresco = true;
            }
        }
        else if (agente.remainingDistance > 0.5f && agente.velocity.magnitude > 0.1f)
        {
            float eta = agente.remainingDistance / agente.velocity.magnitude;
            float etaRedondeado = Mathf.Round(eta * 10f) / 10f; // Redondeo a 1 decimal

            if (Mathf.Abs(etaRedondeado - ultimoETAValor) >= 0.1f || ultimoEstadoETAPendiente)
            {
                ultimoETAValor = etaRedondeado;
                ultimoEstadoETAPendiente = false;
                textoETACache = $"ETA: {etaRedondeado:F1}s";
                requiereRefresco = true;
            }
        }
        else
        {
            if (ultimoETAValor != 0f || ultimoEstadoETAPendiente || textoETACache != "ETA: --")
            {
                textoETACache = "ETA: --";
                ultimoETAValor = 0f;
                ultimoEstadoETAPendiente = false;
                requiereRefresco = true;
            }
        }

        if (robot != null && robot.estadoActual == RobotAMR.EstadoRobot.Recargando)
        {
            int pct = Mathf.RoundToInt(robot.bateriaActual);
            if (pct != ultimoPorcentajeRecargaMostrado)
            {
                ultimoPorcentajeRecargaMostrado = pct;
                requiereRefresco = true;
            }
        }

        if (requiereRefresco)
        {
            RefrescarTextoPildora();
        }
    }

    private void RefrescarTextoPildora()
    {
        if (txtEstado == null) return;

        string id = (robot != null && !string.IsNullOrEmpty(robot.idRobot)) ? robot.idRobot : "AMR";
        RobotAMR.EstadoRobot estado = robot != null ? robot.estadoActual : RobotAMR.EstadoRobot.Inactivo;

        float peso = 0f;
        bool conCarga = EsConCargaFisica();
        if (conCarga && robot.PedidoActual != null)
        {
            peso = robot.PedidoActual.peso_kg;
        }

        if (conCarga && peso > 0f)
        {
            txtEstado.text = $"{id} • <color=#FBBF24>{peso:F0} kg</color> | {textoETACache}";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Inactivo)
        {
            txtEstado.text = $"{id} • <color=#94A3B8>RESERVA</color>";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Recargando)
        {
            bool enMovimientoHaciaPad = agente != null && agente.enabled && agente.isOnNavMesh && agente.velocity.magnitude > 0.15f;
            txtEstado.text = enMovimientoHaciaPad
                ? $"{id} • <color=#38BDF8>RETORNO PAD</color> | {textoETACache}"
                : $"{id} • <color=#38BDF8>CARGA 1C ({robot.bateriaActual:F0}%)</color>";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Interrumpido)
        {
            txtEstado.text = $"{id} • <color=#EF4444>ALERTA</color>";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Apagado)
        {
            txtEstado.text = $"{id} • <color=#EF4444>APAGADO &lt;3%</color>";
            return;
        }

        txtEstado.text = $"{id} • <color=#38BDF8>{ObtenerEstadoCorto(estado)}</color> | {textoETACache}";
    }

    private static string ObtenerEstadoCorto(RobotAMR.EstadoRobot estado)
    {
        switch (estado)
        {
            case RobotAMR.EstadoRobot.En_Transito: return "TRÁNSITO";
            case RobotAMR.EstadoRobot.Extrayendo: return "EXTRAYENDO";
            case RobotAMR.EstadoRobot.Transportando: return "CARGADO";
            case RobotAMR.EstadoRobot.Entregando: return "DESCARGA";
            default: return estado.ToString().ToUpperInvariant();
        }
    }
}
