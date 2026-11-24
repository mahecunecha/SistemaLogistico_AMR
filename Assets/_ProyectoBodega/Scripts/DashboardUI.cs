using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    [Tooltip("ScrollRect de la consola de EVE con nacimiento inferior (Bottom-Up).")]
    public ScrollRect scrollTerminalEVE;

    [Header("Time-Lapse Industrial (Compresión Temporal)")]
    public GameObject panelOverlayTimeLapse;
    public TextMeshProUGUI txtOverlayTimeLapse;

    [Header("Marcador NBA Reloj de Turno (HUD Superior Central)")]
    public GameObject panelMarcadorNBAHud;
    public TextMeshProUGUI txtHoraTurnoNBA;
    public TextMeshProUGUI txtRestanteTurnoNBA;
    public TextMeshProUGUI txtEstadoTurnoNBA;
    public Image imgAcentoMarcadorNBA;
    private string ultimaClaveMarcadorNBA = string.Empty;

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

    [Header("Soporte Virtual AMR-03 (Panel_Flota)")]
    public GameObject filaAMR03;
    public SlotTelemetriaAMR telemetriaAMR03;
    private bool amr03Habilitado = true;
    private bool modoReservaAMR02 = false;

    [Header("Telemetría Dron de Soporte (Panel_Flota)")]
    public GameObject filaDronSoporte;
    public TextMeshProUGUI txtEstadoDronSoporte;
    public TextMeshProUGUI txtSistemaDronSoporte;
    private DronMantenimiento dronSuscritoTelemetria;
    private string ultimaHoraBannerRegistrada = string.Empty;

    [Header("Telemetría y Manifiesto")]
    public Image barraBateriaRelleno;
    public List<TextMeshProUGUI> textosEstadoManifiesto = new List<TextMeshProUGUI>();

    [Header("Alertas")]
    public GameObject panelEmergencia;
    [Tooltip("Asigna aquí el texto del detalle de emergencia (TMP o Legacy).")]
    [SerializeField] private Component textoDetalleEmergencia;

    [Header("Paneles Colapsables (ISA-101)")]
    public RectTransform panelManifiestoRect;
    public RectTransform panelFlotaRect;
    public RectTransform panelTerminalRect;
    public TextMeshProUGUI txtBtnToggleManifiesto;
    public TextMeshProUGUI txtBtnToggleFlota;
    public TextMeshProUGUI txtBtnToggleTerminal;

    private float alturaOriginalManifiesto = 280f;
    private float alturaOriginalFlota = 502f;
    private float alturaOriginalTerminal = 520f;
    private bool manifiestoExpandido = true;
    private bool flotaExpandida = true;
    private bool terminalExpandido = true;

    [Header("Pop-up Ejecutivo SLA y Buzón Supervisor")]
    public GameObject panelPopupSLA;
    public TextMeshProUGUI txtTituloPopupSLA;
    public TextMeshProUGUI txtDetallePopupSLA;
    public Button btnCerrarPopupSLA;
    public TextMeshProUGUI txtBtnBuzonSupervisor;
    private int contadorFeedbackBuzon = 0;
    private List<string> historialFeedbackSupervisor = new List<string>();
    private Coroutine rutinaAutoCierrePopup;

    [Header("Ventana Dedicada Buzón Supervisor (Scroll)")]
    public GameObject panelBuzonSupervisor;
    public ScrollRect scrollBuzonSupervisor;
    public RectTransform contentBuzonSupervisor;
    public TextMeshProUGUI txtTituloBuzonSupervisor;
    public TextMeshProUGUI txtListadoBuzonSupervisor;
    public Button btnCerrarBuzonSupervisor;
    public Button btnBuzonSupervisor;
    private readonly StringBuilder sbBuzon = new StringBuilder(2048);

    public IReadOnlyList<string> HistorialFeedbackSupervisor => historialFeedbackSupervisor;

    public enum TipoAlertaActiva { BateriaCriticaAMR, IncertidumbreDemandaWMS, TurnoFaseCriticaSeguridad }
    private TipoAlertaActiva tipoAlertaActual = TipoAlertaActiva.BateriaCriticaAMR;
    private TextMeshProUGUI txtBtnModalOverride;
    private TextMeshProUGUI txtBtnModalRelevo;
    private string textoOriginalBtnOverride;
    private string textoOriginalBtnRelevo;
    private bool textosModalCacheados = false;
    private RobotManagerEVE.EscenarioHorizonte ultimoEscenarioHorizonte = RobotManagerEVE.EscenarioHorizonte.EscenarioC_FinalTurno;
    private int ultimoLotePedidos = 50;
    private int ultimosAmrRequeridos = 4;
    private float ultimoPesoLoteKg = 42500f;
    private float ultimoDeficitSoC = 72.5f;
    private float ultimoCarryOverPct = 0f;

    [Header("Módulos")]
    [SerializeField] private GameObject panelMenuModulos;

    [Header("Selector Minimizable de Cámaras SCADA")]
    public GameObject panelSelectorCamaras;
    public TextMeshProUGUI txtBtnCambiarCamara;
    public Button btnCamAuto;
    public Button btnCamGeneral;
    public Button btnCamEVE;
    public Button btnCamAMR01;
    public Button btnCamAMR02;
    public Button btnCamAMR03;
    public Button btnCamDron;

    private bool selectorCamarasExpandido = false;
    private bool haSidoDesplegadoAMR03EnTurno = true;
    private string ultimaCabeceraCamara = string.Empty;
    private int ultimoFrameToggleCamaras = -1;
    private Transform hudOperativoTransform;
    private bool hudOperativoEstabaActivo = false;

    [Header("Destello Visual de Alertas (Módulo 4)")]
    public Image imgFilaAMR01;
    public Image imgFilaAMR02;
    public Image imgFilaAMR03;
    public Image imgPanelTerminalEVE;

    private Coroutine rutinaDestelloAMR01;
    private Coroutine rutinaDestelloAMR02;
    private Coroutine rutinaDestelloAMR03;
    private Coroutine rutinaDestelloTerminal;

    private static readonly Color ColorBaseFilaAMR = new Color(0.12f, 0.16f, 0.22f, 0.42f);
    private static readonly Color ColorBaseTerminalEVE = new Color(0.121568628f, 0.141176477f, 0.180392161f, 0.95f);
    private static readonly Color ColorDestelloPreventivoFila = new Color(0.96f, 0.62f, 0.04f, 0.32f);
    private static readonly Color ColorDestelloCriticoFila = new Color(0.94f, 0.27f, 0.27f, 0.38f);

    [Header("Barra Operativa Inferior (Módulo 5)")]
    public GameObject barraOperativaInferior;
    public Button btnPausaSimulacion;
    public Image imgBtnPausaSimulacion;
    public TextMeshProUGUI txtBtnPausaSimulacion;

    [Header("Control de Ingesta JSON (Estado y Capacidad)")]
    public Button btnIngestarJSON;
    public Image imgBtnIngestarJSON;
    public TextMeshProUGUI txtBtnIngestarJSON;

    [Header("Pop-up Compacto Capacidad JSON")]
    public GameObject panelPopupCapacidad;
    public TextMeshProUGUI txtTituloPopupCapacidad;
    public TextMeshProUGUI txtDetallePopupCapacidad;
    public CanvasGroup canvasGroupPopupCapacidad;

    private RobotManagerEVE eveSuscritoPausa;
    private RobotManagerEVE eveSuscritoAdmisionJSON;
    private Coroutine rutinaAutoCierrePopupCapacidad;
    private bool? ultimoEstadoBtnJsonDisponible;
    private string ultimoTextoBtnJson = string.Empty;
    private float tiempoProximoCheckBtnJson = 0f;
    private int ultimoFrameTogglePausa = -1;
    private static readonly Color ColorBtnPausaNormal = new Color(0.118f, 0.161f, 0.231f, 1f);
    private static readonly Color ColorBtnPausaActiva = new Color(0.706f, 0.325f, 0.035f, 1f);
    private static readonly Color ColorBtnJsonDisponible = new Color(0.086f, 0.565f, 0.341f, 1f);
    private static readonly Color ColorBtnJsonBloqueado = new Color(0.765f, 0.165f, 0.188f, 1f);

    [Header("Controles Planta SCADA (Alarma Fábrica y Despeje Muelle)")]
    public Button btnSilenciarAlarmaFabrica;
    public TextMeshProUGUI txtBtnSilenciarAlarmaFabrica;
    public Image imgBtnSilenciarAlarmaFabrica;
    public Button btnConfirmarDespejeMuelle;
    public TextMeshProUGUI txtBtnConfirmarDespejeMuelle;
    public Image imgBtnConfirmarDespejeMuelle;

    private GestorAudioSCADA audioSuscritoAlarma;
    private ModuloDespachoTMS tmsSuscritoMuelle;
    private int ultimoFrameSilenciarAlarma = -1;
    private int ultimoFrameDespejeMuelle = -1;
    private static readonly Color ColorBtnAlarmaActiva = new Color(0.75f, 0.14f, 0.14f, 1f);
    private static readonly Color ColorBtnMuelleClausurado = new Color(0.76f, 0.41f, 0.05f, 1f);

    private static RobotManagerEVE ResolverRobotManagerEVE()
    {
        if (RobotManagerEVE.Instance != null)
        {
            return RobotManagerEVE.Instance;
        }

        RobotManagerEVE activo = FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Exclude);
        if (activo != null)
        {
            return activo;
        }

        return FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
    }

    public void IniciarTurnoNocturno()
    {
        haSidoDesplegadoAMR03EnTurno = true;
        selectorCamarasExpandido = false;
        if (panelSelectorCamaras != null)
        {
            panelSelectorCamaras.SetActive(false);
        }

        if (panelMenuModulos != null) { panelMenuModulos.SetActive(false); }
        
        GameObject menuPrincipal = GameObject.Find("Canvas_MenuPrincipal");
        if (menuPrincipal != null) menuPrincipal.SetActive(false);

        Transform hud = transform.Find("HUD_Operativo");
        if (hud != null)
        {
            hud.gameObject.SetActive(true);
            hudOperativoTransform = hud;
        }

        InicializarPanelesColapsables();
        ConfigurarBarraOperativaInferior();
        ConfigurarBotonIngestaJSON();
        ConfigurarControlesPlantaSCADA();
        ConfigurarTelemetriaDronSoporte();
        if (!SincronizarActivacionHUDSiAplica())
        {
            ConfigurarSelectorCamarasSCADA();
            if (hud != null && hud.gameObject.activeInHierarchy)
            {
                RefrescarEstadoSelectorCamaras(true);
            }
        }

        LimpiarHistorialBuzonSupervisor();
        ResolverRobotManagerEVE()?.IniciarTurno();
        ConfigurarBarraOperativaInferior();
        ConfigurarBotonIngestaJSON();
        ConfigurarControlesPlantaSCADA();
        ConfigurarVentanaBuzonSupervisor();
        ConfigurarTelemetriaDronSoporte();
        ConfigurarMarcadorNBAHud();
        ActualizarMarcadorNBAHud(true);
        RefrescarLayoutPanelFlota();
        RegistrarLog("<color=green>[SISTEMA]:</color> Módulo 4 iniciado por supervisor.");
    }

    public bool EsModalEmergenciaActivo()
    {
        return panelEmergencia != null && panelEmergencia.activeInHierarchy;
    }

    public void MostrarEmergencia(bool mostrar)
    {
        if (panelEmergencia != null)
        {
            if (mostrar && tipoAlertaActual != TipoAlertaActiva.TurnoFaseCriticaSeguridad)
            {
                RobotManagerEVE evePausa = ResolverRobotManagerEVE();
                if (evePausa != null && evePausa.SimulacionPausada)
                {
                    evePausa.EstablecerPausaSimulacion(false);
                }
            }

            bool flancoApertura = mostrar && !panelEmergencia.activeSelf;
            panelEmergencia.SetActive(mostrar);
            if (mostrar)
            {
                ReconstruirLayoutAlertaEmergencia();
            }
            if (flancoApertura)
            {
                if (tipoAlertaActual == TipoAlertaActiva.TurnoFaseCriticaSeguridad)
                {
                    GestorAudioSCADA.Instance?.ReproducirAlarmaCritica();
                }
                else
                {
                    GestorAudioSCADA.Instance?.ReproducirAlertaPreventiva();
                }
            }
        }
    }

    /// <summary>
    /// Aplica un pulso cromático sincronizado sobre la tarjeta AMR afectada y la terminal EVE.
    /// </summary>
    public void DestellarAlertaUnidad(string idRobot, bool esCritica)
    {
        if (!Application.isPlaying || !isActiveAndEnabled) return;

        AsegurarReferenciasDestello();

        Color colorPicoFila = esCritica ? ColorDestelloCriticoFila : ColorDestelloPreventivoFila;
        if (!string.IsNullOrEmpty(idRobot) && idRobot.Contains("02"))
        {
            ReiniciarPulsoImagen(ref rutinaDestelloAMR02, imgFilaAMR02, ColorBaseFilaAMR, colorPicoFila, 2.4f, 3);
        }
        else if (!string.IsNullOrEmpty(idRobot) && idRobot.Contains("03"))
        {
            ReiniciarPulsoImagen(ref rutinaDestelloAMR03, imgFilaAMR03, ColorBaseFilaAMR, colorPicoFila, 2.4f, 3);
        }
        else
        {
            ReiniciarPulsoImagen(ref rutinaDestelloAMR01, imgFilaAMR01, ColorBaseFilaAMR, colorPicoFila, 2.4f, 3);
        }

        Color acentoTerminal = esCritica
            ? new Color(0.94f, 0.27f, 0.27f, 0.96f)
            : new Color(0.96f, 0.62f, 0.04f, 0.96f);
        Color colorPicoTerminal = Color.Lerp(ColorBaseTerminalEVE, acentoTerminal, 0.38f);
        ReiniciarPulsoImagen(ref rutinaDestelloTerminal, imgPanelTerminalEVE, ColorBaseTerminalEVE, colorPicoTerminal, 2.4f, 3);
    }

    private void AsegurarReferenciasDestello()
    {
        const string rutaFlota = "HUD_Operativo/Panel_Flota/Scroll_Flota/Viewport/Content_Flota/";
        if (imgFilaAMR01 == null)
        {
            Transform t = transform.Find(rutaFlota + "Fila_AMR_01");
            if (t != null) imgFilaAMR01 = t.GetComponent<Image>();
        }
        if (imgFilaAMR02 == null)
        {
            Transform t = transform.Find(rutaFlota + "Fila_AMR_02");
            if (t != null) imgFilaAMR02 = t.GetComponent<Image>();
        }
        if (imgFilaAMR03 == null)
        {
            Transform t = transform.Find(rutaFlota + "Fila_AMR_03");
            if (t != null) imgFilaAMR03 = t.GetComponent<Image>();
        }
        if (imgPanelTerminalEVE == null)
        {
            Transform t = transform.Find("HUD_Operativo/Panel_Terminal_EVE");
            if (t != null) imgPanelTerminalEVE = t.GetComponent<Image>();
        }
    }

    private void ReiniciarPulsoImagen(ref Coroutine rutina, Image objetivo, Color colorBase, Color colorPico, float duracion, int ciclos)
    {
        if (objetivo == null) return;

        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }
        objetivo.color = colorBase;
        rutina = StartCoroutine(PulsoCromaticoImagen(objetivo, colorBase, colorPico, duracion, ciclos));
    }

    private IEnumerator PulsoCromaticoImagen(Image objetivo, Color colorBase, Color colorPico, float duracion, int ciclos)
    {
        if (objetivo == null || duracion <= 0f) yield break;

        float transcurrido = 0f;
        while (transcurrido < duracion && objetivo != null)
        {
            transcurrido += Time.unscaledDeltaTime;
            float tNorm = Mathf.Clamp01(transcurrido / duracion);
            float pulso = (1f - Mathf.Cos(tNorm * Mathf.PI * 2f * ciclos)) * 0.5f;
            objetivo.color = Color.Lerp(colorBase, colorPico, pulso);
            yield return null;
        }

        if (objetivo != null)
        {
            objetivo.color = colorBase;
        }
    }

    private void ReconstruirLayoutAlertaEmergencia()
    {
        if (panelEmergencia == null) return;
        Transform alertaTr = panelEmergencia.transform.Find("Alerta");
        if (alertaTr is RectTransform rectAlerta)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectAlerta);
        }
    }

    private void AsignarTextoDetalleEmergencia(string mensaje, bool reconstruirLayout)
    {
        if (textoDetalleEmergencia == null) return;

        var prop = textoDetalleEmergencia.GetType().GetProperty("text");
        if (prop != null)
        {
            prop.SetValue(textoDetalleEmergencia, mensaje);
        }
        else if (textoDetalleEmergencia is Text legacyText)
        {
            legacyText.text = mensaje;
        }

        if (reconstruirLayout)
        {
            ReconstruirLayoutAlertaEmergencia();
        }
    }

    private void AsegurarTextosBotonesModalEmergencia()
    {
        if (textosModalCacheados || panelEmergencia == null) return;

        Transform filaAcciones = panelEmergencia.transform.Find("Alerta/Fila_Acciones_Supervisor");
        if (filaAcciones != null)
        {
            Transform tOv = filaAcciones.Find("Btn_Override/Text (TMP)");
            if (tOv != null)
            {
                txtBtnModalOverride = tOv.GetComponent<TextMeshProUGUI>();
                if (txtBtnModalOverride != null) textoOriginalBtnOverride = txtBtnModalOverride.text;
            }

            Transform tRel = filaAcciones.Find("Btn_Relevo_24_7/Text (TMP)");
            if (tRel != null)
            {
                txtBtnModalRelevo = tRel.GetComponent<TextMeshProUGUI>();
                if (txtBtnModalRelevo != null) textoOriginalBtnRelevo = txtBtnModalRelevo.text;
            }
        }
        textosModalCacheados = true;
    }

    private void RestaurarTextosBotonesModalEmergencia()
    {
        AsegurarTextosBotonesModalEmergencia();
        if (txtBtnModalOverride != null && !string.IsNullOrEmpty(textoOriginalBtnOverride))
        {
            txtBtnModalOverride.text = textoOriginalBtnOverride;
        }
        if (txtBtnModalRelevo != null && !string.IsNullOrEmpty(textoOriginalBtnRelevo))
        {
            txtBtnModalRelevo.text = textoOriginalBtnRelevo;
        }
    }

    public void MostrarEmergencia(bool mostrar, float bateriaProyectada, float varianzaMargen = 3.2f)
    {
        tipoAlertaActual = TipoAlertaActiva.BateriaCriticaAMR;
        RestaurarTextosBotonesModalEmergencia();
        MostrarEmergencia(mostrar);

        if (mostrar && textoDetalleEmergencia != null)
        {
            const string separador = "<color=#334155>--------------------------------------------------------------------</color>";
            string mensaje = "<size=20><b><color=#FF453A>[!] ANOMALÍA PREDICTIVA: VIABILIDAD OPERATIVA COMPROMETIDA</color></b></size>\n" +
                             $"Retorno estimado: <b><color=#FBBF24>{bateriaProyectada:F1}%</color></b> (±{varianzaMargen:F1}% tolerancia térmica)  |  Límite seguro LiFePO4: <b><color=#38BDF8>20.0%</color></b>\n" +
                             separador + "\n" +
                             "<b><color=#94A3B8>MATRIZ DE TRADE-OFF GERENCIAL (DECISIÓN SUPERVISOR - INDUSTRIA 5.0)</color></b>\n\n" +
                             "<b><color=#F97316>> [OPCIÓN A: FORZAR OVERRIDE]</color></b>\n" +
                             "<color=#E2E8F0>  Consumo motor: +30% | Aceleración: -50% | Degradación térmica de celda: ALTA.</color>\n\n" +
                             "<b><color=#10B981>> [OPCIÓN B: ACTIVAR RELEVO 24/7]</color></b>\n" +
                             "<color=#E2E8F0>  Retirar AMR-01 a carga inductiva. Desplegar AMR-02 (80%) para asumir el pedido.</color>\n" +
                             separador;

            AsignarTextoDetalleEmergencia(mensaje, true);
        }
    }

    public void MostrarAlertaFaseCriticaSupervisor(string motivo, int descargasCriticas, int fallasHardware)
    {
        tipoAlertaActual = TipoAlertaActiva.TurnoFaseCriticaSeguridad;
        AsegurarTextosBotonesModalEmergencia();

        ActualizarBannerTurno("<b><color=#EF4444>TURNO EN FASE CRÍTICA:</color> OPERACIÓN DETENIDA POR SEGURIDAD | <color=#FBBF24>INTERVENCIÓN HUMANA EN PISO</color></b>");

        if (txtBtnModalOverride != null)
        {
            txtBtnModalOverride.text = "SILENCIAR ALARMA PLANTA\n<size=11><color=#FED7AA>(Reportes de cierre postergados)</color></size>";
        }
        if (txtBtnModalRelevo != null)
        {
            txtBtnModalRelevo.text = "ACUSAR RECIBO (SUPERVISOR)\n<size=11><color=#A7F3D0>(Mantener Parada de Seguridad)</color></size>";
        }

        MostrarEmergencia(true);

        if (textoDetalleEmergencia != null)
        {
            const string separador = "<color=#334155>--------------------------------------------------------------------</color>";
            string detalleMotivo = !string.IsNullOrEmpty(motivo) ? motivo : "Reincidencia crítica energética/operativa detectada en el turno.";
            string mensaje = "<size=20><b><color=#EF4444>[X] TURNO EN FASE CRÍTICA — PARADA AUTOMÁTICA DE SEGURIDAD</color></b></size>\n" +
                             $"<color=#E2E8F0>Incidentes del turno: <b><color=#FBBF24>{descargasCriticas}</color></b> descarga(s) &le;3%  |  <b><color=#EF4444>{fallasHardware}</color></b> falla(s) hardware</color>\n" +
                             separador + "\n" +
                             $"<b><color=#F87171>CAUSA DE DETENCIÓN:</color></b> <color=#E2E8F0>{detalleMotivo}</color>\n\n" +
                             "<b><color=#94A3B8>PROTOCOLO OBLIGATORIO (SEGURIDAD SOBRE META DE 8 HORAS):</color></b>\n" +
                             "<color=#E2E8F0>• Despachos bloqueados automáticamente y unidades en tránsito detenidas.</color>\n" +
                             "<color=#E2E8F0>• Se requiere inspección y autorización presencial del supervisor humano en piso.</color>\n" +
                             separador;

            AsignarTextoDetalleEmergencia(mensaje, true);
        }
    }

    public void MostrarAlertaIncertidumbreDemanda(
        RobotManagerEVE.EscenarioHorizonte escenario,
        float horaActual,
        float horasRestantes,
        int nuevosPedidos,
        float deficitSoC,
        int amrRequeridos,
        float porcentajeCarryOver,
        float pesoLoteKg)
    {
        tipoAlertaActual = TipoAlertaActiva.IncertidumbreDemandaWMS;
        RestaurarTextosBotonesModalEmergencia();
        ultimoEscenarioHorizonte = escenario;
        ultimoLotePedidos = nuevosPedidos;
        ultimosAmrRequeridos = amrRequeridos;
        ultimoPesoLoteKg = pesoLoteKg;
        ultimoDeficitSoC = deficitSoC;
        ultimoCarryOverPct = porcentajeCarryOver;

        MostrarEmergencia(true);

        if (textoDetalleEmergencia != null)
        {
            const string separador = "<color=#334155>--------------------------------------------------------------------</color>";
            string horaReloj = RelojSimuladorTurno.FormatearHoraRelojNocturno(horaActual);
            string restanteHms = RelojSimuladorTurno.FormatearDuracionHMS(horasRestantes);
            string cierreTexto = RelojSimuladorTurno.HoraCierreTurnoTexto;
            string mensaje;
            if (escenario == RobotManagerEVE.EscenarioHorizonte.EscenarioA_InicioTurno)
            {
                mensaje = "<size=20><b><color=#38BDF8>[i] ANALIZADOR HORIZONTE [ESCENARIO A - INICIO TURNO]: VIABILIDAD SLA OK</color></b></size>\n" +
                          $"<color=#E2E8F0>Reloj oficial: <b><color=#38BDF8>{horaReloj}</color></b> (Restan {restanteHms} hasta {cierreTexto}) | Lote: {nuevosPedidos} ped ({pesoLoteKg:F0} kg) | Déficit: <b><color=#FBBF24>-{deficitSoC:F1}% SoC</color></b>\n" +
                          "Dictamen: Tiempo suficiente antes de cierre, pero demanda (192.5%) supera el 60% nominal por unidad. Flota ideal: 3 AMRs (2 activos + 1 soporte/relevo inductivo).</color>\n" +
                          separador + "\n" +
                          "<b><color=#94A3B8>DECISIÓN GERENCIAL DE HORIZONTE TEMPRANO (INDUSTRIA 5.0)</color></b>\n\n" +
                          "<b><color=#F97316>> [OPCIÓN A: DIFERIR EXCEDENTE / MANTENER 2 AMRs]</color></b>\n" +
                          "<color=#E2E8F0>  Limitar operación al SoC actual sin habilitar el 3er AMR de relevo rotativo.</color>\n\n" +
                          "<b><color=#10B981>> [OPCIÓN B: APROBAR FLOTA DE 3 AMRs (2 ACTIVOS + 1 RELEVO)]</color></b>\n" +
                          "<color=#E2E8F0>  Cumplir 100% del SLA (0% Carry-over) con ciclos de recarga inductiva de 2h.</color>\n" +
                          separador;
            }
            else if (escenario == RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada)
            {
                mensaje = "<size=20><b><color=#F59E0B>[!] ANALIZADOR HORIZONTE [ESCENARIO B - MADRUGADA]: RIESGO MEDIO DE CORTE SLA</color></b></size>\n" +
                          $"<color=#E2E8F0>Reloj oficial: <b><color=#FBBF24>{horaReloj}</color></b> (Restan {restanteHms} hasta {cierreTexto}) | Rezago proyectado: <b><color=#FBBF24>{porcentajeCarryOver:F0}%</color></b> | Déficit: <b><color=#FBBF24>-{deficitSoC:F1}% SoC</color></b>\n" +
                          $"Dictamen: Tasa de absorción actual (10 ped/h) dejará {porcentajeCarryOver:F0}% del lote sin entregar al corte ({cierreTexto}). Incrementar a 4 AMRs o autorizar Carry-over al turno diurno.</color>\n" +
                          separador + "\n" +
                          "<b><color=#94A3B8>DECISIÓN GERENCIAL DE MADRUGADA (TRADE-OFF SLA vs. CAPEX)</color></b>\n\n" +
                          $"<b><color=#F97316>> [OPCIÓN A: AUTORIZAR CARRY-OVER ({porcentajeCarryOver:F0}% REZAGO AL TURNO DIURNO)]</color></b>\n" +
                          $"<color=#E2E8F0>  Mantener 2 AMRs actuales hasta las {cierreTexto} y transferir el saldo pendiente.</color>\n\n" +
                          $"<b><color=#10B981>> [OPCIÓN B: INCREMENTAR FLOTA A {amrRequeridos} AMRs OPERATIVOS]</color></b>\n" +
                          $"<color=#E2E8F0>  Duplicar tasa de absorción (20 ped/h) para eliminar el Carry-over antes de {cierreTexto}.</color>\n" +
                          separador;
            }
            else
            {
                mensaje = "<size=20><b><color=#FF453A>[!] ANALIZADOR HORIZONTE [ESCENARIO C - FINAL TURNO]: COLAPSO LOGÍSTICO / SLA</color></b></size>\n" +
                          $"<color=#E2E8F0>Reloj oficial: <b><color=#FF453A>{horaReloj}</color></b> (Restan {restanteHms} hasta {cierreTexto}) | Déficit Crítico LiFePO4: <b><color=#FBBF24>-{deficitSoC:F1}% SoC</color></b>\n" +
                          $"Dictamen: Cierre próximo y celdas en reserva. Capacidad actual colapsa ({porcentajeCarryOver:F0}% rezago). Se requieren {amrRequeridos} AMRs de emergencia para mantener HBF o diferir el 100% del lote.</color>\n" +
                          separador + "\n" +
                          "<b><color=#94A3B8>DECISIÓN GERENCIAL DE EMERGENCIA (CIERRE DE TURNO NOCTURNO)</color></b>\n\n" +
                          "<b><color=#F97316>> [OPCIÓN A: DIFERIR LOTE COMPLETO A TURNO DIURNO]</color></b>\n" +
                          "<color=#E2E8F0>  Proteger SLA actual y piso térmico del 20% LiFePO4 en las 2 unidades en pista.</color>\n\n" +
                          $"<b><color=#10B981>> [OPCIÓN B: APROBAR {amrRequeridos} AMRs DE EMERGENCIA (+1 BAHÍA TMS)]</color></b>\n" +
                          "<color=#E2E8F0>  Despliegue táctico inmediato para absorber las 42.5 t preservando el HBF.</color>\n" +
                          separador;
            }

            AsignarTextoDetalleEmergencia(mensaje, true);
        }
    }

    public void OnOverridePresionado()
    {
        if (tipoAlertaActual == TipoAlertaActiva.TurnoFaseCriticaSeguridad)
        {
            OnClickSilenciarAlarmaFabrica();
            return;
        }

        if (tipoAlertaActual == TipoAlertaActiva.IncertidumbreDemandaWMS)
        {
            ResolverDecisionHorizonte(false);
            return;
        }

        MostrarEmergencia(false);
        RobotManagerEVE eve = ResolverRobotManagerEVE();
        if (eve != null)
        {
            eve.AutorizarOverrideBateria();
        }
        else
        {
            RobotAMR robot = FindFirstObjectByType<RobotAMR>();
            if (robot != null)
            {
                robot.EjecutarOverride();
            }
        }
    }

    public void OnRelevoPresionado()
    {
        if (tipoAlertaActual == TipoAlertaActiva.TurnoFaseCriticaSeguridad)
        {
            MostrarEmergencia(false);
            RegistrarLog("<color=#F59E0B>[SUPERVISOR]:</color> Acuse de recibo de Fase Crítica registrado. Operación permanece detenida por seguridad.");
            return;
        }

        if (tipoAlertaActual == TipoAlertaActiva.IncertidumbreDemandaWMS)
        {
            ResolverDecisionHorizonte(true);
            return;
        }

        MostrarEmergencia(false);
        ResolverRobotManagerEVE()?.EjecutarRelevoFlota24_7();
    }

    public void OnAbortarMisionPresionado()
    {
        if (tipoAlertaActual == TipoAlertaActiva.TurnoFaseCriticaSeguridad)
        {
            MostrarEmergencia(false);
            return;
        }

        if (tipoAlertaActual == TipoAlertaActiva.IncertidumbreDemandaWMS)
        {
            ResolverDecisionHorizonte(false);
            return;
        }

        MostrarEmergencia(false);
        RobotManagerEVE eve = ResolverRobotManagerEVE();
        RobotAMR robot = (eve != null && eve.amrEnAlertaBateria20 != null)
            ? eve.amrEnAlertaBateria20
            : ((eve != null && eve.robotEjecutor != null) ? eve.robotEjecutor : FindFirstObjectByType<RobotAMR>());
        if (robot != null)
        {
            robot.AbortarMisionPorSeguridad();
        }
        if (eve != null) eve.amrEnAlertaBateria20 = null;
    }

    private void ResolverDecisionHorizonte(bool aprobarExpansion)
    {
        MostrarEmergencia(false);
        tipoAlertaActual = TipoAlertaActiva.BateriaCriticaAMR;
        ResolverRobotManagerEVE()?.NotificarResolucionAlertaDemanda();

        string msg;
        if (aprobarExpansion)
        {
            if (ultimoEscenarioHorizonte == RobotManagerEVE.EscenarioHorizonte.EscenarioA_InicioTurno)
            {
                msg = $"<color=#10B981><b>[DECISIÓN GERENCIAL - ESCENARIO A]:</b></color> Aprobada flota de {ultimosAmrRequeridos} AMRs (2 activos + 1 soporte/relevo inductivo 2h). Se absorben los {ultimoLotePedidos} pedidos con 0% Carry-over.";
            }
            else if (ultimoEscenarioHorizonte == RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada)
            {
                msg = $"<color=#10B981><b>[DECISIÓN GERENCIAL - ESCENARIO B]:</b></color> Aprobado incremento a {ultimosAmrRequeridos} AMRs operativos para eliminar el {ultimoCarryOverPct:F0}% de Carry-over antes de las {RelojSimuladorTurno.HoraCierreTurnoTexto}.";
            }
            else
            {
                msg = $"<color=#10B981><b>[DECISIÓN GERENCIAL - ESCENARIO C]:</b></color> Aprobado despliegue de {ultimosAmrRequeridos} AMRs de emergencia (+1 bahía TMS) para cubrir el déficit de -{ultimoDeficitSoC:F1}% SoC manteniendo HBF.";
            }
        }
        else
        {
            if (ultimoEscenarioHorizonte == RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada)
            {
                msg = $"<color=#F59E0B><b>[DECISIÓN GERENCIAL - ESCENARIO B]:</b></color> Autorizado Carry-over del {ultimoCarryOverPct:F0}% al turno diurno. Flota actual (2 AMRs) conserva piso seguro del 20% LiFePO4.";
            }
            else
            {
                msg = $"<color=#F59E0B><b>[DECISIÓN GERENCIAL - ADMISIÓN]:</b></color> Lote de {ultimoLotePedidos} pedidos ({ultimoPesoLoteKg:F0} kg) diferido a turno diurno. Protegido SLA y piso del 20% LiFePO4 en las 2 unidades actuales.";
            }
        }

        RegistrarLog(msg);
        Debug.Log(msg);
    }

    private static string FormatearColorEficiencia(float eficiencia)
    {
        if (eficiencia >= RobotManagerEVE.UmbralEficienciaSegundoAMR) return "#4ADE80";
        if (eficiencia >= RobotManagerEVE.UmbralEficienciaDualBuzon) return "#FBBF24";
        return "#F87171";
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
                if (slot == telemetriaAMR02 && estado == RobotAMR.EstadoRobot.Inactivo && modoReservaAMR02)
                {
                    slot.txtEstado.text = $"<color=#38BDF8>RESERVA / SOPORTE ({porcentaje * 100f:F0}%)</color>";
                }
                else if (slot == telemetriaAMR03 && estado == RobotAMR.EstadoRobot.Inactivo)
                {
                    slot.txtEstado.text = $"<color=#38BDF8>SOPORTE / RELEVO 24/7 ({porcentaje * 100f:F0}% SoC)</color>";
                }
                else
                {
                    string textoEstado = FormatearEstadoVisual(estado);
                    RobotManagerEVE eve = RobotManagerEVE.Instance;
                    bool enMisionActiva = estado == RobotAMR.EstadoRobot.En_Transito ||
                                          estado == RobotAMR.EstadoRobot.Extrayendo ||
                                          estado == RobotAMR.EstadoRobot.Transportando ||
                                          estado == RobotAMR.EstadoRobot.Entregando;
                    if (eve != null && eve.turnoIniciado && eve.palletsEnColaWMS > 0 && enMisionActiva &&
                        (slot == telemetriaAMR01 || slot == telemetriaAMR02))
                    {
                        float ef = slot == telemetriaAMR01 ? eve.EficienciaUnitariaAMR01 : eve.EficienciaUnitariaAMR02;
                        textoEstado += $" <color=#64748B>|</color> <color={FormatearColorEficiencia(ef)}>Ef: {ef:F0}%</color>";
                    }
                    slot.txtEstado.text = textoEstado;
                }
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

        if (telemetriaAMR03 != null && (id == telemetriaAMR03.idRobot || id.Contains("03") || id.Equals("Robot_AMR_03", System.StringComparison.OrdinalIgnoreCase)))
        {
            return telemetriaAMR03;
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
            case RobotAMR.EstadoRobot.Recargando: return "<color=#38BDF8>RECARGANDO (Inductivo 2h)</color>";
            case RobotAMR.EstadoRobot.Apagado: return "<color=#EF4444>APAGADO (<3% BMS)</color>";
            case RobotAMR.EstadoRobot.FueraDeServicio_Danado: return "<color=#DC2626>DAÑADO / FUERA DE SERVICIO</color>";
            default: return estado.ToString().ToUpper();
        }
    }

    public void ActualizarBateria(float porcentaje)
    {
        ActualizarBateria("AMR-01", porcentaje, RobotAMR.EstadoRobot.Inactivo);
    }

    public void ActualizarEstadoManifiesto(int indice, string nuevoEstado, Color color)
    {
        if (textosEstadoManifiesto != null && indice >= 0 && indice < textosEstadoManifiesto.Count)
        {
            if (textosEstadoManifiesto[indice] != null)
            {
                textosEstadoManifiesto[indice].text = nuevoEstado;
                textosEstadoManifiesto[indice].color = color;
            }
        }
        FindFirstObjectByType<GeneradorTablaManifiesto>(FindObjectsInactive.Include)?.RenderizarVista();
    }

    public void ConfigurarReservaAMR02(bool enReserva)
    {
        modoReservaAMR02 = enReserva;
        if (telemetriaAMR02 != null && telemetriaAMR02.txtEstado != null)
        {
            telemetriaAMR02.txtEstado.text = enReserva
                ? "<color=#38BDF8>RESERVA / SOPORTE (80%)</color>"
                : FormatearEstadoVisual(RobotAMR.EstadoRobot.Inactivo);
        }
    }

    public void ConfigurarEstadoSoporteAMR03(bool activo, string estadoTexto = "<color=#38BDF8>SOPORTE / RELEVO 24/7 (80% SoC)</color>")
    {
        if (panelFlotaRect == null) InicializarPanelesColapsables();
        if (filaAMR03 == null && panelFlotaRect != null)
        {
            Transform t = panelFlotaRect.Find("Scroll_Flota/Viewport/Content_Flota/Fila_AMR_03")
                       ?? panelFlotaRect.Find("Fila_AMR_03");
            if (t != null) filaAMR03 = t.gameObject;
        }

        amr03Habilitado = true;
        haSidoDesplegadoAMR03EnTurno = true;
        alturaOriginalFlota = 502f;

        RobotManagerEVE eve = ResolverRobotManagerEVE();
        float bateria03 = (eve != null && eve.robotAMR_03 != null) ? eve.robotAMR_03.bateriaActual : 80.0f;
        string textoFinal = activo && !string.IsNullOrEmpty(estadoTexto)
            ? estadoTexto
            : $"<color=#38BDF8>SOPORTE / RELEVO 24/7 ({bateria03:F0}% SoC)</color>";

        if (filaAMR03 != null)
        {
            filaAMR03.SetActive(true);

            if (telemetriaAMR03 != null)
            {
                if (telemetriaAMR03.barraBateriaRelleno != null) telemetriaAMR03.barraBateriaRelleno.fillAmount = Mathf.Clamp01(bateria03 / 100f);
                if (telemetriaAMR03.txtBateria != null) telemetriaAMR03.txtBateria.text = $"{bateria03:F1}%";
                if (telemetriaAMR03.txtEstado != null) telemetriaAMR03.txtEstado.text = textoFinal;
            }
            else
            {
                Transform tEstado = filaAMR03.transform.Find("Txt_Estado");
                if (tEstado != null)
                {
                    var tmp = tEstado.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = textoFinal;
                }
            }

            if (filaAMR03.transform.parent is RectTransform contentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }

        if (panelFlotaRect != null && flotaExpandida)
        {
            panelFlotaRect.sizeDelta = new Vector2(panelFlotaRect.sizeDelta.x, alturaOriginalFlota);
        }

        if (!SincronizarActivacionHUDSiAplica())
        {
            RefrescarEstadoSelectorCamaras(false);
        }

        RefrescarLayoutPanelFlota();
    }

    private Queue<string> logQueue = new Queue<string>();
    private const int MaxLineas = 60;
    private StringBuilder stringBuilderLog = new StringBuilder(4096);

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

        RobotManagerEVE eve = ResolverRobotManagerEVE();
        if (eve == null || !eve.turnoIniciado)
        {
            Transform hud = transform.Find("HUD_Operativo");
            if (hud != null) hud.gameObject.SetActive(false);
        }
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

        if (telemetriaAMR02 != null)
        {
            if (telemetriaAMR02.barraBateriaRelleno != null) telemetriaAMR02.barraBateriaRelleno.fillAmount = 0.80f;
            if (telemetriaAMR02.txtBateria != null) telemetriaAMR02.txtBateria.text = "80.0%";
            if (telemetriaAMR02.txtEstado != null) telemetriaAMR02.txtEstado.text = FormatearEstadoVisual(RobotAMR.EstadoRobot.Inactivo);
        }

        if (filaAMR03 != null)
        {
            filaAMR03.SetActive(true);
        }
        if (telemetriaAMR03 != null)
        {
            if (telemetriaAMR03.barraBateriaRelleno != null) telemetriaAMR03.barraBateriaRelleno.fillAmount = 0.80f;
            if (telemetriaAMR03.txtBateria != null) telemetriaAMR03.txtBateria.text = "80.0%";
            if (telemetriaAMR03.txtEstado != null) telemetriaAMR03.txtEstado.text = "<color=#38BDF8>SOPORTE / RELEVO 24/7 (80% SoC)</color>";
        }

        if (barraBateriaRelleno != null)
        {
            barraBateriaRelleno.fillAmount = 0.80f;
        }

        if (panelOverlayTimeLapse != null)
        {
            panelOverlayTimeLapse.SetActive(false);
        }

        if (textoLogTMP is TMP_Text tmpLog)
        {
            tmpLog.alignment = TextAlignmentOptions.BottomLeft;
        }
        if (textoLogLegacy != null)
        {
            textoLogLegacy.alignment = TextAnchor.LowerLeft;
        }

        ActualizarBannerTurno($"<b><color=#38BDF8>TURNO NOCTURNO:</color> {RelojSimuladorTurno.HoraActualFormateada} (Restan {RelojSimuladorTurno.TiempoRestanteFormateado} | Cierre {RelojSimuladorTurno.HoraCierreTurnoTexto}) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        InicializarPanelesColapsables();
        ConfigurarPopUpEjecutivoSLA();
        ConfigurarVentanaBuzonSupervisor();
        ConfigurarSelectorCamarasSCADA();
        ConfigurarBarraOperativaInferior();
        ConfigurarBotonIngestaJSON();
        ConfigurarControlesPlantaSCADA();
        ConfigurarTelemetriaDronSoporte();
        ConfigurarMarcadorNBAHud();
        ActualizarMarcadorNBAHud(true);
        RefrescarLayoutPanelFlota();

        RobotManagerEVE eve = ResolverRobotManagerEVE();
        if (eve == null || !eve.turnoIniciado)
        {
            Transform hud = transform.Find("HUD_Operativo");
            if (hud != null) hud.gameObject.SetActive(false);
        }

        hudOperativoTransform = transform.Find("HUD_Operativo");
        hudOperativoEstabaActivo = hudOperativoTransform != null && hudOperativoTransform.gameObject.activeInHierarchy;
        if (hudOperativoEstabaActivo)
        {
            RefrescarEstadoSelectorCamaras(true);
        }
    }

    void Update()
    {
        if (!SincronizarActivacionHUDSiAplica()
            && txtBtnCambiarCamara != null
            && txtBtnCambiarCamara.gameObject.activeInHierarchy)
        {
            RefrescarEstadoSelectorCamaras(false);
        }

        RobotManagerEVE eve = RobotManagerEVE.Instance;
        if (eve != null && eve.turnoIniciado &&
            !eve.operacionDetenidaPorSeguridad &&
            !eve.turnoEnFaseCritica &&
            !eve.admisionNuevosPedidosBloqueada &&
            !eve.EnCompresionTemporalActiva)
        {
            string horaAhora = RelojSimuladorTurno.HoraActualFormateada;
            string tiempoRestante = RelojSimuladorTurno.TiempoRestanteFormateado;
            bool pausada = eve.SimulacionPausada;
            bool tieneCola = eve.palletsEnColaWMS > 0;
            bool dualActivo = eve.flotaEscaladaDual;
            float efBanner = !tieneCola ? 100f : (dualActivo ? eve.EficienciaPromedioDual : eve.EficienciaUnitariaAMR01);
            string claveBanner = $"{horaAhora}|{tiempoRestante}|{(pausada ? 1 : 0)}|{(dualActivo ? 2 : 1)}|{(tieneCola ? 1 : 0)}|{Mathf.RoundToInt(efBanner)}";
            if (claveBanner != ultimaHoraBannerRegistrada)
            {
                ultimaHoraBannerRegistrada = claveBanner;
                string sufijoCierre = RelojSimuladorTurno.TurnoSuperoCierre
                    ? $"CIERRE ALCANZADO ({RelojSimuladorTurno.HoraCierreTurnoTexto})"
                    : $"Restan {tiempoRestante} | {RelojSimuladorTurno.HoraCierreTurnoTexto}";
                string sufijoPausa = pausada ? " | <color=#F59E0B>PAUSA</color>" : string.Empty;
                string colorEf = FormatearColorEficiencia(efBanner);
                string textoEf = !tieneCola
                    ? $"<color=#4ADE80>Ef: 100%</color>"
                    : (dualActivo
                        ? $"Ef. Dual (2 AMR): <color={colorEf}>{efBanner:F0}%</color>"
                        : $"Ef. 1 AMR: <color={colorEf}>{efBanner:F0}%</color>");
                ActualizarBannerTurno($"<b><color=#38BDF8>TURNO:</color> {horaAhora} ({sufijoCierre}) | {textoEf} | <color=#F59E0B>LiFePO4</color>{sufijoPausa}</b>");
            }
        }

        if (hudOperativoTransform != null && hudOperativoTransform.gameObject.activeInHierarchy)
        {
            ActualizarMarcadorNBAHud(false);
        }

        if (eve != null && eve.turnoIniciado && Time.unscaledTime >= tiempoProximoCheckBtnJson)
        {
            tiempoProximoCheckBtnJson = Time.unscaledTime + 0.25f;
            ActualizarEstadoBotonIngestaJSON();
        }
    }

    private bool SincronizarActivacionHUDSiAplica()
    {
        if (hudOperativoTransform == null)
        {
            hudOperativoTransform = transform.Find("HUD_Operativo");
        }

        bool hudActivoAhora = hudOperativoTransform != null && hudOperativoTransform.gameObject.activeInHierarchy;
        if (hudActivoAhora && !hudOperativoEstabaActivo)
        {
            hudOperativoEstabaActivo = true;
            ConfigurarSelectorCamarasSCADA();
            ConfigurarBarraOperativaInferior();
            ConfigurarBotonIngestaJSON();
            ConfigurarControlesPlantaSCADA();
            ConfigurarTelemetriaDronSoporte();
            ConfigurarMarcadorNBAHud();
            ActualizarMarcadorNBAHud(true);
            RefrescarEstadoSelectorCamaras(true);
            return true;
        }

        if (!hudActivoAhora)
        {
            hudOperativoEstabaActivo = false;
        }

        return false;
    }

    public void ConfigurarTelemetriaDronSoporte()
    {
        if (filaDronSoporte == null)
        {
            Transform tFila = transform.Find("HUD_Operativo/Panel_Flota/Scroll_Flota/Viewport/Content_Flota/Fila_Dron_Soporte");
            if (tFila != null) filaDronSoporte = tFila.gameObject;
        }

        if (filaDronSoporte != null)
        {
            if (txtEstadoDronSoporte == null)
            {
                Transform tEstado = filaDronSoporte.transform.Find("Txt_EstadoDron")
                                 ?? filaDronSoporte.transform.Find("Txt_EstadoEVE");
                if (tEstado != null) txtEstadoDronSoporte = tEstado.GetComponent<TextMeshProUGUI>();
            }
            if (txtSistemaDronSoporte == null)
            {
                Transform tSistema = filaDronSoporte.transform.Find("Txt_SistemaDron")
                                  ?? filaDronSoporte.transform.Find("Txt_BateriaEVE");
                if (tSistema != null) txtSistemaDronSoporte = tSistema.GetComponent<TextMeshProUGUI>();
            }
        }

        DronMantenimiento dron = FindFirstObjectByType<DronMantenimiento>(FindObjectsInactive.Include);
        if (dronSuscritoTelemetria != dron)
        {
            if (dronSuscritoTelemetria != null)
            {
                dronSuscritoTelemetria.OnEstadoDronCambiado -= ActualizarEstadoDronSoporte;
            }
            dronSuscritoTelemetria = dron;
        }

        if (dronSuscritoTelemetria != null)
        {
            dronSuscritoTelemetria.OnEstadoDronCambiado -= ActualizarEstadoDronSoporte;
            dronSuscritoTelemetria.OnEstadoDronCambiado += ActualizarEstadoDronSoporte;
            ActualizarEstadoDronSoporte(dronSuscritoTelemetria.estadoOperativoActual, dronSuscritoTelemetria.descripcionEstadoActual);
        }
        else
        {
            ActualizarEstadoDronSoporte(DronMantenimiento.EstadoOperativoDron.EnBase, "EN BASE (STANDBY)");
        }
    }

    public void ActualizarEstadoDronSoporte(DronMantenimiento.EstadoOperativoDron estado, string detalle)
    {
        if (filaDronSoporte == null)
        {
            Transform tFila = transform.Find("HUD_Operativo/Panel_Flota/Scroll_Flota/Viewport/Content_Flota/Fila_Dron_Soporte");
            if (tFila != null) filaDronSoporte = tFila.gameObject;
        }
        if (txtEstadoDronSoporte == null && filaDronSoporte != null)
        {
            Transform tEstado = filaDronSoporte.transform.Find("Txt_EstadoDron")
                             ?? filaDronSoporte.transform.Find("Txt_EstadoEVE");
            if (tEstado != null) txtEstadoDronSoporte = tEstado.GetComponent<TextMeshProUGUI>();
        }
        if (txtSistemaDronSoporte == null && filaDronSoporte != null)
        {
            Transform tSistema = filaDronSoporte.transform.Find("Txt_SistemaDron")
                              ?? filaDronSoporte.transform.Find("Txt_BateriaEVE");
            if (tSistema != null) txtSistemaDronSoporte = tSistema.GetComponent<TextMeshProUGUI>();
        }

        if (txtEstadoDronSoporte != null)
        {
            string colorHex;
            string etiqueta;
            switch (estado)
            {
                case DronMantenimiento.EstadoOperativoDron.DespejandoPasillo:
                    colorHex = "#FACC15";
                    etiqueta = string.IsNullOrEmpty(detalle) ? "DESPEJANDO PASILLO" : detalle;
                    break;
                case DronMantenimiento.EstadoOperativoDron.RescatandoAMR:
                    colorHex = "#38BDF8";
                    etiqueta = string.IsNullOrEmpty(detalle) ? "RESCATANDO AMR" : detalle;
                    break;
                case DronMantenimiento.EstadoOperativoDron.TrasladandoAZonaTecnica:
                    colorHex = "#EF4444";
                    etiqueta = string.IsNullOrEmpty(detalle) ? "TRASLADANDO A ZONA TÉCNICA" : detalle;
                    break;
                case DronMantenimiento.EstadoOperativoDron.EnBase:
                default:
                    colorHex = "#4ADE80";
                    etiqueta = string.IsNullOrEmpty(detalle) ? "EN BASE (STANDBY)" : detalle;
                    break;
            }

            txtEstadoDronSoporte.text = $"Estado: <color={colorHex}>{etiqueta}</color>";
        }

        if (txtSistemaDronSoporte != null)
        {
            txtSistemaDronSoporte.text = estado == DronMantenimiento.EstadoOperativoDron.EnBase
                ? "Sistema: Grúa Ventosa / Techo"
                : "Sistema: Operación Aérea Activa (ISO)";
        }
    }

    private void InicializarPanelesColapsables()
    {
        if (panelManifiestoRect == null)
        {
            Transform t = transform.Find("HUD_Operativo/Manifiesto");
            if (t != null) panelManifiestoRect = t.GetComponent<RectTransform>();
        }
        if (panelFlotaRect == null)
        {
            Transform t = transform.Find("HUD_Operativo/Panel_Flota");
            if (t != null) panelFlotaRect = t.GetComponent<RectTransform>();
        }
        if (panelTerminalRect == null)
        {
            Transform t = transform.Find("HUD_Operativo/Panel_Terminal_EVE");
            if (t != null) panelTerminalRect = t.GetComponent<RectTransform>();
        }

        if (panelManifiestoRect != null && panelManifiestoRect.sizeDelta.y > 34f)
        {
            alturaOriginalManifiesto = panelManifiestoRect.sizeDelta.y;
        }
        if (panelFlotaRect != null && panelFlotaRect.sizeDelta.y > 74f)
        {
            alturaOriginalFlota = panelFlotaRect.sizeDelta.y;
        }
        if (panelTerminalRect != null && panelTerminalRect.sizeDelta.y > 34f)
        {
            alturaOriginalTerminal = panelTerminalRect.sizeDelta.y;
        }

        if (txtBtnToggleManifiesto == null && panelManifiestoRect != null)
        {
            var t = panelManifiestoRect.Find("Header_Manifiesto/Btn_Toggle_Manifiesto/Text (TMP)");
            if (t != null) txtBtnToggleManifiesto = t.GetComponent<TextMeshProUGUI>();
        }
        if (txtBtnToggleFlota == null && panelFlotaRect != null)
        {
            var t = panelFlotaRect.Find("Header_Flota/Btn_Toggle_Flota/Text (TMP)");
            if (t != null) txtBtnToggleFlota = t.GetComponent<TextMeshProUGUI>();
        }
        if (txtBtnToggleTerminal == null && panelTerminalRect != null)
        {
            var t = panelTerminalRect.Find("Header_Terminal/Btn_Toggle_Terminal/Text (TMP)");
            if (t != null) txtBtnToggleTerminal = t.GetComponent<TextMeshProUGUI>();
        }
    }

    public void TogglePanelManifiesto()
    {
        AlternarPanelManifiesto(!manifiestoExpandido);
    }

    public void TogglePanelFlota()
    {
        AlternarPanelFlota(!flotaExpandida);
    }

    public void TogglePanelTerminal()
    {
        AlternarPanelTerminal(!terminalExpandido);
    }

    public void AlternarPanelManifiesto(bool expandir)
    {
        if (panelManifiestoRect == null) InicializarPanelesColapsables();
        if (panelManifiestoRect == null) return;

        manifiestoExpandido = expandir;
        alturaOriginalManifiesto = 280f;

        if (expandir)
        {
            panelManifiestoRect.sizeDelta = new Vector2(panelManifiestoRect.sizeDelta.x, alturaOriginalManifiesto);
            for (int i = 0; i < panelManifiestoRect.childCount; i++)
            {
                Transform child = panelManifiestoRect.GetChild(i);
                if (child.name != "Header_Manifiesto")
                {
                    child.gameObject.SetActive(true);
                }
            }
            if (txtBtnToggleManifiesto != null) txtBtnToggleManifiesto.text = "[-]";
        }
        else
        {
            for (int i = 0; i < panelManifiestoRect.childCount; i++)
            {
                Transform child = panelManifiestoRect.GetChild(i);
                if (child.name != "Header_Manifiesto")
                {
                    child.gameObject.SetActive(false);
                }
            }
            panelManifiestoRect.sizeDelta = new Vector2(panelManifiestoRect.sizeDelta.x, 34f);
            if (txtBtnToggleManifiesto != null) txtBtnToggleManifiesto.text = "[+]";
        }
    }

    public void AlternarPanelFlota(bool expandir)
    {
        if (panelFlotaRect == null) InicializarPanelesColapsables();
        if (panelFlotaRect == null) return;

        flotaExpandida = expandir;
        alturaOriginalFlota = 502f;
        Transform scrollFlota = panelFlotaRect.Find("Scroll_Flota");

        if (expandir)
        {
            panelFlotaRect.sizeDelta = new Vector2(panelFlotaRect.sizeDelta.x, alturaOriginalFlota);
            if (scrollFlota != null)
            {
                for (int i = 0; i < panelFlotaRect.childCount; i++)
                {
                    panelFlotaRect.GetChild(i).gameObject.SetActive(true);
                }
                Transform fila03 = scrollFlota.Find("Viewport/Content_Flota/Fila_AMR_03");
                if (fila03 != null) fila03.gameObject.SetActive(true);
            }
            else
            {
                for (int i = 0; i < panelFlotaRect.childCount; i++)
                {
                    Transform child = panelFlotaRect.GetChild(i);
                    child.gameObject.SetActive(true);
                }
            }
            if (txtBtnToggleFlota != null) txtBtnToggleFlota.text = "[-]";
            RefrescarLayoutPanelFlota();
        }
        else
        {
            // Conserva Header_Flota y Banner_Turno_Contextual (norma ISA-101)
            for (int i = 0; i < panelFlotaRect.childCount; i++)
            {
                Transform child = panelFlotaRect.GetChild(i);
                bool conservar = child.name == "Header_Flota" || child.name == "Banner_Turno_Contextual";
                child.gameObject.SetActive(conservar);
            }
            panelFlotaRect.sizeDelta = new Vector2(panelFlotaRect.sizeDelta.x, 74f);
            if (txtBtnToggleFlota != null) txtBtnToggleFlota.text = "[+]";
        }
    }

    private void RefrescarLayoutPanelFlota()
    {
        if (panelFlotaRect == null)
        {
            InicializarPanelesColapsables();
        }
        if (panelFlotaRect == null) return;

        Transform scrollTr = panelFlotaRect.Find("Scroll_Flota");
        ScrollRect scrollRect = scrollTr != null ? scrollTr.GetComponent<ScrollRect>() : null;
        if (scrollRect != null)
        {
            scrollRect.scrollSensitivity = 28f;
        }

        RectTransform contentRect = panelFlotaRect.Find("Scroll_Flota/Viewport/Content_Flota") as RectTransform;
        Canvas.ForceUpdateCanvases();
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelFlotaRect);
    }

    public void AlternarPanelTerminal(bool expandir)
    {
        if (panelTerminalRect == null) InicializarPanelesColapsables();
        if (panelTerminalRect == null) return;

        terminalExpandido = expandir;

        ScrollRect scroll = scrollTerminalEVE != null ? scrollTerminalEVE : panelTerminalRect.GetComponent<ScrollRect>();
        Transform viewport = panelTerminalRect.Find("Viewport");
        Transform scrollbar = panelTerminalRect.Find("Scrollbar_Vertical");

        if (expandir)
        {
            float targetHeight = alturaOriginalTerminal > 34f ? alturaOriginalTerminal : 520f;
            panelTerminalRect.sizeDelta = new Vector2(panelTerminalRect.sizeDelta.x, targetHeight);
            if (viewport != null) viewport.gameObject.SetActive(true);
            if (scrollbar != null) scrollbar.gameObject.SetActive(true);
            if (scroll != null)
            {
                scroll.enabled = true;
                scroll.verticalNormalizedPosition = 0f;
            }
            if (txtBtnToggleTerminal != null) txtBtnToggleTerminal.text = "[-]";
        }
        else
        {
            if (panelTerminalRect.sizeDelta.y > 34f)
            {
                alturaOriginalTerminal = panelTerminalRect.sizeDelta.y;
            }
            if (scroll != null) scroll.enabled = false;
            if (viewport != null) viewport.gameObject.SetActive(false);
            if (scrollbar != null) scrollbar.gameObject.SetActive(false);
            panelTerminalRect.sizeDelta = new Vector2(panelTerminalRect.sizeDelta.x, 34f);
            if (txtBtnToggleTerminal != null) txtBtnToggleTerminal.text = "[+]";
        }
    }

    public void MostrarOverlayTimeLapse(bool mostrar)
    {
        if (panelOverlayTimeLapse != null)
        {
            panelOverlayTimeLapse.SetActive(false);
        }
    }

    public void ActualizarProgresoTimeLapse(float t)
    {
        if (panelOverlayTimeLapse != null)
        {
            panelOverlayTimeLapse.SetActive(false);
        }
    }

    public void ConfigurarMarcadorNBAHud()
    {
        if (panelMarcadorNBAHud == null)
        {
            Transform tPanel = transform.Find("HUD_Operativo/Panel_Marcador_NBA_HUD");
            if (tPanel != null) panelMarcadorNBAHud = tPanel.gameObject;
        }

        if (panelMarcadorNBAHud == null) return;

        if (txtHoraTurnoNBA == null)
        {
            Transform t = panelMarcadorNBAHud.transform.Find("Txt_Hora_Turno_NBA");
            if (t != null) txtHoraTurnoNBA = t.GetComponent<TextMeshProUGUI>();
        }
        if (txtRestanteTurnoNBA == null)
        {
            Transform t = panelMarcadorNBAHud.transform.Find("Txt_Restante_Turno_NBA");
            if (t != null) txtRestanteTurnoNBA = t.GetComponent<TextMeshProUGUI>();
        }
        if (txtEstadoTurnoNBA == null)
        {
            Transform t = panelMarcadorNBAHud.transform.Find("Txt_Estado_Turno_NBA");
            if (t != null) txtEstadoTurnoNBA = t.GetComponent<TextMeshProUGUI>();
        }
        if (imgAcentoMarcadorNBA == null)
        {
            Transform t = panelMarcadorNBAHud.transform.Find("Franja_Acento_NBA");
            if (t != null) imgAcentoMarcadorNBA = t.GetComponent<Image>();
        }
    }

    public void ActualizarMarcadorNBAHud(bool forzar = false)
    {
        if (panelMarcadorNBAHud == null)
        {
            ConfigurarMarcadorNBAHud();
        }
        if (panelMarcadorNBAHud == null) return;

        string horaActual = RelojSimuladorTurno.HoraActualFormateada;
        string tiempoRestante = RelojSimuladorTurno.TiempoRestanteFormateado;
        float horasRestantes = RelojSimuladorTurno.HorasRestantesHastaCierre;
        bool cierre = RelojSimuladorTurno.TurnoSuperoCierre;

        RobotManagerEVE eve = RobotManagerEVE.Instance;
        bool turnoIniciado = eve != null && eve.turnoIniciado;
        bool faseCritica = eve != null && (eve.turnoEnFaseCritica || eve.operacionDetenidaPorSeguridad);
        bool pausada = eve != null && eve.SimulacionPausada;
        bool bloqueoAdmision = eve != null && eve.admisionNuevosPedidosBloqueada;
        bool dualActivo = eve != null && eve.flotaEscaladaDual;
        bool tieneCola = eve != null && eve.palletsEnColaWMS > 0;
        float eficiencia = !tieneCola ? 100f : (eve != null ? (dualActivo ? eve.EficienciaPromedioDual : eve.EficienciaUnitariaAMR01) : 100f);

        string estadoTexto;
        Color colorAcento;
        Color colorRestante;

        if (faseCritica)
        {
            estadoTexto = "PARADA SEGURIDAD";
            colorAcento = new Color(0.94f, 0.27f, 0.27f, 1f);
            colorRestante = colorAcento;
        }
        else if (cierre)
        {
            estadoTexto = $"CIERRE {RelojSimuladorTurno.HoraCierreTurnoTexto}";
            colorAcento = new Color(0.94f, 0.27f, 0.27f, 1f);
            colorRestante = colorAcento;
        }
        else if (pausada)
        {
            estadoTexto = "PAUSA SUPERVISOR";
            colorAcento = new Color(0.98f, 0.75f, 0.14f, 1f);
            colorRestante = colorAcento;
        }
        else if (bloqueoAdmision)
        {
            estadoTexto = "CONTINUIDAD";
            colorAcento = new Color(0.98f, 0.75f, 0.14f, 1f);
            colorRestante = horasRestantes <= 0.5f ? new Color(0.94f, 0.27f, 0.27f, 1f) : colorAcento;
        }
        else if (!turnoIniciado)
        {
            estadoTexto = "STANDBY • 8H";
            colorAcento = new Color(0.22f, 0.74f, 0.97f, 1f);
            colorRestante = new Color(0.98f, 0.75f, 0.14f, 1f);
        }
        else
        {
            string prefijo = dualActivo ? "DUAL 2 AMR" : "1 AMR";
            estadoTexto = $"{prefijo} • EF {eficiencia:F0}%";
            colorAcento = horasRestantes <= 0.5f
                ? new Color(0.94f, 0.27f, 0.27f, 1f)
                : (tieneCola ? new Color(0.29f, 0.87f, 0.50f, 1f) : new Color(0.22f, 0.74f, 0.97f, 1f));
            colorRestante = horasRestantes <= 0.5f
                ? new Color(0.94f, 0.27f, 0.27f, 1f)
                : new Color(0.98f, 0.75f, 0.14f, 1f);
        }

        string clave = $"{horaActual}|{tiempoRestante}|{estadoTexto}|{colorAcento.r:F2}{colorRestante.r:F2}";
        if (!forzar && clave == ultimaClaveMarcadorNBA)
        {
            return;
        }
        ultimaClaveMarcadorNBA = clave;

        if (txtHoraTurnoNBA != null) txtHoraTurnoNBA.text = horaActual;
        if (txtRestanteTurnoNBA != null)
        {
            txtRestanteTurnoNBA.text = tiempoRestante;
            txtRestanteTurnoNBA.color = colorRestante;
        }
        if (txtEstadoTurnoNBA != null)
        {
            txtEstadoTurnoNBA.text = estadoTexto;
            txtEstadoTurnoNBA.color = colorAcento;
        }
        if (imgAcentoMarcadorNBA != null) imgAcentoMarcadorNBA.color = colorAcento;
    }

    public void ActualizarBannerTurno(string texto)
    {
        if (txtBannerTurno == null)
        {
            Transform tBanner = transform.Find("HUD_Operativo/Panel_Flota/Banner_Turno_Contextual/Txt_Banner");
            if (tBanner != null) txtBannerTurno = tBanner.GetComponent<TextMeshProUGUI>();
        }

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
        if (dronSuscritoTelemetria != null)
        {
            dronSuscritoTelemetria.OnEstadoDronCambiado -= ActualizarEstadoDronSoporte;
            dronSuscritoTelemetria = null;
        }
        if (eveSuscritoPausa != null)
        {
            eveSuscritoPausa.OnPausaCambiada -= ActualizarVisualBotonPausa;
            eveSuscritoPausa = null;
        }
        if (eveSuscritoAdmisionJSON != null)
        {
            eveSuscritoAdmisionJSON.OnEstadoAdmisionJSONCambiado -= ActualizarEstadoBotonIngestaJSON;
            eveSuscritoAdmisionJSON = null;
        }

        if (audioSuscritoAlarma != null)
        {
            audioSuscritoAlarma.OnEstadoAlarmaFabricaCambiado -= OnEstadoAlarmaFabricaCambiado;
            audioSuscritoAlarma = null;
        }

        if (tmsSuscritoMuelle != null)
        {
            tmsSuscritoMuelle.OnEstadoClausuraMuelleCambiado -= OnEstadoClausuraMuelleCambiado;
            tmsSuscritoMuelle = null;
        }

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void MostrarPopUpEjecutivoSLA(string titulo, string detalle, float duracion = 4.5f)
    {
        AsegurarPopupSLA();
        if (panelPopupSLA == null) return;

        panelPopupSLA.SetActive(true);
        if (panelBuzonSupervisor != null && panelBuzonSupervisor.activeSelf)
        {
            panelBuzonSupervisor.transform.SetAsLastSibling();
        }
        if (txtTituloPopupSLA != null) txtTituloPopupSLA.text = titulo;
        if (txtDetallePopupSLA != null) txtDetallePopupSLA.text = detalle;

        if (rutinaAutoCierrePopup != null)
        {
            StopCoroutine(rutinaAutoCierrePopup);
        }
        rutinaAutoCierrePopup = StartCoroutine(AutoCerrarPopupSLA(Mathf.Max(0.5f, duracion)));
    }

    private IEnumerator AutoCerrarPopupSLA(float duracion)
    {
        yield return new WaitForSecondsRealtime(duracion);
        if (panelPopupSLA != null)
        {
            panelPopupSLA.SetActive(false);
        }
        rutinaAutoCierrePopup = null;
    }

    public void CerrarPopUpEjecutivoSLA()
    {
        if (rutinaAutoCierrePopup != null)
        {
            StopCoroutine(rutinaAutoCierrePopup);
            rutinaAutoCierrePopup = null;
        }
        if (panelPopupSLA != null)
        {
            panelPopupSLA.SetActive(false);
        }
    }

    public void MostrarPopUpRechazo(string motivo)
    {
        MostrarPopUpEjecutivoSLA("<color=#EF4444>RECHAZO DE INGESTA ERP</color>", motivo, 4.5f);
    }

    public void LimpiarHistorialBuzonSupervisor()
    {
        historialFeedbackSupervisor.Clear();
        contadorFeedbackBuzon = 0;

        if (txtBtnBuzonSupervisor == null)
        {
            Transform tBtn = transform.Find("HUD_Operativo/Panel_Controles_SCADA/Btn_Buzon_Supervisor/Text (TMP)");
            if (tBtn != null) txtBtnBuzonSupervisor = tBtn.GetComponent<TextMeshProUGUI>();
        }
        if (txtBtnBuzonSupervisor != null)
        {
            txtBtnBuzonSupervisor.text = "Buzón [0]";
        }
        if (panelBuzonSupervisor != null)
        {
            panelBuzonSupervisor.SetActive(false);
        }
    }

    public void RegistrarFeedbackSupervisor(string informeTecnico)
    {
        if (string.IsNullOrEmpty(informeTecnico)) return;

        historialFeedbackSupervisor.Add(informeTecnico);
        contadorFeedbackBuzon = historialFeedbackSupervisor.Count;

        if (txtBtnBuzonSupervisor == null)
        {
            Transform tBtn = transform.Find("HUD_Operativo/Panel_Controles_SCADA/Btn_Buzon_Supervisor/Text (TMP)");
            if (tBtn != null) txtBtnBuzonSupervisor = tBtn.GetComponent<TextMeshProUGUI>();
        }

        if (txtBtnBuzonSupervisor != null)
        {
            txtBtnBuzonSupervisor.text = $"Buzón [{contadorFeedbackBuzon}]";
        }

        if (panelBuzonSupervisor != null && panelBuzonSupervisor.activeSelf)
        {
            RefrescarContenidoVentanaBuzon(posicionarAlFinal: true);
        }
    }

    public string ConstruirTextoHistorialBuzonNumerado()
    {
        if (historialFeedbackSupervisor == null || historialFeedbackSupervisor.Count == 0)
        {
            return "<color=#94A3B8>Sin reportes técnicos ni contingencias registradas en el turno actual.</color>";
        }

        sbBuzon.Clear();
        for (int i = 0; i < historialFeedbackSupervisor.Count; i++)
        {
            int numero = i + 1;
            sbBuzon.Append("<b><color=#38BDF8>#").Append(numero).Append("</color></b>  ")
                   .Append(historialFeedbackSupervisor[i]);

            if (i < historialFeedbackSupervisor.Count - 1)
            {
                sbBuzon.Append("\n<color=#1E293B>────────────────────────────────────────────────────────</color>\n");
            }
        }
        return sbBuzon.ToString();
    }

    public void MostrarUltimoFeedbackBuzon()
    {
        AbrirVentanaBuzonSupervisor();
    }

    public void AbrirVentanaBuzonSupervisor()
    {
        RobotManagerEVE eve = ResolverRobotManagerEVE();
        if (eve != null && Application.isPlaying && !eve.turnoIniciado) return;

        AsegurarVentanaBuzonSupervisor();
        if (panelBuzonSupervisor == null) return;

        panelBuzonSupervisor.SetActive(true);
        panelBuzonSupervisor.transform.SetAsLastSibling();
        RefrescarContenidoVentanaBuzon(posicionarAlFinal: true);
    }

    public void CerrarVentanaBuzonSupervisor()
    {
        if (panelBuzonSupervisor != null)
        {
            panelBuzonSupervisor.SetActive(false);
        }
    }

    public void RefrescarContenidoVentanaBuzon(bool posicionarAlFinal = false)
    {
        if (txtTituloBuzonSupervisor != null)
        {
            int n = historialFeedbackSupervisor != null ? historialFeedbackSupervisor.Count : 0;
            string sufijo = n == 1 ? "1 REPORTE" : $"{n} REPORTES";
            txtTituloBuzonSupervisor.text = $"BUZÓN DEL SUPERVISOR ({sufijo})";
        }

        if (txtListadoBuzonSupervisor != null)
        {
            txtListadoBuzonSupervisor.text = ConstruirTextoHistorialBuzonNumerado();
        }

        if (contentBuzonSupervisor != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentBuzonSupervisor);
            Canvas.ForceUpdateCanvases();
        }

        if (scrollBuzonSupervisor != null)
        {
            scrollBuzonSupervisor.verticalNormalizedPosition =
                (posicionarAlFinal && historialFeedbackSupervisor != null && historialFeedbackSupervisor.Count > 1)
                    ? 0f
                    : 1f;
        }
    }

    public void ConfigurarVentanaBuzonSupervisor()
    {
        if (btnBuzonSupervisor == null)
        {
            Transform tBtn = transform.Find("HUD_Operativo/Panel_Controles_SCADA/Btn_Buzon_Supervisor");
            if (tBtn != null) btnBuzonSupervisor = tBtn.GetComponent<Button>();
        }
        if (btnBuzonSupervisor != null)
        {
            btnBuzonSupervisor.onClick.RemoveListener(MostrarUltimoFeedbackBuzon);
            if (btnBuzonSupervisor.onClick.GetPersistentEventCount() == 0)
            {
                btnBuzonSupervisor.onClick.AddListener(MostrarUltimoFeedbackBuzon);
            }
        }

        if (panelBuzonSupervisor == null)
        {
            Transform tPanel = transform.Find("HUD_Operativo/Panel_Buzon_Supervisor");
            if (tPanel != null) panelBuzonSupervisor = tPanel.gameObject;
        }
        if (panelBuzonSupervisor == null) return;

        if (txtTituloBuzonSupervisor == null)
        {
            Transform t = panelBuzonSupervisor.transform.Find("Header_Buzon_Supervisor/Txt_Titulo_Buzon_Supervisor");
            if (t != null) txtTituloBuzonSupervisor = t.GetComponent<TextMeshProUGUI>();
        }
        if (btnCerrarBuzonSupervisor == null)
        {
            Transform t = panelBuzonSupervisor.transform.Find("Header_Buzon_Supervisor/Btn_Cerrar_Buzon_Supervisor");
            if (t != null) btnCerrarBuzonSupervisor = t.GetComponent<Button>();
        }
        if (scrollBuzonSupervisor == null)
        {
            Transform t = panelBuzonSupervisor.transform.Find("Scroll_Buzon_Supervisor");
            if (t != null) scrollBuzonSupervisor = t.GetComponent<ScrollRect>();
        }
        if (contentBuzonSupervisor == null)
        {
            Transform t = panelBuzonSupervisor.transform.Find("Scroll_Buzon_Supervisor/Viewport/Content_Buzon_Supervisor");
            if (t != null) contentBuzonSupervisor = t.GetComponent<RectTransform>();
        }
        if (txtListadoBuzonSupervisor == null)
        {
            Transform t = panelBuzonSupervisor.transform.Find("Scroll_Buzon_Supervisor/Viewport/Content_Buzon_Supervisor/Txt_Listado_Buzon_Supervisor");
            if (t != null) txtListadoBuzonSupervisor = t.GetComponent<TextMeshProUGUI>();
        }

        if (btnCerrarBuzonSupervisor != null)
        {
            btnCerrarBuzonSupervisor.onClick.RemoveListener(CerrarVentanaBuzonSupervisor);
            if (btnCerrarBuzonSupervisor.onClick.GetPersistentEventCount() == 0)
            {
                btnCerrarBuzonSupervisor.onClick.AddListener(CerrarVentanaBuzonSupervisor);
            }
        }
    }

    private void AsegurarVentanaBuzonSupervisor()
    {
        ConfigurarVentanaBuzonSupervisor();
        if (panelBuzonSupervisor != null) return;

        Transform padreHUD = transform.Find("HUD_Operativo");
        Transform contenedor = padreHUD != null ? padreHUD : transform;

        panelBuzonSupervisor = new GameObject("Panel_Buzon_Supervisor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        panelBuzonSupervisor.transform.SetParent(contenedor, false);
        RectTransform rt = panelBuzonSupervisor.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(680f, 430f);
        rt.anchoredPosition = new Vector2(35f, -30f);
        panelBuzonSupervisor.GetComponent<Image>().color = new Color(0.055f, 0.082f, 0.133f, 0.97f);
        Outline outline = panelBuzonSupervisor.GetComponent<Outline>();
        outline.effectColor = new Color(0.22f, 0.74f, 0.97f, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject header = new GameObject("Header_Buzon_Supervisor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        header.transform.SetParent(panelBuzonSupervisor.transform, false);
        RectTransform rtHeader = header.GetComponent<RectTransform>();
        rtHeader.anchorMin = new Vector2(0f, 1f);
        rtHeader.anchorMax = new Vector2(1f, 1f);
        rtHeader.pivot = new Vector2(0.5f, 1f);
        rtHeader.offsetMin = new Vector2(0f, -48f);
        rtHeader.offsetMax = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.09f, 0.13f, 0.20f, 1f);

        GameObject goTitulo = new GameObject("Txt_Titulo_Buzon_Supervisor", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goTitulo.transform.SetParent(header.transform, false);
        RectTransform rtTit = goTitulo.GetComponent<RectTransform>();
        rtTit.anchorMin = Vector2.zero;
        rtTit.anchorMax = Vector2.one;
        rtTit.offsetMin = new Vector2(18f, 4f);
        rtTit.offsetMax = new Vector2(-120f, -4f);
        txtTituloBuzonSupervisor = goTitulo.GetComponent<TextMeshProUGUI>();
        txtTituloBuzonSupervisor.fontSize = 15.5f;
        txtTituloBuzonSupervisor.fontStyle = FontStyles.Bold;
        txtTituloBuzonSupervisor.alignment = TextAlignmentOptions.MidlineLeft;
        txtTituloBuzonSupervisor.color = Color.white;
        txtTituloBuzonSupervisor.raycastTarget = false;

        GameObject goCerrar = new GameObject("Btn_Cerrar_Buzon_Supervisor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        goCerrar.transform.SetParent(header.transform, false);
        RectTransform rtCerrar = goCerrar.GetComponent<RectTransform>();
        rtCerrar.anchorMin = new Vector2(1f, 0.5f);
        rtCerrar.anchorMax = new Vector2(1f, 0.5f);
        rtCerrar.pivot = new Vector2(1f, 0.5f);
        rtCerrar.sizeDelta = new Vector2(94f, 30f);
        rtCerrar.anchoredPosition = new Vector2(-12f, 0f);
        goCerrar.GetComponent<Image>().color = new Color(0.76f, 0.17f, 0.19f, 0.95f);
        btnCerrarBuzonSupervisor = goCerrar.GetComponent<Button>();

        GameObject goCerrarTxt = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goCerrarTxt.transform.SetParent(goCerrar.transform, false);
        RectTransform rtCerrarTxt = goCerrarTxt.GetComponent<RectTransform>();
        rtCerrarTxt.anchorMin = Vector2.zero;
        rtCerrarTxt.anchorMax = Vector2.one;
        rtCerrarTxt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmpCerrar = goCerrarTxt.GetComponent<TextMeshProUGUI>();
        tmpCerrar.text = "[Cerrar]";
        tmpCerrar.fontSize = 13f;
        tmpCerrar.fontStyle = FontStyles.Bold;
        tmpCerrar.alignment = TextAlignmentOptions.Center;
        tmpCerrar.color = Color.white;
        tmpCerrar.raycastTarget = false;

        GameObject goScroll = new GameObject("Scroll_Buzon_Supervisor", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        goScroll.transform.SetParent(panelBuzonSupervisor.transform, false);
        RectTransform rtScroll = goScroll.GetComponent<RectTransform>();
        rtScroll.anchorMin = Vector2.zero;
        rtScroll.anchorMax = Vector2.one;
        rtScroll.offsetMin = new Vector2(14f, 14f);
        rtScroll.offsetMax = new Vector2(-14f, -56f);
        goScroll.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.09f, 0.88f);
        scrollBuzonSupervisor = goScroll.GetComponent<ScrollRect>();
        scrollBuzonSupervisor.horizontal = false;
        scrollBuzonSupervisor.vertical = true;
        scrollBuzonSupervisor.movementType = ScrollRect.MovementType.Clamped;
        scrollBuzonSupervisor.scrollSensitivity = 28f;

        GameObject goViewport = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        goViewport.transform.SetParent(goScroll.transform, false);
        RectTransform rtViewport = goViewport.GetComponent<RectTransform>();
        rtViewport.anchorMin = Vector2.zero;
        rtViewport.anchorMax = Vector2.one;
        rtViewport.offsetMin = Vector2.zero;
        rtViewport.offsetMax = new Vector2(-14f, 0f);
        goViewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.004f);

        GameObject goContent = new GameObject("Content_Buzon_Supervisor", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        goContent.transform.SetParent(goViewport.transform, false);
        contentBuzonSupervisor = goContent.GetComponent<RectTransform>();
        contentBuzonSupervisor.anchorMin = new Vector2(0f, 1f);
        contentBuzonSupervisor.anchorMax = new Vector2(1f, 1f);
        contentBuzonSupervisor.pivot = new Vector2(0.5f, 1f);
        contentBuzonSupervisor.anchoredPosition = Vector2.zero;
        contentBuzonSupervisor.sizeDelta = Vector2.zero;
        VerticalLayoutGroup vlg = goContent.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(14, 14, 12, 12);
        vlg.spacing = 8f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter csf = goContent.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject goListado = new GameObject("Txt_Listado_Buzon_Supervisor", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goListado.transform.SetParent(goContent.transform, false);
        txtListadoBuzonSupervisor = goListado.GetComponent<TextMeshProUGUI>();
        txtListadoBuzonSupervisor.fontSize = 13.5f;
        txtListadoBuzonSupervisor.enableWordWrapping = true;
        txtListadoBuzonSupervisor.overflowMode = TextOverflowModes.Overflow;
        txtListadoBuzonSupervisor.alignment = TextAlignmentOptions.TopLeft;
        txtListadoBuzonSupervisor.richText = true;
        txtListadoBuzonSupervisor.color = new Color(0.90f, 0.94f, 0.98f, 1f);
        txtListadoBuzonSupervisor.raycastTarget = false;

        scrollBuzonSupervisor.viewport = rtViewport;
        scrollBuzonSupervisor.content = contentBuzonSupervisor;

        ConfigurarVentanaBuzonSupervisor();
    }

    public void MostrarPopUpExcedeCapacidadActual(int cupoMaximoDisponible, int palletsSolicitados, string tituloPersonalizado = null, float duracion = 3.6f)
    {
        AsegurarPopupCapacidadCompacto();
        if (panelPopupCapacidad == null) return;

        panelPopupCapacidad.transform.SetAsLastSibling();
        panelPopupCapacidad.SetActive(true);
        if (canvasGroupPopupCapacidad != null)
        {
            canvasGroupPopupCapacidad.alpha = 1f;
        }

        if (txtTituloPopupCapacidad != null)
        {
            txtTituloPopupCapacidad.text = string.IsNullOrEmpty(tituloPersonalizado)
                ? "Excede capacidad actual"
                : tituloPersonalizado;
        }

        if (txtDetallePopupCapacidad != null)
        {
            if (cupoMaximoDisponible > 0)
            {
                txtDetallePopupCapacidad.text =
                    $"Cupo máximo disponible: <b><color=#FBBF24>{cupoMaximoDisponible} pallets</color></b>\n" +
                    $"<size=12><color=#94A3B8>Lote solicitado: {palletsSolicitados} pal  •  Detalle en Buzón</color></size>";
            }
            else
            {
                txtDetallePopupCapacidad.text =
                    $"Cupo máximo disponible: <b><color=#F87171>0 pallets</color></b>\n" +
                    "<size=12><color=#94A3B8>Sin margen disponible  •  Detalle en Buzón</color></size>";
            }
        }

        if (rutinaAutoCierrePopupCapacidad != null)
        {
            StopCoroutine(rutinaAutoCierrePopupCapacidad);
        }
        rutinaAutoCierrePopupCapacidad = StartCoroutine(AutoCerrarPopupCapacidad(Mathf.Max(1.0f, duracion)));
    }

    private IEnumerator AutoCerrarPopupCapacidad(float duracionTotal)
    {
        const float duracionFade = 0.35f;
        float esperaVisible = Mathf.Max(0.2f, duracionTotal - duracionFade);
        yield return new WaitForSecondsRealtime(esperaVisible);

        if (canvasGroupPopupCapacidad != null)
        {
            float transcurrido = 0f;
            while (transcurrido < duracionFade)
            {
                transcurrido += Time.unscaledDeltaTime;
                canvasGroupPopupCapacidad.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(transcurrido / duracionFade));
                yield return null;
            }
            canvasGroupPopupCapacidad.alpha = 0f;
        }

        if (panelPopupCapacidad != null)
        {
            panelPopupCapacidad.SetActive(false);
        }
        rutinaAutoCierrePopupCapacidad = null;
    }

    private void AsegurarPopupCapacidadCompacto()
    {
        if (panelPopupCapacidad == null)
        {
            Transform encontrado = transform.Find("HUD_Operativo/Panel_Popup_Capacidad_Compacto");
            if (encontrado != null)
            {
                panelPopupCapacidad = encontrado.gameObject;
            }
        }

        if (panelPopupCapacidad != null)
        {
            if (txtTituloPopupCapacidad == null)
            {
                Transform t = panelPopupCapacidad.transform.Find("Txt_Titulo_Capacidad");
                if (t != null) txtTituloPopupCapacidad = t.GetComponent<TextMeshProUGUI>();
            }
            if (txtDetallePopupCapacidad == null)
            {
                Transform t = panelPopupCapacidad.transform.Find("Txt_Detalle_Capacidad");
                if (t != null) txtDetallePopupCapacidad = t.GetComponent<TextMeshProUGUI>();
            }
            if (canvasGroupPopupCapacidad == null)
            {
                canvasGroupPopupCapacidad = panelPopupCapacidad.GetComponent<CanvasGroup>();
                if (canvasGroupPopupCapacidad == null)
                {
                    canvasGroupPopupCapacidad = panelPopupCapacidad.AddComponent<CanvasGroup>();
                }
            }
            canvasGroupPopupCapacidad.blocksRaycasts = false;
            canvasGroupPopupCapacidad.interactable = false;
            return;
        }

        Transform padreHUD = transform.Find("HUD_Operativo");
        Transform contenedor = padreHUD != null ? padreHUD : transform;

        panelPopupCapacidad = new GameObject("Panel_Popup_Capacidad_Compacto", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(CanvasGroup));
        panelPopupCapacidad.transform.SetParent(contenedor, false);
        RectTransform rt = panelPopupCapacidad.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(390f, 108f);
        rt.anchoredPosition = Vector2.zero;

        Image bg = panelPopupCapacidad.GetComponent<Image>();
        bg.color = new Color(0.055f, 0.082f, 0.125f, 0.96f);
        bg.raycastTarget = false;

        Outline borde = panelPopupCapacidad.GetComponent<Outline>();
        borde.effectColor = new Color(0.937f, 0.267f, 0.267f, 0.90f);
        borde.effectDistance = new Vector2(2f, -2f);

        canvasGroupPopupCapacidad = panelPopupCapacidad.GetComponent<CanvasGroup>();
        canvasGroupPopupCapacidad.blocksRaycasts = false;
        canvasGroupPopupCapacidad.interactable = false;

        GameObject goFranja = new GameObject("Franja_Acento", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        goFranja.transform.SetParent(panelPopupCapacidad.transform, false);
        RectTransform rtFranja = goFranja.GetComponent<RectTransform>();
        rtFranja.anchorMin = new Vector2(0f, 1f);
        rtFranja.anchorMax = new Vector2(1f, 1f);
        rtFranja.pivot = new Vector2(0.5f, 1f);
        rtFranja.sizeDelta = new Vector2(0f, 4f);
        rtFranja.anchoredPosition = Vector2.zero;
        Image imgFranja = goFranja.GetComponent<Image>();
        imgFranja.color = new Color(0.937f, 0.267f, 0.267f, 1f);
        imgFranja.raycastTarget = false;

        TMP_FontAsset fuenteCompartida = txtTituloPopupSLA != null ? txtTituloPopupSLA.font : null;

        GameObject goTitulo = new GameObject("Txt_Titulo_Capacidad", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goTitulo.transform.SetParent(panelPopupCapacidad.transform, false);
        RectTransform rtTitulo = goTitulo.GetComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0f, 1f);
        rtTitulo.anchorMax = new Vector2(1f, 1f);
        rtTitulo.pivot = new Vector2(0.5f, 1f);
        rtTitulo.offsetMin = new Vector2(16f, -40f);
        rtTitulo.offsetMax = new Vector2(-16f, -12f);
        txtTituloPopupCapacidad = goTitulo.GetComponent<TextMeshProUGUI>();
        if (fuenteCompartida != null) txtTituloPopupCapacidad.font = fuenteCompartida;
        txtTituloPopupCapacidad.fontSize = 17f;
        txtTituloPopupCapacidad.fontStyle = FontStyles.Bold;
        txtTituloPopupCapacidad.alignment = TextAlignmentOptions.Center;
        txtTituloPopupCapacidad.color = new Color(0.973f, 0.443f, 0.443f, 1f);
        txtTituloPopupCapacidad.raycastTarget = false;

        GameObject goDetalle = new GameObject("Txt_Detalle_Capacidad", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goDetalle.transform.SetParent(panelPopupCapacidad.transform, false);
        RectTransform rtDetalle = goDetalle.GetComponent<RectTransform>();
        rtDetalle.anchorMin = new Vector2(0f, 0f);
        rtDetalle.anchorMax = new Vector2(1f, 1f);
        rtDetalle.pivot = new Vector2(0.5f, 0.5f);
        rtDetalle.offsetMin = new Vector2(16f, 12f);
        rtDetalle.offsetMax = new Vector2(-16f, -42f);
        txtDetallePopupCapacidad = goDetalle.GetComponent<TextMeshProUGUI>();
        if (fuenteCompartida != null) txtDetallePopupCapacidad.font = fuenteCompartida;
        txtDetallePopupCapacidad.fontSize = 14f;
        txtDetallePopupCapacidad.alignment = TextAlignmentOptions.Center;
        txtDetallePopupCapacidad.color = new Color(0.945f, 0.961f, 0.976f, 1f);
        txtDetallePopupCapacidad.raycastTarget = false;
    }

    public void ConfigurarPopUpEjecutivoSLA()
    {
        if (panelPopupSLA == null) return;

        RectTransform rt = panelPopupSLA.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(580f, 186f);
            rt.anchoredPosition = new Vector2(0f, 260f);
        }

        if (txtTituloPopupSLA != null)
        {
            RectTransform rtTitulo = txtTituloPopupSLA.GetComponent<RectTransform>();
            if (rtTitulo != null)
            {
                rtTitulo.anchorMin = new Vector2(0f, 1f);
                rtTitulo.anchorMax = new Vector2(1f, 1f);
                rtTitulo.pivot = new Vector2(0.5f, 1f);
                rtTitulo.offsetMin = new Vector2(20f, -44f);
                rtTitulo.offsetMax = new Vector2(-108f, -10f);
            }
        }

        if (txtDetallePopupSLA != null)
        {
            txtDetallePopupSLA.fontSize = 13.5f;
            txtDetallePopupSLA.verticalAlignment = VerticalAlignmentOptions.Middle;
            txtDetallePopupSLA.margin = new Vector4(4f, 4f, 4f, 4f);
        }
    }

    public void ConfigurarSelectorCamarasSCADA()
    {
        Transform panelScada = transform.Find("HUD_Operativo/Panel_Controles_SCADA");
        if (panelScada == null) return;

        Transform tBtnCam = panelScada.Find("Btn_Cambiar_Camara");
        if (tBtnCam != null)
        {
            if (txtBtnCambiarCamara == null)
            {
                txtBtnCambiarCamara = tBtnCam.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            Button btnHeader = tBtnCam.GetComponent<Button>();
            if (btnHeader != null)
            {
                btnHeader.onClick.RemoveListener(ToggleSelectorCamaras);
                if (btnHeader.onClick.GetPersistentEventCount() == 0)
                {
                    btnHeader.onClick.AddListener(ToggleSelectorCamaras);
                }
            }
        }

        if (panelSelectorCamaras == null)
        {
            Transform tSelector = panelScada.Find("Panel_Selector_Camaras");
            if (tSelector != null) panelSelectorCamaras = tSelector.gameObject;
        }

        if (panelSelectorCamaras != null)
        {
            if (btnCamAuto == null) btnCamAuto = panelSelectorCamaras.transform.Find("Btn_Cam_Auto")?.GetComponent<Button>();
            if (btnCamGeneral == null) btnCamGeneral = panelSelectorCamaras.transform.Find("Btn_Cam_General")?.GetComponent<Button>();
            if (btnCamEVE == null) btnCamEVE = panelSelectorCamaras.transform.Find("Btn_Cam_EVE")?.GetComponent<Button>();
            if (btnCamAMR01 == null) btnCamAMR01 = panelSelectorCamaras.transform.Find("Btn_Cam_AMR01")?.GetComponent<Button>();
            if (btnCamAMR02 == null) btnCamAMR02 = panelSelectorCamaras.transform.Find("Btn_Cam_AMR02")?.GetComponent<Button>();
            if (btnCamAMR03 == null) btnCamAMR03 = panelSelectorCamaras.transform.Find("Btn_Cam_AMR03")?.GetComponent<Button>();
            if (btnCamDron == null) btnCamDron = panelSelectorCamaras.transform.Find("Btn_Cam_Dron")?.GetComponent<Button>();

            VincularBotonSelectorCamara(btnCamAuto, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.AutoDirectorWMS));
            VincularBotonSelectorCamara(btnCamGeneral, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.GeneralPlanta));
            VincularBotonSelectorCamara(btnCamEVE, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.EveOrquestador));
            VincularBotonSelectorCamara(btnCamAMR01, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.AMR01));
            VincularBotonSelectorCamara(btnCamAMR02, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.AMR02));
            VincularBotonSelectorCamara(btnCamAMR03, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.AMR03));
            VincularBotonSelectorCamara(btnCamDron, () => SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara.DronSoporteISO));

            panelSelectorCamaras.SetActive(selectorCamarasExpandido);
        }

        RefrescarEstadoSelectorCamaras(true);
    }

    private static void VincularBotonSelectorCamara(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if (boton == null) return;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(accion);
    }

    public void ToggleSelectorCamaras()
    {
        if (ultimoFrameToggleCamaras == Time.frameCount) return;
        ultimoFrameToggleCamaras = Time.frameCount;

        selectorCamarasExpandido = !selectorCamarasExpandido;
        if (panelSelectorCamaras != null)
        {
            panelSelectorCamaras.SetActive(selectorCamarasExpandido);
        }
        RefrescarEstadoSelectorCamaras(true);
    }

    private void SeleccionarVistaDesdeUI(DirectorCinematico.ModoVistaCamara modo)
    {
        DirectorCinematico dir = DirectorCinematico.Instance != null
            ? DirectorCinematico.Instance
            : FindFirstObjectByType<DirectorCinematico>(FindObjectsInactive.Include);

        if (dir != null)
        {
            switch (modo)
            {
                case DirectorCinematico.ModoVistaCamara.AutoDirectorWMS: dir.SeleccionarModoAuto(); break;
                case DirectorCinematico.ModoVistaCamara.GeneralPlanta: dir.SeleccionarVistaGeneral(); break;
                case DirectorCinematico.ModoVistaCamara.EveOrquestador: dir.SeleccionarVistaEVE(); break;
                case DirectorCinematico.ModoVistaCamara.AMR01: dir.SeleccionarVistaAMR(1); break;
                case DirectorCinematico.ModoVistaCamara.AMR02: dir.SeleccionarVistaAMR(2); break;
                case DirectorCinematico.ModoVistaCamara.AMR03: dir.SeleccionarVistaAMR(3); break;
                case DirectorCinematico.ModoVistaCamara.DronSoporteISO: dir.SeleccionarVistaDronSoporte(); break;
            }
        }

        selectorCamarasExpandido = false;
        if (panelSelectorCamaras != null)
        {
            panelSelectorCamaras.SetActive(false);
        }
        RefrescarEstadoSelectorCamaras(true);
    }

    public void RefrescarEstadoSelectorCamaras(bool forzarLayout)
    {
        DirectorCinematico dir = DirectorCinematico.Instance;
        string etiqueta = dir != null ? dir.ObtenerEtiquetaVistaActiva() : "AUTO [Director WMS]";
        string flecha = selectorCamarasExpandido ? "▲" : "▼";
        string textoCabecera = $"CÁMARA: {etiqueta}  [{flecha}]";

        bool cambioTexto = textoCabecera != ultimaCabeceraCamara;
        if (cambioTexto && txtBtnCambiarCamara != null)
        {
            txtBtnCambiarCamara.text = textoCabecera;
            ultimaCabeceraCamara = textoCabecera;
        }

        bool cambioVisibilidad = false;
        if (panelSelectorCamaras != null)
        {
            RobotManagerEVE eve = ResolverRobotManagerEVE();
            RobotAMR amr02 = eve != null ? eve.robotAMR_02 : null;
            RobotAMR amr03 = eve != null ? eve.robotAMR_03 : null;
            DronMantenimiento dron = eve != null && eve.dronMantenimiento != null
                ? eve.dronMantenimiento
                : FindFirstObjectByType<DronMantenimiento>(FindObjectsInactive.Include);

            bool verAMR02 = amr02 != null && amr02.gameObject.activeInHierarchy;
            bool verAMR03 = amr03 != null && amr03.gameObject.activeInHierarchy;
            bool verDron = dron != null && dron.gameObject.activeInHierarchy && dron.ocupado;

            if (btnCamAMR02 != null && btnCamAMR02.gameObject.activeSelf != verAMR02)
            {
                btnCamAMR02.gameObject.SetActive(verAMR02);
                cambioVisibilidad = true;
            }
            if (btnCamAMR03 != null && btnCamAMR03.gameObject.activeSelf != verAMR03)
            {
                btnCamAMR03.gameObject.SetActive(verAMR03);
                cambioVisibilidad = true;
            }
            if (btnCamDron != null && btnCamDron.gameObject.activeSelf != verDron)
            {
                btnCamDron.gameObject.SetActive(verDron);
                cambioVisibilidad = true;
            }
        }

        if (forzarLayout || cambioVisibilidad)
        {
            if (selectorCamarasExpandido && panelSelectorCamaras != null && panelSelectorCamaras.activeInHierarchy)
            {
                RectTransform rectSelector = panelSelectorCamaras.GetComponent<RectTransform>();
                if (rectSelector != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectSelector);
                }
            }

            Transform panelScada = transform.Find("HUD_Operativo/Panel_Controles_SCADA");
            if (panelScada is RectTransform rectScada && panelScada.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectScada);
            }
        }
    }

    private void AsegurarPopupSLA()
    {
        if (panelPopupSLA == null)
        {
            Transform encontrado = transform.Find("HUD_Operativo/Panel_Popup_Ejecutivo_SLA");
            if (encontrado != null)
            {
                panelPopupSLA = encontrado.gameObject;
            }
        }

        if (panelPopupSLA != null)
        {
            if (txtTituloPopupSLA == null)
            {
                Transform t = panelPopupSLA.transform.Find("Txt_Titulo_Popup_SLA");
                if (t != null) txtTituloPopupSLA = t.GetComponent<TextMeshProUGUI>();
            }
            if (txtDetallePopupSLA == null)
            {
                Transform t = panelPopupSLA.transform.Find("Txt_Detalle_Popup_SLA");
                if (t != null) txtDetallePopupSLA = t.GetComponent<TextMeshProUGUI>();
            }
            if (btnCerrarPopupSLA == null)
            {
                Transform t = panelPopupSLA.transform.Find("Btn_Cerrar_Popup_SLA");
                if (t != null) btnCerrarPopupSLA = t.GetComponent<Button>();
            }
            ConfigurarPopUpEjecutivoSLA();
            return;
        }

        // Fallback en tiempo de ejecución si el panel no existía en escena
        Transform padreHUD = transform.Find("HUD_Operativo");
        Transform contenedor = padreHUD != null ? padreHUD : transform;

        panelPopupSLA = new GameObject("Panel_Popup_Ejecutivo_SLA", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelPopupSLA.transform.SetParent(contenedor, false);
        RectTransform rt = panelPopupSLA.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(540f, 136f);
        rt.anchoredPosition = new Vector2(0f, 260f);

        Image bg = panelPopupSLA.GetComponent<Image>();
        bg.color = new Color(0.06f, 0.09f, 0.14f, 0.96f);

        GameObject goTitulo = new GameObject("Txt_Titulo_Popup_SLA", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goTitulo.transform.SetParent(panelPopupSLA.transform, false);
        RectTransform rtTitulo = goTitulo.GetComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0f, 1f);
        rtTitulo.anchorMax = new Vector2(1f, 1f);
        rtTitulo.pivot = new Vector2(0.5f, 1f);
        rtTitulo.offsetMin = new Vector2(20f, -44f);
        rtTitulo.offsetMax = new Vector2(-108f, -10f);
        txtTituloPopupSLA = goTitulo.GetComponent<TextMeshProUGUI>();
        txtTituloPopupSLA.fontSize = 16f;
        txtTituloPopupSLA.fontStyle = FontStyles.Bold;
        txtTituloPopupSLA.alignment = TextAlignmentOptions.Left;
        txtTituloPopupSLA.color = Color.white;
        txtTituloPopupSLA.raycastTarget = false;

        GameObject goDetalle = new GameObject("Txt_Detalle_Popup_SLA", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goDetalle.transform.SetParent(panelPopupSLA.transform, false);
        RectTransform rtDetalle = goDetalle.GetComponent<RectTransform>();
        rtDetalle.anchorMin = new Vector2(0f, 0f);
        rtDetalle.anchorMax = new Vector2(1f, 1f);
        rtDetalle.pivot = new Vector2(0.5f, 0.5f);
        rtDetalle.offsetMin = new Vector2(20f, 18f);
        rtDetalle.offsetMax = new Vector2(-20f, -52f);
        txtDetallePopupSLA = goDetalle.GetComponent<TextMeshProUGUI>();
        txtDetallePopupSLA.fontSize = 14f;
        txtDetallePopupSLA.alignment = TextAlignmentOptions.MidlineLeft;
        txtDetallePopupSLA.margin = new Vector4(4f, 4f, 4f, 4f);
        txtDetallePopupSLA.color = new Color(0.9f, 0.94f, 0.98f, 1f);
        txtDetallePopupSLA.raycastTarget = false;

        GameObject goCerrar = new GameObject("Btn_Cerrar_Popup_SLA", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        goCerrar.transform.SetParent(panelPopupSLA.transform, false);
        RectTransform rtCerrar = goCerrar.GetComponent<RectTransform>();
        rtCerrar.anchorMin = new Vector2(1f, 1f);
        rtCerrar.anchorMax = new Vector2(1f, 1f);
        rtCerrar.pivot = new Vector2(1f, 1f);
        rtCerrar.sizeDelta = new Vector2(88f, 28f);
        rtCerrar.anchoredPosition = new Vector2(-12f, -12f);
        goCerrar.GetComponent<Image>().color = new Color(0.18f, 0.24f, 0.34f, 1f);
        btnCerrarPopupSLA = goCerrar.GetComponent<Button>();
        btnCerrarPopupSLA.onClick.AddListener(CerrarPopUpEjecutivoSLA);
        if (goCerrar.GetComponent<BotonClickSFX>() == null)
        {
            goCerrar.AddComponent<BotonClickSFX>();
        }

        GameObject goCerrarTxt = new GameObject("Text (TMP)", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goCerrarTxt.transform.SetParent(goCerrar.transform, false);
        RectTransform rtCT = goCerrarTxt.GetComponent<RectTransform>();
        rtCT.anchorMin = Vector2.zero;
        rtCT.anchorMax = Vector2.one;
        rtCT.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmpCerrar = goCerrarTxt.GetComponent<TextMeshProUGUI>();
        tmpCerrar.text = "[Cerrar]";
        tmpCerrar.fontSize = 13f;
        tmpCerrar.fontStyle = FontStyles.Bold;
        tmpCerrar.alignment = TextAlignmentOptions.Center;
        tmpCerrar.color = Color.white;
        tmpCerrar.raycastTarget = false;

        ConfigurarPopUpEjecutivoSLA();
    }

    public void RegistrarLog(string mensaje)
    {
        string timestamp = $"[{RelojSimuladorTurno.HoraActualFormateada}]";
        string logFinal = $"{timestamp} {mensaje}";

        logQueue.Enqueue(logFinal);

        if (logQueue.Count > MaxLineas)
        {
            logQueue.Dequeue();
        }

        ActualizarVisual();

        if (scrollTerminalEVE != null)
        {
            Canvas.ForceUpdateCanvases();
            if (scrollTerminalEVE.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollTerminalEVE.content);
            }
            Canvas.ForceUpdateCanvases();
            scrollTerminalEVE.verticalNormalizedPosition = 0f;
        }
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

    public IReadOnlyList<string> ObtenerLineasBitacoraLimpia()
    {
        var lineas = new List<string>(logQueue.Count);
        foreach (string entrada in logQueue)
        {
            if (string.IsNullOrEmpty(entrada)) continue;
            string limpia = System.Text.RegularExpressions.Regex.Replace(entrada, "<.*?>", string.Empty).Trim();
            if (!string.IsNullOrEmpty(limpia))
            {
                lineas.Add(limpia);
            }
        }
        return lineas;
    }

    public void ConfigurarBarraOperativaInferior()
    {
        Transform barra = barraOperativaInferior != null
            ? barraOperativaInferior.transform
            : transform.Find("HUD_Operativo/Barra_Operativa_Inferior");

        if (barra == null) return;
        barraOperativaInferior = barra.gameObject;

        if (btnPausaSimulacion == null)
        {
            btnPausaSimulacion = barra.Find("Btn_Pausa_Simulacion")?.GetComponent<Button>();
        }
        if (imgBtnPausaSimulacion == null && btnPausaSimulacion != null)
        {
            imgBtnPausaSimulacion = btnPausaSimulacion.GetComponent<Image>();
        }
        if (txtBtnPausaSimulacion == null && btnPausaSimulacion != null)
        {
            txtBtnPausaSimulacion = btnPausaSimulacion.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (btnPausaSimulacion != null)
        {
            btnPausaSimulacion.onClick.RemoveListener(OnClickPausarReanudarSimulacion);
            if (btnPausaSimulacion.onClick.GetPersistentEventCount() == 0)
            {
                btnPausaSimulacion.onClick.AddListener(OnClickPausarReanudarSimulacion);
            }
        }

        RobotManagerEVE eve = ResolverRobotManagerEVE();

        if (eveSuscritoPausa != eve)
        {
            if (eveSuscritoPausa != null)
            {
                eveSuscritoPausa.OnPausaCambiada -= ActualizarVisualBotonPausa;
            }
            eveSuscritoPausa = eve;
        }

        if (eveSuscritoPausa != null)
        {
            eveSuscritoPausa.OnPausaCambiada -= ActualizarVisualBotonPausa;
            eveSuscritoPausa.OnPausaCambiada += ActualizarVisualBotonPausa;
            ActualizarVisualBotonPausa(eveSuscritoPausa.SimulacionPausada);
        }
        else
        {
            ActualizarVisualBotonPausa(false);
        }
    }

    public void ConfigurarBotonIngestaJSON()
    {
        Transform panelScada = transform.Find("HUD_Operativo/Panel_Controles_SCADA");
        if (panelScada != null)
        {
            if (btnIngestarJSON == null)
            {
                btnIngestarJSON = panelScada.Find("Btn_Ingestar_JSON")?.GetComponent<Button>();
            }
            if (imgBtnIngestarJSON == null && btnIngestarJSON != null)
            {
                imgBtnIngestarJSON = btnIngestarJSON.GetComponent<Image>();
            }
            if (txtBtnIngestarJSON == null && btnIngestarJSON != null)
            {
                txtBtnIngestarJSON = btnIngestarJSON.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        if (imgBtnIngestarJSON != null)
        {
            imgBtnIngestarJSON.raycastTarget = true;
        }

        if (btnIngestarJSON != null)
        {
            ColorBlock colores = btnIngestarJSON.colors;
            colores.normalColor = Color.white;
            colores.highlightedColor = new Color(0.92f, 1f, 0.95f, 1f);
            colores.pressedColor = new Color(0.78f, 0.92f, 0.84f, 1f);
            colores.selectedColor = Color.white;
            colores.disabledColor = new Color(0.96f, 0.96f, 0.96f, 0.95f);
            colores.colorMultiplier = 1f;
            btnIngestarJSON.colors = colores;

            BotonClickSFX sfx = btnIngestarJSON.GetComponent<BotonClickSFX>();
            if (sfx == null)
            {
                sfx = btnIngestarJSON.gameObject.AddComponent<BotonClickSFX>();
            }
            sfx.ConfigurarModo(BotonClickSFX.ModoAudioBoton.IngestaJSON);
        }

        RobotManagerEVE eve = ResolverRobotManagerEVE();
        if (eveSuscritoAdmisionJSON != eve)
        {
            if (eveSuscritoAdmisionJSON != null)
            {
                eveSuscritoAdmisionJSON.OnEstadoAdmisionJSONCambiado -= ActualizarEstadoBotonIngestaJSON;
            }
            eveSuscritoAdmisionJSON = eve;
        }
        if (eveSuscritoAdmisionJSON != null)
        {
            eveSuscritoAdmisionJSON.OnEstadoAdmisionJSONCambiado -= ActualizarEstadoBotonIngestaJSON;
            eveSuscritoAdmisionJSON.OnEstadoAdmisionJSONCambiado += ActualizarEstadoBotonIngestaJSON;
        }

        ultimoEstadoBtnJsonDisponible = null;
        ActualizarEstadoBotonIngestaJSON();
    }

    public void ActualizarEstadoBotonIngestaJSON()
    {
        if (btnIngestarJSON == null) return;

        RobotManagerEVE eve = ResolverRobotManagerEVE();
        bool disponible = true;
        bool bloqueoSeguridad = false;

        if (eve != null && eve.turnoIniciado)
        {
            disponible = eve.PuedeRecibirNuevosPedidos(out _, out _);
            bloqueoSeguridad = eve.admisionNuevosPedidosBloqueada || eve.operacionDetenidaPorSeguridad || eve.turnoEnFaseCritica;
        }

        string etiquetaObjetivo = disponible
            ? "[+] Cargar JSON ERP"
            : (bloqueoSeguridad ? "[X] JSON BLOQUEADO" : "[X] SIN CUPO JSON");

        if (ultimoEstadoBtnJsonDisponible.HasValue &&
            ultimoEstadoBtnJsonDisponible.Value == disponible &&
            ultimoTextoBtnJson == etiquetaObjetivo)
        {
            return;
        }

        ultimoEstadoBtnJsonDisponible = disponible;
        ultimoTextoBtnJson = etiquetaObjetivo;

        btnIngestarJSON.interactable = disponible;
        if (imgBtnIngestarJSON != null)
        {
            imgBtnIngestarJSON.color = disponible ? ColorBtnJsonDisponible : ColorBtnJsonBloqueado;
        }
        if (txtBtnIngestarJSON != null)
        {
            txtBtnIngestarJSON.text = etiquetaObjetivo;
        }
    }

    public void OnClickPausarReanudarSimulacion()
    {
        if (ultimoFrameTogglePausa == Time.frameCount) return;
        ultimoFrameTogglePausa = Time.frameCount;

        RobotManagerEVE eve = ResolverRobotManagerEVE();
        if (eve == null) return;

        bool pausada = eve.AlternarPausaSimulacion();
        ActualizarVisualBotonPausa(pausada);
        ActualizarMarcadorNBAHud(true);
    }

    private void ActualizarVisualBotonPausa(bool pausada)
    {
        if (imgBtnPausaSimulacion != null)
        {
            imgBtnPausaSimulacion.color = pausada ? ColorBtnPausaActiva : ColorBtnPausaNormal;
        }
        if (txtBtnPausaSimulacion != null)
        {
            txtBtnPausaSimulacion.text = pausada ? "REANUDAR [PAUSA]" : "PAUSAR TURNO";
        }
    }

    public void ConfigurarControlesPlantaSCADA()
    {
        Transform panelScada = transform.Find("HUD_Operativo/Panel_Controles_SCADA");
        if (panelScada != null)
        {
            if (btnSilenciarAlarmaFabrica == null)
            {
                btnSilenciarAlarmaFabrica = panelScada.Find("Btn_Silenciar_Alarma_Fabrica")?.GetComponent<Button>();
            }
            if (imgBtnSilenciarAlarmaFabrica == null && btnSilenciarAlarmaFabrica != null)
            {
                imgBtnSilenciarAlarmaFabrica = btnSilenciarAlarmaFabrica.GetComponent<Image>();
            }
            if (txtBtnSilenciarAlarmaFabrica == null && btnSilenciarAlarmaFabrica != null)
            {
                txtBtnSilenciarAlarmaFabrica = btnSilenciarAlarmaFabrica.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (btnConfirmarDespejeMuelle == null)
            {
                btnConfirmarDespejeMuelle = panelScada.Find("Btn_Confirmar_Despeje_Muelle")?.GetComponent<Button>();
            }
            if (imgBtnConfirmarDespejeMuelle == null && btnConfirmarDespejeMuelle != null)
            {
                imgBtnConfirmarDespejeMuelle = btnConfirmarDespejeMuelle.GetComponent<Image>();
            }
            if (txtBtnConfirmarDespejeMuelle == null && btnConfirmarDespejeMuelle != null)
            {
                txtBtnConfirmarDespejeMuelle = btnConfirmarDespejeMuelle.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }

        if (btnSilenciarAlarmaFabrica != null)
        {
            btnSilenciarAlarmaFabrica.onClick.RemoveListener(OnClickSilenciarAlarmaFabrica);
            if (btnSilenciarAlarmaFabrica.onClick.GetPersistentEventCount() == 0)
            {
                btnSilenciarAlarmaFabrica.onClick.AddListener(OnClickSilenciarAlarmaFabrica);
            }
        }

        if (btnConfirmarDespejeMuelle != null)
        {
            btnConfirmarDespejeMuelle.onClick.RemoveListener(OnClickConfirmarDespejeHumanoMuelle);
            if (btnConfirmarDespejeMuelle.onClick.GetPersistentEventCount() == 0)
            {
                btnConfirmarDespejeMuelle.onClick.AddListener(OnClickConfirmarDespejeHumanoMuelle);
            }
        }

        GestorAudioSCADA audio = GestorAudioSCADA.Instance != null
            ? GestorAudioSCADA.Instance
            : FindFirstObjectByType<GestorAudioSCADA>(FindObjectsInactive.Include);
        if (audioSuscritoAlarma != audio)
        {
            if (audioSuscritoAlarma != null)
            {
                audioSuscritoAlarma.OnEstadoAlarmaFabricaCambiado -= OnEstadoAlarmaFabricaCambiado;
            }
            audioSuscritoAlarma = audio;
        }
        if (audioSuscritoAlarma != null)
        {
            audioSuscritoAlarma.OnEstadoAlarmaFabricaCambiado -= OnEstadoAlarmaFabricaCambiado;
            audioSuscritoAlarma.OnEstadoAlarmaFabricaCambiado += OnEstadoAlarmaFabricaCambiado;
        }

        ModuloDespachoTMS tms = ModuloDespachoTMS.Instance != null
            ? ModuloDespachoTMS.Instance
            : FindFirstObjectByType<ModuloDespachoTMS>(FindObjectsInactive.Include);
        if (tmsSuscritoMuelle != tms)
        {
            if (tmsSuscritoMuelle != null)
            {
                tmsSuscritoMuelle.OnEstadoClausuraMuelleCambiado -= OnEstadoClausuraMuelleCambiado;
            }
            tmsSuscritoMuelle = tms;
        }
        if (tmsSuscritoMuelle != null)
        {
            tmsSuscritoMuelle.OnEstadoClausuraMuelleCambiado -= OnEstadoClausuraMuelleCambiado;
            tmsSuscritoMuelle.OnEstadoClausuraMuelleCambiado += OnEstadoClausuraMuelleCambiado;
        }

        ActualizarVisualControlesPlantaSCADA();
    }

    private void OnEstadoAlarmaFabricaCambiado(bool activa, string motivo)
    {
        ActualizarVisualControlesPlantaSCADA();
    }

    private void OnEstadoClausuraMuelleCambiado(int numeroCamion, bool clausurado, string motivo)
    {
        ActualizarVisualControlesPlantaSCADA();
    }

    public void ActualizarVisualControlesPlantaSCADA()
    {
        bool alarmaActiva = audioSuscritoAlarma != null && audioSuscritoAlarma.alarmaFabricaActiva;
        if (imgBtnSilenciarAlarmaFabrica != null)
        {
            imgBtnSilenciarAlarmaFabrica.color = alarmaActiva ? ColorBtnAlarmaActiva : ColorBtnPausaNormal;
        }
        if (txtBtnSilenciarAlarmaFabrica != null)
        {
            txtBtnSilenciarAlarmaFabrica.text = alarmaActiva ? "SILENCIAR ALARMA PLANTA [!]" : "ALARMA PLANTA: NORMAL";
        }

        int muelleAfectado = 0;
        bool hayMuelleBloqueado = tmsSuscritoMuelle != null && tmsSuscritoMuelle.TieneAlgunMuelleClausuradoOEnInspeccion(out muelleAfectado);
        if (imgBtnConfirmarDespejeMuelle != null)
        {
            imgBtnConfirmarDespejeMuelle.color = hayMuelleBloqueado ? ColorBtnMuelleClausurado : ColorBtnPausaNormal;
        }
        if (txtBtnConfirmarDespejeMuelle != null)
        {
            txtBtnConfirmarDespejeMuelle.text = hayMuelleBloqueado
                ? $"DESPEJAR MUELLE 0{muelleAfectado} [HUMANO]"
                : "MUELLES 01-03: OPERATIVOS";
        }
    }

    public void OnClickSilenciarAlarmaFabrica()
    {
        if (ultimoFrameSilenciarAlarma == Time.frameCount) return;
        ultimoFrameSilenciarAlarma = Time.frameCount;

        GestorAudioSCADA audio = GestorAudioSCADA.Instance != null
            ? GestorAudioSCADA.Instance
            : FindFirstObjectByType<GestorAudioSCADA>(FindObjectsInactive.Include);

        bool estabaActiva = audio != null && audio.alarmaFabricaActiva;
        audio?.SilenciarAlarmaFabrica();
        ActualizarVisualControlesPlantaSCADA();

        if (estabaActiva)
        {
            RegistrarLog("<color=#38BDF8>[ACUSE PLANTA]:</color> Alarma sonora de fábrica silenciada/acusada por supervisor (audio SCADA general activo).");
        }
        else
        {
            RegistrarLog("<color=#94A3B8>[ACUSE PLANTA]:</color> Señal de alarma de fábrica ya se encontraba en estado nominal.");
        }
    }

    public void OnClickConfirmarDespejeHumanoMuelle()
    {
        if (ultimoFrameDespejeMuelle == Time.frameCount) return;
        ultimoFrameDespejeMuelle = Time.frameCount;

        RobotManagerEVE eve = ResolverRobotManagerEVE();

        bool despejado = eve != null && eve.ConfirmarDespejeHumanoMuelleDesdeHUD();
        ActualizarVisualControlesPlantaSCADA();

        if (!despejado)
        {
            RegistrarLog("<color=#94A3B8>[TMS]:</color> Todos los muelles (01..03) están actualmente operativos.");
        }
    }
}
