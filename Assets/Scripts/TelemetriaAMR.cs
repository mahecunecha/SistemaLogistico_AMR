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

    [Header("Throttling Settings")]
    private float tiempoProximoRefrescoETA = 0f;
    private const float IntervaloRefrescoETA = 0.15f; // ~6.6 Hz de refresco
    private float ultimoETAValor = -1f;
    private bool ultimoEstadoETAPendiente = false;

    void Awake()
    {
        robot = GetComponentInParent<RobotAMR>();
        agente = GetComponentInParent<NavMeshAgent>();
    }

    void OnEnable()
    {
        if (robot != null)
        {
            robot.OnEstadoCambiado += ActualizarTelemetriaEstadoYPayload;
        }
        ActualizarTelemetriaEstadoYPayload(robot != null ? robot.estadoActual : RobotAMR.EstadoRobot.Inactivo);
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
        ActualizarTelemetriaEstadoYPayload(robot != null ? robot.estadoActual : RobotAMR.EstadoRobot.Inactivo);
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    void Update()
    {
        if (robot == null || agente == null) return;

        // Throttling para cálculo y renderizado de ETA (~6-7 Hz para eliminar GC pressure a 60 FPS)
        if (Time.time >= tiempoProximoRefrescoETA)
        {
            tiempoProximoRefrescoETA = Time.time + IntervaloRefrescoETA;
            ActualizarETAThrottled();
        }
    }

    private void ActualizarTelemetriaEstadoYPayload(RobotAMR.EstadoRobot nuevoEstado)
    {
        if (txtEstado != null)
        {
            txtEstado.text = nuevoEstado.ToString();
        }

        if (txtPayload != null)
        {
            float peso = 0f;
            if (robot != null && (nuevoEstado == RobotAMR.EstadoRobot.Transportando || nuevoEstado == RobotAMR.EstadoRobot.Entregando))
            {
                if (robot.PedidoActual != null)
                {
                    peso = robot.PedidoActual.peso_kg;
                }
            }
            txtPayload.text = peso > 0 ? $"{peso:F0} kg" : "0 kg";
        }
    }

    private void ActualizarETAThrottled()
    {
        if (txtETA == null) return;

        if (agente.pathPending)
        {
            if (!ultimoEstadoETAPendiente)
            {
                txtETA.text = "ETA: --";
                ultimoEstadoETAPendiente = true;
                ultimoETAValor = -1f;
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
                txtETA.text = $"ETA: {etaRedondeado:F1}s";
            }
        }
        else
        {
            if (ultimoETAValor != 0f || ultimoEstadoETAPendiente)
            {
                txtETA.text = "ETA: --";
                ultimoETAValor = 0f;
                ultimoEstadoETAPendiente = false;
            }
        }
    }
}
