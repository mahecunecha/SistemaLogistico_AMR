using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Reflection;

public class TelemetriaAMR : MonoBehaviour
{
    private RobotAMR robot;
    private NavMeshAgent agente;

    [Header("UI Text References")]
    public TextMeshProUGUI txtEstado;
    public TextMeshProUGUI txtPayload;
    public TextMeshProUGUI txtETA;

    private FieldInfo pedidoField;

    void Start()
    {
        robot = GetComponentInParent<RobotAMR>();
        agente = GetComponentInParent<NavMeshAgent>();

        if (robot != null)
        {
            pedidoField = typeof(RobotAMR).GetField("pedidoActual", BindingFlags.NonPublic | BindingFlags.Instance);
        }
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

        if (txtEstado != null)
            txtEstado.text = robot.estadoActual.ToString();

        if (txtPayload != null)
        {
            float peso = 0f;
            if (robot.estadoActual == RobotAMR.EstadoRobot.Transportando || robot.estadoActual == RobotAMR.EstadoRobot.Entregando)
            {
                if (pedidoField != null)
                {
                    object pedidoObj = pedidoField.GetValue(robot);
                    if (pedidoObj != null)
                    {
                        FieldInfo pesoField = pedidoObj.GetType().GetField("peso_kg", BindingFlags.Public | BindingFlags.Instance);
                        if (pesoField != null)
                        {
                            peso = System.Convert.ToSingle(pesoField.GetValue(pedidoObj));
                        }
                    }
                }
            }
            txtPayload.text = peso > 0 ? $"{peso} kg" : "0 kg";
        }

        if (txtETA != null)
        {
            if (agente.pathPending)
            {
                txtETA.text = "ETA: --";
            }
            else if (agente.remainingDistance > 0.5f && agente.velocity.magnitude > 0.1f)
            {
                float eta = agente.remainingDistance / agente.velocity.magnitude;
                txtETA.text = $"ETA: {eta:F1}s";
            }
            else
            {
                txtETA.text = "ETA: --";
            }
        }
    }
}