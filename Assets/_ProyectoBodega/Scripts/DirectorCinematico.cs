using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class DirectorCinematico : MonoBehaviour
{
    public static DirectorCinematico Instance { get; private set; }

    [Header("Emisores")]
    public RobotManagerEVE managerEVE;
    public RobotAMR robotAMR;

    [Header("Cámaras Virtuales")]
    public CinemachineCamera vcam0_Intro;
    public CinemachineCamera vcam1_Menu;
    public CinemachineCamera vcam2_EVE;
    public CinemachineCamera vcam3_AMR_Hombro;
    public CinemachineCamera vcam4_Emergencia;

    [Header("Cinemática de Apertura (Establishing Shot)")]
    public CanvasGroup canvasGroupMenu;
    public bool omitirIntroEnDebug = false;
    public float duracionRecorridoBodega = 5.5f;
    public float duracionOrbitaEVE = 4.5f;
    public float radioOrbitaEVE = 4.0f;

    [Header("Estado de Foco Crítico")]
    private bool focoIncidenteActivo = false;

    public enum ModoVistaCamara
    {
        AutoDirectorWMS,
        GeneralPlanta,
        EveOrquestador,
        AMR01,
        AMR02,
        AMR03,
        DronSoporteISO
    }

    [Header("Control de Cámara SCADA")]
    public bool modoCamaraManual = false;
    public ModoVistaCamara vistaActual = ModoVistaCamara.AutoDirectorWMS;
    public DronMantenimiento dronSoporte;
    private ModoVistaCamara vistaPreviaDron = ModoVistaCamara.AutoDirectorWMS;
    private RobotAMR amrEnfocadoVCam3;
    private readonly Dictionary<RobotAMR, System.Action<RobotAMR.EstadoRobot>> handlersEstadoAMR =
        new Dictionary<RobotAMR, System.Action<RobotAMR.EstadoRobot>>();

    private CinemachineFollow followVCam3;
    private CinemachineRotationComposer composerVCam3;
    private CinemachineFollow followVCam4;
    private CinemachineRotationComposer composerVCam4;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        CinemachineBrain brain = Camera.main != null
            ? Camera.main.GetComponent<CinemachineBrain>()
            : FindFirstObjectByType<CinemachineBrain>();
        if (brain != null)
        {
            brain.IgnoreTimeScale = true;
        }

        SuscribirFlota(true);
        StartCoroutine(SecuenciaIntroCinematica());
    }

    private IEnumerator SecuenciaIntroCinematica()
    {
        if (vcam0_Intro == null || omitirIntroEnDebug)
        {
            if (canvasGroupMenu != null)
            {
                canvasGroupMenu.alpha = 1f;
                canvasGroupMenu.interactable = true;
                canvasGroupMenu.blocksRaycasts = true;
            }
            DesactivarPrioridades();
            if (vcam1_Menu != null) vcam1_Menu.Priority = 10;
            yield break;
        }

        // Preparación: Ocultar menú y activar cámara de introducción
        if (canvasGroupMenu != null)
        {
            canvasGroupMenu.alpha = 0f;
            canvasGroupMenu.interactable = false;
            canvasGroupMenu.blocksRaycasts = false;
        }

        DesactivarPrioridades();
        vcam0_Intro.Priority = 20;

        Vector3 posEVE = managerEVE != null ? managerEVE.transform.position : new Vector3(-24.0f, 9.5f, -10.0f);
        const float yOffsetOrbita = 0.8f;

        // Nodos del trazado ortogonal (Acto 1)
        Vector3[] nodosRuta = new Vector3[]
        {
            new Vector3(34.0f, 10.8f, -10.0f),                                         // Cabecera Noreste
            new Vector3(34.0f, 10.8f, -33.0f),                                         // Crucero Este
            new Vector3(-24.0f, 10.8f, -33.0f),                                        // Crucero Oeste
            new Vector3(posEVE.x, posEVE.y + yOffsetOrbita, posEVE.z - radioOrbitaEVE) // Punto de encuentro frente a EVE (-24.0, 10.3, -14.0)
        };

        float[] longitudesSegmentos = new float[nodosRuta.Length - 1];
        float longitudTotal = 0f;
        for (int i = 0; i < longitudesSegmentos.Length; i++)
        {
            longitudesSegmentos[i] = Vector3.Distance(nodosRuta[i], nodosRuta[i + 1]);
            longitudTotal += longitudesSegmentos[i];
        }

        vcam0_Intro.transform.position = nodosRuta[0];
        Vector3 dirInicial = (nodosRuta[1] - nodosRuta[0]).normalized + new Vector3(-0.25f, -0.30f, 0f);
        vcam0_Intro.transform.rotation = Quaternion.LookRotation(dirInicial.normalized, Vector3.up);

        bool skipSolicitado = false;

        // Acto 1: Recorrido Ortogonal por la Bodega
        float tiempoActo1 = 0f;
        float duracionActo1 = Mathf.Max(0.1f, duracionRecorridoBodega);

        while (tiempoActo1 < duracionActo1)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                skipSolicitado = true;
                break;
            }

            tiempoActo1 += Time.deltaTime;
            float tNorm = Mathf.Clamp01(tiempoActo1 / duracionActo1);
            float tSuave = Mathf.SmoothStep(0f, 1f, tNorm);
            float distanciaActual = tSuave * longitudTotal;

            int segIndex = 0;
            float acumulado = 0f;
            for (int i = 0; i < longitudesSegmentos.Length; i++)
            {
                if (distanciaActual <= acumulado + longitudesSegmentos[i] || i == longitudesSegmentos.Length - 1)
                {
                    segIndex = i;
                    break;
                }
                acumulado += longitudesSegmentos[i];
            }

            float tSegmento = longitudesSegmentos[segIndex] > 0.001f
                ? Mathf.Clamp01((distanciaActual - acumulado) / longitudesSegmentos[segIndex])
                : 1f;

            Vector3 nuevaPos = Vector3.Lerp(nodosRuta[segIndex], nodosRuta[segIndex + 1], tSegmento);
            vcam0_Intro.transform.position = nuevaPos;

            Vector3 dirAvance = (nodosRuta[segIndex + 1] - nodosRuta[segIndex]).normalized;
            Quaternion rotObjetivo;
            if (segIndex == longitudesSegmentos.Length - 1)
            {
                Vector3 miradaEVE = (posEVE + Vector3.up * 0.2f) - nuevaPos;
                rotObjetivo = Quaternion.LookRotation(miradaEVE.normalized, Vector3.up);
            }
            else
            {
                Vector3 inclinacionPlanta = dirAvance + new Vector3(0f, -0.28f, 0f);
                rotObjetivo = Quaternion.LookRotation(inclinacionPlanta.normalized, Vector3.up);
            }

            vcam0_Intro.transform.rotation = Quaternion.Slerp(vcam0_Intro.transform.rotation, rotObjetivo, Time.deltaTime * 4.5f);

            yield return null;
        }

        // Acto 2: Encuentro y Giro Orbital 360° alrededor de EVE
        if (!skipSolicitado)
        {
            float tiempoActo2 = 0f;
            float duracionActo2 = Mathf.Max(0.1f, duracionOrbitaEVE);
            const float anguloInicial = -Mathf.PI * 0.5f;

            while (tiempoActo2 < duracionActo2)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
                {
                    skipSolicitado = true;
                    break;
                }

                tiempoActo2 += Time.deltaTime;
                float tNorm = Mathf.Clamp01(tiempoActo2 / duracionActo2);
                float tSuave = Mathf.SmoothStep(0f, 1f, tNorm);

                posEVE = managerEVE != null ? managerEVE.transform.position : posEVE;
                float anguloActual = anguloInicial + (tSuave * Mathf.PI * 2f);

                Vector3 offsetOrbital = new Vector3(
                    Mathf.Cos(anguloActual) * radioOrbitaEVE,
                    yOffsetOrbita,
                    Mathf.Sin(anguloActual) * radioOrbitaEVE
                );

                vcam0_Intro.transform.position = posEVE + offsetOrbital;
                vcam0_Intro.transform.LookAt(posEVE + Vector3.up * 0.2f);

                yield return null;
            }
        }

        // Acto 3: Transición hacia VCam1_Menu y revelación del Menú Principal
        vcam0_Intro.Priority = 0;
        if (vcam1_Menu != null) vcam1_Menu.Priority = 10;
        Debug.Log("<color=orange>Director:</color> Fin de cinemática de introducción. Transición a VCam1 (Menu).");

        float duracionTransicionMenu = skipSolicitado ? 0.6f : 2.0f;
        float tiempoActo3 = 0f;

        while (tiempoActo3 < duracionTransicionMenu)
        {
            tiempoActo3 += Time.deltaTime;
            float tAlpha = Mathf.Clamp01(tiempoActo3 / duracionTransicionMenu);
            if (canvasGroupMenu != null)
            {
                canvasGroupMenu.alpha = Mathf.SmoothStep(0f, 1f, tAlpha);
            }
            yield return null;
        }

        if (canvasGroupMenu != null)
        {
            canvasGroupMenu.alpha = 1f;
            canvasGroupMenu.interactable = true;
            canvasGroupMenu.blocksRaycasts = true;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private Coroutine rutinaTimeLapseActiva;

    public void EjecutarBarridoTimeLapse(float duracion)
    {
        if (rutinaTimeLapseActiva != null)
        {
            StopCoroutine(rutinaTimeLapseActiva);
        }
        rutinaTimeLapseActiva = StartCoroutine(RutinaBarridoTimeLapse(duracion));
    }

    private IEnumerator RutinaBarridoTimeLapse(float duracion)
    {
        if (vcam0_Intro == null)
        {
            yield break;
        }

        float tiempoTotal = Mathf.Max(0.1f, duracion);
        float tiempo = 0f;

        // Travelling panorámico elevado a Y = 11.5f recorriendo la nave central
        Vector3 puntoInicio = new Vector3(-22.0f, 11.5f, -31.0f);
        Vector3 puntoFin = new Vector3(30.0f, 11.5f, -12.0f);
        Vector3 centroPasillos = new Vector3(5.0f, 3.5f, -21.5f);

        vcam0_Intro.transform.position = puntoInicio;
        Vector3 posInicialEVE = managerEVE != null ? managerEVE.transform.position : new Vector3(-10.0f, 9.5f, -21.5f);
        Vector3 focoInicial = Vector3.Lerp(centroPasillos, posInicialEVE, 0.55f);
        if ((focoInicial - puntoInicio).sqrMagnitude > 0.01f)
        {
            vcam0_Intro.transform.rotation = Quaternion.LookRotation((focoInicial - puntoInicio).normalized, Vector3.up);
        }

        vcam0_Intro.Priority = 25;
        Debug.Log("<color=orange>Director:</color> Iniciando barrido aéreo Time-Lapse en VCam0_Intro (Priority = 25, Y = 11.5f).");

        while (tiempo < tiempoTotal)
        {
            tiempo += Time.deltaTime;
            float tNorm = Mathf.Clamp01(tiempo / tiempoTotal);
            float tSuave = Mathf.SmoothStep(0f, 1f, tNorm);

            Vector3 posCam = Vector3.Lerp(puntoInicio, puntoFin, tSuave);
            posCam.y = 11.5f;
            vcam0_Intro.transform.position = posCam;

            Vector3 posEVEActual = managerEVE != null ? managerEVE.transform.position : posInicialEVE;
            Vector3 puntoMirada = Vector3.Lerp(centroPasillos, posEVEActual, 0.55f);
            Vector3 dirMirada = puntoMirada - posCam;
            if (dirMirada.sqrMagnitude > 0.01f)
            {
                Quaternion rotObjetivo = Quaternion.LookRotation(dirMirada.normalized, Vector3.up);
                vcam0_Intro.transform.rotation = Quaternion.Slerp(vcam0_Intro.transform.rotation, rotObjetivo, Time.deltaTime * 4.0f);
            }

            yield return null;
        }

        vcam0_Intro.Priority = 0;
        rutinaTimeLapseActiva = null;
        Debug.Log("<color=orange>Director:</color> Barrido aéreo Time-Lapse finalizado (VCam0_Intro Priority = 0).");
    }

    public void RelevarRobotActivo(RobotAMR nuevoRobot)
    {
        SuscribirFlota(false);
        robotAMR = nuevoRobot;
        SuscribirFlota(true);

        if (robotAMR != null)
        {
            if (!modoCamaraManual && vcam3_AMR_Hombro != null)
            {
                amrEnfocadoVCam3 = robotAMR;
                vcam3_AMR_Hombro.Target.TrackingTarget = robotAMR.transform;
            }
            if (!focoIncidenteActivo)
            {
                if (vcam4_Emergencia != null) vcam4_Emergencia.Target.TrackingTarget = robotAMR.transform;
                if (!modoCamaraManual)
                {
                    EvaluarEstadoAMR(robotAMR, robotAMR.estadoActual);
                }
            }
        }
    }

    private void OnEnable()
    {
        SuscribirFlota(true);
    }

    private void OnDisable()
    {
        SuscribirFlota(false);
    }

    private void SuscribirFlota(bool suscribir)
    {
        if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();

        if (managerEVE != null)
        {
            managerEVE.OnMisionEvaluada -= EnfocarEVE;
            if (suscribir) managerEVE.OnMisionEvaluada += EnfocarEVE;
        }

        foreach (var kvp in handlersEstadoAMR)
        {
            if (kvp.Key != null)
            {
                kvp.Key.OnEstadoCambiado -= kvp.Value;
            }
        }
        handlersEstadoAMR.Clear();

        if (!suscribir) return;

        var unicos = new HashSet<RobotAMR>();
        if (robotAMR != null) unicos.Add(robotAMR);
        if (managerEVE != null)
        {
            if (managerEVE.robotAMR_01 != null) unicos.Add(managerEVE.robotAMR_01);
            if (managerEVE.robotAMR_02 != null) unicos.Add(managerEVE.robotAMR_02);
            if (managerEVE.robotAMR_03 != null) unicos.Add(managerEVE.robotAMR_03);
            if (managerEVE.robotEjecutor != null) unicos.Add(managerEVE.robotEjecutor);
        }

        foreach (RobotAMR amr in unicos)
        {
            if (amr == null) continue;
            RobotAMR capturaAMR = amr;
            System.Action<RobotAMR.EstadoRobot> handler = estado => EvaluarEstadoAMR(capturaAMR, estado);
            handlersEstadoAMR[capturaAMR] = handler;
            capturaAMR.OnEstadoCambiado += handler;
        }
    }

    private static bool EstaEnMovimientoActivo(RobotAMR amr)
    {
        if (amr == null) return false;
        return amr.estadoActual == RobotAMR.EstadoRobot.En_Transito ||
               amr.estadoActual == RobotAMR.EstadoRobot.Transportando ||
               amr.estadoActual == RobotAMR.EstadoRobot.Extrayendo ||
               amr.estadoActual == RobotAMR.EstadoRobot.Entregando;
    }

    private RobotAMR BuscarOtroAMRActivo(RobotAMR excluir)
    {
        RobotAMR[] candidatos = ObtenerCandidatosFlota();
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR c = candidatos[i];
            if (c == null || c == excluir || !c.gameObject.activeInHierarchy) continue;
            if (EstaEnMovimientoActivo(c)) return c;
        }
        return null;
    }

    private RobotAMR ObtenerAMRPorIndice(int indiceUnidad)
    {
        if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();
        switch (indiceUnidad)
        {
            case 2:
                return (managerEVE != null && managerEVE.robotAMR_02 != null)
                    ? managerEVE.robotAMR_02
                    : GameObject.Find("Robot_AMR_02")?.GetComponent<RobotAMR>();
            case 3:
                return (managerEVE != null && managerEVE.robotAMR_03 != null)
                    ? managerEVE.robotAMR_03
                    : GameObject.Find("Robot_AMR_03")?.GetComponent<RobotAMR>();
            default:
                return (managerEVE != null && managerEVE.robotAMR_01 != null)
                    ? managerEVE.robotAMR_01
                    : GameObject.Find("Robot_AMR")?.GetComponent<RobotAMR>();
        }
    }

    private ModoVistaCamara ObtenerModoPorAMR(RobotAMR amr)
    {
        if (amr == null) return ModoVistaCamara.AutoDirectorWMS;
        if (amr == ObtenerAMRPorIndice(2) || (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("02")))
            return ModoVistaCamara.AMR02;
        if (amr == ObtenerAMRPorIndice(3) || (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("03")))
            return ModoVistaCamara.AMR03;
        return ModoVistaCamara.AMR01;
    }

    private bool HayIncidenteActivoEnFlota()
    {
        RobotAMR[] flota = ObtenerCandidatosFlota();
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR r = flota[i];
            if (r != null && (r.estadoActual == RobotAMR.EstadoRobot.Apagado || r.estadoActual == RobotAMR.EstadoRobot.Interrumpido))
            {
                return true;
            }
        }
        return false;
    }

    private RobotAMR[] ObtenerCandidatosFlota()
    {
        return new RobotAMR[]
        {
            managerEVE != null ? managerEVE.robotAMR_01 : null,
            managerEVE != null ? managerEVE.robotAMR_02 : null,
            managerEVE != null ? managerEVE.robotAMR_03 : null,
            robotAMR
        };
    }

    private RobotAMR BuscarAMRPorEstadoPrioritario(RobotAMR.EstadoRobot estadoBuscado)
    {
        RobotAMR[] flota = ObtenerCandidatosFlota();
        for (int i = 0; i < flota.Length; i++)
        {
            if (flota[i] != null && flota[i].estadoActual == estadoBuscado)
            {
                return flota[i];
            }
        }
        return null;
    }

    public void EnfocarIncidenteCritico(Transform targetIncidente)
    {
        if (targetIncidente == null) return;

        focoIncidenteActivo = true;
        DesactivarPrioridades();

        if (vcam4_Emergencia != null)
        {
            vcam4_Emergencia.Target.TrackingTarget = targetIncidente;
            vcam4_Emergencia.Target.LookAtTarget = targetIncidente;
            vcam4_Emergencia.Priority = 30;
        }

        RobotAMR amr = targetIncidente.GetComponent<RobotAMR>();
        string nombre = amr != null && !string.IsNullOrEmpty(amr.idRobot) ? amr.idRobot : targetIncidente.name;
        Debug.Log($"<color=red>Director:</color> CÁMARA: TELEMETRÍA DE EMERGENCIA / INCIDENTE CRÍTICO ({nombre}).");
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[CÁMARA]:</color> TELEMETRÍA DE EMERGENCIA / INCIDENTE CRÍTICO ({nombre}).");
    }

    public void LiberarFocoIncidente(RobotAMR amrRelevo = null)
    {
        focoIncidenteActivo = false;

        if (modoCamaraManual)
        {
            if (vistaActual == ModoVistaCamara.AMR01 || vistaActual == ModoVistaCamara.AMR02 || vistaActual == ModoVistaCamara.AMR03)
            {
                int idx = vistaActual == ModoVistaCamara.AMR02 ? 2 : (vistaActual == ModoVistaCamara.AMR03 ? 3 : 1);
                RobotAMR objetivoManual = ObtenerAMRPorIndice(idx);
                if ((objetivoManual == null || objetivoManual.estadoActual == RobotAMR.EstadoRobot.Apagado) &&
                    amrRelevo != null && amrRelevo.estadoActual != RobotAMR.EstadoRobot.Apagado)
                {
                    vistaActual = ObtenerModoPorAMR(amrRelevo);
                }
            }
            AplicarVistaActualSinCambiarModo();
            return;
        }

        DesactivarPrioridades();

        if (amrRelevo != null)
        {
            robotAMR = amrRelevo;
            amrEnfocadoVCam3 = amrRelevo;
            if (vcam3_AMR_Hombro != null)
            {
                vcam3_AMR_Hombro.Target.TrackingTarget = amrRelevo.transform;
                vcam3_AMR_Hombro.Priority = 20;
            }
            Debug.Log($"<color=orange>Director:</color> Foco de incidente liberado. CÁMARA: SEGUIMIENTO EN RUTA ({amrRelevo.idRobot}).");
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[CÁMARA]:</color> SEGUIMIENTO EN RUTA ({amrRelevo.idRobot}).");
        }
        else if (vcam2_EVE != null)
        {
            amrEnfocadoVCam3 = null;
            vcam2_EVE.Priority = 10;
            Debug.Log("<color=orange>Director:</color> Foco de incidente liberado. Retorno a VCam2 (EVE).");
        }
    }

    private void EnfocarEVE()
    {
        // Guarda de protección: si hay incidente crítico activo o cámara manual, respeta la vista actual
        if (focoIncidenteActivo || HayIncidenteActivoEnFlota() || modoCamaraManual)
        {
            return;
        }

        // Si ya hay un AMR en tránsito/transporte enfocado en VCam3, no robar el foco al despachar otra unidad
        if (amrEnfocadoVCam3 != null && amrEnfocadoVCam3.gameObject.activeInHierarchy && EstaEnMovimientoActivo(amrEnfocadoVCam3))
        {
            return;
        }

        DesactivarPrioridades();
        amrEnfocadoVCam3 = null;
        if (vcam2_EVE != null) vcam2_EVE.Priority = 10;
        Debug.Log("<color=orange>Director:</color> Transición a VCam2 (EVE).");
    }

    private void EvaluarEstadoAMR(RobotAMR amrEmisor, RobotAMR.EstadoRobot estado)
    {
        if (estado == RobotAMR.EstadoRobot.Apagado || estado == RobotAMR.EstadoRobot.Interrumpido)
        {
            RobotAMR amrIncidente = BuscarAMRPorEstadoPrioritario(RobotAMR.EstadoRobot.Apagado)
                ?? BuscarAMRPorEstadoPrioritario(RobotAMR.EstadoRobot.Interrumpido);
            if (amrIncidente != null)
            {
                EnfocarIncidenteCritico(amrIncidente.transform);
                return;
            }
        }

        if (focoIncidenteActivo) return;
        if (modoCamaraManual) return;

        switch (estado)
        {
            case RobotAMR.EstadoRobot.En_Transito:
            case RobotAMR.EstadoRobot.Transportando:
            case RobotAMR.EstadoRobot.Extrayendo:
            case RobotAMR.EstadoRobot.Entregando:
            {
                bool hayOtroEnfocadoActivo = amrEnfocadoVCam3 != null &&
                                             amrEnfocadoVCam3 != amrEmisor &&
                                             amrEnfocadoVCam3.gameObject.activeInHierarchy &&
                                             EstaEnMovimientoActivo(amrEnfocadoVCam3);
                bool esEjecutorActual = managerEVE != null && managerEVE.robotEjecutor == amrEmisor;

                if (hayOtroEnfocadoActivo && !esEjecutorActual)
                {
                    return;
                }

                RobotAMR objetivoActivo = amrEmisor != null
                    ? amrEmisor
                    : ((managerEVE != null && managerEVE.robotEjecutor != null && managerEVE.robotEjecutor.estadoActual != RobotAMR.EstadoRobot.Apagado)
                        ? managerEVE.robotEjecutor
                        : robotAMR);

                if (vcam3_AMR_Hombro != null && objetivoActivo != null)
                {
                    DesactivarPrioridades();
                    amrEnfocadoVCam3 = objetivoActivo;
                    vcam3_AMR_Hombro.Target.TrackingTarget = objetivoActivo.transform;
                    vcam3_AMR_Hombro.Priority = 10;
                }
                Debug.Log("<color=orange>Director:</color> Transición a VCam3 (AMR_Hombro).");
                break;
            }
            case RobotAMR.EstadoRobot.Recargando:
            {
                if (amrEmisor != null && amrEnfocadoVCam3 != null && amrEmisor != amrEnfocadoVCam3 &&
                    amrEnfocadoVCam3.gameObject.activeInHierarchy && EstaEnMovimientoActivo(amrEnfocadoVCam3))
                {
                    return;
                }

                RobotAMR otroActivo = BuscarOtroAMRActivo(amrEmisor);
                if (otroActivo != null && vcam3_AMR_Hombro != null)
                {
                    DesactivarPrioridades();
                    amrEnfocadoVCam3 = otroActivo;
                    vcam3_AMR_Hombro.Target.TrackingTarget = otroActivo.transform;
                    vcam3_AMR_Hombro.Priority = 10;
                    return;
                }

                amrEnfocadoVCam3 = null;
                DesactivarPrioridades();
                if (managerEVE != null && managerEVE.turnoIniciado && vcam2_EVE != null)
                {
                    vcam2_EVE.Priority = 10;
                    Debug.Log("<color=orange>Director:</color> AMR recargando. Transición a VCam2 (EVE).");
                }
                else if (vcam1_Menu != null)
                {
                    vcam1_Menu.Priority = 10;
                    Debug.Log("<color=orange>Director:</color> AMR recargando (Fin de turno). Transición a VCam1 (Menu).");
                }
                break;
            }
            case RobotAMR.EstadoRobot.Inactivo:
            {
                if (amrEmisor != null && amrEnfocadoVCam3 != null && amrEmisor != amrEnfocadoVCam3 &&
                    amrEnfocadoVCam3.gameObject.activeInHierarchy && EstaEnMovimientoActivo(amrEnfocadoVCam3))
                {
                    return;
                }

                RobotAMR otroActivo = BuscarOtroAMRActivo(amrEmisor);
                if (otroActivo != null && vcam3_AMR_Hombro != null)
                {
                    DesactivarPrioridades();
                    amrEnfocadoVCam3 = otroActivo;
                    vcam3_AMR_Hombro.Target.TrackingTarget = otroActivo.transform;
                    vcam3_AMR_Hombro.Priority = 10;
                    return;
                }

                amrEnfocadoVCam3 = null;
                DesactivarPrioridades();
                if (managerEVE != null && managerEVE.turnoIniciado && vcam2_EVE != null)
                {
                    vcam2_EVE.Priority = 10;
                    Debug.Log("<color=orange>Director:</color> Transición a VCam2 (EVE - Turno en curso).");
                }
                else if (vcam1_Menu != null)
                {
                    vcam1_Menu.Priority = 10;
                    Debug.Log("<color=orange>Director:</color> Transición a VCam1 (Menu).");
                }
                break;
            }
        }
    }

    public void ActivarVistaGeneralFinDeTurno()
    {
        focoIncidenteActivo = false;
        modoCamaraManual = false;
        vistaActual = ModoVistaCamara.AutoDirectorWMS;
        amrEnfocadoVCam3 = null;
        DesactivarPrioridades();
        if (vcam1_Menu != null) vcam1_Menu.Priority = 10;
        Debug.Log("<color=orange>Director:</color> Fin de turno. Transición a VCam1 (Vista General / Menu).");
    }

    private int indiceCamaraSCADA = 0;

    public RobotAMR ObtenerAMREnfocadoActual()
    {
        if (vcam3_AMR_Hombro == null || vcam3_AMR_Hombro.Target.TrackingTarget == null)
        {
            return null;
        }

        int pCam3 = vcam3_AMR_Hombro.Priority.Value;
        int pCam1 = vcam1_Menu != null ? vcam1_Menu.Priority.Value : int.MinValue;
        int pCam2 = vcam2_EVE != null ? vcam2_EVE.Priority.Value : int.MinValue;
        int pCam0 = vcam0_Intro != null ? vcam0_Intro.Priority.Value : int.MinValue;

        if (pCam3 >= 10 && pCam3 >= pCam1 && pCam3 >= pCam2 && pCam3 >= pCam0)
        {
            return vcam3_AMR_Hombro.Target.TrackingTarget.GetComponent<RobotAMR>();
        }
        return null;
    }

    public string ObtenerEtiquetaVistaActiva()
    {
        switch (vistaActual)
        {
            case ModoVistaCamara.GeneralPlanta: return "General [Planta]";
            case ModoVistaCamara.EveOrquestador: return "EVE [Orquestador]";
            case ModoVistaCamara.AMR01: return "AMR-01 [Principal]";
            case ModoVistaCamara.AMR02: return "AMR-02 [Soporte]";
            case ModoVistaCamara.AMR03: return "AMR-03 [Reserva 24/7]";
            case ModoVistaCamara.DronSoporteISO: return "Dron Soporte [ISO]";
            case ModoVistaCamara.AutoDirectorWMS:
            default:
                return "AUTO [Director WMS]";
        }
    }

    public void SeleccionarModoAuto()
    {
        modoCamaraManual = false;
        vistaActual = ModoVistaCamara.AutoDirectorWMS;
        focoIncidenteActivo = false;

        if (HayIncidenteActivoEnFlota())
        {
            RobotAMR amrIncidente = BuscarAMRPorEstadoPrioritario(RobotAMR.EstadoRobot.Apagado)
                ?? BuscarAMRPorEstadoPrioritario(RobotAMR.EstadoRobot.Interrumpido);
            if (amrIncidente != null)
            {
                EnfocarIncidenteCritico(amrIncidente.transform);
                return;
            }
        }

        DesactivarPrioridades();
        RobotAMR candidato = (managerEVE != null && EstaEnMovimientoActivo(managerEVE.robotEjecutor))
            ? managerEVE.robotEjecutor
            : BuscarOtroAMRActivo(null);

        if (candidato != null && vcam3_AMR_Hombro != null)
        {
            amrEnfocadoVCam3 = candidato;
            robotAMR = candidato;
            vcam3_AMR_Hombro.Target.TrackingTarget = candidato.transform;
            vcam3_AMR_Hombro.Priority = 10;
        }
        else if (managerEVE != null && managerEVE.turnoIniciado && vcam2_EVE != null)
        {
            amrEnfocadoVCam3 = null;
            vcam2_EVE.Priority = 10;
        }
        else if (vcam1_Menu != null)
        {
            amrEnfocadoVCam3 = null;
            vcam1_Menu.Priority = 10;
        }

        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[CÁMARA]:</color> Modo AUTO [Director WMS] activado.");
    }

    public void SeleccionarVistaGeneral()
    {
        modoCamaraManual = true;
        vistaActual = ModoVistaCamara.GeneralPlanta;
        focoIncidenteActivo = false;
        amrEnfocadoVCam3 = null;
        DesactivarPrioridades();
        if (vcam1_Menu != null) vcam1_Menu.Priority = 20;
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[CÁMARA]:</color> Vista manual General [Planta].");
    }

    public void SeleccionarVistaEVE()
    {
        modoCamaraManual = true;
        vistaActual = ModoVistaCamara.EveOrquestador;
        focoIncidenteActivo = false;
        amrEnfocadoVCam3 = null;
        DesactivarPrioridades();
        if (vcam2_EVE != null) vcam2_EVE.Priority = 20;
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[CÁMARA]:</color> Vista manual EVE [Orquestador].");
    }

    public void SeleccionarVistaAMR(int indiceUnidad)
    {
        RobotAMR objetivo = ObtenerAMRPorIndice(indiceUnidad);
        if (objetivo == null || vcam3_AMR_Hombro == null) return;

        modoCamaraManual = true;
        vistaActual = indiceUnidad == 2
            ? ModoVistaCamara.AMR02
            : (indiceUnidad == 3 ? ModoVistaCamara.AMR03 : ModoVistaCamara.AMR01);
        focoIncidenteActivo = false;
        amrEnfocadoVCam3 = objetivo;
        robotAMR = objetivo;

        DesactivarPrioridades();
        vcam3_AMR_Hombro.Target.TrackingTarget = objetivo.transform;
        vcam3_AMR_Hombro.Priority = 20;
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[CÁMARA]:</color> Seguimiento manual {ObtenerEtiquetaVistaActiva()}.");
    }

    public void SeleccionarVistaDronSoporte()
    {
        if (dronSoporte == null) dronSoporte = FindFirstObjectByType<DronMantenimiento>();
        if (dronSoporte == null || !dronSoporte.ocupado || vcam3_AMR_Hombro == null) return;

        if (vistaActual != ModoVistaCamara.DronSoporteISO)
        {
            vistaPreviaDron = vistaActual;
        }

        modoCamaraManual = true;
        vistaActual = ModoVistaCamara.DronSoporteISO;
        focoIncidenteActivo = false;
        amrEnfocadoVCam3 = null;

        DesactivarPrioridades();
        vcam3_AMR_Hombro.Target.TrackingTarget = dronSoporte.transform;
        vcam3_AMR_Hombro.Priority = 20;
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[CÁMARA]:</color> Seguimiento aéreo Dron Soporte [ISO].");
    }

    private void RestaurarVistaPreviaDron()
    {
        ModoVistaCamara destino = vistaPreviaDron;
        if (destino == ModoVistaCamara.DronSoporteISO || destino == ModoVistaCamara.AutoDirectorWMS)
        {
            SeleccionarModoAuto();
            return;
        }
        vistaActual = destino;
        AplicarVistaActualSinCambiarModo();
    }

    private void AplicarVistaActualSinCambiarModo()
    {
        switch (vistaActual)
        {
            case ModoVistaCamara.GeneralPlanta:
                SeleccionarVistaGeneral();
                break;
            case ModoVistaCamara.EveOrquestador:
                SeleccionarVistaEVE();
                break;
            case ModoVistaCamara.AMR01:
                SeleccionarVistaAMR(1);
                break;
            case ModoVistaCamara.AMR02:
                SeleccionarVistaAMR(2);
                break;
            case ModoVistaCamara.AMR03:
                SeleccionarVistaAMR(3);
                break;
            case ModoVistaCamara.DronSoporteISO:
                if (dronSoporte != null && dronSoporte.ocupado && vcam3_AMR_Hombro != null)
                {
                    DesactivarPrioridades();
                    vcam3_AMR_Hombro.Target.TrackingTarget = dronSoporte.transform;
                    vcam3_AMR_Hombro.Priority = 20;
                }
                else
                {
                    RestaurarVistaPreviaDron();
                }
                break;
            case ModoVistaCamara.AutoDirectorWMS:
            default:
                SeleccionarModoAuto();
                break;
        }
    }

    public void CiclarCamaraSCADA()
    {
        AlternarCamaraManual();
    }

    public void AlternarCamaraManual()
    {
        indiceCamaraSCADA = (indiceCamaraSCADA + 1) % 6;
        switch (indiceCamaraSCADA)
        {
            case 0: SeleccionarModoAuto(); break;
            case 1: SeleccionarVistaGeneral(); break;
            case 2: SeleccionarVistaAMR(1); break;
            case 3: SeleccionarVistaAMR(2); break;
            case 4: SeleccionarVistaAMR(3); break;
            case 5: SeleccionarVistaEVE(); break;
        }
    }

    private void LateUpdate()
    {
        if (vistaActual == ModoVistaCamara.DronSoporteISO && !focoIncidenteActivo)
        {
            if (dronSoporte == null) dronSoporte = FindFirstObjectByType<DronMantenimiento>();
            if (dronSoporte == null || !dronSoporte.ocupado)
            {
                RestaurarVistaPreviaDron();
            }
        }

        ActualizarEncuadreDinamico();
    }

    private void ActualizarEncuadreDinamico()
    {
        float dt = Time.unscaledDeltaTime * 3.5f;

        if (vcam3_AMR_Hombro != null)
        {
            if (followVCam3 == null) followVCam3 = vcam3_AMR_Hombro.GetComponent<CinemachineFollow>();
            if (composerVCam3 == null) composerVCam3 = vcam3_AMR_Hombro.GetComponent<CinemachineRotationComposer>();

            Transform target3 = vcam3_AMR_Hombro.Target.TrackingTarget;
            RobotAMR amr3 = target3 != null ? target3.GetComponent<RobotAMR>() : null;
            bool conCarga3 = amr3 != null && (amr3.TieneCargaFisica ||
                amr3.estadoActual == RobotAMR.EstadoRobot.Transportando ||
                amr3.estadoActual == RobotAMR.EstadoRobot.Extrayendo ||
                amr3.estadoActual == RobotAMR.EstadoRobot.Entregando);

            Vector3 followObj3 = conCarga3 ? new Vector3(1.85f, 2.75f, -4.15f) : new Vector3(1.50f, 2.00f, -3.00f);
            Vector3 targetObj3 = conCarga3 ? new Vector3(0.00f, 1.45f, 0.00f) : new Vector3(0.00f, 1.00f, 0.00f);

            if (followVCam3 != null)
            {
                followVCam3.FollowOffset = Vector3.Lerp(followVCam3.FollowOffset, followObj3, dt);
            }
            if (composerVCam3 != null)
            {
                composerVCam3.TargetOffset = Vector3.Lerp(composerVCam3.TargetOffset, targetObj3, dt);
            }
        }

        if (vcam4_Emergencia != null)
        {
            if (followVCam4 == null) followVCam4 = vcam4_Emergencia.GetComponent<CinemachineFollow>();
            if (composerVCam4 == null) composerVCam4 = vcam4_Emergencia.GetComponent<CinemachineRotationComposer>();

            Transform target4 = vcam4_Emergencia.Target.TrackingTarget;
            RobotAMR amr4 = target4 != null ? target4.GetComponent<RobotAMR>() : null;
            bool encuadreAmplio4 = amr4 != null && (amr4.TieneCargaFisica || amr4.EnParadaSeguridadLiDAR);

            Vector3 followObj4 = encuadreAmplio4 ? new Vector3(1.95f, 2.25f, -3.90f) : new Vector3(1.10f, 1.35f, -2.40f);
            Vector3 targetObj4 = encuadreAmplio4 ? new Vector3(0.00f, 1.35f, 0.00f) : new Vector3(0.00f, 0.85f, 0.00f);

            if (followVCam4 != null)
            {
                followVCam4.FollowOffset = Vector3.Lerp(followVCam4.FollowOffset, followObj4, dt);
            }
            if (composerVCam4 != null)
            {
                composerVCam4.TargetOffset = Vector3.Lerp(composerVCam4.TargetOffset, targetObj4, dt);
            }
        }
    }

    private void DesactivarPrioridades()
    {
        if (vcam0_Intro != null) vcam0_Intro.Priority = 0;
        if (vcam1_Menu != null) vcam1_Menu.Priority = 0;
        if (vcam2_EVE != null) vcam2_EVE.Priority = 0;
        if (vcam3_AMR_Hombro != null) vcam3_AMR_Hombro.Priority = 0;
        if (vcam4_Emergencia != null) vcam4_Emergencia.Priority = 0;
    }
}
