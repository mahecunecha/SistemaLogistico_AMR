using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Globalization;

public class TelemetriaAMR : MonoBehaviour
{
    public const string TextoETACalculando = "ETA: Calculando";

    private RobotAMR robot;
    private NavMeshAgent agente;
    private Canvas canvasFlotante;

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
    private string textoETACache = TextoETACalculando;
    private int ultimoPorcentajeRecargaMostrado = -1;

    public static string FormatearETASeguro(float distanciaRestanteM, float velocidadActualMps, bool pathPending)
    {
        if (pathPending ||
            float.IsNaN(distanciaRestanteM) || float.IsInfinity(distanciaRestanteM) ||
            float.IsNaN(velocidadActualMps) || float.IsInfinity(velocidadActualMps) ||
            distanciaRestanteM <= 0.5f || velocidadActualMps <= 0.1f)
        {
            return TextoETACalculando;
        }

        float eta = distanciaRestanteM / velocidadActualMps;
        if (float.IsNaN(eta) || float.IsInfinity(eta) || eta < 0f || eta > 3600f)
        {
            return TextoETACalculando;
        }

        float etaRedondeado = Mathf.Round(eta * 10f) / 10f;
        return string.Format(CultureInfo.InvariantCulture, "ETA: {0:F1}s", etaRedondeado);
    }

    void Awake()
    {
        robot = GetComponentInParent<RobotAMR>();
        agente = GetComponentInParent<NavMeshAgent>();
        canvasFlotante = GetComponent<Canvas>();
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
        if (canvasFlotante == null)
        {
            canvasFlotante = GetComponent<Canvas>();
        }

        bool ocultarPorModal = DashboardUI.Instance != null && DashboardUI.Instance.EsModalEmergenciaActivo();
        if (canvasFlotante != null && canvasFlotante.enabled == ocultarPorModal)
        {
            canvasFlotante.enabled = !ocultarPorModal;
        }
        if (ocultarPorModal) return;

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
            if (textoETACache != TextoETACalculando)
            {
                textoETACache = TextoETACalculando;
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
            if (textoETACache != TextoETACalculando)
            {
                textoETACache = TextoETACalculando;
                RefrescarTextoPildora();
            }
            return;
        }

        bool requiereRefresco = false;

        float distanciaTotal = robot != null ? robot.CalcularDistanciaRestanteRutaActual() : agente.remainingDistance;
        float velocidadActual = agente.velocity.magnitude;
        string nuevoTextoETA = FormatearETASeguro(distanciaTotal, velocidadActual, agente.pathPending);

        if (nuevoTextoETA != textoETACache)
        {
            textoETACache = nuevoTextoETA;
            requiereRefresco = true;
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
                : $"{id} • <color=#38BDF8>CARGA 2h ({robot.bateriaActual:F0}%)</color>";
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

        if (estado == RobotAMR.EstadoRobot.FueraDeServicio_Danado)
        {
            txtEstado.text = $"{id} • <color=#DC2626>FALLA HW — SIN RESPUESTA</color>";
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
            case RobotAMR.EstadoRobot.FueraDeServicio_Danado: return "DAÑADO HW";
            default: return estado.ToString().ToUpperInvariant();
        }
    }
}
