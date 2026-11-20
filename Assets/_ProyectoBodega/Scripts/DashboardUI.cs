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
    private bool amr03Habilitado = false;
    private bool modoReservaAMR02 = false;

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

    public enum TipoAlertaActiva { BateriaCriticaAMR, IncertidumbreDemandaWMS }
    private TipoAlertaActiva tipoAlertaActual = TipoAlertaActiva.BateriaCriticaAMR;
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
    private bool haSidoDesplegadoAMR03EnTurno = false;
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

    private static readonly Color ColorBaseFilaAMR = new Color(0.12f, 0.16f, 0.22f, 0.0f);
    private static readonly Color ColorBaseTerminalEVE = new Color(0.121568628f, 0.141176477f, 0.180392161f, 0.95f);
    private static readonly Color ColorDestelloPreventivoFila = new Color(0.96f, 0.62f, 0.04f, 0.32f);
    private static readonly Color ColorDestelloCriticoFila = new Color(0.94f, 0.27f, 0.27f, 0.38f);

    [Header("Barra Operativa Inferior (Módulo 5)")]
    public GameObject barraOperativaInferior;
    public Button btnPausaSimulacion;
    public Image imgBtnPausaSimulacion;
    public TextMeshProUGUI txtBtnPausaSimulacion;
    public Slider sliderVelocidadAMR;
    public TextMeshProUGUI txtValorVelocidadAMR;
    public Button btnExportarCSV;

    private RobotManagerEVE eveSuscritoPausa;
    private int ultimoFrameTogglePausa = -1;
    private static readonly Color ColorBtnPausaNormal = new Color(0.118f, 0.161f, 0.231f, 1f);
    private static readonly Color ColorBtnPausaActiva = new Color(0.706f, 0.325f, 0.035f, 1f);

    public void IniciarTurnoNocturno()
    {
        haSidoDesplegadoAMR03EnTurno = false;
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
        if (!SincronizarActivacionHUDSiAplica())
        {
            ConfigurarSelectorCamarasSCADA();
            if (hud != null && hud.gameObject.activeInHierarchy)
            {
                RefrescarEstadoSelectorCamaras(true);
            }
        }

        FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include)?.IniciarTurno();
        ConfigurarBarraOperativaInferior();
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
            if (mostrar)
            {
                RobotManagerEVE evePausa = RobotManagerEVE.Instance != null
                    ? RobotManagerEVE.Instance
                    : FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
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
                GestorAudioSCADA.Instance?.ReproducirAlertaPreventiva();
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

    public void MostrarEmergencia(bool mostrar, float bateriaProyectada, float varianzaMargen = 3.2f)
    {
        tipoAlertaActual = TipoAlertaActiva.BateriaCriticaAMR;
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
            string mensaje;
            if (escenario == RobotManagerEVE.EscenarioHorizonte.EscenarioA_InicioTurno)
            {
                mensaje = "<size=20><b><color=#38BDF8>[i] ANALIZADOR HORIZONTE [ESCENARIO A - INICIO TURNO]: VIABILIDAD SLA OK</color></b></size>\n" +
                          $"<color=#E2E8F0>Reloj: Hora <b><color=#38BDF8>{horaActual:F1}/8</color></b> (Restan {horasRestantes:F1}h) | Lote: {nuevosPedidos} ped ({pesoLoteKg:F0} kg) | Déficit: <b><color=#FBBF24>-{deficitSoC:F1}% SoC</color></b>\n" +
                          "Dictamen: Tiempo suficiente antes del amanecer, pero demanda (192.5%) supera el 60% nominal por unidad. Flota ideal: 3 AMRs (2 activos + 1 soporte/relevo inductivo).</color>\n" +
                          separador + "\n" +
                          "<b><color=#94A3B8>DECISIÓN GERENCIAL DE HORIZONTE TEMPRANO (INDUSTRIA 5.0)</color></b>\n\n" +
                          "<b><color=#F97316>> [OPCIÓN A: DIFERIR EXCEDENTE / MANTENER 2 AMRs]</color></b>\n" +
                          "<color=#E2E8F0>  Limitar operación al SoC actual sin habilitar el 3er AMR de relevo rotativo.</color>\n\n" +
                          "<b><color=#10B981>> [OPCIÓN B: APROBAR FLOTA DE 3 AMRs (2 ACTIVOS + 1 RELEVO)]</color></b>\n" +
                          "<color=#E2E8F0>  Cumplir 100% del SLA (0% Carry-over) con ciclos de carga de oportunidad 1C.</color>\n" +
                          separador;
            }
            else if (escenario == RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada)
            {
                mensaje = "<size=20><b><color=#F59E0B>[!] ANALIZADOR HORIZONTE [ESCENARIO B - MADRUGADA]: RIESGO MEDIO DE CORTE SLA</color></b></size>\n" +
                          $"<color=#E2E8F0>Reloj: Hora <b><color=#FBBF24>{horaActual:F1}/8</color></b> (Restan {horasRestantes:F1}h) | Rezago proyectado: <b><color=#FBBF24>{porcentajeCarryOver:F0}%</color></b> | Déficit: <b><color=#FBBF24>-{deficitSoC:F1}% SoC</color></b>\n" +
                          $"Dictamen: Tasa de absorción actual (10 ped/h) dejará {porcentajeCarryOver:F0}% del lote sin entregar al corte (06:00 AM). Incrementar a 4 AMRs o autorizar Carry-over al turno diurno.</color>\n" +
                          separador + "\n" +
                          "<b><color=#94A3B8>DECISIÓN GERENCIAL DE MADRUGADA (TRADE-OFF SLA vs. CAPEX)</color></b>\n\n" +
                          $"<b><color=#F97316>> [OPCIÓN A: AUTORIZAR CARRY-OVER ({porcentajeCarryOver:F0}% REZAGO AL TURNO DIURNO)]</color></b>\n" +
                          "<color=#E2E8F0>  Mantener 2 AMRs actuales hasta las 06:00 AM y transferir el saldo pendiente.</color>\n\n" +
                          $"<b><color=#10B981>> [OPCIÓN B: INCREMENTAR FLOTA A {amrRequeridos} AMRs OPERATIVOS]</color></b>\n" +
                          "<color=#E2E8F0>  Duplicar tasa de absorción (20 ped/h) para eliminar el Carry-over antes de 06:00 AM.</color>\n" +
                          separador;
            }
            else
            {
                mensaje = "<size=20><b><color=#FF453A>[!] ANALIZADOR HORIZONTE [ESCENARIO C - FINAL TURNO]: COLAPSO LOGÍSTICO / SLA</color></b></size>\n" +
                          $"<color=#E2E8F0>Reloj: Hora <b><color=#FF453A>{horaActual:F1}/8</color></b> (Restan {horasRestantes:F1}h) | Déficit Crítico LiFePO4: <b><color=#FBBF24>-{deficitSoC:F1}% SoC</color></b>\n" +
                          $"Dictamen: Faltan 30 min y celdas en reserva. Capacidad actual colapsa ({porcentajeCarryOver:F0}% rezago). Se requieren {amrRequeridos} AMRs de emergencia para mantener HBF o diferir el 100% del lote.</color>\n" +
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
        if (tipoAlertaActual == TipoAlertaActiva.IncertidumbreDemandaWMS)
        {
            ResolverDecisionHorizonte(false);
            return;
        }

        MostrarEmergencia(false);
        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>();
        if (eve != null)
        {
            eve.AutorizarOverrideBateria();
        }
        else
        {
            RobotAMR robot = UnityEngine.Object.FindObjectOfType<RobotAMR>();
            if (robot != null)
            {
                robot.EjecutarOverride();
            }
        }
    }

    public void OnRelevoPresionado()
    {
        if (tipoAlertaActual == TipoAlertaActiva.IncertidumbreDemandaWMS)
        {
            ResolverDecisionHorizonte(true);
            return;
        }

        MostrarEmergencia(false);
        FindFirstObjectByType<RobotManagerEVE>()?.EjecutarRelevoFlota24_7();
    }

    public void OnAbortarMisionPresionado()
    {
        if (tipoAlertaActual == TipoAlertaActiva.IncertidumbreDemandaWMS)
        {
            ResolverDecisionHorizonte(false);
            return;
        }

        MostrarEmergencia(false);
        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>();
        RobotAMR robot = (eve != null && eve.amrEnAlertaBateria20 != null)
            ? eve.amrEnAlertaBateria20
            : ((eve != null && eve.robotEjecutor != null) ? eve.robotEjecutor : UnityEngine.Object.FindObjectOfType<RobotAMR>());
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
        FindFirstObjectByType<RobotManagerEVE>()?.NotificarResolucionAlertaDemanda();

        string msg;
        if (aprobarExpansion)
        {
            if (ultimoEscenarioHorizonte == RobotManagerEVE.EscenarioHorizonte.EscenarioA_InicioTurno)
            {
                msg = $"<color=#10B981><b>[DECISIÓN GERENCIAL - ESCENARIO A]:</b></color> Aprobada flota de {ultimosAmrRequeridos} AMRs (2 activos + 1 soporte/relevo inductivo 1C). Se absorben los {ultimoLotePedidos} pedidos con 0% Carry-over.";
            }
            else if (ultimoEscenarioHorizonte == RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada)
            {
                msg = $"<color=#10B981><b>[DECISIÓN GERENCIAL - ESCENARIO B]:</b></color> Aprobado incremento a {ultimosAmrRequeridos} AMRs operativos para eliminar el {ultimoCarryOverPct:F0}% de Carry-over antes de las 06:00 AM.";
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
                else if (slot == telemetriaAMR03 && estado == RobotAMR.EstadoRobot.Inactivo && amr03Habilitado)
                {
                    slot.txtEstado.text = $"<color=#38BDF8>SOPORTE / RELEVO 24/7 ({porcentaje * 100f:F0}% SoC)</color>";
                }
                else
                {
                    slot.txtEstado.text = FormatearEstadoVisual(estado);
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
            case RobotAMR.EstadoRobot.Recargando: return "<color=#38BDF8>RECARGANDO (Inductivo 1C)</color>";
            case RobotAMR.EstadoRobot.Apagado: return "<color=#EF4444>APAGADO (<3% BMS)</color>";
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

        amr03Habilitado = activo;
        if (activo)
        {
            haSidoDesplegadoAMR03EnTurno = true;
        }
        alturaOriginalFlota = 502f;

        if (filaAMR03 != null)
        {
            filaAMR03.SetActive(activo);

            if (telemetriaAMR03 != null)
            {
                if (telemetriaAMR03.barraBateriaRelleno != null) telemetriaAMR03.barraBateriaRelleno.fillAmount = 0.80f;
                if (telemetriaAMR03.txtBateria != null) telemetriaAMR03.txtBateria.text = "80.0%";
                if (telemetriaAMR03.txtEstado != null) telemetriaAMR03.txtEstado.text = estadoTexto;
            }
            else
            {
                Transform tEstado = filaAMR03.transform.Find("Txt_Estado");
                if (tEstado != null)
                {
                    var tmp = tEstado.GetComponent<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = estadoTexto;
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

        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
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

        ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 1/8 (Operación Nominal) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        InicializarPanelesColapsables();
        ConfigurarPopUpEjecutivoSLA();
        ConfigurarSelectorCamarasSCADA();
        ConfigurarBarraOperativaInferior();

        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
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
            RefrescarEstadoSelectorCamaras(true);
            return true;
        }

        if (!hudActivoAhora)
        {
            hudOperativoEstabaActivo = false;
        }

        return false;
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
                if (fila03 != null) fila03.gameObject.SetActive(amr03Habilitado);
            }
            else
            {
                for (int i = 0; i < panelFlotaRect.childCount; i++)
                {
                    Transform child = panelFlotaRect.GetChild(i);
                    child.gameObject.SetActive(child.name != "Fila_AMR_03" || amr03Habilitado);
                }
            }
            if (txtBtnToggleFlota != null) txtBtnToggleFlota.text = "[-]";
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
            panelOverlayTimeLapse.SetActive(mostrar);
        }
    }

    public void ActualizarProgresoTimeLapse(float t)
    {
        t = Mathf.Clamp01(t);
        float minutosTotales = Mathf.Lerp(30f, 330f, t); // 00:30 AM (30m) -> 05:30 AM (330m)
        int horasReloj = Mathf.FloorToInt(minutosTotales / 60f);
        int minsReloj = Mathf.FloorToInt(minutosTotales % 60f);
        float horaTurno = Mathf.Lerp(1.0f, 7.5f, t);

        string cronometroTurno = $"{horasReloj:00}:{minsReloj:00} AM (Hora {horaTurno:F1}/8) — SIMULANDO FATIGA OPERATIVA DE FLOTA";

        if (txtOverlayTimeLapse != null)
        {
            txtOverlayTimeLapse.text = $"<b><color=#F59E0B>[COMPRESIÓN TEMPORAL]</color>  <color=#38BDF8>{horasReloj:00}:{minsReloj:00} AM (Hora {horaTurno:F1}/8)</color> — SIMULANDO FATIGA OPERATIVA DE FLOTA</b>";
        }

        ActualizarBannerTurno($"<b><color=#F59E0B>TIME-LAPSE:</color> {cronometroTurno}</b>");
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
        if (eveSuscritoPausa != null)
        {
            eveSuscritoPausa.OnPausaCambiada -= ActualizarVisualBotonPausa;
            eveSuscritoPausa = null;
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

    public void RegistrarFeedbackSupervisor(string informeTecnico)
    {
        if (string.IsNullOrEmpty(informeTecnico)) return;

        historialFeedbackSupervisor.Add(informeTecnico);
        contadorFeedbackBuzon++;

        if (txtBtnBuzonSupervisor == null)
        {
            Transform tBtn = transform.Find("HUD_Operativo/Panel_Controles_SCADA/Btn_Buzon_Supervisor/Text (TMP)");
            if (tBtn != null) txtBtnBuzonSupervisor = tBtn.GetComponent<TextMeshProUGUI>();
        }

        if (txtBtnBuzonSupervisor != null)
        {
            txtBtnBuzonSupervisor.text = $"Buzón [{contadorFeedbackBuzon}]";
        }
    }

    public void MostrarUltimoFeedbackBuzon()
    {
        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
        if (eve == null || !eve.turnoIniciado) return;

        if (historialFeedbackSupervisor.Count == 0)
        {
            MostrarPopUpEjecutivoSLA("BUZÓN DEL SUPERVISOR", "Sin reportes técnicos de rechazo pendientes.", 4.5f);
        }
        else
        {
            string ultimoInforme = historialFeedbackSupervisor[historialFeedbackSupervisor.Count - 1];
            MostrarPopUpEjecutivoSLA($"BUZÓN SUPERVISOR ({historialFeedbackSupervisor.Count})", ultimoInforme, 6.0f);
        }
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
            rt.sizeDelta = new Vector2(540f, 136f);
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
            RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
            RobotAMR amr02 = eve != null ? eve.robotAMR_02 : null;
            RobotAMR amr03 = eve != null ? eve.robotAMR_03 : null;
            DronMantenimiento dron = eve != null && eve.dronMantenimiento != null
                ? eve.dronMantenimiento
                : FindFirstObjectByType<DronMantenimiento>(FindObjectsInactive.Include);

            bool verAMR02 = amr02 != null && amr02.gameObject.activeInHierarchy;
            bool verAMR03 = amr03 != null && amr03.gameObject.activeInHierarchy && (haSidoDesplegadoAMR03EnTurno || amr03Habilitado);
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
        string timestamp = $"[{System.DateTime.Now:HH:mm:ss}]";
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

        if (sliderVelocidadAMR == null)
        {
            sliderVelocidadAMR = barra.Find("Grupo_Velocidad_AMR/Slider_Velocidad_AMR")?.GetComponent<Slider>();
        }
        if (txtValorVelocidadAMR == null)
        {
            txtValorVelocidadAMR = barra.Find("Grupo_Velocidad_AMR/Txt_Valor_Velocidad")?.GetComponent<TextMeshProUGUI>();
        }

        if (btnExportarCSV == null)
        {
            btnExportarCSV = barra.Find("Btn_Exportar_CSV")?.GetComponent<Button>();
        }

        if (btnPausaSimulacion != null)
        {
            btnPausaSimulacion.onClick.RemoveListener(OnClickPausarReanudarSimulacion);
            if (btnPausaSimulacion.onClick.GetPersistentEventCount() == 0)
            {
                btnPausaSimulacion.onClick.AddListener(OnClickPausarReanudarSimulacion);
            }
        }

        if (btnExportarCSV != null)
        {
            btnExportarCSV.onClick.RemoveListener(OnClickExportarTelemetriaCSV);
            if (btnExportarCSV.onClick.GetPersistentEventCount() == 0)
            {
                btnExportarCSV.onClick.AddListener(OnClickExportarTelemetriaCSV);
            }
        }

        RobotManagerEVE eve = RobotManagerEVE.Instance != null
            ? RobotManagerEVE.Instance
            : FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);

        float velocidadActual = eve != null ? eve.VelocidadFlotaActual : 3.5f;
        if (sliderVelocidadAMR != null)
        {
            sliderVelocidadAMR.minValue = 2.0f;
            sliderVelocidadAMR.maxValue = 5.5f;
            sliderVelocidadAMR.onValueChanged.RemoveListener(OnVelocidadFlotaCambiada);
            sliderVelocidadAMR.SetValueWithoutNotify(velocidadActual);
            sliderVelocidadAMR.onValueChanged.AddListener(OnVelocidadFlotaCambiada);
        }

        if (txtValorVelocidadAMR != null)
        {
            txtValorVelocidadAMR.text = $"{velocidadActual.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)} m/s";
        }

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

    public void OnClickPausarReanudarSimulacion()
    {
        if (ultimoFrameTogglePausa == Time.frameCount) return;
        ultimoFrameTogglePausa = Time.frameCount;

        RobotManagerEVE eve = RobotManagerEVE.Instance != null
            ? RobotManagerEVE.Instance
            : FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
        if (eve == null) return;

        bool pausada = eve.AlternarPausaSimulacion();
        ActualizarVisualBotonPausa(pausada);
    }

    public void OnVelocidadFlotaCambiada(float valor)
    {
        float vel = Mathf.Clamp(valor, 2.0f, 5.5f);
        RobotManagerEVE eve = RobotManagerEVE.Instance != null
            ? RobotManagerEVE.Instance
            : FindFirstObjectByType<RobotManagerEVE>(FindObjectsInactive.Include);
        eve?.AjustarVelocidadFlotaEnVivo(vel);

        if (txtValorVelocidadAMR != null)
        {
            txtValorVelocidadAMR.text = $"{vel.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)} m/s";
        }
    }

    public void OnClickExportarTelemetriaCSV()
    {
        ExportadorTelemetriaCSV.ExportarReporteTurnoCSV(out _);
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
}
