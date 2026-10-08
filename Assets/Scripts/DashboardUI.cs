using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Text;

public class DashboardUI : MonoBehaviour
{
    /*Al usar static, le estamos diciendo a Unity: "Esta variable no le pertenece a un objeto individual,
    le pertenece a la clase globalmente"
    Por convención mundial en C#, cuando se hace un Singleton, a la variable que guarda la copia única se le llama Instance.*/
    public static DashboardUI Instance { get; private set; }

    [Header("UI Referencias")]
    [Tooltip("Asigna aquí el TextMeshProUGUI. Si usas Text heredado, asígnalo abajo.")]
    [SerializeField] private Component textoLogTMP;
    [Tooltip("Asigna aquí el Text heredado si no usas TMPro.")]
    [SerializeField] private Text textoLogLegacy;

    [Header("Telemetría y Manifiesto")]
    public Image barraBateriaRelleno;
    public List<TextMeshProUGUI> textosEstadoManifiesto = new List<TextMeshProUGUI>();

    [Header("Alertas")]
    public GameObject panelEmergencia;

    [Header("Módulos")]
    [SerializeField] private GameObject panelMenuModulos;

    public void IniciarTurnoNocturno()
    {
        if (panelMenuModulos != null) panelMenuModulos.SetActive(false);
        
        GameObject menuPrincipal = GameObject.Find("Canvas_MenuPrincipal");
        if (menuPrincipal != null) menuPrincipal.SetActive(false);

        FindObjectOfType<RobotManagerEVE>()?.IniciarTurno();
        RegistrarLog("<color=green>[SISTEMA]:</color> Módulo 4 iniciado por supervisor.");
    }

    public void MostrarEmergencia(bool mostrar)
    {
        if (panelEmergencia != null)
        {
            panelEmergencia.SetActive(mostrar);
        }
    }

    public void OnOverridePresionado()
    {
        MostrarEmergencia(false);
        RobotAMR robot = UnityEngine.Object.FindObjectOfType<RobotAMR>();
        if (robot != null)
        {
            robot.EjecutarOverride();
        }
    }

    public void ActualizarBateria(float porcentaje)
    {
        if (barraBateriaRelleno != null)
        {
            barraBateriaRelleno.fillAmount = Mathf.Clamp01(porcentaje);
        }
    }

    public void ActualizarEstadoManifiesto(int indice, string nuevoEstado, Color color)
    {
        if (indice >= 0 && indice < textosEstadoManifiesto.Count && textosEstadoManifiesto[indice] != null)
        {
            textosEstadoManifiesto[indice].text = nuevoEstado;
            textosEstadoManifiesto[indice].color = color;
        }
    }

    private Queue<string> logQueue = new Queue<string>();
    private const int MaxLineas = 5;
    private StringBuilder stringBuilderLog = new StringBuilder(250);

    void Awake()
    {
        /*Aquí le estamos diciendo: "Si ya existe una instancia de esta clase en la escena,
         y no soy yo mismo, destrúyeme". Esto evita que Unity cree copias o duplicados.*/
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegistrarLog(string mensaje)
    {
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}]";
        string logFinal = $"{timestamp} {mensaje}";

        logQueue.Enqueue(logFinal);

        if (logQueue.Count > MaxLineas)
        {
            logQueue.Dequeue();
        }

        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        stringBuilderLog.Clear();
        foreach (var log in logQueue)
        {
            stringBuilderLog.AppendLine(log);
        }
        string contenido = stringBuilderLog.ToString();
        
        if (textoLogTMP != null)
        {
            var prop = textoLogTMP.GetType().GetProperty("text");
            if (prop != null)
            {
                prop.SetValue(textoLogTMP, contenido);
            }
        }
        else if (textoLogLegacy != null)
        {
            textoLogLegacy.text = contenido;
        }
    }
}
