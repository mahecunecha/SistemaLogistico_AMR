using UnityEngine;
using TMPro;

/// <summary>
/// Sincroniza el marcador físico 3D estilo Arena NBA en la bodega con el reloj determinista de 8h.
/// Utiliza MaterialPropertyBlock para modular el color LED sin instanciar materiales en tiempo de ejecución.
/// </summary>
public class MarcadorTurnoNBA : MonoBehaviour
{
    [Header("Textos de las 4 Caras del Marcador")]
    [SerializeField] private TMP_Text[] textosHoraTurno;
    [SerializeField] private TMP_Text[] textosCuentaRegresiva;
    [SerializeField] private TMP_Text[] textosEstadoTurno;

    [Header("Bordes LED de Acento")]
    [SerializeField] private MeshRenderer[] renderersAcentoLED;

    private string ultimaClaveVisual = string.Empty;
    private MaterialPropertyBlock propiedadLED;

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor = Shader.PropertyToID("_Color");

    private static readonly Color ColorCyanNominal = new Color(0.22f, 0.74f, 0.97f, 1f);
    private static readonly Color ColorAmbarOperativo = new Color(0.98f, 0.75f, 0.14f, 1f);
    private static readonly Color ColorRojoCritico = new Color(0.94f, 0.27f, 0.27f, 1f);
    private static readonly Color ColorVerdeOptimo = new Color(0.29f, 0.87f, 0.50f, 1f);

    void Start()
    {
        RefrescarMarcador(forzar: true);
    }

    void Update()
    {
        RefrescarMarcador(forzar: false);
    }

    public void RefrescarMarcador(bool forzar = false)
    {
        string horaActual = RelojSimuladorTurno.HoraActualFormateada;
        string tiempoRestante = RelojSimuladorTurno.TiempoRestanteFormateado;
        float horasRestantes = RelojSimuladorTurno.HorasRestantesHastaCierre;
        bool cierreAlcanzado = RelojSimuladorTurno.TurnoSuperoCierre;

        RobotManagerEVE eve = RobotManagerEVE.Instance;
        bool turnoIniciado = eve != null && eve.turnoIniciado;
        bool faseCritica = eve != null && (eve.turnoEnFaseCritica || eve.operacionDetenidaPorSeguridad);
        bool pausada = eve != null && eve.SimulacionPausada;
        bool bloqueoAdmision = eve != null && eve.admisionNuevosPedidosBloqueada;
        bool dualActivo = eve != null && eve.flotaEscaladaDual;
        bool tieneCola = eve != null && eve.palletsEnColaWMS > 0;
        float eficiencia = !tieneCola ? 100f : (eve != null ? (dualActivo ? eve.EficienciaPromedioDual : eve.EficienciaUnitariaAMR01) : 100f);

        string textoEstado;
        Color colorAcento;
        Color colorRestante;

        if (faseCritica)
        {
            textoEstado = "PARADA SEGURIDAD";
            colorAcento = ColorRojoCritico;
            colorRestante = ColorRojoCritico;
        }
        else if (cierreAlcanzado)
        {
            textoEstado = "CIERRE 02:00:00";
            colorAcento = ColorRojoCritico;
            colorRestante = ColorRojoCritico;
        }
        else if (pausada)
        {
            textoEstado = "PAUSA SUPERVISOR";
            colorAcento = ColorAmbarOperativo;
            colorRestante = ColorAmbarOperativo;
        }
        else if (bloqueoAdmision)
        {
            textoEstado = "CONTINUIDAD • BLOQ.";
            colorAcento = ColorAmbarOperativo;
            colorRestante = horasRestantes <= 0.5f ? ColorRojoCritico : ColorAmbarOperativo;
        }
        else if (!turnoIniciado)
        {
            textoEstado = "STANDBY • 8H";
            colorAcento = ColorCyanNominal;
            colorRestante = ColorAmbarOperativo;
        }
        else
        {
            string modo = dualActivo ? "DUAL 2 AMR" : "1 AMR";
            textoEstado = $"{modo} • EF {eficiencia:F0}%";
            colorAcento = horasRestantes <= 0.5f ? ColorRojoCritico : (tieneCola ? ColorVerdeOptimo : ColorCyanNominal);
            colorRestante = horasRestantes <= 0.5f ? ColorRojoCritico : ColorAmbarOperativo;
        }

        string clave = $"{horaActual}|{tiempoRestante}|{textoEstado}|{colorAcento.r:F2}{colorAcento.g:F2}{colorRestante.r:F2}";
        if (!forzar && clave == ultimaClaveVisual)
        {
            return;
        }
        ultimaClaveVisual = clave;

        if (textosHoraTurno != null)
        {
            for (int i = 0; i < textosHoraTurno.Length; i++)
            {
                if (textosHoraTurno[i] != null)
                {
                    textosHoraTurno[i].text = horaActual;
                }
            }
        }

        if (textosCuentaRegresiva != null)
        {
            string etiquetaRestante = cierreAlcanzado ? "RESTAN 00:00:00" : $"RESTAN {tiempoRestante}";
            for (int i = 0; i < textosCuentaRegresiva.Length; i++)
            {
                if (textosCuentaRegresiva[i] != null)
                {
                    textosCuentaRegresiva[i].text = etiquetaRestante;
                    textosCuentaRegresiva[i].color = colorRestante;
                }
            }
        }

        if (textosEstadoTurno != null)
        {
            for (int i = 0; i < textosEstadoTurno.Length; i++)
            {
                if (textosEstadoTurno[i] != null)
                {
                    textosEstadoTurno[i].text = textoEstado;
                    textosEstadoTurno[i].color = colorAcento;
                }
            }
        }

        AplicarColorAcentoLED(colorAcento);
    }

    private void AplicarColorAcentoLED(Color color)
    {
        if (renderersAcentoLED == null || renderersAcentoLED.Length == 0) return;
        if (propiedadLED == null)
        {
            propiedadLED = new MaterialPropertyBlock();
        }

        for (int i = 0; i < renderersAcentoLED.Length; i++)
        {
            MeshRenderer mr = renderersAcentoLED[i];
            if (mr == null) continue;
            mr.GetPropertyBlock(propiedadLED);
            propiedadLED.SetColor(IdBaseColor, color);
            propiedadLED.SetColor(IdColor, color);
            mr.SetPropertyBlock(propiedadLED);
        }
    }
}
