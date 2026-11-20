using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controlador de audio espacial 3D pasivo para AMRs terrestres, dron orquestador EVE y dron de soporte.
/// </summary>
public class AudioEntidadIndustrial : MonoBehaviour
{
    public enum TipoEntidadAudio { AMR_Terrestre, Dron_EVE, Dron_Soporte }

    [Header("Configuración de Entidad")]
    public TipoEntidadAudio tipoEntidad = TipoEntidadAudio.AMR_Terrestre;
    public AudioSource fuente3D;

    private RobotAMR amr;
    private NavMeshAgent agente;
    private RobotManagerEVE eve;
    private DronMantenimiento dron;
    private Vector3 posAnterior;
    private float volumenActual = 0f;
    private float pitchActual = 1f;

    void Awake()
    {
        if (!Application.isPlaying) return;

        ConfigurarFuente3D();
        ResolverReferenciasEntidad();
        posAnterior = transform.position;
    }

    void Start()
    {
        if (!Application.isPlaying) return;

        IntentarAsignarClipLoop();
        posAnterior = transform.position;
    }

    void Update()
    {
        if (!Application.isPlaying || fuente3D == null) return;

        if (fuente3D.clip == null)
        {
            IntentarAsignarClipLoop();
            if (fuente3D.clip == null)
            {
                posAnterior = transform.position;
                return;
            }
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 deltaPos = transform.position - posAnterior;
        float velocidadMundo = deltaPos.magnitude / dt;
        float velocidadVertical = deltaPos.y / dt;
        posAnterior = transform.position;

        EvaluarObjetivoAudio(velocidadMundo, velocidadVertical, out float volumenObjetivo, out float pitchObjetivo);

        if (DashboardUI.Instance != null && DashboardUI.Instance.EsModalEmergenciaActivo())
        {
            volumenObjetivo *= 0.25f;
        }

        volumenActual = Mathf.MoveTowards(volumenActual, volumenObjetivo, Time.unscaledDeltaTime * 0.85f);
        pitchActual = Mathf.Lerp(pitchActual, pitchObjetivo, Mathf.Clamp01(Time.unscaledDeltaTime * 6f));

        fuente3D.volume = volumenActual;
        fuente3D.pitch = Mathf.Clamp(pitchActual, 0.5f, 2.0f);

        if (volumenActual > 0.005f)
        {
            if (!fuente3D.isPlaying)
            {
                fuente3D.Play();
            }
        }
        else if (fuente3D.isPlaying)
        {
            fuente3D.Stop();
        }
    }

    private void ConfigurarFuente3D()
    {
        if (fuente3D == null)
        {
            fuente3D = GetComponent<AudioSource>();
            if (fuente3D == null)
            {
                fuente3D = gameObject.AddComponent<AudioSource>();
            }
        }

        fuente3D.playOnAwake = false;
        fuente3D.loop = true;
        fuente3D.spatialBlend = 0.90f;
        fuente3D.rolloffMode = AudioRolloffMode.Logarithmic;
        fuente3D.dopplerLevel = 0f;
        fuente3D.volume = 0f;

        switch (tipoEntidad)
        {
            case TipoEntidadAudio.Dron_EVE:
                fuente3D.minDistance = 3.0f;
                fuente3D.maxDistance = 20.0f;
                break;
            case TipoEntidadAudio.Dron_Soporte:
                fuente3D.minDistance = 3.5f;
                fuente3D.maxDistance = 26.0f;
                break;
            case TipoEntidadAudio.AMR_Terrestre:
            default:
                fuente3D.minDistance = 2.5f;
                fuente3D.maxDistance = 16.0f;
                break;
        }
    }

    private void ResolverReferenciasEntidad()
    {
        if (amr == null) amr = GetComponent<RobotAMR>();
        if (agente == null) agente = GetComponent<NavMeshAgent>();
        if (eve == null) eve = GetComponent<RobotManagerEVE>();
        if (dron == null) dron = GetComponent<DronMantenimiento>();
    }

    private void IntentarAsignarClipLoop()
    {
        if (fuente3D == null || GestorAudioSCADA.Instance == null) return;

        switch (tipoEntidad)
        {
            case TipoEntidadAudio.Dron_EVE:
                fuente3D.clip = GestorAudioSCADA.Instance.ClipRotoresEVE;
                break;
            case TipoEntidadAudio.Dron_Soporte:
                fuente3D.clip = GestorAudioSCADA.Instance.ClipRotoresDronSoporte;
                break;
            case TipoEntidadAudio.AMR_Terrestre:
            default:
                fuente3D.clip = GestorAudioSCADA.Instance.ClipMotorAMR;
                break;
        }
    }

    private void EvaluarObjetivoAudio(float velocidadMundo, float velocidadVertical, out float volumenObjetivo, out float pitchObjetivo)
    {
        volumenObjetivo = 0f;
        pitchObjetivo = 1f;

        switch (tipoEntidad)
        {
            case TipoEntidadAudio.AMR_Terrestre:
                if (amr == null) return;
                float vel = (agente != null && agente.enabled) ? agente.velocity.magnitude : velocidadMundo;
                bool enDesplazamiento = (amr.estadoActual == RobotAMR.EstadoRobot.En_Transito ||
                                         amr.estadoActual == RobotAMR.EstadoRobot.Transportando) &&
                                        !amr.EnParadaSeguridadLiDAR &&
                                        vel > 0.15f;
                if (enDesplazamiento)
                {
                    float normVel = Mathf.Clamp01(vel / 3.5f);
                    volumenObjetivo = 0.22f * Mathf.Lerp(0.45f, 1.0f, normVel);
                    pitchObjetivo = Mathf.Lerp(0.85f, 1.22f, normVel) - (amr.TieneCargaFisica ? 0.10f : 0f);
                }
                else
                {
                    pitchObjetivo = 0.85f;
                }
                break;

            case TipoEntidadAudio.Dron_EVE:
                if (eve == null) return;
                volumenObjetivo = eve.turnoIniciado ? 0.18f : 0.08f;
                float normEve = Mathf.Clamp01(velocidadMundo / Mathf.Max(0.5f, eve.velocidadVuelo));
                pitchObjetivo = Mathf.Lerp(0.95f, 1.12f, normEve);
                break;

            case TipoEntidadAudio.Dron_Soporte:
                if (dron == null || !dron.ocupado)
                {
                    pitchObjetivo = 0.90f;
                    return;
                }
                volumenObjetivo = 0.26f;
                float factorAscenso = Mathf.Clamp01((velocidadVertical + 2.0f) / 6.0f);
                pitchObjetivo = dron.tieneCargaAcoplada
                    ? Mathf.Lerp(1.02f, 1.18f, factorAscenso)
                    : Mathf.Lerp(0.90f, 1.06f, factorAscenso);
                break;
        }
    }
}
