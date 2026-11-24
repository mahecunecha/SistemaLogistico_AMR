using System;
using UnityEngine;

/// <summary>
/// Fuente única de verdad del reloj oficial del turno nocturno (18:00:00 a 02:00:00, 8 horas exactas).
/// Usa un acumulador determinista en segundos (1:1 con segundos reales) sin compresión temporal
/// y es completamente independiente de la hora y zona horaria del sistema operativo.
/// Expone propiedades de solo lectura para RobotManagerEVE, DashboardUI, ModuloDespachoTMS y MarcadorTurnoNBA.
/// </summary>
[DefaultExecutionOrder(-100)]
public class RelojSimuladorTurno : MonoBehaviour
{
    public const float DuracionOperativaTurnoHoras = 8.0f;
    public const int SegundosInicioTurno = 18 * 3600; // 18:00:00
    public const int DuracionTurnoSegundos = 8 * 3600; // 28,800 s (18:00:00 -> 02:00:00)
    public const string HoraCierreTurnoTexto = "02:00:00";

    private static double segundosTranscurridosTurno = 0.0;
    private static RelojSimuladorTurno instanciaActiva;

    public static double SegundosTranscurridos => segundosTranscurridosTurno;

    public static long SegundosTranscurridosEnteros
    {
        get
        {
            double clamped = Math.Max(0.0, Math.Min(DuracionTurnoSegundos, segundosTranscurridosTurno));
            long seg = (long)Math.Floor(clamped + 1e-9);
            if (seg < 0L) return 0L;
            if (seg > DuracionTurnoSegundos) return DuracionTurnoSegundos;
            return seg;
        }
    }

    public static long SegundosRestantesEnteros => DuracionTurnoSegundos - SegundosTranscurridosEnteros;

    public static double SegundosRestantes => Math.Max(0.0, DuracionTurnoSegundos - segundosTranscurridosTurno);

    public static float HorasTranscurridas => (float)(segundosTranscurridosTurno / 3600.0);

    public static float HorasRestantesHastaCierre => (float)(SegundosRestantes / 3600.0);

    public static bool TurnoSuperoCierre => segundosTranscurridosTurno >= (DuracionTurnoSegundos - 1e-6);

    public static string HoraActualFormateada => FormatearHoraDesdeSegundos(SegundosTranscurridosEnteros);

    public static string TiempoRestanteFormateado => FormatearDuracionDesdeSegundos(SegundosRestantesEnteros);

    public static string ResumenRelojTurno =>
        $"{HoraActualFormateada} (Restan {TiempoRestanteFormateado} | Cierre {HoraCierreTurnoTexto})";

    public static float CalcularHorasRestantesDesde(float horasOperativas)
    {
        if (float.IsNaN(horasOperativas) || float.IsInfinity(horasOperativas))
        {
            return DuracionOperativaTurnoHoras;
        }
        return Mathf.Max(0f, DuracionOperativaTurnoHoras - Mathf.Clamp(horasOperativas, 0f, DuracionOperativaTurnoHoras));
    }

    public static string FormatearHoraDesdeSegundos(long segundosOperativos)
    {
        long offset = segundosOperativos < 0L ? 0L : (segundosOperativos > DuracionTurnoSegundos ? DuracionTurnoSegundos : segundosOperativos);
        long segundosDia = (SegundosInicioTurno + offset) % 86400L;

        long horas = segundosDia / 3600L;
        long minutos = (segundosDia % 3600L) / 60L;
        long segundos = segundosDia % 60L;
        return $"{horas:00}:{minutos:00}:{segundos:00}";
    }

    public static string FormatearDuracionDesdeSegundos(long totalSegundos)
    {
        long clamped = totalSegundos < 0L ? 0L : totalSegundos;
        long horas = clamped / 3600L;
        long minutos = (clamped % 3600L) / 60L;
        long segundos = clamped % 60L;
        return $"{horas:00}:{minutos:00}:{segundos:00}";
    }

    public static string FormatearHoraRelojNocturno(float horasOperativas)
    {
        double horasValidas = (float.IsNaN(horasOperativas) || float.IsInfinity(horasOperativas))
            ? 0.0
            : Math.Max(0.0, Math.Min(DuracionOperativaTurnoHoras, (double)horasOperativas));
        long segundosOffset = (long)Math.Round(horasValidas * 3600.0, MidpointRounding.AwayFromZero);
        return FormatearHoraDesdeSegundos(segundosOffset);
    }

