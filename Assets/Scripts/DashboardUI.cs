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

    [System.Serializable]
    public class SlotTelemetriaAMR
    {
        public string idRobot;
        public Image barraBateriaRelleno;
        public TextMeshProUGUI txtBateria;
        public TextMeshProUGUI txtEstado;
    }

    [Header("Telemetría de Flota Dual (Gemelo Digital)")]
    public SlotTelemetriaAMR telemetriaAMR01;
    public SlotTelemetriaAMR telemetriaAMR02;
    public TextMeshProUGUI txtBannerTurno;

    [Header("Telemetría y Manifiesto")]
    public Image barraBateriaRelleno;
    public List<TextMeshProUGUI> textosEstadoManifiesto = new List<TextMeshProUGUI>();

    [Header("Alertas")]
    public GameObject panelEmergencia;
    [Tooltip("Asigna aquí el texto del detalle de emergencia (TMP o Legacy).")]
    [SerializeField] private Component textoDetalleEmergencia;

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

    public void MostrarEmergencia(bool mostrar, float bateriaProyectada, float varianzaMargen = 3.2f)
    {
        MostrarEmergencia(mostrar);
        
        if (mostrar && textoDetalleEmergencia != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<size=22><b><color=#FF3B30>ANOMALÍA PREDICTIVA: VIABILIDAD OPERATIVA COMPROMETIDA</color></b></size>");
            sb.AppendLine($"Retorno estimado: <b>{bateriaProyectada:F1}%</b> (±{varianzaMargen:F1}% tolerancia térmica)  |  Límite seguro de celda: <b>20.0%</b> (LiFePO4)");
            sb.AppendLine("─────────────────────────────────────────────────────────────────");
            sb.AppendLine("<b>MATRIZ DE TRADE-OFF GERENCIAL (DECISIÓN SUPERVISOR - INDUSTRIA 5.0):</b>\n");
            sb.AppendLine("• <b>[OPCIÓN A: FORZAR OVERRIDE]</b>");
            sb.AppendLine("  Consumo motor: +30% | Aceleración: -50% | Degradación térmica de celda: ALTA.");
            sb.AppendLine("  <i>Riesgo:</i> Agotamiento severo en retorno; detención catastrófica si la demanda supera la cota.\n");
            sb.AppendLine("• <b>[OPCIÓN B: ACTIVAR RELEVO 24/7]</b>");
            sb.AppendLine("  Daño de hardware: 0% | Vida útil de celda protegida al 20% | Acople inductivo Pad 01.");
            sb.AppendLine("  <i>Impacto:</i> Relevo inmediato con AMR-02. Cero paradas operativas en el turno.");
            string mensaje = sb.ToString();
            
            var prop = textoDetalleEmergencia.GetType().GetProperty("text");
            if (prop != null)
            {
                prop.SetValue(textoDetalleEmergencia, mensaje);
            }
            else if (textoDetalleEmergencia is Text legacyText)
            {
                legacyText.text = mensaje;
            }
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

    public void OnRelevoPresionado()
    {
        MostrarEmergencia(false);
        FindFirstObjectByType<RobotManagerEVE>()?.EjecutarRelevoFlota24_7();
    }

    public void OnAbortarMisionPresionado()
    {
        MostrarEmergencia(false);
        RobotAMR robot = UnityEngine.Object.FindObjectOfType<RobotAMR>();
        if (robot != null)
        {
            robot.AbortarMisionPorSeguridad();
        }
    }

    public void ActualizarBateria(string id, float porcentaje, RobotAMR.EstadoRobot estado)
    {
        SlotTelemetriaAMR slot = ObtenerSlotPorId(id);
        if (slot != null)
        {
            if (slot.barraBateriaRelleno != null)
            {
                slot.barraBateriaRelleno.fillAmount = Mathf.Clamp01(porcentaje);
            }
            if (slot.txtBateria != null)
            {
                slot.txtBateria.text = $"{porcentaje * 100f:F1}%";
            }
            if (slot.txtEstado != null)
            {
                slot.txtEstado.text = FormatearEstadoVisual(estado);
            }
        }

        // Compatibilidad con la referencia legacy
        if ((slot == null || slot == telemetriaAMR01) && barraBateriaRelleno != null)
        {
            barraBateriaRelleno.fillAmount = Mathf.Clamp01(porcentaje);
        }
    }

    private SlotTelemetriaAMR ObtenerSlotPorId(string id)
    {
        if (string.IsNullOrEmpty(id)) return telemetriaAMR01;

        if (telemetriaAMR01 != null && (id == telemetriaAMR01.idRobot || id.Contains("01") || id.Equals("Robot_AMR", System.StringComparison.OrdinalIgnoreCase)))
        {
            return telemetriaAMR01;
        }

        if (telemetriaAMR02 != null && (id == telemetriaAMR02.idRobot || id.Contains("02") || id.Equals("Robot_AMR_02", System.StringComparison.OrdinalIgnoreCase)))
        {
            return telemetriaAMR02;
        }

        return telemetriaAMR01;
    }

    private string FormatearEstadoVisual(RobotAMR.EstadoRobot estado)
    {
        switch (estado)
        {
            case RobotAMR.EstadoRobot.Inactivo: return "<color=#94A3B8>INACTIVO</color>";
            case RobotAMR.EstadoRobot.En_Transito: return "<color=#FACC15>EN TRÁNSITO</color>";
            case RobotAMR.EstadoRobot.Extrayendo: return "<color=#38BDF8>EXTRAYENDO</color>";
            case RobotAMR.EstadoRobot.Transportando: return "<color=#C084FC>TRANSPORTANDO</color>";
            case RobotAMR.EstadoRobot.Entregando: return "<color=#4ADE80>ENTREGANDO</color>";
            case RobotAMR.EstadoRobot.Interrumpido: return "<color=#EF4444>INTERRUMPIDO</color>";
            case RobotAMR.EstadoRobot.Recargando: return "<color=#38BDF8>RECARGANDO (Inductivo 1C)</color>";
            default: return estado.ToString().ToUpper();
        }
    }

    public void ActualizarBateria(float porcentaje)
    {
        ActualizarBateria("AMR-01", porcentaje, RobotAMR.EstadoRobot.Inactivo);
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

    void Start()
    {
        if (telemetriaAMR01 != null)
        {
            if (telemetriaAMR01.barraBateriaRelleno != null)
            {
                telemetriaAMR01.barraBateriaRelleno.fillAmount = 0.80f;
            }
            if (telemetriaAMR01.txtBateria != null)
            {
                telemetriaAMR01.txtBateria.text = "80.0%";
            }
            if (telemetriaAMR01.txtEstado != null)
            {
                telemetriaAMR01.txtEstado.text = FormatearEstadoVisual(RobotAMR.EstadoRobot.Inactivo);
            }
        }

        if (barraBateriaRelleno != null)
        {
            barraBateriaRelleno.fillAmount = 0.80f;
        }

        ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 1/8 (Operación Nominal) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");
    }

    public void ActualizarBannerTurno(string texto)
    {
        if (txtBannerTurno != null)
        {
            txtBannerTurno.text = texto;
        }
        else
        {
            var bannerGO = GameObject.Find("Txt_Banner");
            if (bannerGO != null)
            {
                var tmp = bannerGO.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = texto;
            }
        }
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
