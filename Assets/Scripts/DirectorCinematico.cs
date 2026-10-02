using UnityEngine;
using Unity.Cinemachine;

public class DirectorCinematico : MonoBehaviour
{
    [Header("Emisores")]
    public RobotManagerEVE managerEVE;
    public RobotAMR robotAMR;

    [Header("Cámaras Virtuales")]
    public CinemachineCamera vcam1_Menu;
    public CinemachineCamera vcam2_EVE;
    public CinemachineCamera vcam3_AMR_Hombro;
    public CinemachineCamera vcam4_Emergencia;

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
            case RobotAMR.EstadoRobot.Inactivo:
                if (vcam1_Menu != null) vcam1_Menu.Priority = 10;
                Debug.Log("<color=orange>Director:</color> Transición a VCam1 (Menu).");
                break;
        }
    }

    private void DesactivarPrioridades()
    {
        if (vcam1_Menu != null) vcam1_Menu.Priority = 0;
        if (vcam2_EVE != null) vcam2_EVE.Priority = 0;
        if (vcam3_AMR_Hombro != null) vcam3_AMR_Hombro.Priority = 0;
        if (vcam4_Emergencia != null) vcam4_Emergencia.Priority = 0;
    }
}
