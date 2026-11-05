using System.Collections;
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

    public void RelevarRobotActivo(RobotAMR nuevoRobot)
    {
        if (robotAMR != null) robotAMR.OnEstadoCambiado -= EvaluarEstadoAMR;
        robotAMR = nuevoRobot;
        if (robotAMR != null)
        {
            robotAMR.OnEstadoCambiado += EvaluarEstadoAMR;
            if (vcam3_AMR_Hombro != null) vcam3_AMR_Hombro.Target.TrackingTarget = robotAMR.transform;
            if (vcam4_Emergencia != null) vcam4_Emergencia.Target.TrackingTarget = robotAMR.transform;
            EvaluarEstadoAMR(robotAMR.estadoActual);
        }
    }

    private void OnEnable()
    {
        if (managerEVE != null)
            managerEVE.OnMisionEvaluada += EnfocarEVE;
        
        if (robotAMR != null)
            robotAMR.OnEstadoCambiado += EvaluarEstadoAMR;
    }

    private void OnDisable()
    {
        if (managerEVE != null)
            managerEVE.OnMisionEvaluada -= EnfocarEVE;
        
        if (robotAMR != null)
            robotAMR.OnEstadoCambiado -= EvaluarEstadoAMR;
    }

    private void EnfocarEVE()
    {
        // Guarda de protección: Si el AMR está en parada crítica/evaluando modal, la cámara de emergencia retiene el foco
        if (robotAMR != null && robotAMR.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
        {
            Debug.Log("<color=orange>Director:</color> Enfoque a EVE abortado: AMR en estado de emergencia/interrupción.");
            return;
        }

        DesactivarPrioridades();
        if (vcam2_EVE != null) vcam2_EVE.Priority = 10;
        Debug.Log("<color=orange>Director:</color> Transición a VCam2 (EVE).");
    }

    private void EvaluarEstadoAMR(RobotAMR.EstadoRobot estado)
    {
        DesactivarPrioridades();

        switch (estado)
        {
            case RobotAMR.EstadoRobot.En_Transito:
            case RobotAMR.EstadoRobot.Transportando:
            case RobotAMR.EstadoRobot.Extrayendo:
            case RobotAMR.EstadoRobot.Entregando:
                if (vcam3_AMR_Hombro != null) vcam3_AMR_Hombro.Priority = 10;
                Debug.Log("<color=orange>Director:</color> Transición a VCam3 (AMR_Hombro).");
                break;
            case RobotAMR.EstadoRobot.Interrumpido:
                if (vcam4_Emergencia != null) vcam4_Emergencia.Priority = 10;
                Debug.Log("<color=orange>Director:</color> Transición a VCam4 (Emergencia).");
                break;
            case RobotAMR.EstadoRobot.Recargando:
                if (managerEVE != null && managerEVE.turnoIniciado && vcam2_EVE != null)
                {
                    vcam2_EVE.Priority = 10;
                    Debug.Log("<color=orange>Director:</color> AMR recargando. Transición a VCam2 (EVE).");
                }
                break;
            case RobotAMR.EstadoRobot.Inactivo:
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

    private void DesactivarPrioridades()
    {
        if (vcam0_Intro != null) vcam0_Intro.Priority = 0;
        if (vcam1_Menu != null) vcam1_Menu.Priority = 0;
        if (vcam2_EVE != null) vcam2_EVE.Priority = 0;
        if (vcam3_AMR_Hombro != null) vcam3_AMR_Hombro.Priority = 0;
        if (vcam4_Emergencia != null) vcam4_Emergencia.Priority = 0;
    }
}
