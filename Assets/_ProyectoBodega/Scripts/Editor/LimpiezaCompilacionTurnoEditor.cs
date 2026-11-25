#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Hook de seguridad en el Editor para limpiar estado estático y residuos operativos
/// tras recompilación o transiciones de Play Mode.
/// </summary>
[InitializeOnLoad]
public static class LimpiezaCompilacionTurnoEditor
{
    static LimpiezaCompilacionTurnoEditor()
    {
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

        AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnBeforeAssemblyReload()
    {
        LimpiarEstadoGlobalYRuntime();
    }

    private static void OnAfterAssemblyReload()
    {
        LimpiarEstadoGlobalYRuntime();
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            LimpiarEstadoGlobalYRuntime();
        }
    }

    private static void LimpiarEstadoGlobalYRuntime()
    {
        Time.timeScale = 1f;
        RelojSimuladorTurno.ReiniciarRelojTurno(0.0f);
        RobotAMR.LimpiarEstaticosTrafico();

        if (EditorApplication.isPlaying && RobotManagerEVE.Instance != null)
        {
            RobotManagerEVE.Instance.ReiniciarEstadoPorCompilacion();
        }
    }
}
#endif