    public static string FormatearDuracionHMS(float horasDuracion)
    {
        double horasValidas = (float.IsNaN(horasDuracion) || float.IsInfinity(horasDuracion))
            ? 0.0
            : Math.Max(0.0, (double)horasDuracion);
        long totalSegundos = (long)Math.Round(horasValidas * 3600.0, MidpointRounding.AwayFromZero);
        return FormatearDuracionDesdeSegundos(totalSegundos);
    }

    /// <summary>
    /// Reinicia el reloj oficial al comenzar un nuevo turno del simulador (18:00:00 por defecto).
    /// </summary>
    public static void ReiniciarRelojTurno(float horaInicial = 0.0f)
    {
        EstablecerHoraSimuladorInterno(horaInicial);
    }

    /// <summary>
    /// Uso exclusivo del ciclo interno del simulador y pruebas unitarias.
    /// La lógica de admisión de EVE debe consultar únicamente propiedades de solo lectura.
    /// </summary>
    public static void EstablecerHoraSimuladorInterno(float horaOperativa)
    {
        double horasValidas = (float.IsNaN(horaOperativa) || float.IsInfinity(horaOperativa))
            ? 0.0
            : Math.Max(0.0, Math.Min(DuracionOperativaTurnoHoras, (double)horaOperativa));
        segundosTranscurridosTurno = Math.Max(0.0, Math.Min(DuracionTurnoSegundos, horasValidas * 3600.0));
    }

    /// <summary>
    /// Establece el tiempo transcurrido exacto en segundos operativos [0, 28800].
    /// </summary>
    public static void EstablecerSegundosSimuladorInterno(double segundosOperativos)
    {
        if (double.IsNaN(segundosOperativos) || double.IsInfinity(segundosOperativos))
        {
            segundosTranscurridosTurno = 0.0;
            return;
        }
        segundosTranscurridosTurno = Math.Max(0.0, Math.Min(DuracionTurnoSegundos, segundosOperativos));
    }

    /// <summary>
    /// Avanza el reloj del turno de forma determinista 1:1 en segundos reales hasta el cierre (28,800 s).
    /// No aplica factores de compresión temporal sobre el movimiento/ciclo de los AMR.
    /// </summary>
    public static void AvanzarTickSimulador(double deltaSegundos)
    {
        if (double.IsNaN(deltaSegundos) || double.IsInfinity(deltaSegundos) || deltaSegundos <= 0.0)
        {
            return;
        }

        if (segundosTranscurridosTurno >= DuracionTurnoSegundos)
        {
            segundosTranscurridosTurno = DuracionTurnoSegundos;
            return;
        }

        segundosTranscurridosTurno = Math.Min(DuracionTurnoSegundos, segundosTranscurridosTurno + deltaSegundos);
    }

    public static bool AvanzarTickSiTurnoActivo(bool turnoIniciado, bool simulacionPausada, double deltaSegundos)
    {
        if (!turnoIniciado || simulacionPausada || TurnoSuperoCierre)
        {
            return false;
        }

        if (double.IsNaN(deltaSegundos) || double.IsInfinity(deltaSegundos) || deltaSegundos <= 0.0)
        {
            return false;
        }

        AvanzarTickSimulador(deltaSegundos);
        return true;
    }

    void Awake()
    {
        if (instanciaActiva == null)
        {
            instanciaActiva = this;
        }

        if (Application.isPlaying && (RobotManagerEVE.Instance == null || !RobotManagerEVE.Instance.turnoIniciado))
        {
            ReiniciarRelojTurno(0.0f);
        }
    }

    void OnEnable()
    {
        if (instanciaActiva == null)
        {
            instanciaActiva = this;
        }
    }

    void OnDisable()
    {
        if (instanciaActiva == this)
        {
            instanciaActiva = null;
        }
    }

    void OnDestroy()
    {
        if (instanciaActiva == this)
        {
            instanciaActiva = null;
        }
    }

    void Update()
    {
        if (instanciaActiva != null && instanciaActiva != this) return;
        if (Time.timeScale <= 0f) return;

        RobotManagerEVE eve = RobotManagerEVE.Instance;
        if (eve == null) return;

        AvanzarTickSiTurnoActivo(eve.turnoIniciado, eve.SimulacionPausada, Time.deltaTime);
    }
}
