This file is a merged representation of a subset of the codebase, containing specifically included files and files not matching ignore patterns, combined into a single document by Repomix.

# File Summary

## Purpose
This file contains a packed representation of a subset of the repository's contents that is considered the most important context.
It is designed to be easily consumable by AI systems for analysis, code review,
or other automated processes.

## File Format
The content is organized as follows:
1. This summary section
2. Repository information
3. Directory structure
4. Repository files (if enabled)
5. Multiple file entries, each consisting of:
  a. A header with the file path (## File: path/to/file)
  b. The full contents of the file in a code block

## Usage Guidelines
- This file should be treated as read-only. Any changes should be made to the
  original repository files, not this packed version.
- When processing this file, use the file path to distinguish
  between different files in the repository.
- Be aware that this file may contain sensitive information. Handle it with
  the same level of security as you would the original repository.

## Notes
- Some files may have been excluded based on .gitignore rules and Repomix's configuration
- Binary files are not included in this packed representation. Please refer to the Repository Structure section for a complete list of file paths, including binary files
- Only files matching these patterns are included: Assets/**/*.cs, Assets/**/*.json, Packages/manifest.json, ProjectSettings/ProjectVersion.txt, *.md
- Files matching these patterns are excluded: Assets/TextMesh Pro/**, Assets/Plugins/**
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Files are sorted by Git change count (files with more changes are at the bottom)

# Directory Structure
````
Assets/
  _ProyectoBodega/
    Scripts/
      Editor/
        OrphanAssetFinder.cs
        RestructureProject.cs
      DashboardUI.cs
      DirectorCinematico.cs
      DronePropellers.cs
      DronMantenimiento.cs
      GeneradorTablaManifiesto.cs
      ModelosLogistica.cs
      ModuloDespachoTMS.cs
      RobotAMR.cs
      RobotManagerEVE.cs
      TelemetriaAMR.cs
  Plugins_Terceros/
    Realistic Drone/
      drone/
        Level0_Stabilizing/
          optimizations/
            geneticOptimization/
              GAmanager.cs
              geneticBehaviour.cs
              GeneticSettings.cs
            twidOptimization/
              twiddleBehaviour.cs
              twiddleManager.cs
              twiddleSettings.cs
            OptimizationBehaviour.cs
            OptimizationManager.cs
          sensors/
            Accelerometer.cs
            Barometer.cs
            GPS.cs
            Gyro.cs
            Magnetometer.cs
            noiseAdder.cs
            noiseAddVector3.cs
          droneMovementController.cs
          droneSettings.cs
          PID.cs
          rotor.cs
        Level1_Trajectories/
          singlePoint.cs
          Waypoint.cs
          WaypointCircuit.cs
          WaypointProgressTracker.cs
        Level2_IA/
          maze/
            drawing/
              dataDrawer.cs
            dataCollector.cs
            mazeExplorer.cs
          IA.cs
        Tools/
          configReader.cs
          dataReader.cs
          dataSaver.cs
          lineDrawer.cs
      cameraBehaviour.cs
      lookAt.cs
      screen.cs
    Single_detailed_truck/
      Scripts/
        CameraFreeflight.cs
  StreamingAssets/
    lote_260_pallets.json
    lote_30_pallets.json
    lote_450_pallets.json
    manifiesto.json
  TutorialInfo/
    Scripts/
      Editor/
        ReadmeEditor.cs
      Readme.cs
Packages/
  manifest.json
ProjectSettings/
  ProjectVersion.txt
AGENTS.md
README.md
````

# Files

## File: Assets/_ProyectoBodega/Scripts/Editor/OrphanAssetFinder.cs
````csharp
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class OrphanAssetFinder : EditorWindow
{
    private const string SafeTrashFolder = "Assets/Papelera_Segura";

    private DefaultAsset targetFolderAsset;
    private string targetFolderPath = "Assets/Import";
    private bool onlyModelsAndTextures = false;
    private Vector2 scrollPosition;

    [Serializable]
    private class OrphanItem
    {
        public string assetPath;
        public string assetType;
        public UnityEngine.Object assetRef;
        public bool selectedForMove = true;
    }

    private readonly List<OrphanItem> orphanAssets = new List<OrphanItem>();
    private bool hasScanned = false;
    private int scannedPrefabsCount = 0;
    private int scannedScenesCount = 0;
    private int scannedMaterialsCount = 0;
    private int totalFolderAssetsCount = 0;
    private string statusMessage = "Seleccione una carpeta del proyecto y presione 'Escanear Assets Huérfanos'.";

    [MenuItem("Tools/Auditoría de Assets")]
    public static void ShowWindow()
    {
        OrphanAssetFinder window = GetWindow<OrphanAssetFinder>("Auditoría de Assets");
        window.minSize = new Vector2(620f, 480f);
        window.SyncFolderAssetFromPath();
        window.Show();
    }

    private void OnEnable()
    {
        SyncFolderAssetFromPath();
    }

    private void SyncFolderAssetFromPath()
    {
        if (!string.IsNullOrEmpty(targetFolderPath) && AssetDatabase.IsValidFolder(targetFolderPath))
        {
            targetFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(targetFolderPath);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Auditoría de Assets Huérfanos (Limpieza Segura)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Escanea todos los Prefabs, Escenas y Materiales del proyecto usando AssetDatabase y EditorUtility.CollectDependencies. " +
            "Los archivos no referenciados pueden moverse de forma segura a '" + SafeTrashFolder + "' usando AssetDatabase.MoveAsset (preservando GUIDs y archivos .meta).",
            MessageType.Info);

        EditorGUILayout.Space(6f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("1. Carpeta Objetivo a Auditar", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        DefaultAsset newFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
            "Carpeta (Drag & Drop)",
            targetFolderAsset,
            typeof(DefaultAsset),
            false);
        if (EditorGUI.EndChangeCheck() && newFolderAsset != null)
        {
            string chosenPath = AssetDatabase.GetAssetPath(newFolderAsset);
            if (AssetDatabase.IsValidFolder(chosenPath))
            {
                targetFolderAsset = newFolderAsset;
                targetFolderPath = chosenPath;
            }
            else
            {
                EditorUtility.DisplayDialog("Selección inválida", "Por favor seleccione una carpeta válida dentro de Assets.", "OK");
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        string manualPath = EditorGUILayout.TextField("Ruta en Proyecto", targetFolderPath);
        if (EditorGUI.EndChangeCheck())
        {
            targetFolderPath = manualPath.TrimEnd('/', '\\');
            SyncFolderAssetFromPath();
        }

        if (GUILayout.Button("Elegir Carpeta...", GUILayout.Width(130f)))
        {
            string absSelected = EditorUtility.OpenFolderPanel("Seleccionar carpeta dentro de Assets", Application.dataPath, "");
            if (!string.IsNullOrEmpty(absSelected))
            {
                string dataPathNormalized = Application.dataPath.Replace('\\', '/');
                string selectedNormalized = absSelected.Replace('\\', '/');
                if (selectedNormalized.StartsWith(dataPathNormalized, StringComparison.OrdinalIgnoreCase))
                {
                    string relPath = "Assets" + selectedNormalized.Substring(dataPathNormalized.Length);
                    targetFolderPath = relPath.TrimEnd('/');
                    SyncFolderAssetFromPath();
                }
                else
                {
                    EditorUtility.DisplayDialog("Ruta fuera del proyecto", "Debe seleccionar una carpeta dentro de la carpeta Assets del proyecto.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        onlyModelsAndTextures = EditorGUILayout.ToggleLeft(
            "Filtrar únicamente Modelos 3D (.fbx, .obj, .blend) y Texturas",
            onlyModelsAndTextures);

        EditorGUILayout.Space(4f);
        GUI.backgroundColor = new Color(0.25f, 0.65f, 0.95f);
        if (GUILayout.Button("Escanear Assets Huérfanos", GUILayout.Height(30f)))
        {
            ScanForOrphans();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Estado del Escaneo:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(statusMessage, EditorStyles.wordWrappedLabel);

        if (hasScanned)
        {
            EditorGUILayout.LabelField(
                string.Format(
                    "Raíces analizadas -> Prefabs: {0} | Escenas: {1} | Materiales: {2} | Archivos en carpeta: {3} | Huérfanos: {4}",
                    scannedPrefabsCount,
                    scannedScenesCount,
                    scannedMaterialsCount,
                    totalFolderAssetsCount,
                    orphanAssets.Count),
                EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6f);

        EditorGUILayout.LabelField(
            string.Format("2. Assets Huérfanos Detectados ({0})", orphanAssets.Count),
            EditorStyles.boldLabel);

        if (orphanAssets.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Seleccionar Todos", GUILayout.Width(130f)))
            {
                for (int i = 0; i < orphanAssets.Count; i++) orphanAssets[i].selectedForMove = true;
            }
            if (GUILayout.Button("Deseleccionar Todos", GUILayout.Width(140f)))
            {
                for (int i = 0; i < orphanAssets.Count; i++) orphanAssets[i].selectedForMove = false;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox);
        if (!hasScanned)
        {
            EditorGUILayout.LabelField("Aún no se ha ejecutado el escaneo.", EditorStyles.centeredGreyMiniLabel);
        }
        else if (orphanAssets.Count == 0)
        {
            EditorGUILayout.LabelField("No se encontraron assets huérfanos en la carpeta seleccionada.", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            for (int i = 0; i < orphanAssets.Count; i++)
            {
                OrphanItem item = orphanAssets[i];
                EditorGUILayout.BeginHorizontal();
                item.selectedForMove = EditorGUILayout.Toggle(item.selectedForMove, GUILayout.Width(20f));
                EditorGUILayout.ObjectField(item.assetRef, typeof(UnityEngine.Object), false, GUILayout.Width(180f));
                EditorGUILayout.LabelField("[" + item.assetType + "]", GUILayout.Width(95f));
                EditorGUILayout.SelectableLabel(item.assetPath, GUILayout.Height(18f));
                if (GUILayout.Button("Ping", GUILayout.Width(50f)))
                {
                    if (item.assetRef != null)
                    {
                        Selection.activeObject = item.assetRef;
                        EditorGUIUtility.PingObject(item.assetRef);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6f);

        EditorGUI.BeginDisabledGroup(orphanAssets.Count == 0);
        GUI.backgroundColor = new Color(0.95f, 0.55f, 0.2f);
        if (GUILayout.Button("Mover Huérfanos a " + SafeTrashFolder + " (MoveAsset Seguro)", GUILayout.Height(34f)))
        {
            MoveOrphansToSafeTrash();
        }
        GUI.backgroundColor = Color.white;
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(6f);
    }

    private void ScanForOrphans()
    {
        orphanAssets.Clear();
        hasScanned = false;
        scannedPrefabsCount = 0;
        scannedScenesCount = 0;
        scannedMaterialsCount = 0;
        totalFolderAssetsCount = 0;

        if (string.IsNullOrEmpty(targetFolderPath) || !AssetDatabase.IsValidFolder(targetFolderPath))
        {
            statusMessage = "Error: La ruta seleccionada no es una carpeta válida dentro de Assets.";
            EditorUtility.DisplayDialog("Carpeta Inválida", statusMessage, "OK");
            return;
        }

        if (string.Equals(targetFolderPath, SafeTrashFolder, StringComparison.OrdinalIgnoreCase) ||
            targetFolderPath.StartsWith(SafeTrashFolder + "/", StringComparison.OrdinalIgnoreCase))
        {
            statusMessage = "No se puede auditar la carpeta de Papelera Segura directamente.";
            EditorUtility.DisplayDialog("Operación no permitida", statusMessage, "OK");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Auditoría de Assets", "Recolectando Prefabs, Escenas y Materiales...", 0.1f);

            HashSet<string> referencedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");

            int totalRoots = prefabGuids.Length + sceneGuids.Length + materialGuids.Length;
            int processedRoots = 0;

            for (int i = 0; i < prefabGuids.Length; i++)
            {
                processedRoots++;
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (!IsValidRootPath(path)) continue;

                scannedPrefabsCount++;
                if (processedRoots % 10 == 0)
                {
                    EditorUtility.DisplayProgressBar(
                        "Auditoría de Assets",
                        "Escaneando Prefab: " + path,
                        (float)processedRoots / Mathf.Max(1, totalRoots));
                }

                CollectDependenciesFromAsset(path, referencedPaths);
            }

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                processedRoots++;
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                if (!IsValidRootPath(path)) continue;

                scannedScenesCount++;
                EditorUtility.DisplayProgressBar(
                    "Auditoría de Assets",
                    "Escaneando Escena: " + path,
                    (float)processedRoots / Mathf.Max(1, totalRoots));

                CollectDependenciesFromScene(path, referencedPaths);
            }

            for (int i = 0; i < materialGuids.Length; i++)
            {
                processedRoots++;
                string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
                if (!IsValidRootPath(path)) continue;

                scannedMaterialsCount++;
                if (processedRoots % 20 == 0)
                {
                    EditorUtility.DisplayProgressBar(
                        "Auditoría de Assets",
                        "Escaneando Material: " + path,
                        (float)processedRoots / Mathf.Max(1, totalRoots));
                }

                CollectDependenciesFromAsset(path, referencedPaths);
            }

            EditorUtility.DisplayProgressBar("Auditoría de Assets", "Cruzando referencias con " + targetFolderPath + "...", 0.9f);

            string[] candidateGuids = AssetDatabase.FindAssets("", new string[] { targetFolderPath });
            HashSet<string> visitedCandidatePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < candidateGuids.Length; i++)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(candidateGuids[i]);
                if (string.IsNullOrEmpty(candidatePath)) continue;
                if (!visitedCandidatePaths.Add(candidatePath)) continue;
                if (AssetDatabase.IsValidFolder(candidatePath)) continue;
                if (candidatePath.StartsWith(SafeTrashFolder + "/", StringComparison.OrdinalIgnoreCase)) continue;
                if (IsCodeOrIgnoredAsset(candidatePath)) continue;
                if (onlyModelsAndTextures && !IsModelOrTextureAsset(candidatePath)) continue;

                totalFolderAssetsCount++;

                if (!referencedPaths.Contains(candidatePath))
                {
                    UnityEngine.Object assetObj = AssetDatabase.LoadMainAssetAtPath(candidatePath);
                    string typeName = assetObj != null ? assetObj.GetType().Name : Path.GetExtension(candidatePath);
                    orphanAssets.Add(new OrphanItem
                    {
                        assetPath = candidatePath,
                        assetType = typeName,
                        assetRef = assetObj,
                        selectedForMove = true
                    });
                }
            }

            hasScanned = true;
            statusMessage = string.Format(
                "Escaneo completado en '{0}'. Se encontraron {1} assets huérfanos de {2} archivos analizados.",
                targetFolderPath,
                orphanAssets.Count,
                totalFolderAssetsCount);

            Debug.Log("[Auditoría de Assets] " + statusMessage);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static bool IsValidRootPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith(SafeTrashFolder + "/", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }

    private static void CollectDependenciesFromAsset(string rootPath, HashSet<string> referencedPaths)
    {
        UnityEngine.Object mainAsset = AssetDatabase.LoadMainAssetAtPath(rootPath);
        if (mainAsset != null)
        {
            UnityEngine.Object[] collectedObjects = EditorUtility.CollectDependencies(new UnityEngine.Object[] { mainAsset });
            for (int i = 0; i < collectedObjects.Length; i++)
            {
                if (collectedObjects[i] == null) continue;
                string depPath = AssetDatabase.GetAssetPath(collectedObjects[i]);
                if (!string.IsNullOrEmpty(depPath) && !string.Equals(depPath, rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    referencedPaths.Add(depPath);
                }
            }
        }

        string[] depPaths = AssetDatabase.GetDependencies(rootPath, true);
        for (int i = 0; i < depPaths.Length; i++)
        {
            string depPath = depPaths[i];
            if (!string.IsNullOrEmpty(depPath) && !string.Equals(depPath, rootPath, StringComparison.OrdinalIgnoreCase))
            {
                referencedPaths.Add(depPath);
            }
        }
    }

    private static void CollectDependenciesFromScene(string scenePath, HashSet<string> referencedPaths)
    {
        string[] sceneDepPaths = AssetDatabase.GetDependencies(scenePath, true);
        List<UnityEngine.Object> loadedSceneObjects = new List<UnityEngine.Object>();

        for (int i = 0; i < sceneDepPaths.Length; i++)
        {
            string depPath = sceneDepPaths[i];
            if (string.IsNullOrEmpty(depPath) || string.Equals(depPath, scenePath, StringComparison.OrdinalIgnoreCase))
                continue;

            referencedPaths.Add(depPath);

            UnityEngine.Object depObj = AssetDatabase.LoadMainAssetAtPath(depPath);
            if (depObj != null)
            {
                loadedSceneObjects.Add(depObj);
            }
        }

        if (loadedSceneObjects.Count > 0)
        {
            UnityEngine.Object[] collected = EditorUtility.CollectDependencies(loadedSceneObjects.ToArray());
            for (int i = 0; i < collected.Length; i++)
            {
                if (collected[i] == null) continue;
                string depPath = AssetDatabase.GetAssetPath(collected[i]);
                if (!string.IsNullOrEmpty(depPath) && !string.Equals(depPath, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    referencedPaths.Add(depPath);
                }
            }
        }
    }

    private static bool IsCodeOrIgnoredAsset(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".cs" || ext == ".dll" || ext == ".asmdef" || ext == ".asmref";
    }

    private static bool IsModelOrTextureAsset(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        switch (ext)
        {
            case ".fbx":
            case ".obj":
            case ".blend":
            case ".dae":
            case ".3ds":
            case ".png":
            case ".jpg":
            case ".jpeg":
            case ".tga":
            case ".psd":
            case ".tif":
            case ".tiff":
            case ".exr":
            case ".hdr":
            case ".bmp":
                return true;
            default:
                return false;
        }
    }

    private void MoveOrphansToSafeTrash()
    {
        int selectedCount = 0;
        for (int i = 0; i < orphanAssets.Count; i++)
        {
            if (orphanAssets[i].selectedForMove) selectedCount++;
        }

        if (selectedCount == 0)
        {
            EditorUtility.DisplayDialog("Sin selección", "Seleccione al menos un asset huérfano para mover a la Papelera Segura.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(SafeTrashFolder))
        {
            string createdGuid = AssetDatabase.CreateFolder("Assets", "Papelera_Segura");
            if (string.IsNullOrEmpty(createdGuid))
            {
                EditorUtility.DisplayDialog("Error", "No se pudo crear la carpeta " + SafeTrashFolder, "OK");
                return;
            }
        }

        int movedCount = 0;
        int errorCount = 0;

        try
        {
            for (int i = orphanAssets.Count - 1; i >= 0; i--)
            {
                OrphanItem item = orphanAssets[i];
                if (!item.selectedForMove) continue;

                EditorUtility.DisplayProgressBar(
                    "Moviendo a Papelera Segura",
                    item.assetPath,
                    (float)(orphanAssets.Count - i) / Mathf.Max(1, orphanAssets.Count));

                string fileName = Path.GetFileName(item.assetPath);
                string rawTarget = SafeTrashFolder + "/" + fileName;
                string uniqueTarget = AssetDatabase.GenerateUniqueAssetPath(rawTarget);

                string moveError = AssetDatabase.MoveAsset(item.assetPath, uniqueTarget);
                if (string.IsNullOrEmpty(moveError))
                {
                    movedCount++;
                    Debug.Log(string.Format("[Auditoría de Assets] Movido a Papelera Segura: '{0}' -> '{1}'", item.assetPath, uniqueTarget));
                    orphanAssets.RemoveAt(i);
                }
                else
                {
                    errorCount++;
                    Debug.LogWarning(string.Format("[Auditoría de Assets] Error al mover '{0}': {1}", item.assetPath, moveError));
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        statusMessage = string.Format(
            "Limpieza segura completada: {0} assets movidos a '{1}' (Errores: {2}).",
            movedCount,
            SafeTrashFolder,
            errorCount);

        EditorUtility.DisplayDialog("Papelera Segura", statusMessage, "OK");
    }
}
````

## File: Assets/_ProyectoBodega/Scripts/Editor/RestructureProject.cs
````csharp
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class RestructureProject
{
    private struct FolderMoveRule
    {
        public string sourcePath;
        public string destinationPath;

        public FolderMoveRule(string sourcePath, string destinationPath)
        {
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
        }
    }

    [MenuItem("Tools/Internal/Ejecutar Reestructuración Nivel 1")]
    public static void RunFromMenu()
    {
        List<string> results = ExecuteRestructure();
        for (int i = 0; i < results.Count; i++)
        {
            Debug.Log("[RestructureProject] " + results[i]);
        }
    }

    public static List<string> ExecuteRestructure()
    {
        List<string> logs = new List<string>();

        if (!AssetDatabase.IsValidFolder("Assets/_ProyectoBodega"))
        {
            string guid = AssetDatabase.CreateFolder("Assets", "_ProyectoBodega");
            logs.Add("Carpeta creada: Assets/_ProyectoBodega (GUID: " + guid + ")");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Plugins_Terceros"))
        {
            string guid = AssetDatabase.CreateFolder("Assets", "Plugins_Terceros");
            logs.Add("Carpeta creada: Assets/Plugins_Terceros (GUID: " + guid + ")");
        }
        else
        {
            logs.Add("Carpeta destino ya existe: Assets/Plugins_Terceros");
        }

        FolderMoveRule[] rules = new FolderMoveRule[]
        {
            new FolderMoveRule("Assets/Escenas", "Assets/_ProyectoBodega/Escenas"),
            new FolderMoveRule("Assets/Prefabs", "Assets/_ProyectoBodega/Prefabs"),
            new FolderMoveRule("Assets/Scripts", "Assets/_ProyectoBodega/Scripts"),
            new FolderMoveRule("Assets/Realistic Drone", "Assets/Plugins_Terceros/Realistic Drone"),
            new FolderMoveRule("Assets/Single_detailed_truck", "Assets/Plugins_Terceros/Single_detailed_truck"),
            new FolderMoveRule("Assets/Import/FactoryModularPack", "Assets/Plugins_Terceros/FactoryModularPack")
        };

        for (int i = 0; i < rules.Length; i++)
        {
            string oldPath = rules[i].sourcePath;
            string newPath = rules[i].destinationPath;

            if (!AssetDatabase.IsValidFolder(oldPath))
            {
                logs.Add("Ignorado (origen no existe): " + oldPath + " -> " + newPath);
                continue;
            }

            if (AssetDatabase.IsValidFolder(newPath))
            {
                logs.Add("Ignorado (destino ya existe): " + oldPath + " -> " + newPath);
                continue;
            }

            string moveError = AssetDatabase.MoveAsset(oldPath, newPath);
            if (string.IsNullOrEmpty(moveError))
            {
                logs.Add("Movido exitosamente: " + oldPath + " -> " + newPath);
            }
            else
            {
                logs.Add("Error al mover " + oldPath + " -> " + newPath + ": " + moveError);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return logs;
    }
}
````

## File: Assets/_ProyectoBodega/Scripts/DronePropellers.cs
````csharp
using UnityEngine;

public class DronePropellers : MonoBehaviour
{
    [Header("Hélices")]
    [Tooltip("Hélices que giran en sentido horario")]
    public Transform[] clockwisePropellers;

    [Tooltip("Hélices que giran en sentido antihorario")]
    public Transform[] counterClockwisePropellers;

    [Header("Ajustes de Giro")]
    public float maxSpeed = 2000f; // Grados por segundo a máxima velocidad
    public float accelerationRate = 2f; // Tiempo en alcanzar la velocidad máxima

    [Header("Eje de Giro")]
    public Vector3 rotationAxis = Vector3.up; // Cambiar a Vector3.forward o Vector3.right si el eje local varía

    [Header("Estado")]
    public bool isEngineOn = true;

    private float currentSpeed = 0f;

    void Update()
    {
        // Interpola la velocidad para dar un efecto de aceleración/frenado natural
        float targetSpeed = isEngineOn ? maxSpeed : 0f;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, maxSpeed * accelerationRate * Time.deltaTime);

        // Giro en sentido horario
        foreach (Transform prop in clockwisePropellers)
        {
            if (prop != null)
                prop.Rotate(rotationAxis * currentSpeed * Time.deltaTime, Space.Self);
        }

        // Giro en sentido antihorario
        foreach (Transform prop in counterClockwisePropellers)
        {
            if (prop != null)
                prop.Rotate(-rotationAxis * currentSpeed * Time.deltaTime, Space.Self);
        }
    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/geneticOptimization/GAmanager.cs
````csharp
using UnityEngine;
using System.Collections.Generic;
 
public class GAmanager : OptimizationManager {

    public GameObject originalIndividual;
    public GameObject waypointCircuit;
    private GameObject[] population;

    public float distanceBetweenClones = 5f;
    public int rowLength;
    private Vector3 storeInitialPos;
    public Vector3 nextPosition;

    /// <summary>
    /// Creates the first generation of the drones, initializing randomly the population
    /// </summary>
    void initializeRndPopulation()
    {
        initializePopulationVector(true);

        for (int i = 0; i < GeneticSettings.maxPopulation; i++)
        {
            population[i] = (GameObject)Instantiate(originalIndividual, nextPosition, Quaternion.identity);
            nextPosition += ((i + 1) % rowLength == 0 ? 
                new Vector3(storeInitialPos.x - nextPosition.x, 0, -distanceBetweenClones) : 
                new Vector3(distanceBetweenClones, 0, 0));

            activeDrone(population[i].GetComponent<geneticBehaviour>());
            if (readFile)
            {
                float[,] t = dataReader.readFile(folder + "\\" + PathFileToRead);
                if (t == null) { Debug.Log("Check the filename"); readFile = false; return; }

                float[] vals = new float[t.GetUpperBound(1) + 1];
                for (int k = 0; k < vals.Length; k++)
                    vals[k] = t[0, k];

                float[] changes = new float[t.GetUpperBound(1) + 1];
                for (int k = 0; k < changes.Length; k++)
                    changes[k] = t[1, k];

                //Debug.Log("file readed");

                population[i].GetComponent<geneticBehaviour>().setInitialKs(waypointCircuit, vals, changes);
            }
            else
                population[i].GetComponent<geneticBehaviour>().setInitialKs(waypointCircuit);
            
            population[i].GetComponent<geneticBehaviour>().setFather(this);
            population[i].transform.SetParent(transform);
            nOfSonsAlive++;
        }
        nextPosition = storeInitialPos;
        step = -1;
        actGeneration = 1;
    }

    /// <summary>
    /// Initializes the array containing all the drones
    /// </summary>
    /// <param name="firstTime">Indicates if it is the first call or it just has to be emptied</param>
    void initializePopulationVector(bool firstTime)
    {
        if (!firstTime) population.Initialize();
        population = new GameObject[GeneticSettings.maxPopulation];
        nOfSonsAlive = 0;            
        rowLength = (int)Mathf.Sqrt(GeneticSettings.maxPopulation);
    }

    /// <summary>
    /// Use the stored genes, that come from the list filled up with the results crossing of the genes of the best individual.
    /// <para>To speed up the process it initializes a group of drones for each call.
    /// Initializing them all toghether would slow down the simulation, while initializing them one by one would need too much time</para>
    /// </summary>
    /// <param name="howMany">Number of drones that has to be initialized in this frame</param>
    /// <returns>returns true if the max number of drones has been reached, while it return false if not</returns>
    bool generatePopulationFromStoredGenes(int howMany)
    {
        if (howMany == 0) return false;

        if(nOfSonsAlive < GeneticSettings.maxPopulation && ksList.Count > 0)
        {
            float[] ktmp = ksList.First.Value;
            ksList.RemoveFirst();

            population[nOfSonsAlive] = (GameObject)Instantiate(originalIndividual, nextPosition + Vector3.up * 5, Quaternion.identity);
            nextPosition += ((nOfSonsAlive + 1) % rowLength == 0 ?
                    new Vector3(storeInitialPos.x - nextPosition.x, 0, -distanceBetweenClones) :
                    new Vector3(distanceBetweenClones, 0, 0));

            activeDrone(population[nOfSonsAlive].GetComponent<geneticBehaviour>());
            population[nOfSonsAlive].GetComponent<geneticBehaviour>().setKeys(ktmp, waypointCircuit);
            population[nOfSonsAlive].GetComponent<geneticBehaviour>().setFather(this);
            population[nOfSonsAlive].transform.SetParent(transform);

            nOfSonsAlive++;
            return generatePopulationFromStoredGenes(howMany - 1);
        }

        hasReachedMaximum = false;
        nextPosition = storeInitialPos;
        return true;
    }

    // list containing the genes for the next population
    LinkedList<float[]> ksList = new LinkedList<float[]>();
    bool hasReachedMaximum = false;
    /// <summary>
    /// Add a set of keys to the list containing the genes used to initialize the next population
    /// </summary>
    /// <param name="ks">Set of keys that will be added to the list</param>
    void storeSons(float[] ks) { ksList.AddFirst((float[]) ks.Clone()); if (ksList.Count > GeneticSettings.maxPopulation + 2) hasReachedMaximum = true; }

    dataSaver ds;
    public bool save;
    /// <summary>
    /// initialize the dataSaver instances so it'll be possible to save the genes and restart the simulation from a specific situation
    /// </summary>
    void initializeds()
    {
        // this dataSaver will be used to see the fitness trend of the drones in different populations
        ds = new dataSaver("geneticData" ,new string[] {"lifetime", "fitness"});
    }

    /// <summary>
    /// Add an array of floats to the first dataSaver
    /// </summary>
    /// <param name="data">array of floats containing the lifetime of a drone, its fitness and the constants of a single PID. </param>
    public void addDataToDs(float[] data) { ds.addLine(data); }
    
    int indxSaving = 0;

    /// <summary>
    /// Save a set of parameters to a file.
    /// <para>This function is used to save the parameters of the best individuals</para>
    /// </summary>
    /// <param name="keys">array of parameters used for an experiment</param>
    /// <param name="fitness">fitness obtained with that set of keys</param>
    void SaveStats(float[] keys, float fitness)
    {
        if (!autoSave) return;

        float[] stats = new float[keys.Length + 1];
        stats[0] = fitness;
        for (int i = 1; i < stats.Length; i++)
            stats[i] = keys[i - 1];
        
        dataSaver ds = new dataSaver(folder + "\\Stat" + indxSaving + ".gen " + actGeneration + ".fit #" + fitness + "#", attributes);
        ds.addLine(stats);
        ds.saveOnFile();
        indxSaving++;
    }

    public string PathFileToRead;
    public bool readFile = false;
    public bool autoSave = false;
    public string folder;
    string[] attributes;

    /// <summary>
    /// Function called before of the first update, used to initialize
    /// </summary>
    void Start()
    {
        storeInitialPos = nextPosition;
        step = -2;
        Random.seed = (int)System.DateTime.Now.Ticks;
        initializeRndPopulation();
        initializeds();
        timeStepLevel = GeneticSettings.timelimit;        
        sons = new geneticBehaviour[transform.childCount];
        fitnesses = new float[transform.childCount];

        int nOfAttr = transform.GetChild(0).GetComponent<geneticBehaviour>().getKeys().Length;
        attributes = new string[nOfAttr + 1];
        attributes[0] = "fitness";
        for (int i = 1; i < nOfAttr; i++)
            attributes[i] = "Attr" + (i + 1).ToString();
    }

    int nOfSonsAlive = 0;

    /// <summary>
    /// Register the result of the experiment to the manager class.
    /// </summary>
    /// <param name="fitness">Fitness of the drone obtained from the experiment</param>
    /// <param name="naturalDeath">Boolean that indicates if the drone died because the time has expired or because it get to far from the path.</param>
    public override void setExperimentResult(float fitness, bool naturalDeath) { /* Not used in this optimization */ }

    float timeStepLevel = 0;
    int actualLevel = 1;
    int generationsEachLevel = 5;
    int actGeneration = 0;

    int step = -1;
    geneticBehaviour[] sons;
    float[] fitnesses;
    int fillIndx = 0;
    geneticBehaviour[] survivedSons;
    geneticBehaviour[] bestIndividual;

    /// <summary>
    /// Function used to store the sons once they die.
    /// </summary>
    /// <param name="ge">GeneticBehaviour containing the information we want to store</param>
    public void storeDataAboutIndividualLife(geneticBehaviour ge) { if (ge == null) return; sons[fillIndx] = ge; fitnesses[fillIndx] = ge.fitness(); fillIndx++; }

    /// <summary>
    /// Called once per frame. In this section is stored the algorithm that manage the population
    /// </summary>
    void Update()
    {
        if (save) { save = false; ds.saveOnFile();}
       

        switch (step)
        {
            case -1:
                // the population has been created. Now we have to wait until all of them die
                if (transform.childCount == 0)
                {
                    step++;
                    Debug.Log("-1 passed - All sons have lived until their limit and sent the data -");
                }
                else if (transform.childCount < 4)
                {
                    foreach (geneticBehaviour g in GetComponentsInChildren<geneticBehaviour>())
                        g.fatherSayIHaveToSendMyData = true;
                    step++;
                    Debug.Log("-1 passed - most of the sons died, saved the most adapted to the experiment -");
                }
                break;
            case 0:
                // in this step we sort the individuals, accordingly to their fitness
                // in this way it'll be easy to extract the best ones
                survivedSons = new geneticBehaviour[fillIndx];
                float[] fitnessesOfSurvived = new float[fillIndx];

                for (int k = 0; k < fillIndx; k++)
                {
                    survivedSons[k] = sons[k];
                    fitnessesOfSurvived[k] = fitnesses[k];
                }
                System.Array.Sort(fitnessesOfSurvived, survivedSons);
                step++;
                Debug.Log("0 passed - Sons are stored and sorted -");
                break;
            case 1:
                // best individuals are selected and saved to another vector
                int size = Mathf.Min(GeneticSettings.numberOfBest, fillIndx);
                bestIndividual = new geneticBehaviour[size];
                int diff = fillIndx - size;
                for (int k = 0; k < bestIndividual.Length; k++)
                    bestIndividual[k] = survivedSons[k + diff];
                step++;
                Debug.Log("1 passed - best sons have been selected -");
                break;
            case 2:
                // best individuals are saved to file for analysis and put in the list so they'll be respawned in the next generation
                int size2 = Mathf.Min(GeneticSettings.reinjectedIndividual, fillIndx);
                for (int i = bestIndividual.Length - size2; i < bestIndividual.Length; i++)
                {
                    storeSons(bestIndividual[i].getKeys());                   
                    SaveStats(bestIndividual[i].getKeys(), bestIndividual[i].fitness());
                    Debug.Log("generatedReinjectedSon");
                }
                step++;
                Debug.Log("2 passed - elité sons have been reinjected in the birth-list and saved -");
                break;
            case 3:
                // now we generate new sons using randomly selected individuals
                while (!hasReachedMaximum)
                {
                    Random.seed = (int)System.DateTime.Now.Ticks;
                    geneticBehaviour A = survivedSons[Random.Range(0, survivedSons.Length)];
                    geneticBehaviour B = survivedSons[Random.Range(0, survivedSons.Length)];

                    if ((A.getID() != B.getID() || bestIndividual.Length == 1) && A.numberOfSonsCreated < GeneticSettings.maxNumberOfSons)
                    {
                        storeSons(A.cross(B.getKeys()));
                        Debug.Log("generatedNewSon");
                    }
                }                    
                step++;
                Debug.Log("3 passed - the rest of the population has been queued in the birth-list -");
                break;
            case 4:
                // reinitializing variables for the next generation
                nextPosition = storeInitialPos;
                initializePopulationVector(false);
                if (actGeneration % generationsEachLevel == 0)
                {
                    GeneticSettings.timelimit = timeStepLevel * ++actualLevel;
                    timeStepLevel += 0.25f;
                    GeneticSettings.timelimit = (GeneticSettings.timelimit <= GeneticSettings.maxTime ? GeneticSettings.timelimit : GeneticSettings.maxTime);
                }
                    
                step++;
                Debug.Log("4 passed - inizialized variables and setted time -");
                break;
            case 5:
                // regenerating the population, eventually changing the experiment duration and setting the step to -1 so the process restart from the beginning
                if (generatePopulationFromStoredGenes(20))
                {
                    fillIndx = 0;
                    sons = new geneticBehaviour[transform.childCount];
                    fitnesses = new float[transform.childCount];
                    actGeneration++;
                    step = -1;
                    Debug.Log("5 passed - new population has been generated. Next experiment duration:" + GeneticSettings.timelimit + " -");
                }                    

                break;
        }

    }




}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/geneticOptimization/geneticBehaviour.cs
````csharp
using UnityEngine;

public class geneticBehaviour : OptimizationBehaviour {
     
    public bool hidebody;

    public bool fixedValues = false;
    public float[] myVals;
    public float[] valsAlphas;

    /// <summary>
    /// Gets the fitness of the individual
    /// </summary>
    public override float fitness()
    {
        float f1 = distanceMade * 15f;
        float f2 = -accumulatedDistanceFromRoutePosition / 5;
        return GeneticSettings.normalizeFitness(f1 + f2);
    }

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start() {
        startOperations(GeneticSettings.timelimit, hidebody, GetComponent<WaypointProgressTracker>().getRoutePosition());        
    }

    /// <summary>
    /// Function called each frame
    /// </summary>
    void Update() {
        updateOperations(GetComponent<WaypointProgressTracker>().getRoutePosition());
        if (hasToBeKilled()) die(false);
        if (isOldDead() || fatherSayIHaveToSendMyData) { ((GAmanager) father).storeDataAboutIndividualLife(this); die(true); }
    }
    public bool fatherSayIHaveToSendMyData = false;

    /// <summary>
    /// Kill the individual
    /// </summary>
    /// <param name="fullFitness">indicates if the drone died for natural death or not</param>
    public void die (bool fullFitness)
    {
        if (GeneticSettings.hasToBeSaved(id) && fullFitness)
            ((GAmanager)father).addDataToDs(new float[] { getAge(), fitness()} );        
        Destroy(spawnedWaypoint);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// returns the keys used for the experiment
    /// </summary>
    public override float[] getKeys() { return myVals; }

    /// <summary>
    /// Sets the keys used for the experiment and the waypoint
    /// </summary>
    /// <param name="ks">Array of float containing the parameters</param>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    public override void setKeys(float[] ks, GameObject waypoint) {

        myVals = new float[valsAlphas.Length];
        for (int i = 0; i < myVals.Length; i++)
            myVals[i] = ks[i];
        if (Random.Range(0f, 1f) < GeneticSettings.mutationProbability) mutate();
        writeKeysOnDMC(myVals, waypoint);
    }

    /// <summary>
    /// Special case where the keys are not setted by the manager, 
    /// they are calculated more or less randomly by the same individual
    /// </summary>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    public override void setInitialKs(GameObject waypoint)
    {
        if (!fixedValues)
        {
            myVals = new float[valsAlphas.Length];
            for (int i = 0; i < myVals.Length; i++)
                myVals[i] = Random.Range(-1f, 1f);
        }
        else
        {
            for (int i = 0; i < myVals.Length; i++)
            {
                float limit = Mathf.Abs(myVals[i] * valsAlphas[i] * 2);
                myVals[i] += Random.Range(-limit, limit);
            }
                
        }

        writeKeysOnDMC(myVals, waypoint);
    }

    /// <summary>
    /// Sets the keys used for the experiment and the waypoint
    /// </summary>
    /// <param name="ks">Array of float containing the parameters</param>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    /// <param name="changes">array of floats that represents the maximum change of the keys</param>
    public void setInitialKs(GameObject waypoint, float[] vals, float[] changes)
    {
        for (int i = 0; i < myVals.Length; i++)
            myVals[i] = vals[i] + Random.Range(-changes[i], changes[i]);
        writeKeysOnDMC(myVals, waypoint);
    }

    /// <summary>
    /// Genetic cross between the key of the individual and the keys of other one
    /// </summary>
    /// <returns>A new set of keys, resulting from the cross</returns>
    public float[] cross(float[] otherKs)
    {
        numberOfSonsCreated++;
        float[] kTmp = new float[myVals.Length];
        for (int i = 0; i < kTmp.Length; i++)
            kTmp[i] = BLXalpha_cross(myVals[i], otherKs[i], valsAlphas[i]);   
        return kTmp;
    }
    
    /// <summary>
    /// Randomly mutate the keys of the individual
    /// </summary>
    protected void mutate()
    {

        for (int i = 0; i < myVals.Length; i++)
            myVals[i] = BLXalpha_mutation(myVals[i], valsAlphas[i] / 5f);
    }

    public float numberOfSonsCreated = 0;

    /// <summary>
    /// Perform a BLXalpha cross without specifying the alpha (0.3f as default)
    /// </summary>
    /// <param name="A">Value of a gene of the first individual</param>
    /// <param name="B">Value of a gene of the second individual</param>
    /// <returns>Resulting gene, obtained crossing the two values</returns>
    float BLXalpha_cross(float A, float B) { return BLXalpha_cross(A, B, 0.3f); }

    /// <summary>
    /// Perform a BLXalpha cross specifying the alpa
    /// </summary>
    /// <param name="A">Value of a gene of the first individual</param>
    /// <param name="B">Value of a gene of the second individual</param>
    /// <param name="rangeM">Alpha value</param>
    /// <returns>Resulting gene, obtained crossing the two values</returns>
    float BLXalpha_cross(float A, float B, float rangeM)
    {
        float min = System.Math.Min(A, B);
        float max = System.Math.Max(A, B);
        float alpha = Random.Range(0, rangeM * (max - min));
        return Random.Range(System.Math.Max(min - alpha, 0),
                            System.Math.Min(max + alpha, 1));
    }

    /// <summary>
    /// Perform a BLXalpha mutation without specifying the alpha (0.5f as default)
    /// </summary>
    /// <param name="A">Value of the gene to mutate</param>
    /// <returns>Mutated gene</returns>
    float BLXalpha_mutation(float A) { return BLXalpha_mutation(A, 0.5f); }

    /// <summary>
    /// Perform a BLXalpha mutation specifying the alpha value
    /// </summary>
    /// <param name="A">Value of the gene to mutate</param>
    /// <param name="rangeM">Alpha value</param>
    /// <returns>Mutated gene</returns>
    float BLXalpha_mutation(float A, float alpha)
    {
        float k = Random.Range(0, alpha * A);
        return Random.Range(System.Math.Max(A - k, 0),
                            System.Math.Min(A + k, 1));
    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/geneticOptimization/GeneticSettings.cs
````csharp
using UnityEngine;
using System.Collections;

public class GeneticSettings {
    
    // probability, for each new individual, to mutate 
    public const float mutationProbability = 0.03f;

    // max number of individuals
    public const int maxPopulation = 30;
    // individuals that will be used to generate the next generation
    public const int numberOfBest = 20;
    // individuals that will be reinjected to the next generation
    public const int reinjectedIndividual = 5;

    // modify this parameter to save the statistics of more or less drones
    private const float modOfSavedIndividualForEachGeneration = 1;
    public static bool hasToBeSaved(int id) { return (id % modOfSavedIndividualForEachGeneration == 0); }

    // fitness functions of the individuals
    const float minFitness = -5000;
    const float maxFitness = 5000;
    public static float normalizeFitness(float f) { return droneSettings.normalizeBetween(f, minFitness, maxFitness); }

    // Variable used to avoid the monopolization of the new born from one single parents
    // it is setted high because it is not a real problem
    public const int maxNumberOfSons = 30;

    // life of a drone, it'll be increased until it reaches maxTime
    public static float timelimit = 20;
    public static float maxTime = 240;

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/twidOptimization/twiddleBehaviour.cs
````csharp
using UnityEngine;
using System.Collections;
using System;

public class twiddleBehaviour : OptimizationBehaviour {

    public bool hidebody;

    /// <summary>
    /// Gets the fitness of the individual
    /// </summary>
    public override float fitness()
    {
        float f6 = distanceMade * 15f;
        float f7 = -accumulatedDistanceFromRoutePosition / 4;
        return GeneticSettings.normalizeFitness(f6 + f7);
    }

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start()
    {
        startOperations(twiddleSettings.timelimit, hidebody, GetComponent<WaypointProgressTracker>().getRoutePosition());
    }

    /// <summary>
    /// Function called each frame
    /// </summary>
    void Update()
    {
        updateOperations(GetComponent<WaypointProgressTracker>().getRoutePosition());
        if (hasToBeKilled()) { die(false); }
        if (isOldDead()) { die(true); }
    }

    /// <summary>
    /// Kill the individual
    /// </summary>
    /// <param name="naturalDeath">indicates if the drone died for natural death or not</param>
    public void die(bool naturalDeath)
    {

        father.setExperimentResult(fitness(), naturalDeath);
        Destroy(spawnedWaypoint);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Sets the keys used for the experiment and the waypoint
    /// </summary>
    /// <param name="ks">Array of float containing the parameters</param>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    public override void setKeys(float[] ks, GameObject waypointC)
    {
        writeKeysOnDMC(ks, waypointC);
    }

    /// <summary>
    /// [NOT USED IN twiddle]
    /// <para> Special case where the keys are not setted by the manager, 
    /// they are calculated more or less randomly by the same individual </para>
    /// </summary>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    public override void setInitialKs(GameObject waypoint)
    {
        Debug.Log("Something is wrong. You shouldn't be here!");
    }

    /// <summary>
    /// returns the keys used for the experiment [NOT USED IN twiddle]
    /// </summary>
    public override float[] getKeys()
    {
        Debug.Log("Something is wrong. You shouldn't be here!");
        return new float[] { 0 };
    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/twidOptimization/twiddleManager.cs
````csharp
using UnityEngine;
using System.Collections;

public class twiddleManager : OptimizationManager {

    public GameObject originalIndividual;
    int actualGeneration = 0;

    public float startingTime;
    public float actualTime;
    public float maxExpTime;
    public float step;

    public GameObject[] experiments;

    public float[] myVals;
    public float[] potencialChanges;
     


    /// <summary>
    /// Initializes drones and circuits for a new experiment. Once they'll finish they'll send back the data
    /// </summary>
    void startExperiment()
    {
        Vector3 initialPosition = transform.position;
        foreach(GameObject g in experiments)
        {
            GameObject tmp = (GameObject)Instantiate(originalIndividual, initialPosition, Quaternion.identity);
            twiddleBehaviour twiddleB = tmp.GetComponent<twiddleBehaviour>();
            activeDrone(twiddleB);
            twiddleB.setKeys(myVals, g);
            twiddleB.setFather(this);
            tmp.transform.SetParent(transform);

            initialPosition += Vector3.forward * 75;
        }

        actualGeneration++;
        Debug.Log("Experiment " + actualGeneration + " started");
    }

    bool resObtained = false;
    float tmpFitness = 0;
    int expCounter = 0;
    /// <summary>
    /// Register the result of the experiment to the manager class.
    /// </summary>
    /// <param name="fitness">Fitness of the drone obtained from the experiment</param>
    /// <param name="naturalDeath">Boolean that indicates if the drone died because the time has expired or because it get to far from the path.</param>
    public override void setExperimentResult(float fitness, bool naturalDeath) {
        if (expCounter == 0) tmpFitness = 0;

        tmpFitness += (naturalDeath ? fitness : fitness / 50f);
        expCounter++;

        if (expCounter == experiments.Length) { resObtained = true; expCounter = 0; ds.addLine(new float[] { tmpFitness }); }
    }

    public string PathFileToRead;
    public bool readFile = false;
    public bool autoSave = false;
    public string folder;

    public bool save = false;
    dataSaver ds;

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start()
    {
        twiddleSettings.timelimit = startingTime;
        twiddleSettings.step = step;
        twiddleSettings.maxTime = maxExpTime;

        ds = new dataSaver("TwiddleTrend", new string[] {"overallFitness"});
    }

    int indxSaving = 0;
    /// <summary>
    /// Save on file a parameters+changes configuration
    /// </summary>
    /// <param name="stats">array of parameters</param>
    /// <param name="pc">array of potencial changes</param>
    void SaveStats(float[] stats, float[] pc) {
        string[] attributes = new string[myVals.Length];
        for (int i = 0; i < myVals.Length; i++)
            attributes[i] = "Attr" + (i + 1).ToString();

        dataSaver ds = new dataSaver(folder + "\\Stat" + indxSaving + ".fit #" + bestFitness + "#", attributes);
        ds.addLine(stats);
        ds.addLine(pc);
        ds.saveOnFile();
        indxSaving++;
    }

    /// <summary>
    /// Function called each frame
    /// </summary> 
    void Update()
    {
        if (save) { save = false; ds.saveOnFile(); }

        // if the boolean 'readFile' is true, we substitute the array setted in the editor with the one in the file
        // in this way is possible to continue the training until a certain point and later start from that point
        if (readFile) {
            float[,] t = dataReader.readFile(folder + "\\" + PathFileToRead);
            if (t == null) { Debug.Log("Check the filename"); readFile = false; return; }

            for (int i = 0; i <= t.GetUpperBound(1); i++)
                myVals[i] = t[0,i];
            for (int i = 0; i <= t.GetUpperBound(1); i++)
                potencialChanges[i] = t[1,i];
            
            Debug.Log("file readed");
            readFile = false;
            twiddleStep = -2;
            iPC = 0;
        }
        else if (twiddle()) Debug.Log("Experiment finished");
    }

    float bestFitness = -50f;
    float threshold = 0.0000001f;
    float minimumValueToCorrect = 0.000001f;
    int twiddleStep = -2;
    int iPC = 0;

    /// <summary>
    /// twiddle function. It is called every frame.
    /// <para>It is composed of differents steps, each one manage one part of the algorithm</para>
    /// </summary>
    /// <returns>Returns true if the process is completed, otherwise fale</returns>
    bool twiddle()
    {
        float sumPC = 0;
        foreach (float f in potencialChanges) sumPC += Mathf.Abs(f);
        if (sumPC < threshold) return true;       

        // periodically save the configuration
        if (autoSave && iPC % myVals.Length == 0 && twiddleStep == 0)
        {
            SaveStats(myVals, potencialChanges);
            Debug.Log("Saved Individual on file " + iPC);
        }
        
        iPC %= myVals.Length;
        int startIndx = iPC;
        // moves to the next value that has to be modified. If there is no-one the process finish
        while (Mathf.Abs(potencialChanges[iPC]) < minimumValueToCorrect)
        {
            iPC = (iPC + 1) % myVals.Length;
            if (iPC == startIndx)
                return true;
        }

        switch (twiddleStep)
        {
            case -2:
                // FIRST experiment. It is needed to calculate the initial fitness of the individual
                Debug.Log("Launched experiment -1 ");
                startExperiment();
                twiddleStep++;
                break;
            case -1:
                if (resObtained)    // wait until the experiment ends
                {
                    // associates the bestFitness with the result obtained and go on
                    Debug.Log("obteined results of exp -1, initial fitness [" + tmpFitness + "]");
                    resObtained = false;
                    bestFitness = tmpFitness;
                    twiddleStep = 0;                   
                }
                break;
            case 0:
                // sum the value of potencial changes to the parameter taken in consideration
                actualTime = twiddleSettings.increaseTimeLimit();
                Debug.Log("twiddle started on indx " + iPC + "; Experiments duration ->" + actualTime);                
                myVals[iPC] += potencialChanges[iPC];
                startExperiment();
                twiddleStep++;
                break;
            case 1:
                if (resObtained)    // wait until the experiment ends
                {                    
                    resObtained = false;
                    if (tmpFitness > bestFitness)   // There was some improvement. Increase this PC and move on next 
                    {
                        Debug.Log("IMPROVED  [" + tmpFitness + "<->" + bestFitness + "]. increase & next iPC");
                        bestFitness = tmpFitness;
                        potencialChanges[iPC] *= 1.1f;
                        iPC++;
                        twiddleStep = 0;
                    }
                    else    // no improvement. Try on the other direction
                    {
                        Debug.Log("no improvements  [" + tmpFitness + "<->" + bestFitness + "]. revert & repeat experiment");
                        myVals[iPC] -= 2 * potencialChanges[iPC];
                        startExperiment();
                        twiddleStep++;                        
                    }
                }
                break;
            case 2:
                if (resObtained)    // wait until the experiment ends
                {                    
                    resObtained = false;
                    if (tmpFitness > bestFitness)   // There was some improvement. Increase this PC and move on next 
                    {
                        Debug.Log("IMPROVED  [" + tmpFitness + "<->" + bestFitness + "]. increase & next iPC");
                        bestFitness = tmpFitness;
                        potencialChanges[iPC] *= 1.1f;
                    }
                    else    // no improvement. Adjust val and reduce this PC
                    {
                        Debug.Log("no improvements  [" + tmpFitness + "<->" + bestFitness + "]. reset changes & next iPC");
                        myVals[iPC] += potencialChanges[iPC];
                        potencialChanges[iPC] *= 0.9f;
                    }
                    // we move to the next index
                    iPC++;
                    //every time we finish a complete round we recalculate the actual values to update the bestFitness at the current experiment length
                    twiddleStep = (iPC == myVals.Length ? -2 : 0);
                }
                break;
        }
        return false;
    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/twidOptimization/twiddleSettings.cs
````csharp
using UnityEngine;
using System.Collections;

public class twiddleSettings {

    // time of the experiment. It'll increase by 'step', until it reaches 'maxTime'
    public static float timelimit = 3f;
    public static float maxTime = 30;
    public static float step = 0.25f;
    public static float increaseTimeLimit() { timelimit += step; timelimit = (timelimit >= maxTime ? maxTime : timelimit); return timelimit;  }
     
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/OptimizationBehaviour.cs
````csharp
using UnityEngine;
using System.Collections;

public abstract class OptimizationBehaviour : MonoBehaviour {

    // list of PIDs that will be initialized with the values of the training
    // and injected to the drone, substituting the default ones
    protected PID yawPID;
    protected PID rollPID;
    protected PID pitchPID;
    protected PID yPID;
    protected PID zPID;
    protected PID xPID;

    protected int id = 0;
    private static int idNr = 1;
    /// <summary>
    /// Sets the id of the drone using the static variable idNr
    /// </summary>
    private void setID() { id = idNr++; }
    /// <summary>
    /// Gets the id of the drone
    /// </summary>
    public int getID() { return id; }

    /// <summary>
    /// Function that hides the mesh of the drone. 
    /// In this way it is possible to use more of them without overloading the GPU
    /// </summary>
    /// <param name="b">Boolean that indicates to hide the body or not</param>
    private void hideBody(bool b)
    {
        if (!b) return;
        transform.Find("Body").gameObject.SetActive(false);
        transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(4).GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
    }

    private float age = 0;
    /// <summary>
    /// Gets the age, in seconds, of the drone
    /// </summary>
    public float getAge() { return age; }

    private float lifeTime;
    /// <summary>
    /// Sets the lifetime of the drone, in seconds. 
    /// After this time it'll die of natural death and send its data to the manager
    /// </summary>
    /// <param name="lt"></param>
    private void setLifeTime(float lt) { lifeTime = lt; }

    /// <summary>
    /// returns true if the age of the drone is greather than its lifetime
    /// <para>If this function is true the drone die of natural death and send its data to the manager</para>
    /// </summary>
    protected bool isOldDead() { return age > lifeTime; }

    /// <summary>
    /// returns true if the drone gets too far from the circuit or too near to the ground
    /// <para>If this function is true the drone die of non-natural death and send its data to the manager, 
    /// but it won't be taken in consideration as if it were dead of natural death</para>
    /// </summary>
    protected bool hasToBeKilled() { return Vector3.Distance(transform.position, routePosition) > maxDistance || transform.position.y < 1f; }

    private float maxDistance = 10f;

    protected OptimizationManager father;
    /// <summary>
    /// Sets the manager of the drone. 
    /// The reference will be used to send the result of the experiment (the fitness)
    /// </summary>
    public void setFather(OptimizationManager om) { father = om; }
    protected droneMovementController dmc;
    protected WaypointProgressTracker wpt;

    protected GameObject spawnedWaypoint;

    private Vector3 lastroutePosition;
    private Vector3 routePosition;   
    protected float distanceMade;
    protected float accumulatedDistanceFromRoutePosition = 0;

    /// <summary>
    /// Function called in the Start function that initializes the variables used for the experiment
    /// </summary>
    /// <param name="lifeTime">Time the drone will live before of dying of natural death </param>
    /// <param name="hidebody">Indicate if the mesh of the drone has to be hidden or not </param>
    /// <param name="routePos">Route position in the circuit. 
    /// It is used to estabilish if the drone has to be killed because too far from the circuit </param>
    protected void startOperations(float lifeTime, bool hidebody, Vector3 routePos) {
        setID();
        hideBody(hidebody);
        setLifeTime(lifeTime);
        dmc = gameObject.GetComponent<droneMovementController>();
        routePosition = routePos;
        lastroutePosition = routePos;
    }

    /// <summary>
    /// Function called in the Update that keep update the parameter
    /// </summary>
    /// <param name="routePos">Route position in the circuit. 
    /// It is used to estabilish if the drone has to be killed because too far from the circuit </param>
    protected void updateOperations(Vector3 routePos) {
        age += Time.deltaTime;
        lastroutePosition = routePosition;
        routePosition = routePos;
        distanceMade += Vector3.Distance(lastroutePosition, routePosition);
        accumulatedDistanceFromRoutePosition += Vector3.Distance(transform.position, routePosition);
    }

    /// <summary>
    /// function used to calculate the fitness of the configuration used
    /// </summary>
    public abstract float fitness();
    
    /// <summary>
    /// returns the keys used for the experiment
    /// </summary>
    public abstract float[] getKeys();

    /// <summary>
    /// Sets the keys used for the experiment and the waypoint
    /// </summary>
    /// <param name="ks">Array of float containing the parameters</param>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    public abstract void setKeys(float[] ks, GameObject waypoint);

    /// <summary>
    /// Special case where the keys are not setted by the manager, 
    /// they are calculated randomly by the same individual
    /// </summary>
    /// <param name="waypoint">WaypointCircuit that will be used for the experiment</param>
    public abstract void setInitialKs(GameObject waypoint);

    /// <summary>
    /// Sets the keys used for the experiment and the waypoint
    /// </summary>
    /// <param name="myVals">Array of float containing the parameters</param>
    /// <param name="waypointCircuit">WaypointCircuit that will be used for the experiment</param>
    protected void writeKeysOnDMC(float[] myVals, GameObject waypointCircuit)
    {
        // if the variables are empty, is assigns them
        dmc = (dmc != null ? dmc : GetComponent<droneMovementController>());
        wpt = (wpt != null ? wpt : GetComponent<WaypointProgressTracker>());

        // initializing new PIDs
        yPID = new PID(myVals[0], myVals[1], myVals[2], myVals[3]);
        zPID = new PID(myVals[4], myVals[5], myVals[6], myVals[7]);
        xPID = new PID(myVals[4], myVals[5], myVals[6], myVals[7]);
        yawPID = new PID(myVals[8], myVals[9], myVals[10], myVals[11]);
        rollPID = new PID(myVals[12], myVals[13], myVals[14], myVals[15]);
        pitchPID = new PID(myVals[12], myVals[13], myVals[14], myVals[15]);

        // calls the functions in the droneMovementController to set the keys and the constants
        dmc.setKs(yPID, zPID, xPID, pitchPID, rollPID, yawPID);
        dmc.setConsts(myVals[16], myVals[17], myVals[18], myVals[19], myVals[20], myVals[22], myVals[23]);

        // spawns the WaypointCircuit and assign it to the drone
        Vector3 wpcPosition = this.transform.position + new Vector3(2, 6, 2);
        spawnedWaypoint = (GameObject)Instantiate(waypointCircuit, wpcPosition, Quaternion.identity);
        wpt.setWaypoint(spawnedWaypoint.GetComponent<WaypointCircuit>());
    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/optimizations/OptimizationManager.cs
````csharp
using UnityEngine;
using System.Collections;

public abstract class OptimizationManager : MonoBehaviour {

    /// <summary>
    /// Register the result of the experiment to the manager class.
    /// </summary>
    /// <param name="fitness">Fitness of the drone obtained from the experiment</param>
    /// <param name="naturalDeath">Boolean that indicates if the drone died because the time has expired or because it get to far from the path.</param>
    public abstract void setExperimentResult(float fitness, bool naturalDeath);

    /// <summary>
    /// Enables the OptimizationBehaviour script passed as argument
    /// <para>This permits to switch off the scripts, so it is possible to use it just if it is necessary.
    /// In this way it won't work if a manager doesn't active it first.</para>
    /// </summary>
    /// <param name="opt">OptimizationBehaviour class that we want to activate</param>
    protected void activeDrone(OptimizationBehaviour opt) { opt.enabled = true; }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/Accelerometer.cs
````csharp
using UnityEngine;
using System.Collections;

/// <summary>
/// Accelerometer sensor class
/// </summary>
public class Accelerometer : MonoBehaviour {

    private Vector3 LocalLinearVelocity;
    /// <summary>
    /// Gets the local linear velocity of the drone
    /// </summary>
    /// <returns></returns>
    public Vector3 getLocalLinearVelocity() { return noiseVel.getNoise(LocalLinearVelocity); }
    private Vector3 lastVelocity;

    private Vector3 LocalLinearAcceleration;
    /// <summary>
    /// Gets the local linear acceleration of the drone
    /// </summary>
    /// <returns></returns>
    public Vector3 getLinearAcceleration() { return noiseAcc.getNoise(LocalLinearAcceleration); }

    public Vector3 LV;

    noiseAddVector3 noiseVel;
    noiseAddVector3 noiseAcc;
    /// <summary>
    /// Function Called when the object is activated for the first time
    /// </summary>
    void Awake() {
        noiseAcc = new noiseAddVector3();
        noiseVel = new noiseAddVector3();
    }

    /// <summary>
    /// Function at regular time interval
    /// </summary>
    void FixedUpdate()
    {
        //rotate the velocity obtained from the rigidbody by 45° to obtain the local velocity
        LocalLinearVelocity = droneSettings.setZeroIflessThan(
            Quaternion.AngleAxis(45,transform.up) * transform.InverseTransformDirection(transform.GetComponent<Rigidbody>().velocity), 
            0.0001f);
        LocalLinearAcceleration = droneSettings.setZeroIflessThan((LocalLinearVelocity - lastVelocity) / Time.deltaTime, 0.0001f);

        lastVelocity = LocalLinearVelocity;
        LV = droneSettings.setZeroIflessThan(transform.GetComponent<Rigidbody>().velocity, 0.0001f);

    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/Barometer.cs
````csharp
using UnityEngine;
using System.Collections;

/// <summary>
/// Barometer sensor class
/// </summary>
public class Barometer : MonoBehaviour {

    private float height;
    /// <summary>
    /// Gets the height of the drone
    /// </summary>
    public float getHeight() { return nHeight.getNoise(height); }
    private float lastHeight;

    private float verticalSpeed;
    /// <summary>
    /// Gets the vertical speed of the drone
    /// </summary>
    public float getverticalSpeed() { return nSpeed.getNoise(verticalSpeed); }
    private float lastSpeed;

    private float verticalAcc;
    /// <summary>
    /// Gets the vertical acceleration of the drone
    /// </summary>
    public float getverticalAcc() { return nAcc.getNoise(verticalAcc); }

    noiseAdder nHeight;
    noiseAdder nSpeed;
    noiseAdder nAcc;

    /// <summary>
    /// Function Called when the object is activated for the first time
    /// </summary>
    void Awake()
    {
        nHeight = new noiseAdder();
        nSpeed = new noiseAdder();
        nAcc = new noiseAdder();

        lastHeight = height = transform.position.y;
        lastSpeed = verticalSpeed = 0;
        verticalAcc = 0;
    }

    /// <summary>
    /// Function at regular time interval
    /// </summary>
    void FixedUpdate()
    {
        height = transform.position.y;
        verticalSpeed = (height - lastHeight) / Time.deltaTime;
        verticalAcc = (verticalSpeed - lastSpeed) / Time.deltaTime;

        lastHeight = height;
        lastSpeed = verticalSpeed;
    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/GPS.cs
````csharp
using UnityEngine;
using System.Collections;

/// <summary>
/// GPS sensor class
/// </summary>
public class GPS : MonoBehaviour
{
    /// <summary>
    /// Gets the coordinates of the drone in a Vector2 object
    /// </summary>
    public Vector2 getCoords() { return new Vector2(noiseX.getNoise(transform.position.x), noiseY.getNoise(transform.position.z)); }

    noiseAdder noiseX;
    noiseAdder noiseY;

    /// <summary>
    /// Function Called when the object is activated for the first time
    /// </summary>
    void Awake() { noiseX = new noiseAdder(); noiseY = new noiseAdder(); }
     
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/Gyro.cs
````csharp
using UnityEngine;
using System.Collections;

/// <summary>
/// Gyro sensor class
/// </summary>
public class Gyro : MonoBehaviour {
    
    private rotor helixV1;
    private rotor helixV2;
    private rotor helixO1;
    private rotor helixO2;
    private float distanceBetweenHelixes;

    private float vDifference;
    private float oDifference;

    private float pitch = 0;
    /// <summary>
    /// Gets the actual pitch
    /// </summary>
    public float getPitch() { return nPitch.getNoise(pitch); }
    private float lastPitch = 0;    
    private float pitchVel = 0;
    /// <summary>
    /// Gets the actual pitch-velocity
    /// </summary>
    public float getPitchVel() { return nPitchSpeed.getNoise(pitchVel); }
    private float lastPitchVel = 0;
    private float pitchAcc = 0;
    /// <summary>
    /// Gets the actual pitch-acceleration
    /// </summary>
    public float getPitchAcc() { return nPitchAcc.getNoise(pitchAcc); }

    private float roll = 0;
    /// <summary>
    /// Gets the actual roll
    /// </summary>
    public float getRoll() { return nRoll.getNoise(roll); }
    private float lastRoll = 0;
    private float rollVel = 0;
    /// <summary>
    /// Gets the actual roll-velocity
    /// </summary>
    public float getRollVel() { return nRollSpeed.getNoise(rollVel); }
    private float lastRollVel = 0;
    private float rollAcc = 0;
    /// <summary>
    /// Gets the actual roll-acceleration
    /// </summary>
    public float getRollAcc() { return nRollAcc.getNoise(rollAcc); }

    noiseAdder nRoll;
    noiseAdder nRollSpeed;
    noiseAdder nRollAcc;
    noiseAdder nPitch;
    noiseAdder nPitchSpeed;
    noiseAdder nPitchAcc;

    public Transform body;

    Vector3 AngularVelocity;
    Vector3 AngularAcceleration;

    private float normalize (float n) { return droneSettings.normalizeBetween(n, 0, distanceBetweenHelixes); }

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start()
    {
        droneMovementController dmc = gameObject.GetComponent<droneMovementController>();

        // associate the rotors
        helixO1 = dmc.helixO1;
        helixO2 = dmc.helixO2;
        helixV1 = dmc.helixV1;
        helixV2 = dmc.helixV2;

        // finds the rotors position
        Vector3 v1 = helixV1.transform.position;
        Vector3 v2 = helixV2.transform.position;
        Vector3 o1 = helixO1.transform.position;
        Vector3 o2 = helixO2.transform.position;

        Vector3 mid_v1o1 = (v1 + o1) / 2f;
        Vector3 mid_v2o2 = (o2 + v2) / 2f;
        distanceBetweenHelixes = Vector3.Distance(mid_v1o1, mid_v2o2);

        // instantiates the noiseAdders, to add the noise to the measurement
        nRoll = new noiseAdder();
        nRollSpeed = new noiseAdder();
        nRollAcc = new noiseAdder();
        nPitch = new noiseAdder();
        nPitchSpeed = new noiseAdder();
        nPitchAcc = new noiseAdder();
    }

    /// <summary>
    /// Function at regular time interval
    /// </summary>
    void FixedUpdate()
    {
        Vector3 v1 = helixV1.transform.position;
        Vector3 v2 = helixV2.transform.position;
        Vector3 o1 = helixO1.transform.position;
        Vector3 o2 = helixO2.transform.position;

        Vector3 mid_v1o2 = (v1 + o2) / 2f;
        Vector3 mid_v1o1 = (v1 + o1) / 2f;
        Vector3 mid_v2o2 = (o2 + v2) / 2f;
        Vector3 mid_v2o1 = (o1 + v2) / 2f;
         
        // calculate pitch values
        pitch = mid_v2o2.y - mid_v1o1.y;  
        pitchVel = (pitch - lastPitch) / Time.deltaTime;        
        pitchAcc = (pitchVel - lastPitchVel) / Time.deltaTime;
        lastPitch = pitch;
        lastPitchVel = pitchVel;
        pitch = normalize(pitch);

        // calculate roll values
        roll = mid_v2o1.y - mid_v1o2.y;
        rollVel = (roll - lastRoll) / Time.deltaTime;
        rollAcc = (rollVel - lastRollVel) / Time.deltaTime;
        lastRoll = roll;
        lastRollVel = rollVel;
        roll = normalize(roll);


        // --- NOT USED --- //
        // calculate angular acceleration and velocity of the drone     
        //AngularAcceleration = droneSettings.setZeroIflessThan((transform.GetComponent<Rigidbody>().angularVelocity - AngularVelocity) / Time.deltaTime, 0.00001f);
        //AngularVelocity = droneSettings.setZeroIflessThan(transform.GetComponent<Rigidbody>().angularVelocity, 0.00001f);

    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/Magnetometer.cs
````csharp
using UnityEngine;
using System.Collections;

/// <summary>
/// Magnetometer sensor class
/// </summary>
public class Magnetometer : MonoBehaviour {

    // the north pole is the fixed point used by the magnetometer to measure the rotation
    private static Vector2 northPolePosition = new Vector2(0,40000);
    public Vector3 getNorthPolePosition() { return northPolePosition; }

    private rotor helixV1;
    private rotor helixV2;
    private rotor helixO1;
    private rotor helixO2;
     
    noiseAdder nYaw;
    noiseAdder nSpeed;
     
    private float yaw;
    /// <summary>
    /// Gets the actual Yaw of the drone
    /// </summary>
    public float getYaw() { return nYaw.getNoise(yaw); }
    private float lastYaw;
    private float yawVel;
    /// <summary>
    /// Gets the actual Yaw of the drone
    /// </summary>
    public float getYawVel() { return nSpeed.getNoise(yawVel); }

    /// <summary>
    /// Trasforms world coordinates to local coordinates, using as rotation reference the lookingAtPoint
    /// </summary>
    /// <param name="worldPt">Global coordinates that we want to transform to local </param>
    /// <param name="lookingAtPoint">Point the drone is facing to</param>
    /// <returns>The world coordinates transformed to local</returns>
    public Vector3 worldToLocalPoint(Vector3 worldPt, Vector3 lookingAtPoint)
    {
        Vector3 forwardVector = new Vector3(0, 0, Vector3.Distance(transform.position, worldPt));
        return Quaternion.AngleAxis((getYawToCenterOn(lookingAtPoint) - getYaw()) * -180, transform.up) * forwardVector;        
    }

    /// <summary>
    /// Function Called when the object is activated for the first time
    /// </summary>
    void Awake()
    {
        // initializes the noiseAdders
        nYaw = new noiseAdder();
        nSpeed = new noiseAdder();

        // look for the rotors in the droneMovementController
        droneMovementController dmc = gameObject.GetComponent<droneMovementController>();
        helixO1 = dmc.helixO1;
        helixO2 = dmc.helixO2;
        helixV1 = dmc.helixV1;
        helixV2 = dmc.helixV2;
        lastYaw = yaw = getYawINT();     
    }

    /// <summary>
    /// Yaw that corresponds to the rotation needed to face the destPoint
    /// </summary>
    /// <param name="destPoint">Point the drone has to face</param>
    /// <returns>Yaw to face the destPoint from the actual position </returns>
    public float getYawToCenterOn(Vector3 destPoint) { return getYawToCenterOn(new Vector2(destPoint.x, destPoint.z)); }
    /// <summary>
    /// Yaw that corresponds to the rotation needed to face the destPoint
    /// </summary>
    /// <param name="destPoint">Point the drone has to face</param>
    /// <returns>Yaw to face the destPoint from the actual position </returns>
    public float getYawToCenterOn(Vector2 destPoint)
    {
        Vector2 myPos = new Vector2(transform.position.x, transform.position.z);
        
        Vector2 direction = destPoint - myPos;
        Vector2 reference = northPolePosition - myPos;
         
        bool isPositive = myPos.x > destPoint.x; //&& destPoint.x < northPolePosition.x || myPos.x > destPoint.x && destPoint.x > northPolePosition.x;
        float alpha = Vector2.Angle(direction, reference);

        return droneSettings.normalizeBetween(alpha, 0, 180f) * (isPositive ? 1 : -1);
    }

    /// <summary>
    /// Private function used to calculate the Yaw of the drone
    /// </summary>
    /// <returns>Yaw of the drone</returns>
    private float getYawINT()
    {
        Vector2 v1 = new Vector2(helixV1.transform.position.x, helixV1.transform.position.z);
        Vector2 v2 = new Vector2(helixV2.transform.position.x, helixV2.transform.position.z);
        Vector2 o1 = new Vector2(helixO1.transform.position.x, helixO1.transform.position.z);
        Vector2 o2 = new Vector2(helixO2.transform.position.x, helixO2.transform.position.z);

        // The line between the midpoints is our direction
        Vector2 direction = ((v1 + o1) / 2f) - ((v2 + o2) / 2f);
        Vector2 reference = northPolePosition - new Vector2(transform.position.x, transform.position.z);

        bool isPositive = Vector2.Distance((v2 + o1) / 2f, northPolePosition) > Vector2.Distance((v1 + o2) / 2f, northPolePosition);
        float alpha = Vector2.Angle(direction, reference);

        return droneSettings.normalizeBetween(alpha, 0,180f) * (isPositive ? 1 : -1);

    }

    /// <summary>
    /// Function at regular time interval
    /// </summary>
    void FixedUpdate()
    {
        yaw = getYawINT();
        float covedredDistance = (yaw - lastYaw);
        covedredDistance = (Mathf.Abs(covedredDistance) < 1 ? covedredDistance : (covedredDistance > 0 ? covedredDistance - 2 : covedredDistance + 2));
        yawVel = covedredDistance / Time.deltaTime;

        lastYaw = yaw;
    }


}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/noiseAdder.cs
````csharp
using UnityEngine;
using System.Collections.Generic;

public class noiseAdder {

    float sigma = 1f;

    LinkedList<float> lastValues;
    const int maxNr = 50;
    int filled = 0;
     
    /// <summary>
    /// constructor of noiseAdder class
    /// </summary>
    public noiseAdder()
    {
        lastValues = new LinkedList<float>();
    }

    /// <summary>
    /// Obtain the noise using the value passed as parameter and the past values
    /// </summary>
    /// <param name="val">Actual value which we need to apply the noise to</param>
    /// <returns></returns>
    public float getNoise(float val) {
        if (filled < maxNr)
        {
            lastValues.AddFirst(val);
            filled++;
        }
        else
        {
            lastValues.AddFirst(val);
            lastValues.RemoveLast();
        }

        // we calculate the average and the sigma, to obtain a normalized value
        float average = 0;
        foreach (float f in lastValues)
            average += f;
        average /= lastValues.Count;
        sigma = (val - average) * (val - average);
        sigma = droneSettings.keepOnRange(sigma, 0, 2f);

        return val + sigma * gaussianFloatBetween1_1() * 0.1f;
    }

    /// <summary>
    /// Return a floating point value, using a normal distribution, in the range [-1,1]
    /// </summary>
    private float gaussianFloatBetween1_1() {
        float f = gaussianFloat();
        f = (!droneSettings.isInsideRange(f, -3, 3) ? 0 : f);
        return droneSettings.normalizeBetween(f, -3, 3) - 0.5f;        
    }

    /// <summary>
    /// Obtain a number using a normal distribution
    /// </summary>
    /// <returns></returns>
    private float gaussianFloat()
    {
        float u, v, S;
        do
        {
            u = 2.0f * Random.Range(0f, 1f) - 1.0f;
            v = 2.0f * Random.Range(0f, 1f) - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/sensors/noiseAddVector3.cs
````csharp
using UnityEngine;
using System.Collections;

public class noiseAddVector3 {

    noiseAdder xN;
    noiseAdder yN;
    noiseAdder zN;
     
    /// <summary>
    /// Constructor of the class
    /// <para>Just create a noiseAdder class for each element of the vector</para>
    /// </summary>
    public noiseAddVector3()
    {
        xN = new noiseAdder();
        yN = new noiseAdder();
        zN = new noiseAdder();      
    }

    /// <summary>
    /// Obtain the noise using the value passed as parameter and the past values
    /// </summary>
    /// <param name="val">Actual Vector3 which we need to apply the noise to</param>
    /// <returns></returns>
    public Vector3 getNoise(Vector3 val) { return new Vector3(xN.getNoise(val.x), yN.getNoise(val.y), zN.getNoise(val.z)); }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/droneMovementController.cs
````csharp
using UnityEngine;
using System.Collections;
using System;


public class droneMovementController : MonoBehaviour {

    #region phisical Parts and related functions

    // Sensors of the drone (have to be associated to the sensors object in the drone model)
    public Gyro gyro;
    public Accelerometer acc;
    public Barometer bar;
    public GPS gps;
    public Magnetometer mag;

    // Rotors of the drone (have to be associated to the four rotors of the drone, with the order V1,O1,V2,O2)
    public rotor helixV1;
    public rotor helixV2;
    public rotor helixO1;
    public rotor helixO2;

    // PIDs of the drone. Instanciated in run-time
    public PID yawPID;
    public PID rollPID;
    public PID pitchPID;
    public PID yPID;
    public PID zPID;
    public PID xPID;

    /// <summary>
    /// Used to simulate the torque applied to the drone, following the differences between the rotors power
    /// </summary>
    /// <param name="amount">sum of the results of <c>verse(Rotor r)</c>, applied over every rotor</param>
    void applyTorque(float amount) { transform.Rotate(transform.up, amount * Time.deltaTime); }

    /// <summary>
    /// Calculates the amount of torque that a single rotor is generating over the entire system.
    /// <para>The sum of the results of this funcion, applied to the four rotors have to be passed as 
    /// parameter to the function <c>applyTorque(float amount)</c></para>
    /// </summary>
    /// <param name="r">Rotor class</param>
    /// <returns>The amount of torque that a single rotor is generating over the entire system</returns>
    float torqueGeneratedBy(rotor r) { return (r.counterclockwise ? -1 : 1) * denormalizeTorque(r.getPower()) * 10; }

    /// <summary>
    /// Transform the power calculated by the algorithms (that is always between 0 and 1) so it can be used by the rotors class
    /// </summary>
    /// <param name="pow">Power of the rotor, calculated by the algorithms</param>
    /// <returns>A value between [saturationValues.minRotationSpeed, saturationValues.maxRotationSpeed] </returns>
    float denormalizePower(float pow) { return denormalize(pow, droneSettings.saturationValues.minRotationSpeed, droneSettings.saturationValues.maxRotationSpeed); }

    /// <summary>
    /// Transform the power calculated by the algorithms (that is always between 0 and 1) so it can be used to calculate the overall torque
    /// </summary>
    /// <param name="pow">Power of the rotor, calculated by the algorithms</param>
    /// <returns>A value between [saturationValues.minTorque, saturationValues.maxTorque] </returns>
    float denormalizeTorque(float pow) { return denormalize(pow, droneSettings.saturationValues.minTorque, droneSettings.saturationValues.maxTorque); }

    /// <summary>
    /// Generic function used to denormalize
    /// </summary>
    /// <param name="pow">Number to denormalize that belongs to the interval [0,1]</param>
    /// <param name="lBound">Lower bound of the denormalized number</param>
    /// <param name="uBound">Upper bound of the denormalized number</param>
    /// <returns>The number passed as argument, denormalized in the interval [lBound,uBound]</returns>
    float denormalize(float pow, float lBound, float uBound) { return pow * (uBound - lBound) + lBound; }

    /// <summary>
    /// Keep a number in the interval [0,1], truncating it if it is outside that range
    /// </summary>
    /// <param name="num">Number that has to be maintained in the interval [0,1]</param>
    /// <returns>if (num € [0,1]) -> num; else if (num is less than 0) -> 0; else -> 1</returns>
    float keepOnRange01(float num) { return (float.IsNaN(num) ? 0 : droneSettings.keepOnRange(num, 0f, 1f)); }
    #endregion

    #region targets 

    // Variables that represent the ideal-State of the drone. They are used to calculate the error
    public float idealPitch = 0;
    public float idealRoll = 0;
    public float idealYaw = 0;
    public float targetX = 0;
    public float targetY = 10;
    public float targetZ = 0;

    // Point used to calculate the local Z position of the drone
    public Transform target;
    // Point used to calculate the local X position of the drone
    private Vector3 routePosition;
    /// <summary>
    /// Sets the routePosition, used by the spatial-stabilization algorithm to move
    /// </summary>
    /// <param name="v">Position in the route</param>
    public void setRoutePos(Vector3 v) { routePosition = v; }

    // Point that the drone has to look at. Determine the orientation
    private Vector3 lookingAtPoint;
    /// <summary>
    /// Sets the lookingPoint, used by the Yaw-stabilization algorithm
    /// </summary>
    /// <param name="v">Point the drone has to look at</param>
    public void setLookingPoint(Vector3 v) { lookingAtPoint = v; }
    
    // Indicates if the drone has to stabilize itself to the routePosition or can keep following the target
    public bool stayOnFixedPoint = false;    
    public void followTarget(bool b) { stayOnFixedPoint = b; }

    #endregion

    #region internal inputs
    // This part permits to the optimizations algorithms to modify directly the settings of the drone    

    // if this value is TRUE, the drone is using these constants instead of the ones saved in the settings
    bool testing = false;
    float constVertVel, constVertAcc;
    float constAxisVel, constAxisAcc;
    float constYawVel;
    float constHorizVel, constHorizAcc;
    /// <summary>
    /// Sets the constants used in the stabilization algorithms
    /// <para>This function is used ONLY by the optimizations algorithm (Genetic and twiddle)</para>
    /// </summary>
    public void setConsts(float vVel, float vAcc, float aVel, float aAcc, float yVel, float orVel, float orAcc)
    {
        testing = true;
        constVertVel = vVel;
        constVertAcc = vAcc;
        constAxisVel = aVel;
        constAxisAcc = aAcc;
        constYawVel = yVel;
        constHorizVel = orVel;
        constHorizAcc = orAcc;
    }
    /// <summary>
    /// Sets the PIDs of the drone
    /// <para>This function is used ONLY by the optimizations algorithm (Genetic and twiddle)</para>
    /// </summary>
    public void setKs(PID yPID, PID zPID, PID xPID, PID pitchPID, PID rollPID, PID yawPID)
    {
        //testing = true;
        this.xPID = xPID;
        this.zPID = zPID;
        this.yPID = yPID;
        this.pitchPID = pitchPID;
        this.rollPID = rollPID;
        this.yawPID = yawPID;
    }
    #endregion

    #region outputs to the rotors

    // variables where is stored, in a range [0,1], the power of each rotor
    public float pV1;
    public float pV2;
    public float pO1;
    public float pO2;

    /// <summary>
    /// Modify the power of all 4 rotors, in order to modify the height of the drone
    /// </summary>
    /// <param name="intensity">Magnitute of the modification</param>
    private void modifyAllRotorsRotation(float intensity)
    {
        pV1 += intensity;
        pV2 += intensity;
        pO1 += intensity;
        pO2 += intensity;
    }
  
    /// <summary>
    /// Modify the power of the rotors, in order to modify the roll of the drone
    /// </summary>
    /// <param name="intensity">Magnitute of the modification</param>
    private void modifyRollRotorsRotation(float intensity)
    {
        pV1 += intensity; pV2 -= intensity;
        pO2 += intensity; pO1 -= intensity;
    }

    /// <summary>
    /// Modify the power of the rotors, in order to modify the pitch of the drone
    /// </summary>
    /// <param name="intensity">Magnitute of the modification</param>
    private void modifyPitchRotorsRotation(float intensity)
    {
        pV1 += intensity; pV2 -= intensity;
        pO1 += intensity; pO2 -= intensity;
    }

    /// <summary>
    /// Modify the power of the rotors, in order to modify the yaw of the drone
    /// </summary>
    /// <param name="intensity">Magnitute of the modification</param>
    private void modifyPairsRotorsRotation(float intensity)
    {
        pV1 += intensity;
        pV2 += intensity;
        pO1 -= intensity;
        pO2 -= intensity;
    }

    #endregion

    #region Stabilizations

    /// <summary>
    /// Vertical Stabilization algorithm
    /// </summary>
    /// <param name="targetAltitude">Altitude that we want to reach. It'll be compared with the actual to extract the error</param>
    void yStabilization(float targetAltitude)
    {
        //calculates the error and extracts the measurements from the sensors
        float distanceToPoint = (targetAltitude - bar.getHeight());

        // adding the value to the test class
        //tHeight.addValue(distanceToPoint);
        float acc = bar.getverticalAcc();
        float vel = bar.getverticalSpeed();

        //calculates the idealVelocity, we'll use this to extract an error that will be given to the PID
        float idealVel = distanceToPoint * (testing ? constVertVel : droneSettings.constVerticalIdealVelocity);
        idealVel = droneSettings.keepOnRange(idealVel, droneSettings.saturationValues.minVerticalVel, droneSettings.saturationValues.maxVerticalVel);

        //calculates the idealAcc, we'll use this to extract an error that will be given to the PID
        float idealAcc = (idealVel - vel) * (testing ? constVertAcc : droneSettings.constVerticalIdealAcceler);
        idealAcc = droneSettings.keepOnRange(idealAcc, droneSettings.saturationValues.minVerticalAcc, droneSettings.saturationValues.maxVerticalAcc);

        //Error used by the PID
        float Err = (idealAcc - acc);

        //If this is TRUE we are near the point and with a low velocity. It is not necessary to modify the Power
        if (Mathf.Abs(vel) + Mathf.Abs(distanceToPoint) > 0.005f)
            //modifying the rotors rotation, using the output of the PID
            modifyAllRotorsRotation(yPID.getU(Err, Time.deltaTime));        
    }

    /// <summary>
    /// Roll Stabilization algorithm
    /// </summary>
    /// <param name="idealRoll">Roll value that we want to reach. It'll be compared with the actual to extract the error</param>
    void rollStabilization(float idealRoll)
    {
        //calculates the error and extracts the measurements from the sensors
        float rollDistance = idealRoll - this.gyro.getRoll();
        float acc = this.gyro.getRollAcc();
        float vel = this.gyro.getRollVel();

        //calculates idealVelocity and idealAcceleration, we'll use this to extract an error that will be given to the PID
        float idealVel = rollDistance * (testing ? constHorizVel : droneSettings.constHorizontalIdealVelocity);
        float idealAcc = (idealVel - vel) * (testing ? constHorizAcc : droneSettings.constHorizontalIdealAcceler);

        //Error used by the PID
        float Err = (idealAcc - acc);

        //modifying the rotors rotation, using the output of the PID
        modifyRollRotorsRotation(rollPID.getU(-Err, Time.deltaTime));
    }

    /// <summary>
    /// Pitch Stabilization algorithm
    /// </summary>
    /// <param name="idealPitch">Pitch value that we want to reach. It'll be compared with the actual to extract the error</param>
    void pitchStabilization(float idealPitch)
    {
        //calculates the error and extracts the measurements from the sensors
        float pitchDistance = idealPitch - this.gyro.getPitch();
        float acc = this.gyro.getPitchAcc();
        float vel = this.gyro.getPitchVel();

        //calculates idealVelocity and idealAcceleration, we'll use this to extract an error that will be given to the PID
        float idealVel = pitchDistance * (testing ? constHorizVel : droneSettings.constHorizontalIdealVelocity);
        float idealAcc = (idealVel - vel) * (testing ? constHorizAcc : droneSettings.constHorizontalIdealAcceler);

        //Error used by the PID
        float Err = (idealAcc - acc);

        //modifying the rotors rotation, using the output of the PID
        modifyPitchRotorsRotation(pitchPID.getU(-Err, Time.deltaTime));
    }

    /// <summary>
    /// Yaw Stabilization algorithm
    /// </summary>
    /// <param name="idealYaw">Yaw value that we want to reach. It'll be compared with the actual to extract the error</param>
    /// <returns>The absolute value of the error, used to decrease the effect of the others stabilization algorithms</returns>
    float yawStabilization(float idealYaw)
    {
        //calculates the error and extracts the measurements from the sensors
        float yawDistance = mag.getYaw() - idealYaw;
        yawDistance = (Mathf.Abs(yawDistance) < 1 ? yawDistance : (yawDistance > 0 ? yawDistance - 2 : yawDistance + 2));

        //calculates idealVelocity, we'll use this to extract an error that will be given to the PID
        float vel = mag.getYawVel();
        float idealVel = -yawDistance * (testing ? constYawVel : droneSettings.constYawIdealVelocity);

        //Error used by the PID
        float Err = (idealVel - vel);
        Err *= Mathf.Abs(yawDistance) * (Mathf.Abs(yawDistance) > 0.3f ? -10 : -50);

        //modifying the rotors rotation, using the output of the PID
        float res = yawPID.getU(Err, Time.deltaTime);
        modifyPairsRotorsRotation(res);

        return Math.Abs(idealYaw - mag.getYaw());
    }

    /// <summary>
    /// Z Stabilization algorithm
    /// </summary>
    /// <param name="targetZ">Z value that we want to reach. It'll be compared with the actual to extract the error</param>
    /// <returns>Returns an error that has to be given to the PITCH_stabilization function</returns>
    float zStabilization(float targetZ)
    {
        //calculates the error and extracts the measurements from the sensors 
        float distanceToPoint = droneSettings.keepOnAbsRange(targetZ, 30f);
        float acc = this.acc.getLinearAcceleration().z;
        float vel = this.acc.getLocalLinearVelocity().z;
        float yawVel = this.mag.getYawVel();

        //calculates idealVelocity and idealAcceleration, we'll use this to extract an error that will be given to the PID
        float idealVel = distanceToPoint * (testing ? constAxisVel : droneSettings.constAxisIdealVelocity);
        idealVel = droneSettings.keepOnAbsRange(idealVel, droneSettings.saturationValues.maxHorizontalVel);
        float idealAcc = (idealVel - vel) * (testing ? constAxisAcc : droneSettings.constAxisIdealAcceler);
        idealAcc = droneSettings.keepOnAbsRange(idealAcc, 3f);

        //Error used by the PID
        float Err = (idealAcc - acc);
        Err *= 1 - keepOnRange01(Math.Abs(idealYaw - mag.getYaw()));

        //dS.addLine(new float[] { Err, distanceToPoint, vel, idealVel, acc, idealAcc  });      // use this to save the data to the DataSaver class
        return zPID.getU(Err, Time.deltaTime);                
    }

    /// <summary>
    /// X Stabilization algorithm
    /// </summary>
    /// <param name="targetX">X value that we want to reach. It'll be compared with the actual to extract the error</param>
    /// <returns>Returns an error that has to be given to the ROLL_stabilization function</returns>
    float xStabilization(float targetX)
    {
        //calculates the error and extracts the measurements from the sensors
        float distanceToPoint = droneSettings.keepOnAbsRange(targetX, 30f);
        float acc = this.acc.getLinearAcceleration().x;
        float vel = this.acc.getLocalLinearVelocity().x;

        //calculates idealVelocity and idealAcceleration, we'll use this to extract an error that will be given to the PID
        float idealVel = distanceToPoint * (testing ? constAxisVel : droneSettings.constAxisIdealVelocity);
        idealVel = droneSettings.keepOnAbsRange(idealVel, droneSettings.saturationValues.maxHorizontalVel);
        float idealAcc = (idealVel - vel) * (testing ? constAxisAcc : droneSettings.constAxisIdealAcceler);
        idealAcc = droneSettings.keepOnAbsRange(idealAcc, 3f);

        //Error used by the PID
        float Err = (idealAcc - acc);
        Err *= 1 - keepOnRange01(Math.Abs(idealYaw - mag.getYaw()));

        return xPID.getU(Err, Time.deltaTime);
    }

    #endregion

    // classes used to print lines (direction vectors for example). Used for debugging
    lineDrawer linedrawer;
    int ticket1;
    int ticket2;
    int ticket3;
    int ticket4;

    // classes used to save the stats of the drone. Used for debugging
    dataSaver dS;
    dataSaver dSOut;

    //Test tHeight;

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start()
    {
        // initialize the DataSaver class in this way
        //dSOut = new dataSaver("outputData", new string[] { "pOut", "iOut", "dOut", "u" });
        //dS = new dataSaver("zData", new string[] {"Err", "distance", "vel", "idealVel","acc", "idealAcc" });
        //dS = new dataSaver("yawData", new string[] { "Err", "Yaw", "YawVel", "sum", "yawModifier", "result"});

        //tHeight = new Test("Height test", 1, 20);

        // if one of these scripts are enabled, they'll think about the initialization of the PIDs
        if (gameObject.GetComponent<geneticBehaviour>().enabled == false &&  
            gameObject.GetComponent<twiddleBehaviour>().enabled == false &&
            gameObject.GetComponent<configReader>().enabled == false)
        { 
            // if not, we get the values from the settings
            yPID = new PID(droneSettings.verticalPID_P, droneSettings.verticalPID_I, droneSettings.verticalPID_D, droneSettings.verticalPID_U);
            yawPID = new PID(droneSettings.yawPID_P, droneSettings.yawPID_I, droneSettings.yawPID_D, droneSettings.yawPID_U);
            rollPID = new PID(droneSettings.orizPID_P, droneSettings.orizPID_I, droneSettings.orizPID_D, droneSettings.orizPID_U);
            pitchPID = new PID(droneSettings.orizPID_P, droneSettings.orizPID_I, droneSettings.orizPID_D, droneSettings.orizPID_U);
            zPID = new PID(droneSettings.axisPID_P, droneSettings.axisPID_I, droneSettings.axisPID_D, droneSettings.axisPID_U);
            xPID = new PID(droneSettings.axisPID_P, droneSettings.axisPID_I, droneSettings.axisPID_D, droneSettings.axisPID_U);
        }

        linedrawer = gameObject.GetComponent<lineDrawer>();
        ticket1 = linedrawer.getTicket();
        ticket2 = linedrawer.getTicket();
        ticket3 = linedrawer.getTicket();
        ticket4 = linedrawer.getTicket();
    }

    
    public bool save = false;
    /// <summary>
    /// Function called each frame
    /// </summary>
    void Update()
    {             
        if (save) { save = false; dS.saveOnFile(); }
    }

    float startAfter = 0.1f;
    /// <summary>
    /// Function at regular time interval
    /// </summary>
    void FixedUpdate()
    {
        // wait 0.1 sec to avoid inizialization problem
        if ((startAfter -= Time.deltaTime) > 0) return;
             
        if (stayOnFixedPoint)            
        {
            Vector3 p = mag.worldToLocalPoint(routePosition, target.position);
            targetZ = p.z;
            targetX = p.x;
            targetY = routePosition.y;
        }
        else
        {          
            targetZ = mag.worldToLocalPoint(target.position, lookingAtPoint).z;
            targetX = mag.worldToLocalPoint(routePosition, lookingAtPoint).x;
            targetY = (routePosition.y + target.position.y) / 2f;
        }

        
        Vector3 thrustVector = Quaternion.AngleAxis(-45, Vector3.up) *  new Vector3(targetX, targetY - transform.position.y, targetZ);
        Vector3 xComponent = Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(targetX, 0,0);
        Vector3 zComponent = Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(0, 0, targetZ);

        // drawing the direction vectors, for debugging
        linedrawer.drawPosition(ticket3, thrustVector);
        linedrawer.drawPosition(ticket1, xComponent);
        linedrawer.drawPosition(ticket2, zComponent);

        // calling the stabilization algorithms that will modify the rotation power
        idealPitch = droneSettings.keepOnAbsRange(zStabilization(targetZ), 0.40f);
        idealRoll = droneSettings.keepOnAbsRange(xStabilization(targetX), 0.40f);
        idealYaw = mag.getYawToCenterOn(lookingAtPoint);
        yStabilization(targetY);
        pitchStabilization(idealPitch);
        rollStabilization(idealRoll);
        float yawErr = yawStabilization(idealYaw);

        // if the drone has to rotate more than 0.22 (~40°) it stabilizes itself to a fixed point to avoid getting off the route and to increase stability
        followTarget(yawErr < 0.22f);

        // truncate and applies the power to the rotors
        pV1 = keepOnRange01(pV1);
        pV2 = keepOnRange01(pV2);
        pO1 = keepOnRange01(pO1);
        pO2 = keepOnRange01(pO2);
        helixV1.setPower(denormalizePower(pV1));
        helixV2.setPower(denormalizePower(pV2));
        helixO1.setPower(denormalizePower(pO1));
        helixO2.setPower(denormalizePower(pO2));

        // Calculate the torque generated by each rotor and applies it to the drone
        applyTorque(torqueGeneratedBy(helixV1) + torqueGeneratedBy(helixV2) + torqueGeneratedBy(helixO1) + torqueGeneratedBy(helixO2));
    }


}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/droneSettings.cs
````csharp
using UnityEngine;
using System.Collections;

public class droneSettings {
    /// <summary>
    /// Values that represent the limits.
    /// <para>It could be because a drone in the physic world wouldn't exceede them
    /// or because go beyond of them means introduce instability</para> 
    /// </summary>
    public static class saturationValues {
        public const float minRotationSpeed = 0.2f;
        public const float maxRotationSpeed = 3f;

        public const float minTorque = 0.2f;
        public const float maxTorque = 3f;

        public const float minVerticalVel = -8f;
        public const float maxVerticalVel = 15f;
        public const float minVerticalAcc = -6f;
        public const float maxVerticalAcc = 3f;

        public const float maxHorizontalVel = 5f;
    }

    /// <summary>
    /// Constants used by the vertical stabilization algorithm
    /// <para>There are the PID constants (P, I, D, U) 
    /// and the constants used to transform the error given to the PID class</para>
    /// </summary>
    #region Vertical_Stabilization

    public static float constVerticalIdealVelocity = 0.385772f;
    public static float constVerticalIdealAcceler = 0.6716582f;

    public const float verticalPID_P = 0.7635331f;
    public const float verticalPID_I = 0.001476288f;
    public const float verticalPID_D = 0.0001088255f;
    public const float verticalPID_U = 0.1f;

    #endregion

    /// <summary>
    /// Constants used by the horizontal stabilization algorithms (X and Z stabilization)
    /// <para>There are the PID constants (P, I, D, U) 
    /// and the constants used to transform the error given to the PID class</para>
    /// </summary>
    #region Axis_Stabilization

    public static float constAxisIdealVelocity = 0.482393f;
    public static float constAxisIdealAcceler = 0.9510251f;

    public const float axisPID_P = 0.2242663f;
    public const float axisPID_I = 6.129676E-05f;
    public const float axisPID_D = 0.007565225f;
    public const float axisPID_U = 0.5f;

    #endregion

    /// <summary>
    /// Constants used by the orientation stabilization algorithms (Yaw stabilization)
    /// <para>There are the PID constants (P, I, D, U) 
    /// and the constants used to transform the error given to the PID class</para>
    /// </summary>
    #region Yaw_Stabilization

    public static float constYawIdealVelocity = 0.5534329f;

    public const float yawPID_P = 0.07649516f;
    public const float yawPID_I = 2.469936E-05f;
    public const float yawPID_D = 0.002099928f;
    public const float yawPID_U = 0.2f;

    #endregion

    /// <summary>
    /// Constant used by the horizontal rotation stabilization algorithms (Roll and Pitch stabilization)
    /// <para>There are the PID constants (P, I, D, U) 
    /// and the constants used to transform the error given to the PID class</para>
    /// </summary>
    #region Horizontal_Stabilization

    public static float constHorizontalIdealVelocity = 0.9380789f;
    public static float constHorizontalIdealAcceler = 0.9398623f;

    public const float orizPID_P = 0.05998019f;
    public const float orizPID_I = 5.116195E-05f;
    public const float orizPID_D = 0.002372454f;
    public const float orizPID_U = 0.3f;

    #endregion


    #region Common_Functions

    /// <summary>
    /// Normalize a number between an upper and a lower bound
    /// </summary>
    /// <param name="n">Number to normalize</param>
    /// <param name="lbound">Lower bound taken in consideration</param>
    /// <param name="ubound">Upper bound taken in consideration</param>
    /// <returns>A number normalized into the given interval</returns>
    public static float normalizeBetween(float n, float lbound, float ubound)
    {
        return (n - lbound) / (ubound - lbound);
    }

    /// <summary>
    /// If the number is inside the bounds it just returns it. If it is outside it return the nearest bound
    /// </summary>
    /// <param name="num">Number that we want to keep inside the range</param>
    /// <param name="lbound">Lower bound taken in consideration</param>
    /// <param name="ubound">Upper bound taken in consideration</param>
    /// <returns>if (num € [lbound,ubound]) -> num, else if (num is less than lbound) -> lbound, else -> ubound </returns>
    public static float keepOnRange(float num, float lBound, float uBound) { return (num > uBound ? uBound : (num < lBound ? lBound : num)); }
    /// <summary>
    /// If the number is inside the absolute bound it just returns it. If it is outside it return the nearest bound
    /// </summary>
    /// <param name="num">Number that we want to keep inside the range</param>
    /// <param name="bound">Absolute bound taken in consideration</param>
    /// <returns>if (num € [-bound,bound]) -> num, else if (num is less than -bound) -> -bound, else -> bound </returns>
    public static float keepOnAbsRange(float num, float bound) { return keepOnRange(num, -bound, bound); }
    /// <summary>
    /// Says if the number is inside the interval specified by the bounds
    /// </summary>
    /// <param name="num">Number that we want to check</param>
    /// <param name="lbound">Lower bound taken in consideration</param>
    /// <param name="ubound">Upper bound taken in consideration</param>
    /// <returns>if (num € [-bound,bound]) -> TRUE, else -> FALSE</returns>
    public static bool isInsideRange(float num, float lBound, float uBound) { return (num >= lBound && num <= uBound); }

    /// <summary>
    /// Sets a number to 0 if it is less than a bound. If not just returns the original number
    /// </summary>
    /// <param name="num">Number that we want to keep greather than 'bound'</param>
    /// <param name="bound">Lower bound taken in consideration</param>
    /// <returns>if (num is greater than bound) -> num, else -> 0s </returns>
    public static float setZeroIflessThan(float num, float bound) { return (Mathf.Abs(num) > bound ? num : 0); }
    
    /// <summary>
    /// Sets the elements of a Vector3 to 0 if they are less than a specified bound
    /// </summary>
    /// <param name="v">Vector3, which elements we want to keep greather than 'bound'</param>
    /// <param name="bound">Lower bound taken in consideration</param>
    /// <returns>foreach(number n in the Vector3) if (n is greater than bound) -> num, else -> 0s </returns>
    public static Vector3 setZeroIflessThan(Vector3 v, float bound) { return new Vector3(setZeroIflessThan(v.x, bound), setZeroIflessThan(v.y, bound), setZeroIflessThan(v.z, bound)); }

    #endregion
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/PID.cs
````csharp
using UnityEngine;
using System.Collections;

public class PID
{
    // ID. Used just for debugging
    static int id = 0;
    int myId = 0;

    // PID constants. Set them in the constructor
    float kP;
    float kI;
    float kD;
    float kU;

    float lastErr = 0;
    float totErr = 0;
    float absError = 0;
    /// <summary>
    /// Gets the overall error accumulated by the PID
    /// </summary>
    /// <returns>Total error accumulated by the PID</returns>
    public float getTotError() { return absError; }

    /// <summary>
    /// Constructor of the PID class
    /// </summary>
    /// <param name="kP">Proporcional constant</param>
    /// <param name="kI">Integrative constant</param>
    /// <param name="kD">Derivative constant</param>
    /// <param name="kU">Constant multiplied for the output error</param>
    public PID(float kP, float kI, float kD, float kU)
    {
        myId = id++;
        this.kP = kP;
        this.kI = kI;
        this.kD = kD;
        this.kU = kU;

        trashBuffer = new float[4];
    }

    float[] trashBuffer;

    /// <summary>
    /// Gets the PID output
    /// </summary>
    /// <param name="Err">Actual error measured by the sensors</param>
    /// <param name="deltaTime">Time from the last request</param>
    /// <returns>The output of the PID, multiplied for the constant U</returns>
    public float getU(float Err, float deltaTime) { return getU(Err, deltaTime, trashBuffer); }

    float SampleTime = 0.05f;
    float accDTime = 0;
    float lastU = 0;
    /// <summary>
    /// Gets the PID output
    /// </summary>
    /// <param name="Err">Actual error measured by the sensors</param>
    /// <param name="deltaTime">Time from the last request</param>
    /// <param name="values">Array used to store the results of the PID. Used for debugging</param>
    /// <returns>The output of the PID, multiplied for the constant U</returns>
    public float getU(float Err, float deltaTime, float[] values)
    {
        // if not enough time is passed since the last call it returns the old result 
        if (deltaTime + accDTime > SampleTime)
        {
            accDTime = deltaTime + accDTime - SampleTime;

            //calculating P factor
            float pFactor = Err;
            //calculating D factor
            float dFactor = (Err - lastErr);
            lastErr = Err;
            totErr += Err;
            //calculating I factor
            float iFactor = totErr;
            absError += Mathf.Abs(Err);

            // calculating the output of the PID
            float u = (kP * pFactor + kD / SampleTime * dFactor + kI * SampleTime * iFactor) * kU;
            
            //storing the results in the array passed as parameter
            values[0] = kP * pFactor;
            values[1] = kI * iFactor;
            values[2] = kD * dFactor;
            values[3] = kU * u;

            lastU = u;
            return u;
        }
        else
        {
            //just increases the timer and returns the last result calculated
            accDTime += deltaTime;
            return lastU;
        }
    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level0_Stabilizing/rotor.cs
````csharp
using UnityEngine;
using System.Collections;

public class rotor : MonoBehaviour {

    Rigidbody rBody;
    float power;
    
    /// <summary>
    /// Specify the verse of the rotation
    /// <para> Set this in the editor
    /// </summary>
    public bool counterclockwise = false;
    
    /// <summary>
    /// Specify is the rotors are animated or static. Just a visual effect
    /// <para> Set this in the editor
    /// </summary>
    public bool animationActivated = false;

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start()
    {        
        Transform t = this.transform;
        while (t.parent != null && t.tag != "Player") t = t.parent;
        rBody = t.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Sets the rotating power of the rotor
    /// </summary>
    /// <param name="intensity"> The rotating power of the rotor </param>
    public void setPower(float intensity) { power = intensity; }

    /// <summary>
    /// Gets the rotating power of the rotor
    /// </summary>
    /// <returns>the actual rotating power of the rotor</returns>
    public float getPower() { return power; }
    
    /// <summary>
    /// Function called once per frame
    /// </summary>
    void Update() { if (animationActivated) transform.Rotate(0, 0, power * 700 * Time.deltaTime * (counterclockwise ? -1 : 1)); }

    /// <summary>
    /// Function at regular time interval
    /// </summary>
    void FixedUpdate()
    {        
        rBody.AddForceAtPosition(transform.forward * power, transform.position);
        //lr.SetPosition(1, new Vector3(0, 0, power / 3f));
    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level1_Trajectories/singlePoint.cs
````csharp
using UnityEngine;
using System.Collections;
using System;

public class singlePoint : Waypoint {

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start() { setReferencePoint(transform.position); }

    // -- single point view vars
    private float rotationVelocity = 0.8f;
    public float distance = 8;
    private float angle = 0;
    public Transform viewPoint;
    public bool moveBackToHomeDuringIdle = true;

    /// <summary>
    /// Function called each frame. It is used to give commands to the SinglePoint
    /// </summary>
    void Update() {
        //setReferencePoint(transform.position);

        if (!acceptCommands) return;

        Vector3 v = new Vector3();
        if (Input.GetKey(KeyCode.UpArrow)) v = transform.forward;
        else if (Input.GetKey(KeyCode.DownArrow)) v = transform.forward * -1;
        if (Input.GetKey(KeyCode.RightArrow)) v = transform.right;
        else if (Input.GetKey(KeyCode.LeftArrow)) v = transform.right * -1;
        if (Input.GetKey(KeyCode.W)) v = Vector3.up;
        else if (Input.GetKey(KeyCode.S)) v = Vector3.up * -1;
        if (Input.GetKey(KeyCode.A)) rotate(1);
        else if (Input.GetKey(KeyCode.D)) rotate(-1);

        // if a command is given it moves the point
        if (!v.Equals(new Vector3()))
            moveSinglePoint(v);
        // else it stays OR goes back to the home (the drone)
        else if (moveBackToHomeDuringIdle && home != null)
            setReferencePoint(Vector3.MoveTowards(transform.position, home.position, pointMovingStep / 4f * Time.deltaTime));
    }

    /// <summary>
    /// Rotates the SinglePoint, so the orientation of the drone will change too
    /// </summary>
    /// <param name="verse">Verse of the rotation</param>
    public void rotate(int verse) { angle += rotationVelocity * (verse == 0 ? 0 : Mathf.Sign(verse)) * Time.deltaTime; }


    public bool acceptCommands = false;
    public float pointMovingStep = 5f;

    /// <summary>
    /// Move the singlePoint using a direction vector
    /// </summary>
    /// <param name="v">Directional vector that indicates the direction and distance of movement</param>
    public void moveSinglePoint(Vector3 v)
    {
        transform.position += v * pointMovingStep * Time.deltaTime;
        setReferencePoint(transform.position);
    }

    private Vector3 referencePoint;
    private Transform home;
    /// <summary>
    /// Sets the variable referencePoint and moves the SinglePoint on it
    /// </summary>
    /// <param name="pt">New position of referencePoint</param>
    public void setReferencePoint(Vector3 pt) { referencePoint = pt; transform.position = pt; }

    /// <summary>
    /// Sets the transform that will be used as home.
    /// <para>The home is the point where the drone will go if it didn't recibe any command</para>
    /// </summary>
    /// <param name="t">Transform used as home (the drone)</param>
    public void setHome(Transform t) { home = t; }

    /// <summary>
    /// Execute a calculation and find the point the drone has to look at
    /// </summary>
    /// <returns>The point the drone has to look at</returns>
    public Vector3 getLookingAtPoint()
    {        
        viewPoint.transform.position = new Vector3(Mathf.Cos(angle) * distance + transform.position.x, transform.position.y, Mathf.Sin(angle) * distance + transform.position.z);
        transform.LookAt(viewPoint.transform.position);
        return viewPoint.transform.position;
    }


    // --- Methods of the abstract class

    /// <summary>
    /// Given a point, it returns the distance from the beginning of the nearest point in the circuit to this point
    /// </summary>
    /// <param name="position">Point we use to look for the nearest point in the circuit</param>
    /// <returns>It is a single point so it'll always return 0</returns>
    public override float getNearestPointTo(Vector3 position) { return 0; }

    /// <summary>
    /// Given an absolute distance from the first point, it returns a RoutePoint at that distance
    /// </summary>
    /// <param name="distance">Float containing the distance from the first point</param>
    /// <returns>It is a single point, so it'll return a RoutePoint containing the SinglePoint position and a fake direction </returns>
    public override RoutePoint GetRoutePoint(float v) { return new RoutePoint(referencePoint, new Vector3()); }

    /// <summary>
    /// Given an absolute distance from the first point, it returns a point in the circuit at that distance
    /// </summary>
    /// <param name="progressDistance">Float containing the distance from the first point</param>
    /// <returns>It is a single point, so it'll return the SinglePoint position</returns>
    public override Vector3 GetRoutePosition(float progressDistance) { return referencePoint; }

    /// <summary>
    /// Specify is the waypoint is a WaypointCircuit or a CinglePoint
    /// </summary>
    /// <returns>Returns always FALSE</returns>
    public override bool isCircuit() { return false; }

    /// <summary>
    /// Given two points that can be inside or outside the circuit, the function returns a number 'nPoints' of points
    /// in the circuit between these points
    /// </summary>
    /// <param name="pt1">First point</param>
    /// <param name="pt2">Second point</param>
    /// <param name="nOfPoints">Number of points needed to extract</param>
    /// <returns>It is a single point so it'll always return an empty array of points </returns>
    public override Vector3[] pointsBetween(Vector3 vector3, Vector3 position, int nOfPoints) { return new Vector3[0]; }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level1_Trajectories/Waypoint.cs
````csharp
using UnityEngine;
using System.Collections;

public abstract class Waypoint : MonoBehaviour {

    /// <summary>
    /// Structure that save the position and direction of the target
    /// </summary>
    public struct RoutePoint
    {
        public Vector3 position;
        public Vector3 direction;


        public RoutePoint(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }

    /// <summary>
    /// Specify is the waypoint is a WaypointCircuit or a CinglePoint
    /// </summary>
    /// <returns>True if it is a WaypointCircuit, otherwise false</returns>
    public abstract bool isCircuit();

    /// <summary>
    /// Given two points that can be inside or outside the circuit, the function returns a number 'nPoints' of points
    /// in the circuit between these points
    /// </summary>
    /// <param name="pt1">First point</param>
    /// <param name="pt2">Second point</param>
    /// <param name="nOfPoints">Number of points needed to extract</param>
    /// <returns>Returns an array of points containing all the points IN the circuit between the points passed as parameters </returns>
    public abstract Vector3[] pointsBetween(Vector3 pt1, Vector3 pt2, int nOfPoints);

    /// <summary>
    /// Given a point, it returns the distance from the beginning of the nearest point in the circuit to this point
    /// </summary>
    /// <param name="position">Point we use to look for the nearest point in the circuit</param>
    /// <returns>The nearest point to the one passed as parameter, in the circuit</returns>
    public abstract float getNearestPointTo(Vector3 position);

    /// <summary>
    /// Given an absolute distance from the first point, it returns a RoutePoint at that distance
    /// </summary>
    /// <param name="distance">Float containing the distance from the first point</param>
    /// <returns>A routePoint containing the position and the direction of the resulting point </returns>
    public abstract RoutePoint GetRoutePoint(float distance);

    /// <summary>
    /// Given an absolute distance from the first point, it returns a point in the circuit at that distance
    /// </summary>
    /// <param name="progressDistance">Float containing the distance from the first point</param>
    /// <returns>returns a point in the circuit at the specified distance</returns>
    public abstract Vector3 GetRoutePosition(float progressDistance);
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level1_Trajectories/WaypointCircuit.cs
````csharp
using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class WaypointCircuit : Waypoint
{

    /// <summary>
    /// Specify is the waypoint is a WaypointCircuit or a CinglePoint
    /// </summary>
    /// <returns>Returns always TRUE</returns>
    public override bool isCircuit() { return true; }

    public GameObject waypointCircuitSinglePoint;

    public WaypointList waypointList = new WaypointList();
    private int numPoints;
    private Vector3[] points;
    private float[] distances;

    public float editorVisualisationSubsteps = 100;
    public float Length { get; private set; }

    int actualChildrenIndx = 2;
    int minChildrenIndx = 0;

    const int digitsNumber = 3;
    /// <summary>
    /// Given a number, returns it in a string of prefixed number of digits
    /// </summary>
    /// <param name="number">Number to be converted in string</param>
    /// <returns>Returns a string of prefixed number of digits containing the number passed as argument</returns>
    string toStringOfXdigits(int number)
    {
        String tmp = "";
        int k = digitsNumber - 1;
        while (k >= 0)
        {
            int res = number / (int)Mathf.Pow(10, k);
            tmp += res;
            number -= res * (int) Mathf.Pow(10, k);
            k--;
        }
        return tmp;
    }

    /// <summary>
    /// Adds a point to the circuit
    /// </summary>
    /// <param name="pt">Points that has to be added</param>
    public void addPointAtTheEndOfTheCircuit(Vector3 pt) {
        //Debug.Log("added point at pos:" + pt.ToString());
        GameObject g = (GameObject)Instantiate(waypointCircuitSinglePoint, pt, Quaternion.identity);
        g.name = "Waypoint " + toStringOfXdigits(actualChildrenIndx++);
        g.transform.parent = this.transform;
        //updatePointsList();
    }

    /// <summary>
    /// Remove the oldest (first) point in the cirucuit
    /// </summary>
    public void cleanOldestWaypoint()
    {
        Destroy(transform.Find("Waypoint " + toStringOfXdigits(minChildrenIndx++)).gameObject);
        //updatePointsList();
    }

    /// <summary>
    /// Checks the points that compose the circuit and update the data structure containing them
    /// </summary>
    void updatePointsList()
    {
        var children = new Transform[transform.childCount];
        int n = 0;
        foreach (Transform child in transform)
        {
            children[n++] = child;
        }
        Array.Sort(children, new TransformNameComparer());
        waypointList.items = new Transform[children.Length];
        for (n = 0; n < children.Length; ++n)
        {
            waypointList.items[n] = children[n];
        }
    }

    public Transform S1;
    public Transform S2;
    public Transform S3;
    public Transform S4;

    /// <summary>
    /// Given two points that can be inside or outside the circuit, the function returns a number 'nPoints' of points
    /// in the circuit between these points
    /// </summary>
    /// <param name="pt1">First point</param>
    /// <param name="pt2">Second point</param>
    /// <param name="nOfPoints">Number of points needed to extract</param>
    /// <returns>It is a single point so it'll always return an empty array of points </returns>
    public override Vector3[] pointsBetween(Vector3 pt1, Vector3 pt2, int nPoints)
    {
        //CachePositionsAndDistances();
        Length = distances[distances.Length - 1];
        
        // FIRST -> we calculate the nearest point to pt1 and pt2
        float dPt1 = 0, dPt2 = 0;
        float distance1 = float.MaxValue;
        float distance2 = float.MaxValue;

        for (float dist = 0; dist < Length; dist += Length / editorVisualisationSubsteps)
        {
            Vector3 p = GetRoutePosition(dist + 1);
            float distToPt1 = Vector3.Distance(pt1, p);
            float distToPt2 = Vector3.Distance(pt2, p);
            
            if (distToPt1 < distance1) { dPt1 = dist; distance1 = distToPt1; }
            if (distToPt2 < distance2) { dPt2 = dist; distance2 = distToPt2; }
        }

        // SECOND -> we create the vector of points
        float distanceBetweenPoints = dPt2 > dPt1 ? dPt2 - dPt1 : dPt2 + Length - dPt1;
        float step = distanceBetweenPoints / nPoints;
        Vector3[] POINTS = new Vector3[nPoints + 2];

        for (int i = 0; i < POINTS.Length; i++)
            POINTS[i] = GetRoutePosition((dPt1 + step * (i +1)) % Length);

        return POINTS;        
    }

    /// <summary>
    /// Comparer for check distances in ray cast hits
    /// </summary>
    public class TransformNameComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((Transform)x).name.CompareTo(((Transform)y).name);
        }
    }

    /// <summary>
    /// Gets the total length of the circuit
    /// </summary>
    /// <returns>Returns the total length of the circuit</returns>
    public float getTotalLengthOfCircuit() { if (distances.Length > 0) return distances[distances.Length - 1]; else return 0; }

    public Transform[] Waypoints
    {
        get { updatePointsList(); return waypointList.items; }
    }

    //this being here will save GC allocs
    private int p0n;
    private int p1n;
    private int p2n;
    private int p3n;

    // point used for the catmull-rom equation
    private float i;
    private Vector3 P0;
    private Vector3 P1;
    private Vector3 P2;
    private Vector3 P3;

    /// <summary>
    /// Function Called when the object is activated for the first time. Used for initialization
    /// </summary>
    private void Awake()
    {        
        if (Waypoints.Length > 1)
        {
            CachePositionsAndDistances();
        }
        numPoints = Waypoints.Length;
    }

    /// <summary>
    /// Given an absolute distance from the first point, it returns a RoutePoint at that distance
    /// </summary>
    /// <param name="distance">Float containing the distance from the first point</param>
    /// <returns>A routePoint containing the position and the direction of the resulting point </returns>
    public override RoutePoint GetRoutePoint(float dist)
    {
        // position and direction
        Vector3 p1 = GetRoutePosition(dist);
        Vector3 p2 = GetRoutePosition(dist + 0.1f);
        Vector3 delta = p2 - p1;
        return new RoutePoint(p1, delta.normalized);
    }

    /// <summary>
    /// Given an absolute distance from the first point, it returns a point in the circuit at that distance
    /// </summary>
    /// <param name="progressDistance">Float containing the distance from the first point</param>
    /// <returns>It is a single point, so it'll return the SinglePoint position</returns>
    public override Vector3 GetRoutePosition(float dist)
    {
        int point = 0;

        if (Length == 0)
        {
            Length = distances[distances.Length - 1];
        }

        dist = Mathf.Repeat(dist, Length);

        while (distances[point] < dist)
        {
            ++point;
        }


        // get nearest two points, ensuring points wrap-around start & end of circuit
        p1n = ((point - 1) + numPoints)%numPoints;
        p2n = point;
         
        // found point numbers, now find interpolation value between the two middle points

        i = Mathf.InverseLerp(distances[p1n], distances[p2n], dist);

        // --- smooth catmull-rom calculation between the two relevant points ---
        
        // get indices for the surrounding 2 points, because
        // four points are required by the catmull-rom function
        p0n = ((point - 2) + numPoints)%numPoints;
        p3n = (point + 1)%numPoints;

        // 2nd point may have been the 'last' point - a dupe of the first,
        // (to give a value of max track distance instead of zero)
        // but now it must be wrapped back to zero if that was the case.
        p2n = p2n%numPoints;

        P0 = points[p0n];
        P1 = points[p1n];
        P2 = points[p2n];
        P3 = points[p3n];

        return CatmullRom(P0, P1, P2, P3, i);
  
    }

    /// <summary>
    /// Given a point, it returns the distance from the beginning of the nearest point in the circuit to this point
    /// </summary>
    /// <param name="position">Point we use to look for the nearest point in the circuit</param>
    /// <returns>The nearest point to the one passed as parameter, in the circuit</returns>
    public override float getNearestPointTo(Vector3 position)
    {
        Length = distances[distances.Length - 1];
        float dPt = 0;
        float actualDistance = float.MaxValue;
        for (float dist = 0; dist < Length; dist += Length / editorVisualisationSubsteps)
        {
            Vector3 p = GetRoutePosition(dist + 1);
            float distToPt1 = Vector3.Distance(position, p);
            if (distToPt1 < actualDistance) { dPt = dist; actualDistance = distToPt1; }          
        }
        return dPt;
    }

    /// <summary>
    /// Catmull-Rom equation to obtain a spline. More details in the documentation
    /// </summary>
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
    {
        return 0.5f*
                ((2*p1) + (-p0 + p2)*i + (2*p0 - 5*p1 + 4*p2 - p3)*i*i +
                (-p0 + 3*p1 - 3*p2 + p3)*i*i*i);
    }

    /// <summary>
    /// Transfers the position of each point and distances between points to arrays for
    /// speed of lookup at runtime
    /// </summary>
    private void CachePositionsAndDistances()
    {

        points = new Vector3[Waypoints.Length + 1];
        distances = new float[Waypoints.Length + 1];
        
        float accumulateDistance = 0;
        for (int i = 0; i < points.Length; ++i)
        {
            var t1 = Waypoints[(i)%Waypoints.Length];
            var t2 = Waypoints[(i + 1)%Waypoints.Length];
            if (t1 != null && t2 != null)
            {
                Vector3 p1 = t1.position;
                Vector3 p2 = t2.position;
                points[i] = Waypoints[i%Waypoints.Length].position;
                distances[i] = accumulateDistance;
                accumulateDistance += (p1 - p2).magnitude;
            }
        }
        editorVisualisationSubsteps = (int) distances[Waypoints.Length];
    }

    // --- functions used to draw lines and shapes in the editor-mode

    private void OnDrawGizmos()
    {
        DrawGizmos(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos(true);
    }

    private void DrawGizmos(bool selected)
    {
        waypointList.circuit = this;
        if (Waypoints.Length > 1)
        {
            numPoints = Waypoints.Length;

            CachePositionsAndDistances();
            Length = distances[distances.Length - 1];

            Gizmos.color = selected ? Color.green : Color.green;  //new Color(1, 1, 0, 0.5f);
            Vector3 prev = Waypoints[0].position;
                    
            for (float dist = 0; dist < Length; dist += Length/editorVisualisationSubsteps)
            {
                Vector3 next = GetRoutePosition(dist + 1);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
            Gizmos.DrawLine(prev, Waypoints[0].position);            
            
        }
    }


    [Serializable]
    public class WaypointList
    {
        public WaypointCircuit circuit;
        public Transform[] items = new Transform[0];
    }

}

// part that add functionalities to the editor. It is used for debugging because we could delete it if we want

namespace UnityStandardAssets.Utility.Inspector
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(WaypointCircuit.WaypointList))]
    public class WaypointListDrawer : PropertyDrawer
    {
        private float lineHeight = 18;
        private float spacing = 4;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float x = position.x;
            float y = position.y;
            float inspectorWidth = position.width;

            // Draw label


            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var items = property.FindPropertyRelative("items");
            var titles = new string[] { "Transform", "", "", "" };
            var props = new string[] { "transform", "^", "v", "-" };
            var widths = new float[] { .7f, .1f, .1f, .1f };
            float lineHeight = 18;
            bool changedLength = false;
            if (items.arraySize > 0)
            {
                for (int i = -1; i < items.arraySize; ++i)
                {
                    var item = items.GetArrayElementAtIndex(i);

                    float rowX = x;
                    for (int n = 0; n < props.Length; ++n)
                    {
                        float w = widths[n] * inspectorWidth;

                        // Calculate rects
                        Rect rect = new Rect(rowX, y, w, lineHeight);
                        rowX += w;

                        if (i == -1)
                        {
                            EditorGUI.LabelField(rect, titles[n]);
                        }
                        else
                        {
                            if (n == 0)
                            {
                                EditorGUI.ObjectField(rect, item.objectReferenceValue, typeof(Transform), true);
                            }
                            else
                            {
                                if (GUI.Button(rect, props[n]))
                                {
                                    switch (props[n])
                                    {
                                        case "-":
                                            items.DeleteArrayElementAtIndex(i);
                                            items.DeleteArrayElementAtIndex(i);
                                            changedLength = true;
                                            break;
                                        case "v":
                                            if (i > 0)
                                            {
                                                items.MoveArrayElement(i, i + 1);
                                            }
                                            break;
                                        case "^":
                                            if (i < items.arraySize - 1)
                                            {
                                                items.MoveArrayElement(i, i - 1);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    y += lineHeight + spacing;
                    if (changedLength)
                    {
                        break;
                    }
                }
            }
            else
            {
                // add button
                var addButtonRect = new Rect((x + position.width) - widths[widths.Length - 1] * inspectorWidth, y,
                                                widths[widths.Length - 1] * inspectorWidth, lineHeight);
                if (GUI.Button(addButtonRect, "+"))
                {
                    items.InsertArrayElementAtIndex(items.arraySize);
                }

                y += lineHeight + spacing;
            }

            // add all button
            var addAllButtonRect = new Rect(x, y, inspectorWidth, lineHeight);
            if (GUI.Button(addAllButtonRect, "Assign using all child objects"))
            {
                var circuit = property.FindPropertyRelative("circuit").objectReferenceValue as WaypointCircuit;
                var children = new Transform[circuit.transform.childCount];
                int n = 0;
                foreach (Transform child in circuit.transform)
                {
                    children[n++] = child;
                }
                Array.Sort(children, new TransformNameComparer());
                circuit.waypointList.items = new Transform[children.Length];
                for (n = 0; n < children.Length; ++n)
                {
                    circuit.waypointList.items[n] = children[n];
                }
            }
            y += lineHeight + spacing;

            // rename all button
            var renameButtonRect = new Rect(x, y, inspectorWidth, lineHeight);
            if (GUI.Button(renameButtonRect, "Auto Rename numerically from this order"))
            {
                var circuit = property.FindPropertyRelative("circuit").objectReferenceValue as WaypointCircuit;
                int n = 0;
                foreach (Transform child in circuit.waypointList.items)
                {
                    child.name = "Waypoint " + (n++).ToString("000");
                }
            }
            y += lineHeight + spacing;

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = property.FindPropertyRelative("items");
            float lineAndSpace = lineHeight + spacing;
            return 40 + (items.arraySize * lineAndSpace) + lineAndSpace;
        }


        // comparer for check distances in ray cast hits
        public class TransformNameComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return ((Transform)x).name.CompareTo(((Transform)y).name);
            }
        }
    }

#endif
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level1_Trajectories/WaypointProgressTracker.cs
````csharp
using System;
using UnityEngine;

public class WaypointProgressTracker : MonoBehaviour
{

    // A reference to the waypoint-based route we should follow
    [SerializeField] private Waypoint waypoint; 

    /// <summary>
    /// Sets the waypoint that the drone will use to move
    /// </summary>
    /// <param name="wpc">Waypoint used by the drone. It can be a SinglePoint or a WaypointCircuit</param>
    public void setWaypoint(Waypoint wpc) {
        waypoint = wpc;
        progressDistance = 0;
        if (!wpc.isCircuit())
            ((singlePoint)waypoint).setHome(transform);
        else
            hasToRecalculateDistance();
    }

    [SerializeField] private float lookAheadForTargetOffset = 5;
    [SerializeField] private float maxDistFromCircuit = 1;
    [SerializeField] private float overallDistanceFromTrajectory = 0;
    [SerializeField] private float distanceOfPointToLookAt = 8;   

    public Transform target;
    private float progressDistance; // The progress round the route
    /// <summary>
    /// Gets the position of the drone in the circuit
    /// </summary>
    /// <returns>the position of the drone in the circuit</returns>
    public Vector3 getRoutePosition() { return waypoint.GetRoutePosition(progressDistance); }    

    float timer = 0.1f;
    float actualTimer = 1;

    /// <summary>
    /// We specify a rect and a point outside the rect. This function return the point INSIDE the rect, 
    /// that forms, with the point specified, a rect perpendicular to the one passed as argument
    /// </summary>
    /// <param name="ptR1">First point of the rect</param>
    /// <param name="ptR2">Second point of the rect</param>
    /// <param name="point">Point outside of the rect, that we want to find the corresponding perpendicular point</param>
    /// <returns>A point that forms a rect, with the point passed as argument, perpendicolar to the rect specified </returns>
    private Vector3 getPerpendicolarPoint(Vector3 ptR1, Vector3 ptR2, Vector3 point)
    {
        Vector2 A = new Vector2(ptR1.x, ptR1.z);
        Vector2 B = new Vector2(ptR2.x, ptR2.z);
        Vector2 C = new Vector2(point.x, point.z);

        float m = (B.y - A.y) / (B.x - A.x);
        float k = m * C.y + C.x;       
        float n = B.y - A.y;
        float o = B.x - A.x;
        float l = o * A.y - n * A.x;

        float newX = -(l * m - k * o) / (o + m * n);
        float newY = (k * n + l) / (o + m * n);

        // we do not consider the Y axes, so we always use the same as 'point' as they were all in the same Y plane
        return new Vector3(newX, point.y, newY);
    }

    private bool needToRecalculateDistanceFromCircuit = false;
    /// <summary>
    /// In some cases is necessary to recalculate the actual distance 
    /// instead of using the old one and summing to it the distance traveled.
    /// This function forces the recalculation.
    /// </summary>
    public void hasToRecalculateDistance() { needToRecalculateDistanceFromCircuit = true; }

    /// <summary>
    /// Function called each frame
    /// </summary>
    private void Update()
    {
        // if we are using a WaypointCircuit we recalculate the position of the objects used to navigate it
        if (waypoint.isCircuit())
        {
            if (actualTimer >= 0)
                actualTimer -= Time.deltaTime;
            else {
                actualTimer += timer;

                // calculating test-Points in circuit
                int nOfPoints = (int)lookAheadForTargetOffset;
                Vector3[] pBetween = waypoint.pointsBetween(getRoutePosition(), target.transform.position, nOfPoints);

                // calculating the matching points in the line between the drone and the target
                Vector3[] destPoints = new Vector3[pBetween.Length];
                for (int i = 0; i < pBetween.Length; i++)
                    destPoints[i] = getPerpendicolarPoint(transform.position, target.position, pBetween[i]);

                // calculating sum of distances between the test point and it's matching point
                overallDistanceFromTrajectory = 0;
                for (int i = 0; i < pBetween.Length - 1; i++)
                    overallDistanceFromTrajectory += Vector3.Distance(pBetween[i], destPoints[i]);

                // so we increase o decrease the distance of the target 
                if (overallDistanceFromTrajectory > maxDistFromCircuit)
                    lookAheadForTargetOffset -= 0.25f;
                else
                    lookAheadForTargetOffset += 0.25f;
            

                lookAheadForTargetOffset = droneSettings.keepOnRange(lookAheadForTargetOffset, 3f, 12f);            
            }

            // determine the position we should currently be aiming for
            // (this is different to the current progress position, it is a certain amount ahead along the route)        
            target.position = waypoint.GetRoutePoint(progressDistance + lookAheadForTargetOffset).position;
            target.rotation = Quaternion.LookRotation(waypoint.GetRoutePoint(progressDistance).direction);
            // get our current progress along the route       

            needToRecalculateDistanceFromCircuit = Vector3.Distance(transform.position, getRoutePosition()) > 10 ? true : needToRecalculateDistanceFromCircuit;
            if (needToRecalculateDistanceFromCircuit)
            {
                progressDistance = waypoint.getNearestPointTo(transform.position);
                needToRecalculateDistanceFromCircuit = false;
            }
            else
            {
                WaypointCircuit.RoutePoint progressPoint = waypoint.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude * 0.5f;
                }
            }
            
            // setting drones variables
            gameObject.GetComponent<droneMovementController>().setRoutePos(getRoutePosition());
            float distToRoute = Vector3.Distance(transform.position, getRoutePosition());
            gameObject.GetComponent<droneMovementController>().setLookingPoint(waypoint.GetRoutePosition(progressDistance + distanceOfPointToLookAt - distToRoute));
            gameObject.GetComponent<droneMovementController>().stayOnFixedPoint = false;
        }
        else
        {
            Vector3 routePos = getRoutePosition();

            // setting drones variables
            gameObject.GetComponent<droneMovementController>().setRoutePos(routePos);
            target.position = getRoutePosition();// + Vector3.forward;
            gameObject.GetComponent<droneMovementController>().stayOnFixedPoint = true;         
            gameObject.GetComponent<droneMovementController>().setLookingPoint(((singlePoint) waypoint).getLookingAtPoint());
    
        }

        
        
    }

    // --- function used to draw lines and shapes in the editor-mode

    public bool drawGizoms = true;
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && drawGizoms)
            { 
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.DrawWireSphere(waypoint.GetRoutePosition(progressDistance), 0.25f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(target.position, target.position + target.forward);
            }
        }
    }
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level2_IA/maze/drawing/dataDrawer.cs
````csharp
using UnityEngine;
using System.Collections.Generic;

public class dataDrawer : MonoBehaviour {

    public GameObject wallObject;
    public GameObject dronePin;
    GameObject[] dronesPins;

    public float maxMapSize = 200;
    dataCollector.Data data;
    LinkedList<dataCollector.indx> dirtyList;
    /// <summary>
    /// Sets the matrix containing the information that the dataDrawer will use to paint a map
    /// </summary>
    /// <param name="data">Matrix where the information is contained</param>
    public void setData(dataCollector.Data data) { this.data = data; dirtyList = data.getDirtyValuesList(); }
    
    float interval = 0.01f;
    float actualTime = 1f;

    dataCollector dC;
    LineRenderer lr;

    //model used for painting the walls
    Transform wallspinscontainer;
    //model used to show the drone position
    Transform dronepinscontainer;

    /// <summary>
    /// Function called before of the first update. Used to initialize the components.
    /// </summary>
    void Start()
    {
        dC = GetComponent<dataCollector>();

        wallspinscontainer = transform.Find("wallspinscontainer");
        dronepinscontainer = transform.Find("dronepinscontainer");

        lr = gameObject.AddComponent<LineRenderer>();
        lr.SetVertexCount(5);
        Vector3 pt = transform.position;

        // draw the borders
        lr.SetPosition(0, pt);
        lr.SetPosition(1, (pt += Vector3.right * maxMapSize));
        lr.SetPosition(2, (pt += Vector3.forward * maxMapSize));
        lr.SetPosition(3, (pt += Vector3.right * -maxMapSize));
        lr.SetPosition(4, (pt += Vector3.forward * -maxMapSize));

        // initialize the drone-Pins, used to show the drones positions
        dronesPins = new GameObject[10];
        for (int i = 0; i < dronesPins.Length; i++)
        {
            dronesPins[i] = (GameObject)Instantiate(dronePin, new Vector3(), Quaternion.identity * Quaternion.AngleAxis(90, Vector3.right));
            dronesPins[i].transform.parent = dronepinscontainer;
        }
            
    }

    public int nrOfBricks = 0;
    /// <summary>
    /// Function called once per frame
    /// </summary>
    void Update()
    {
        if ((actualTime -= Time.deltaTime) < 0)
        {
            // for each element in the list of recently added values, it spawns a new pin (to represent a wall)
            // and place it in the correct position
            actualTime += interval;
            // to speed up the process is possible to process more than 1 value per execution
            int counter = 10;
            while (dirtyList.Count > 0 && counter > 0)
            {
                GameObject g = (GameObject)Instantiate(wallObject, 
                    transform.position + dirtyList.First.Value.divideFloat(data.getArraySize()) * maxMapSize, 
                    Quaternion.identity * Quaternion.AngleAxis(90, Vector3.right));
                g.transform.parent = wallspinscontainer;
                dirtyList.RemoveFirst();
                counter--;
                nrOfBricks++;
            }

            // update the drones positions in the map
            for (int i = 0; i < dC.ticket; i++)
                dronesPins[i].transform.position = transform.position + dC.dronesPositions[i].divideFloat(data.getArraySize()) * maxMapSize;
        }

    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level2_IA/maze/dataCollector.cs
````csharp
using UnityEngine;
using System.Collections.Generic;

public class dataCollector : MonoBehaviour {
    /// <summary>
    /// Structure that represents real coordinates in the data matrix
    /// </summary>
    public struct indx {
        public int x;
        public int z;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X position in the data matrix</param>
        /// <param name="z">Z position in the data matrix</param>
        public indx(int x, int z) { this.x = x; this.z = z; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v">Vector3 from which we'll extract the X and Z to create an Indx</param>
        public indx(Vector3 v) { this.x = (int) v.x; this.z = (int)v.z; }

        /// <summary>
        /// Sum of Indxs
        /// </summary>
        /// <param name="i">Other Indx we'll sum to this</param>
        /// <returns>Returns the indx resulting from the sum of the current Indx and the one passed as argument</returns>
        public indx add(indx i) { return new indx(x + i.x, z + i.z); }

        /// <summary>
        /// Subtraction of Indxs
        /// </summary>
        /// <param name="i">Other Indx we'll subtract to this</param>
        /// <returns>Returns the indx resulting from the subtraction of the Indx passed as argument to the this Indx</returns>
        public indx subtract(indx i) { return new indx(x - i.x, z - i.z); }

        /// <summary>
        /// Multiplication of Indxs
        /// </summary>
        /// <param name="i">Other Indx we'll multiply to this</param>
        /// <returns>Returns the indx resulting from the multiplication of the current Indx and the one passed as argument</returns>
        public indx multiply(int i) { return new indx(x * i, z * i); }

        /// <summary>
        /// Scalar division
        /// </summary>
        /// <param name="i">Scalar value we will divide the Indx to</param>
        /// <returns>Returns a new Vector3, resulting from the division of this and the scalar passed as argument</returns>
        public Vector3 divideFloat(float i) { return multiplyFloat(1 / i); }

        /// <summary>
        /// Scalar multiplication
        /// </summary>
        /// <param name="i">Scalar value we will multiply to Indx</param>
        /// <returns>Returns a new Vector3, resulting from the multiplication of this and the scalar passed as argument</returns>
        public Vector3 multiplyFloat(float f) { return new Vector3(x * f, 0, z * f); }

        /// <summary>
        /// Converts the indx to a Vector3
        /// </summary>
        /// <returns>Returns a Vector3 obtained from the Indx</returns>
        public Vector3 ToVector3() { return multiplyFloat(1f); }

        /// <summary>
        /// Returns a nicely formatted string for this object
        /// </summary>
        public override string ToString() { return "[x:" + x + ",z:" + z +"]"; }

        /// <summary>
        /// Returns true if the two objects are equals
        /// </summary>
        public static bool Equals(indx i1, indx i2) { return (i1.x == i2.x && i1.z == i2.z); }
    }
    public class Data
    {                      
        Vector3 zeroPoint;
        indx zeroPointInData;
        /// <summary>
        /// Gets the zero-point in the matrix. This is used to calculate all other points
        /// </summary>
        /// <returns></returns>
        public indx getZeroPointInData() { return zeroPointInData; }
        float maxExplorableDistance;
        int arraySize;
        /// <summary>
        /// Gets the size of the matrix
        /// </summary>
        public int getArraySize() { return arraySize; }

        public byte[,] dataArray;

        /// <summary>
        /// Constructor
        /// <para>Associates 'Vector3 zeroPoint' to 'Indx zeroPointInData' </para>
        /// </summary>
        /// <param name="dataArraySize">Size of the matrix</param>
        /// <param name="maxExplorableDistance">Size of the area we can explore</param>
        /// <param name="zeroPoint">Initial point represented by world-coordinates </param>
        /// <param name="zeroPointInData">Initial point represented by an indx</param>
        public Data(int dataArraySize, float maxExplorableDistance, Vector3 zeroPoint, indx zeroPointInData) {
            dirtyValues = new LinkedList<indx>();

            arraySize = dataArraySize;
            dataArray = new byte[dataArraySize, dataArraySize];

            for (int i = 0; i < dataArraySize; i++)
                for (int j = 0; j < dataArraySize; j++)
                    dataArray[i, j] = 2;

            this.zeroPoint = zeroPoint;
            this.zeroPointInData = zeroPointInData;
            this.maxExplorableDistance = maxExplorableDistance;
        }

        /// <summary>
        /// Converts the real-world coordinates specified by the Vector3 to an Indx in the matrix
        /// </summary>
        /// <param name="v">Vector3 we want to convert</param>
        public indx v3ToIndx(Vector3 v) { return new indx((v - zeroPoint) / maxExplorableDistance * arraySize).add(zeroPointInData); }
        
        /// <summary>
        /// Converts the Indx in the matrix to a real-world coordinate
        /// </summary>
        /// <param name="i">Indx we want to convert</param>
        public Vector3 indxToV3(indx i) { return i.subtract(zeroPointInData).multiplyFloat(maxExplorableDistance / arraySize) + zeroPoint; }

        // list where we save the indxs that have been modified recently
        LinkedList<indx> dirtyValues;
        /// <summary>
        /// Gets the list of values recently modified
        /// </summary>
        public LinkedList<indx> getDirtyValuesList() { return dirtyValues; }

        /// <summary>
        /// Sets the value of the byte in position <c>v3ToIndx(v)</c>
        /// </summary>
        /// <param name="v"></param>
        /// <param name="val"></param>
        /// <returns>Returns TRUE if the value has been changed, FALSE otherwise</returns>
        public bool setValue(Vector3 v, byte val) {
            indx i = v3ToIndx(v);
            if (dataArray[i.x, i.z] != val && dataArray[i.x, i.z] != 1)
            {
                if (val == 1) dirtyValues.AddLast(i); 
                dataArray[i.x, i.z] = val;
                return true;
            }
            return false;            
        }

        /// <summary>
        /// Gets the value in the matrix at position (x,z)
        /// </summary>
        /// <param name="x">First coordinate in the matrix</param>
        /// <param name="z">Second coordinate in the matrix</param>
        /// <returns>Returns the value in the matrix at position (x,z)</returns>
        private int getValue(int x, int z) { return dataArray[x, z]; }

        /// <summary>
        /// Gets the value in the matrix, corresponding to the real-world coordinates specified by the Vector3
        /// </summary>
        /// <param name="v">Vector3 specifying the real-world coordinates of the point we are interested</param>
        /// <returns>Returns the value in the matrix at position <c>v3ToIndx(v)</c></returns>
        public int getValue(Vector3 v) { indx i = v3ToIndx(v); return getValue(i.x, i.z); }

        /// <summary>
        /// Returns true if there are no walls or unknown cells in the square with center 'v' and specified radius
        /// </summary>
        /// <param name="v">Center taken in consideration to explore if the cells are all free</param>
        /// <param name="radius">Radius of the search</param>
        /// <returns>0 -> areAllFree; 1 -> there are walls; 2 -> no walls but unknown cells</returns>
        public int areAllFree(Vector3 v, int radius)
        {
            bool thereAreUnknown = false;
            
            indx center = v3ToIndx(v);
            for (int i = center.x - radius / 2; i <= center.x + radius / 2; i++)
                for (int j = center.z - radius / 2; j <= center.z + radius / 2; j++)
                    if (dataArray[i, j] == 1)
                        return 1;
                    else if (dataArray[i, j] == 2)
                        thereAreUnknown = true;
            
            return thereAreUnknown ? 2 : 0;
        }

    }

    const int dataMaxSize = 1000;
    const float maxExplorableDistance = 500;
    public Data data;
    public dataDrawer dW;

    public int ticket = 0;
    public indx[] dronesPositions;
    bool initialized = false;
    /// <summary>
    /// Each drone has to call this function to obtain a ticket used for updating their position in the drawer
    /// <para>The first drone that calls this function determine the zeroPointPosition</para>
    /// </summary>
    /// <param name="dronePosition">The actual position of the drone</param>
    /// <returns>Returns a ticket that will be used for updating the position</returns>
    public int initialize(Vector3 dronePosition)
    {
        if (!initialized)
        {
            // the first drone that register itself determine the zeroPointPosition, that correspond to the center of the map
            dronesPositions = new indx[10]; 
            indx actualPosition = new indx(dataMaxSize / 2, dataMaxSize / 2);
            data = new Data(dataMaxSize, maxExplorableDistance, dronePosition, actualPosition);
            dW.setData(data);
            initialized = true;
        }
        
        // other drones just takes a ticket to actualize their position 
        dronesPositions[ticket] = new indx(dronePosition);
        return ticket++;
    }

    /// <summary>
    /// Using the ticket obtained with the initialization, the drone can update its position
    /// </summary>
    /// <param name="ticket">Ticket obtained with the initialization</param>
    /// <param name="pos">The actual position of the drone</param>
    public void updatePosition (int ticket, Vector3 pos) { dronesPositions[ticket] = data.v3ToIndx(pos); }

    /// <summary>
    /// Adds data to the matrix
    /// </summary>
    /// <param name="v">Position of the data we want to add</param>
    /// <param name="b">Value registered from sensors that indicates if there is a wall or it is empty space</param>
    public void addDataToTheMatrix(Vector3 v, byte b) { data.setValue(v, b); }
    
    /// <summary>
    /// If the Indxs created with the Vectors passed are equals returns true, false otherwise.
    /// </summary>
    public bool sameIndx(Vector3 v1, Vector3 v2) { return indx.Equals(data.v3ToIndx(v1), data.v3ToIndx(v2)); }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level2_IA/maze/mazeExplorer.cs
````csharp
using UnityEngine;
using System.Collections.Generic;
using System;

public class mazeExplorer : IA {

    Graph myGraph;
    public dataCollector dC;
    int ticket;

    //set this to true in the editor to hide the graph
    public bool doNotSpawnAnything = false;
    public GameObject NodeObject;
    public Material deadNodeMaterial;

    /// <summary>
    /// Class representing the node of the graph.
    /// </summary>
    class Node
    {
        static int nodesID = 0;
        int ID;
        /// <summary>
        /// Gets the ID of the node.
        /// </summary>
        public int getID() { return ID; }

        int nrOfSons = 0;
        const int maxNrOfSons = 8;
        public bool acceptMoreSons() { return nrOfSons <= maxNrOfSons; }

        Vector3 center;
        /// <summary>
        /// Gets the position associated with the node.
        /// </summary>
        public Vector3 getCenter() { return center; }
        List<Node> joinedNodes;
        /// <summary>
        /// Gets a list of all the nodes joined to this one.
        /// </summary>
        public List<Node> getJoinedNodes() { return joinedNodes; }

        Graph graph;

        private bool doNotSpawnAnything;
        private GameObject phisicalObject;
        private Material deadNodeMaterial;
        private LineRenderer lr;

        bool active = true;
        /// <summary>
        /// Indicate if the node is active (can still create sons) or not
        /// </summary>
        public bool isActive() { return active; }
        /// <summary>
        /// Deactivate the node and eventually change its color
        /// </summary>
        public void deactivateNode() { active = false; if (!doNotSpawnAnything) phisicalObject.GetComponent<MeshRenderer>().material = deadNodeMaterial; }
          
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">Position of the node</param>
        /// <param name="g">Model of the drone that will be spawn in its position</param>
        /// <param name="deadNodeMaterial">Material that will be assigned to the gameObject representing the node
        /// when it'll be deactivated</param>
        /// <param name="graph">Graph of the node</param>
        /// <param name="father">Node that generates this node</param>
        /// <param name="doNotSpawnAnything">If TRUE indicates that the node doesn't spawn the model and the archs between nodes</param>
        public Node(Vector3 pos, GameObject g, Material deadNodeMaterial, Graph graph, Node father = null, bool doNotSpawnAnything = false) {
            ID = ++nodesID;
            this.doNotSpawnAnything = doNotSpawnAnything;
            this.graph = graph;
            center = pos;
            joinedNodes = new List<Node>();

            if (this.doNotSpawnAnything) return;

            // if !doNotSpawnAnything

            phisicalObject = (GameObject) Instantiate(g, pos, Quaternion.identity);
            this.deadNodeMaterial = deadNodeMaterial;
            
            // initializing LineRenderer
            lr = phisicalObject.AddComponent<LineRenderer>();
            lr.SetVertexCount(maxNrOfSons * 2);
            lr.SetWidth(0.35f, 0f);
            for (int i = 0; i < maxNrOfSons * 2; i++)
                lr.SetPosition(i, center);

        }
      
        /// <summary>
        /// Obtains a list of the neighbors nodes, check if it is possible to create a connection with all of them
        /// and if it is possible it creates it.
        /// </summary>
        public void createConnections()
        {
            if (!acceptMoreSons()) return;

            List<Node> neighbors = graph.getNeighbors(this);
            foreach(Node n in neighbors)
                if (isConnectable(n))
                {
                    reJoinWith(n);
                    n.reJoinWith(this);
                }
        }

        /// <summary>
        /// Complete the join with a node
        /// </summary>
        /// <param name="n">The node is requesting the rejoin</param>
        public void reJoinWith(Node n)
        {
            if (!doNotSpawnAnything)
                lr.SetPosition(nrOfSons * 2 + 1, n.center);

            joinedNodes.Add(n);
            nrOfSons++;
        }

        /// <summary>
        /// Check if it is possible to create a connection between this node and the one passed as parameter
        /// </summary>
        /// <param name="n">Node we want to check</param>
        /// <returns>Returns TRUE if it is possible to establish a connection, otherwise FALSE</returns>
        private bool isConnectable(Node n) {
            return (!n.Equals(this) && !joinedNodes.Contains(n) && n.acceptMoreSons() &&  graph.areConnectables(this, n));
        }      

        /// <summary>
        /// Gets the hashCode of the node object.
        /// </summary>
        public override int GetHashCode() { return center.GetHashCode(); }

        /// <summary>
        /// Returns TRUE if the object passed as parameter is a node and is equal to this one
        /// </summary>
        /// <param name="obj">The object we want to check</param>
        public override bool Equals(object obj)
        {
            Node tmp;
            if (obj == null) return false;
            else if ((tmp = (Node)obj) == null) return false;
            else return tmp.center.Equals(this.center);
        }


        // -- path searching -- //

        float estimation;
        /// <summary>
        /// Get the estimation. Is related to the Dijkstra algorithm of path-finding.
        /// </summary>
        public float getEstimation() { return estimation; }
        bool hasBeenChecked;
        bool isSource;
        Node predecessor;
        /// <summary>
        /// Get the predecessor of the node. Is related to the Dijkstra algorithm of path-finding.
        /// </summary>
        public Node getPredecessor() { return predecessor; }

        /// <summary> 
        /// Initializes the variables used for the path finding functions 
        /// </summary>
        public void initializeForPathFinding() {
            estimation = int.MaxValue;
            hasBeenChecked = false;
            isSource = false;
            predecessor = null;
        }

        /// <summary> 
        /// Execute a Relax on the adjacents node, changing their value if necessary (node to node) 
        /// </summary>
        private void relax(Node u)
        {           
            hasBeenChecked = true;
            this.estimation = Vector3.Distance(this.getCenter(), u.getCenter()) + u.estimation;
            predecessor = u;

            foreach (Node n in joinedNodes)
                if (!n.hasBeenChecked || n.estimation > this.estimation + Vector3.Distance(this.getCenter(), n.getCenter()))
                    n.relax(this);            
        }
        
        /// <summary> 
        /// Execute a Relax on the node, changing its value if necessary (graph to node) 
        /// </summary>
        public void relax()
        {
            estimation = 0;
            isSource = true;
            hasBeenChecked = true;

            foreach (Node n in joinedNodes)
                if (!n.hasBeenChecked || n.estimation > this.estimation + Vector3.Distance(this.getCenter(), n.getCenter()))
                    n.relax(this);
        }
    }

    /// <summary>
    /// Class representing a graph.
    /// </summary>
    class Graph
    {
        const int minimumFreeCellsRadiusToCreateANode = 10;
        const int minimumDistanceBetweenCells = 5;

        Node root;
        /// <summary>
        /// Gets the first-spawned node of the graph.
        /// </summary>
        public Node getRoot() { return root; }

        // list of all active nodes.
        LinkedList<Node> activeNodes;
        // list of all nodes.
        List<Node> allNodes;
        // Data-Structure containing the matrix where the information about the environment is saved
        dataCollector.Data data;

        GameObject nodeObj;
        Material deadNodeMaterial;

        // matrix of lists. Each node is assigned to a position in the matrix depending of its position
        List<Node>[,] sectors;
        Vector3 upperLeftPoint;
        float sectorWidth;
        int sectorsDim;   
        const float sectorPercentualWidth = 0.05f;
        bool doNotSpawnAnything;
        public bool stoppedGrowing = false;

        /// <summary>
        /// Constructor of the graph class
        /// </summary>
        /// <param name="rootPosition">Position of the first node</param>
        /// <param name="data">Data-Structure containing the matrix where the information about the environment is saved</param>
        /// <param name="nodeObj">Model of the node that will be spawned</param>
        /// <param name="deadNodeMaterial">Material assigned to the node once it has been deactivated</param>
        /// <param name="doNotSpawnAnything">If TRUE indicates that the node doesn't spawn the model and the archs between nodes</param>
        public Graph(Vector3 rootPosition, dataCollector.Data data, GameObject nodeObj, Material deadNodeMaterial, bool doNotSpawnAnything = false)
        {
            this.nodeObj = nodeObj;
            this.deadNodeMaterial = deadNodeMaterial;
            this.doNotSpawnAnything = doNotSpawnAnything;

            root = new Node(rootPosition, nodeObj, deadNodeMaterial, this, null, doNotSpawnAnything);
            activeNodes = new LinkedList<Node>();
            activeNodes.AddFirst(root);
            allNodes = new List<Node>();
            allNodes.Add(root);
            this.data = data;

            // sectors structure inizialization
            upperLeftPoint = data.indxToV3(new dataCollector.indx(0, 0));
            float totalLengthOfExplorableZone = Mathf.Abs(upperLeftPoint.x - data.indxToV3(new dataCollector.indx(data.getArraySize()-1 , 0)).x);
            sectorWidth = totalLengthOfExplorableZone * sectorPercentualWidth;
            sectorsDim = (int)(1 / sectorPercentualWidth);
            sectors = new List<Node>[sectorsDim, sectorsDim];
            for (int i = 0; i < sectorsDim; i++)
                for (int j = 0; j < sectorsDim; j++)
                    sectors[i, j] = new List<Node>();

            assignNodeToSector(root);   
        }

        /// <summary>
        /// Given a position and the upperLeft point of the sectors, it returns a Vector2 that represents the index of the 
        /// matrix containing the nodes belonging to that sector
        /// </summary>
        /// <param name="pos">The position we want to discover the sector</param>
        /// <param name="zeroPoint">Upper left point of the sectors matrix</param>
        /// <returns></returns>
        private Vector2 getSectorIndxs(Vector3 pos, Vector3 zeroPoint)
        {
            int x = 0;
            while (!(zeroPoint.x + sectorWidth * x < pos.x && zeroPoint.x + sectorWidth * (x + 1) >= pos.x)) { x++; if (x > sectorsDim) throw new Exception(); }
            int z = 0;
            while (!(zeroPoint.z + sectorWidth * z < pos.z && zeroPoint.z + sectorWidth * (z + 1) >= pos.z)) { z++; if (z > sectorsDim) throw new Exception(); }
            return new Vector2(x, z);
        }

        /// <summary>
        /// Put a point in its list inside the sectors-matrix, using its position to find the right sector
        /// </summary>
        /// <param name="n">Node we want to put in the matrix of sectors</param>
        private void assignNodeToSector(Node n)
        {
            Vector2 indx = getSectorIndxs(n.getCenter(), upperLeftPoint);
            sectors[(int)indx.x, (int)indx.y].Add(n);            
        }

        /// <summary>
        /// Function that returns a list with all the nodes that are in the sector of the node passed as argument
        /// and the nodes that lie in the adjacent sectors
        /// </summary>
        /// <param name="n">We want to obtain the neighbors of this node</param>
        /// <returns>A list of all the neighbors node of 'n'</returns>
        public List<Node> getNeighbors(Node n)
        {
            List<Node> tmpList = new List<Node>();
            Vector2 indx = getSectorIndxs(n.getCenter(), upperLeftPoint);
            int x = (int)indx.x;
            int z = (int)indx.y;

            for (int i = (x > 0 ? x - 1 : x); i <= (x < sectorsDim - 1 ? x + 1 : x); i++)
                for (int j = (z > 0 ? z - 1 : z); j <= (z < sectorsDim - 1 ? z + 1 : z); j++)
                    foreach (Node node in sectors[i, j])
                        tmpList.Add(node);
            return tmpList;
        }

        /// <summary>
        /// Function that returns a list with all the nodes that are in the sector of the position passed as argument
        /// </summary>
        /// <param name="pos">We want to obtain the nodes in the sector where lies this position</param>
        /// <returns>A list with all the nodes in the same sector of the given position</returns>
        public List<Node> getCloseNeighbors(Vector3 pos)
        {
            List<Node> tmpList = new List<Node>();
            Vector2 indx = getSectorIndxs(pos, upperLeftPoint);
            int x = (int)indx.x;
            int z = (int)indx.y;

            foreach (Node node in sectors[x,z])
                tmpList.Add(node);
            return tmpList;
        }

        /// <summary>
        /// Given an active node, the algorithm spawn every possible node around it and check if it is still active
        /// or has to be deactivated
        /// </summary>
        /// <param name="node">Node we want to expand</param>
        /// <returns>Returns a list with all the sons of the node. It could be empty.</returns>
        private LinkedList<Node> expandNode(Node node)
        {

            LinkedList<Node> sons = new LinkedList<Node>();
            bool deadNode = true;

            bool reloadNeighbors = false;
            List<Node> neighbors = getNeighbors(node);

            // check all around the node if it is possible to create a son
            for (float angle = 0; angle < 2 * Mathf.PI; angle += 0.5f)
            {
                if (reloadNeighbors) neighbors = getNeighbors(node);

                bool enoughDistanceFromOthersNode = true;
                Vector3 newPos = node.getCenter() + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized * (minimumDistanceBetweenCells * 1.1f);
                // check if the path is free (no unknown cells, no walls)
                if (noWallsBetween(node.getCenter(), newPos))
                {
                    // check the distances between the nodes in the same sector
                    foreach (Node n in neighbors)
                        if (Vector3.Distance(n.getCenter(), newPos) < minimumDistanceBetweenCells)
                            enoughDistanceFromOthersNode = false;
                }
                else
                    enoughDistanceFromOthersNode = false;


                if (enoughDistanceFromOthersNode)               // there is enough distance between the choosen position and all its neighbors
                {                                        
                    int result = data.areAllFree(newPos, minimumFreeCellsRadiusToCreateANode);
                    // 0 -> areAllFree; 1 -> there are walls; 2 -> no walls but unknown cells

                    if (result == 0)
                    {
                        Node newNode = new Node(newPos, nodeObj, deadNodeMaterial, this, node, doNotSpawnAnything);
                        assignNodeToSector(newNode);
                        node.createConnections();
                        sons.AddFirst(newNode);
                        reloadNeighbors = true;
                    }
                    else if (result == 2)
                        deadNode = false;
                }

            }

            if (deadNode || !node.acceptMoreSons()) node.deactivateNode();
            return sons;
        }

        /// <summary>
        /// Search in the active nodes of the graph and if it is possible to expand them, it does it.
        /// </summary>
        public void expandGraph()
        {
            if (activeNodes.Count == 0) { stoppedGrowing = true; return; }

            // copying the active nodes to a vector
            Node[] actualNodes = new Node[activeNodes.Count];           
            activeNodes.CopyTo(actualNodes,0);
            activeNodes = new LinkedList<Node>();

            foreach(Node n in actualNodes)
            {
                LinkedList<Node> sons = expandNode(n);
                while (sons.Count > 0)
                {
                    // add the new node to the data-structures
                    activeNodes.AddFirst(sons.First.Value);
                    allNodes.Add(sons.First.Value);
                    sons.RemoveFirst();
                }

                // the expansion procedure cheks also if the node is active. If not we deactivate it
                if (n.isActive())   activeNodes.AddFirst(n);
            }
        }

        /// <summary>
        /// Check if two nodes are connectables
        /// </summary>
        /// <param name="n1">First node</param>
        /// <param name="n2">Second node</param>
        /// <returns>Returns TRUE if it is possible to create a connection between the centers of the two nodes</returns>
        public bool areConnectables(Node n1, Node n2) { return areConnectables(n1.getCenter(), n2.getCenter()); }

        /// <summary>
        /// Check if two points are connectables
        /// </summary>
        /// <param name="v1">First point</param>
        /// <param name="v2">Second point</param>
        /// <returns>Returns TRUE if it is possible to create a connection the two points</returns>
        public bool areConnectables(Vector3 v1, Vector3 v2)
        {
            if (Vector3.Distance(v1, v2) > minimumDistanceBetweenCells * 1.8f) return false;

            while (Vector3.Distance((v1 = Vector3.MoveTowards(v1, v2, 1)), v2) > 0)
                if (data.getValue(v1) != 0)
                    return false;

            return true;
        }

        /// <summary>
        /// Check if there are no walls between two points.
        /// <para> This function is stricter than <c>areConnectables()</c> because this assures that there is a 
        /// wide line of empty cells between the two points</para>
        /// </summary>
        /// <param name="v1">First point</param>
        /// <param name="v2">Second point</param>
        /// <returns>Returns TRUE if it is possible to create a connection the two points</returns>
        public bool noWallsBetween(Vector3 v1, Vector3 v2)
        {
            Vector3 tmpV = v1;
            while (Vector3.Distance((tmpV = Vector3.MoveTowards(tmpV, v2, 1)), v2) > 0)
                if (data.getValue(tmpV) == 1)
                    return false;

            tmpV = v1 + Vector3.right * 0.5f;
            while (Vector3.Distance((tmpV = Vector3.MoveTowards(tmpV, v2, 1)), v2) > 0)
                if (data.getValue(tmpV) == 1)
                    return false;

            tmpV = v1 + Vector3.forward * 0.5f;
            while (Vector3.Distance((tmpV = Vector3.MoveTowards(tmpV, v2, 1)), v2) > 0)
                if (data.getValue(tmpV) == 1)
                    return false;

            tmpV = v1 + Vector3.right * -0.5f;
            while (Vector3.Distance((tmpV = Vector3.MoveTowards(tmpV, v2, 1)), v2) > 0)
                if (data.getValue(tmpV) == 1)
                    return false;

            tmpV = v1 + Vector3.forward * -0.5f;
            while (Vector3.Distance((tmpV = Vector3.MoveTowards(tmpV, v2, 1)), v2) > 0)
                if (data.getValue(tmpV) == 1)
                    return false;

            return true;
        }
       

        // -- path searching -- //

        /// <summary>
        /// Given a position and a list of nodes it returns the nearest node. 
        /// It is used to find the first node of a path.
        /// </summary>
        /// <param name="pos">Position we use to check the nearest node to</param>
        /// <param name="nodes">List of nodes where we have to look for the nearest one</param>
        /// <returns>Return the nearest node to the position</returns>
        public Node nearestTo(Vector3 pos, List<Node> nodes)
        {
            float distance = float.MaxValue;
            Node tmpNode = getRoot();
            foreach (Node n in nodes)
                if (Vector3.Distance(pos, n.getCenter()) < distance)
                {
                    distance = Vector3.Distance(pos, n.getCenter());
                    tmpNode = n;
                }
            return tmpNode;
        }

        /// <summary>
        /// Runs the path-finding algorithm and returns the nearest destination.
        /// <para>As the drones is always going to the nearest active node to deactivate it
        /// we are reading the estimation of each node still active and from this we get the nearest</para>
        /// </summary>
        /// <param name="initialNode">Node that the path-finding algorithm use as starting point</param>
        /// <returns>Returns the nearest active node to the initialNode</returns>
        public Node getNearestDestination(Node initialNode)
        {
            foreach (Node n in allNodes)
                n.initializeForPathFinding();

            initialNode.relax();

            Node nearestNode = null;
            float distance = float.MaxValue;
            foreach (Node n in activeNodes)
                if (n.getEstimation() < distance)
                {
                    distance = n.getEstimation();
                    nearestNode = n;
                }
            return nearestNode;
        }

        /// <summary>
        /// Given the nearest node (that is obtained with <c>getNearestDestination(Node initialNode)</c>
        /// the function returns a list of nodes that represents the path to get from the initial node to the final one.
        /// </summary>
        /// <param name="nearestNode">Destination node</param>
        /// <returns>Returns a list of nodes that represents the path to get from the initial node to the final one.</returns>
        public LinkedList<Node> getPathTo(Node nearestNode)
        {
            if (nearestNode == null) return null;

            LinkedList<Node> path = new LinkedList<Node>();
            while (nearestNode.getPredecessor() != null)
            {
                path.AddFirst(nearestNode);
                nearestNode = nearestNode.getPredecessor();
            }

            return path;
        }

        /// <summary>
        /// Function that runs sporadically and creates new connections between nodes
        /// </summary>
        public void recreateConnections() { foreach (Node n in allNodes) n.createConnections(); }
    }

    /// <summary>
    /// Function called before of the first update. Used to initialize the components.
    /// </summary>
    void Start () {
        dC = GameObject.FindGameObjectWithTag("dataCollector").GetComponent<dataCollector>();
        ticket = dC.initialize(transform.position);
        setSinglePointcommandResponsive(false);
        lockSinglePoint();
        setDistanceOfPointToLookAt(1000);
        myGraph = new Graph(transform.position, dC.data, NodeObject, deadNodeMaterial, doNotSpawnAnything);
	}

    float interval = 0.5f;
    float actualTime = 0.25f;
    int connectionCounter = 0;

    /// <summary>
    /// Function called once per frame.
    /// </summary>
    void Update () {

        if (FINISHED) return;

        // every time the timer tics (actualTime <= 0)..
        if ((actualTime -= Time.deltaTime) < 0)
        {
            // launches raycasts all around the drone to get informations about the walls and the empty space surrounding it
            for (float angle = 0; angle < 2 * Mathf.PI; angle += 0.02f)
                raycast(new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
            actualTime += interval;

            // updates the position of the drone in the dataCollector
            dC.updatePosition(ticket, transform.position);

            // try to expand the graph with the new information
            myGraph.expandGraph();

            // sporadically tries to create new connections
            if (++connectionCounter % 100 == 0)
                myGraph.recreateConnections(); 
        }

        // function that manage the exploration (path finding)
        pathManager();
    }


    public float raycastDistance = 50f;
    /// <summary>
    /// Perform a raycast in the given direction and update the matrix containing the information
    /// about the environment
    /// </summary>
    /// <param name="direction">Direction we are launching the raycast to</param>
    void raycast(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 vt = transform.position;
        if (Physics.Raycast(vt, direction, out hit, raycastDistance))
        {
            //Debug.DrawLine(transform.position, hit.point);            
            while (!dC.sameIndx(vt, hit.point))
            {
                dC.addDataToTheMatrix(vt, 0);                         
                vt = Vector3.MoveTowards(vt, hit.point, 1);
            }
            dC.addDataToTheMatrix(vt, 1);

        }
        else
        {
            Vector3 destination = vt + direction * raycastDistance;
            while (!dC.sameIndx(vt, destination))
            {
                dC.addDataToTheMatrix(vt, 0);
                vt = Vector3.MoveTowards(vt, destination, 1);
            }
        }        
    }


    // -- path searching -- //

    // boolean that indicates that the exploration is done.
    bool FINISHED = false;

    bool pathInitialized;    
    LinkedList<Node> path;
    Node actualNode;
    Node actualDestination;
    Node finalDestination;

    /// <summary>
    /// Returns TRUE if the drone has arrived to the destination.
    /// </summary>
    /// <param name="destination">Destination of the drone</param>
    bool isArrived(Vector3 destination) { return Vector3.Distance(transform.position, destination) < 1f; }

    float initialInterval = 5f;

    /// <summary>
    /// Function that manages the exploration through path-finding functions of the tree
    /// and moving the SinglePoint through the AI protected functions
    /// </summary>
    private void pathManager()
    {
        // We wait some time in the beggining, 
        // so the drone moves a little and starts mapping the maze
        if ((initialInterval -= Time.deltaTime) > 0)                            
        {
            setSinglePointcommandResponsive(false);
            setSinglePointPosition(transform.position - Vector3.right * 0.25f);
        }
        // calculate the path to the nearest active node
        else if (!pathInitialized)                                              
        {
            actualNode = myGraph.nearestTo(transform.position, myGraph.getCloseNeighbors(transform.position));
            finalDestination = myGraph.getNearestDestination(actualNode);
            path = myGraph.getPathTo(finalDestination);

            if (path == null)
            {
                // we have finished exploring!
                FINISHED = true;
                return;
            }
            if (path.Count < 1)
            {
                // The root is still active. We keep waiting a little bit
                initialInterval += 0.2f;
                return;
            }
            actualDestination = path.First.Value;
            path.RemoveFirst();

            setSinglePointPosition(transform.position);
            pathInitialized = true;
        }
        // when the drone arrives to the actual destination 
        // we remove it and start moving the SinglePoint to the next one
        else if (isArrived(actualDestination.getCenter())) 
        {
            if (path.Count == 0) { pathInitialized = false; initialInterval = 1; return; }

            actualNode = actualDestination;
            actualDestination = path.First.Value;
            path.RemoveFirst();            
        }
        // if the finalDestination get deactivated we look for another destination
        else if (!finalDestination.isActive())
        {
            pathInitialized = false;
            initialInterval = 1;
        }
        // if these conditions are false we simply move the singlePoint toward the actual destination
        else
        {
            if (getDistanceFromRoutePoint() < 5)
                setSinglePointPosition(Vector3.MoveTowards(getSinglePointPosition(), actualDestination.getCenter(), 2 * Time.deltaTime));

        }

    }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Level2_IA/IA.cs
````csharp
using UnityEngine;
using System.Collections;

public class IA : MonoBehaviour {

    // model and istance of the WaypointCircuit
    public GameObject baseWaypointCircuit;
    WaypointCircuit usedWaypoint;

    // model and istance of the SinglePoint
    public GameObject singlePointObject;
    singlePoint usedSinglePoint;

    WaypointProgressTracker myWpPrTrk;

    // fixed height for the WaypointCircuit
    float wpHeight = 3;

    // number of points in the WaypointCircuit
    int actualDotsQty;
    int maxDotsQty = 15;

    /// <summary>
    /// Function Called when the object is activated for the first time. Used for initializations
    /// </summary>
    void Awake () {
        myWpPrTrk = transform.GetComponent<WaypointProgressTracker>();        
        usedWaypoint = ((GameObject)Instantiate(baseWaypointCircuit, transform.position + Vector3.up * wpHeight, Quaternion.identity)).GetComponent<WaypointCircuit>();
        usedSinglePoint = ((GameObject)Instantiate(singlePointObject, transform.position + Vector3.up * wpHeight, Quaternion.identity)).GetComponent<singlePoint>();
        
        myWpPrTrk.setWaypoint(usedSinglePoint);
        actualDotsQty = usedWaypoint.transform.childCount;
	}    
    
    /// <summary>
    /// Specifies the max number of points in the circuit. When its points exceedes this number, we'll start to remove the old ones
    /// </summary>
    /// <param name="pts">Max number of points in the circuit</param>
    protected void setNrOfPointsInCircuit(int pts) { maxDotsQty = pts; }

    /// <summary>
    /// Adds a point to the circuit and eventually removes the oldest one
    /// </summary>
    /// <param name="p">Point that has to be added to the circuit</param>
    protected void addPointToCircuit(Vector3 p)
    {
        usedWaypoint.addPointAtTheEndOfTheCircuit(p);
        actualDotsQty++;
        if (actualDotsQty > maxDotsQty)
        {
            usedWaypoint.cleanOldestWaypoint();
            actualDotsQty--;
        }
        myWpPrTrk.hasToRecalculateDistance();
    }

    float pointMovingStep = 0.1f;
    /// <summary>
    /// The position of the singlePoint
    /// </summary>
    /// <returns>The position of the singlePoint</returns>
    protected Vector3 getSinglePointPosition() { return usedSinglePoint.transform.position; }
    
    /// <summary>
    /// Move the singlePoint using a direction vector
    /// </summary>
    /// <param name="v">Directional vector that indicates the direction and distance of movement</param>
    protected void moveSinglePoint(Vector3 v)
    {
        usedSinglePoint.moveSinglePoint(v);
    }

    /// <summary>
    /// Sets a new position for the SinglePoint
    /// </summary>
    /// <param name="pt">New position of SinglePoint</param>
    protected void setSinglePointPosition(Vector3 v)
    {
        usedSinglePoint.setReferencePoint(v);
    }


    /// <summary>
    /// Lock the SinglePoint so it won't move to the drone if it doesn't receibe commands
    /// </summary>
    protected void lockSinglePoint() { usedSinglePoint.moveBackToHomeDuringIdle = false; }
    
    /// <summary>
    /// Unlock the SinglePoint so it will move to the drone if it doesn't receibe commands
    /// </summary>
    protected void unlockSinglePoint() { usedSinglePoint.moveBackToHomeDuringIdle = true; }

    /// <summary>
    /// With this function we can activate o deactivate the reception of commands in the SinglePoint
    /// </summary>
    /// <param name="b">boolean that specify is the SinglePoint is going to receibe commands or not</param>
    protected void setSinglePointcommandResponsive(bool b) { usedSinglePoint.acceptCommands = b; }

    /// <summary>
    /// Sets the distance of the point the SinglePoint is using as reference for rotation. 
    /// <para> when we want the drone doesn't change its orientation we just have to set this point really far</para>
    /// </summary>
    /// <param name="f">Distance of the point the SinglePoint is using as reference for rotation</param>
    protected void setDistanceOfPointToLookAt(float f) { usedSinglePoint.distance = f; }

    /// <summary>
    /// Modify the Waypoint in the WaypointProgressTracker, putting the SinglePoint
    /// </summary>
    protected void switchToSinglePoint() { myWpPrTrk.setWaypoint(usedSinglePoint); usedSinglePoint.acceptCommands = true;  }

    /// <summary>
    /// Modify the Waypoint in the WaypointProgressTracker, putting the WaypointCircuit
    /// </summary>
    protected void switchToCircuit() { myWpPrTrk.setWaypoint(usedWaypoint); usedSinglePoint.acceptCommands = false; }

    /// <summary>
    /// Function that says if the drone has completed the circuit or not.
    /// <para>!!Now it always return FALSE!!</para>
    /// </summary>
    /// <returns>Returns true if the drone has completed the circuit, otherwise false</returns>
    protected bool hasDoneAllTheCircuit() { return false; }

    /// <summary>
    /// Says to the WaypointProgressTracker that is necessary to recalculate the actual distance, 
    /// from the starting point, of the RoutePosition in the circuit
    /// </summary>
    protected void needToRecalculateDistanceFromCircuit() { myWpPrTrk.hasToRecalculateDistance(); }

    /// <summary>
    /// Gets the distance of the drone from its routePosition
    /// </summary>
    /// <returns>The distance of the drone from its routePosition</returns>
    protected float getDistanceFromRoutePoint() { return Vector3.Distance(transform.position, myWpPrTrk.getRoutePosition()); }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Tools/configReader.cs
````csharp
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Class used to read a PID configuration and inject into the drone automatically.
/// <para>Works with <seealso cref="dataReader"/></para>
/// </summary>
public class configReader : MonoBehaviour {

    public bool readFile;
    public string folder;
    public string PathFileToRead;

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start () {

        // if the boolean readFile is true the script reads the configuration and inject it into the drone
        if (readFile)
        {
            string path = folder;
            Debug.Log(path + "\\" + PathFileToRead);
            float[,] t = dataReader.readFile(path + "\\" + PathFileToRead);
            if (t == null) { Debug.Log("Check the filename, " + path + "\\" + PathFileToRead + "- not found"); readFile = false; return; }

            
            float[] myVals = new float[t.GetUpperBound(1) + 1];
            for (int i = 0; i <= t.GetUpperBound(1); i++)
                myVals[i] = t[0, i];

            writeKeysOnDMC(myVals);

            Debug.Log("file readed");
            readFile = false;
        }
    }

    /// <summary>
    /// Sets the keys to the drone
    /// </summary>
    /// <param name="myVals">Array of float containing the parameters</param>
    protected void writeKeysOnDMC(float[] myVals)
    {
        droneMovementController dmc = GetComponent<droneMovementController>();

        PID yPID = new PID(myVals[0], myVals[1], myVals[2], myVals[3]);
        PID zPID = new PID(myVals[4], myVals[5], myVals[6], myVals[7]);
        PID xPID = new PID(myVals[4], myVals[5], myVals[6], myVals[7]);
        PID yawPID = new PID(myVals[8], myVals[9], myVals[10], myVals[11]);
        PID rollPID = new PID(myVals[12], myVals[13], myVals[14], myVals[15]);
        PID pitchPID = new PID(myVals[12], myVals[13], myVals[14], myVals[15]);

        dmc.setKs(yPID, zPID, xPID, pitchPID, rollPID, yawPID);
        dmc.setConsts(myVals[16], myVals[17], myVals[18], myVals[19], myVals[20], myVals[22], myVals[23]);
    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Tools/dataReader.cs
````csharp
using System;
using System.IO;

/// <summary>
/// Class used to read values from file
/// <para>Works with <seealso cref="dataSaver"/></para>
/// </summary>
public class dataReader
{
    /// <summary>
    /// Reads data from a file and put it in a matrix of float
    /// </summary>
    /// <param name="filename">Name of the file we want to read</param>
    /// <returns></returns>
    public static float[,] readFile(string filename)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + filename + ".csv";
        string[] buffer;
        

        if (File.Exists(path))
        {            
            buffer = File.ReadAllLines(path);
        }            
        else
            return null;

        //reads line 1
        string[] stringRes = buffer[1].Split(',');
        //reads line 2
        string[] stringPC = buffer[2].Split(',');
        float[,] res = new float[2 ,stringRes.Length - 1];

        for (int i = 0; i <= res.GetUpperBound(1); i++)
            res[0, i] = float.Parse(stringRes[i]);
        for (int i = 0; i <= res.GetUpperBound(1); i++)
            res[1, i] = float.Parse(stringPC[i]);

        return res;
    }

 
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Tools/dataSaver.cs
````csharp
using UnityEngine;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// Class used to save values to file
/// <para>Works with <seealso cref="dataReader"/></para>
/// </summary>
public class dataSaver {

    string[] attributes;
    float[,] values;
    string filename;

    public int counter = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="filename">Name of the file</param>
    /// <param name="attributes">Array of strings containing the name of the attributes</param>
    public dataSaver(string filename, string[] attributes)
    {
        this.filename = filename;
        this.attributes = attributes;
        values = new float[50000, attributes.Length];
    }

    /// <summary>
    /// Add a line to the matrix that will be saved in the file
    /// </summary>
    /// <param name="valLine">array of float conaining the data</param>
    public void addLine(float[] valLine) { for (int i = 0; i < valLine.Length; i++) values[counter , i] = valLine[i]; counter++; }
    
    /// <summary>
    /// Save the data in a file in the desktop.
    /// </summary>
    public void saveOnFile()
    {
        string[] writeThis = new string[counter + 1];
        string tmp = "";

        //Debug.Log(counter);

        foreach (string s in attributes)
            tmp += (s + ", ");
        //tmp += Environment.NewLine;
        writeThis[0] = (string) tmp.Clone();


        for (int r = 1; r <= counter; r++)
        {
            tmp = "";
            for (int c = 0; c <= values.GetUpperBound(1); c++)
                tmp += values[r - 1, c] + ", ";
            
            writeThis[r] = (string)tmp.Clone();
        }

        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + filename + ".csv";        
        // This text is added only once to the file.
        if (!File.Exists(path))
        {            
            File.WriteAllLines(path, writeThis);
        }
            
        else
        {
            File.Delete(path);
            File.WriteAllLines(path, writeThis);
        }
            

    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/drone/Tools/lineDrawer.cs
````csharp
using UnityEngine;
using System.Collections;

public class lineDrawer : MonoBehaviour {

    public int initialPosition;
    public int lengthOfLineRenderer = 20;
    private int ticket;
    private LineRenderer lr;

    /// <summary>
    /// Function called before of the first update
    /// </summary>
    void Start() {        
        lr = gameObject.GetComponent<LineRenderer>();
        lr.SetVertexCount(lengthOfLineRenderer);

        ticket = initialPosition;
         
        int t = initialPosition;
        while (t < lengthOfLineRenderer)
        {
            lr.SetPosition(t, new Vector3());
            t++;
        }
    }

    /// <summary>
    /// Assign a ticket on the lineRender component. It can be used to print a line
    /// </summary>
    public int getTicket() { int t = ticket; ticket += 2; return t; }

    /// <summary>
    /// Function used to draw a line on the lineRenderer
    /// </summary>
    /// <param name="ticket">The ticket is received at the registration moment and it is associated to a particular line</param>
    /// <param name="point">The final point of the line we want to print</param>
    public void drawPosition(int ticket, Vector3 point) { if (lr == null) return; lr.SetPosition(ticket, point); }

}
````

## File: Assets/Plugins_Terceros/Realistic Drone/cameraBehaviour.cs
````csharp
using UnityEngine;
using System.Collections;

public class cameraBehaviour : MonoBehaviour {

    public Transform followThis;

    public float radius = 10;
    public float angle = 0;

    public float runningSpeed = 5f;
    public float turningSpeed = 5f;
    public float zoomSpeed = 5f;

    public float yOffset = 5f;

    private Vector3 velocity = new Vector3();

    KeyCode UP = KeyCode.Keypad8;
    KeyCode DW = KeyCode.Keypad2;
    KeyCode RI = KeyCode.Keypad6;
    KeyCode LE = KeyCode.Keypad4;

    void Update()
    {
        if (Input.GetKey(LE)) angle -= turningSpeed * Time.deltaTime;
        if (Input.GetKey(RI)) angle += turningSpeed * Time.deltaTime;

        if (Input.GetKey(UP)) radius -= zoomSpeed * Time.deltaTime;
        if (Input.GetKey(DW)) radius += zoomSpeed * Time.deltaTime;


        float x = Mathf.Cos(angle) * radius + followThis.position.x;
        float y = followThis.position.y + radius / 2f;
        float z = Mathf.Sin(angle) * radius + followThis.position.z;

        transform.position = new Vector3(x, y, z);//  Vector3.SmoothDamp(transform.position, new Vector3(x, y, z), ref velocity, runningSpeed);// * Time.deltaTime);
        transform.LookAt(followThis);
    }


}
````

## File: Assets/Plugins_Terceros/Realistic Drone/lookAt.cs
````csharp
using UnityEngine;
using System.Collections;

public class lookAt : MonoBehaviour {

    public Transform target;
    public Vector3[] positions;
    int indx = 0;

	// Use this for initialization
	void Start () {
        if (positions.Length == 0) positions = new Vector3[] { transform.position };
        transform.position = positions[indx];
	} 
	
	// Update is called once per frame
	void Update () {
        

        if (Input.GetKeyDown(KeyCode.C))
        {
            indx = (indx + 1) % positions.Length;
            transform.position = positions[indx];
        }

        transform.LookAt(target.position);
    }
}
````

## File: Assets/Plugins_Terceros/Realistic Drone/screen.cs
````csharp
using UnityEngine;
using System.Collections;

public class screen : MonoBehaviour {

    public float tic = 0.25f;
    float timer = 0.25f;
    int i = 0;
    public int supersize = 1;

    void Start() { timer = tic; }

    void Update()
    {
        if (timer <= 0) { ScreenCapture.CaptureScreenshot("C:\\Users\\Alvin\\Desktop\\droneslife\\A_Screenshot" + i++ + ".png", supersize); timer += tic; }
        else timer -= Time.deltaTime;

    }

}
````

## File: Assets/Plugins_Terceros/Single_detailed_truck/Scripts/CameraFreeflight.cs
````csharp
using UnityEngine;
using System.Collections;

public class CameraFreeflight : MonoBehaviour 
{
    public float speedNormal = 10.0f;
    public float speedFast   = 50.0f;

    public float mouseSensitivityX = 5.0f;
	public float mouseSensitivityY = 5.0f;
    
	float rotY = 0.0f;
    
	void Start()
	{
		if (GetComponent<Rigidbody>())
			GetComponent<Rigidbody>().freezeRotation = true;
	}

	void Update()
	{
        if(Input.GetKeyDown(KeyCode.Keypad8))
            speedNormal += 1.0f;
        if(Input.GetKeyDown(KeyCode.Keypad5))
            speedNormal -= 1.0f;

        if(speedNormal < 0.0f)
            speedNormal = 0.0f;


        // rotation        
        if (Input.GetMouseButton(1)) 
        {
            float rotX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivityX;
            rotY += Input.GetAxis("Mouse Y") * mouseSensitivityY;
            rotY = Mathf.Clamp(rotY, -89.5f, 89.5f);
            transform.localEulerAngles = new Vector3(-rotY, rotX, 0.0f);
        }
        
        float forward = Input.GetAxis("Vertical");
        float strafe  = Input.GetAxis("Horizontal");
        
        // move forwards/backwards
        if (forward != 0.0f)  
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? speedFast : speedNormal;
            Vector3 trans = new Vector3(0.0f, 0.0f, forward * speed * Time.deltaTime);
            gameObject.transform.localPosition += gameObject.transform.localRotation * trans;
        }

        // strafe left/right
        if (strafe != 0.0f) 
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? speedFast : speedNormal;
            Vector3 trans = new Vector3(strafe * speed * Time.deltaTime, 0.0f, 0.0f);
            gameObject.transform.localPosition += gameObject.transform.localRotation * trans;
        }
	}
}
````

## File: Assets/StreamingAssets/lote_260_pallets.json
````json
{
    "id_lote": "LOTE-ERP-260",
    "total_pallets": 260,
    "peso_total_kg": 221000,
    "pedidos": [
        {
            "id_pallet": "PAL-8491",
            "nombre_producto": "CRubia Aguila (Lata 330ml)",
            "codigo_sku": "AGU-RUB-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C1-F1"
        },
        {
            "id_pallet": "PAL-8492",
            "nombre_producto": "CNegra ClubColombia (Botella 330ml)",
            "codigo_sku": "CLU-NEG-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C2-F1"
        },
        {
            "id_pallet": "PAL-8493",
            "nombre_producto": "CEspecial Oktoberfest (Barril 50L)",
            "codigo_sku": "OKT-BAR-50L",
            "peso_kg": 920,
            "coordenada_bodega": "P2-C4-F1"
        }
    ]
}
````

## File: Assets/StreamingAssets/lote_30_pallets.json
````json
{
    "id_lote": "LOTE-ERP-030",
    "total_pallets": 30,
    "peso_total_kg": 25500,
    "pedidos": [
        {
            "id_pallet": "PAL-8491",
            "nombre_producto": "CRubia Aguila (Lata 330ml)",
            "codigo_sku": "AGU-RUB-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C1-F1"
        },
        {
            "id_pallet": "PAL-8492",
            "nombre_producto": "CNegra ClubColombia (Botella 330ml)",
            "codigo_sku": "CLU-NEG-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C2-F1"
        },
        {
            "id_pallet": "PAL-8493",
            "nombre_producto": "CEspecial Oktoberfest (Barril 50L)",
            "codigo_sku": "OKT-BAR-50L",
            "peso_kg": 920,
            "coordenada_bodega": "P2-C4-F1"
        }
    ]
}
````

## File: Assets/StreamingAssets/lote_450_pallets.json
````json
{
    "id_lote": "LOTE-ERP-450",
    "total_pallets": 450,
    "peso_total_kg": 382500,
    "pedidos": [
        {
            "id_pallet": "PAL-8491",
            "nombre_producto": "CRubia Aguila (Lata 330ml)",
            "codigo_sku": "AGU-RUB-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C1-F1"
        },
        {
            "id_pallet": "PAL-8492",
            "nombre_producto": "CNegra ClubColombia (Botella 330ml)",
            "codigo_sku": "CLU-NEG-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C2-F1"
        },
        {
            "id_pallet": "PAL-8493",
            "nombre_producto": "CEspecial Oktoberfest (Barril 50L)",
            "codigo_sku": "OKT-BAR-50L",
            "peso_kg": 920,
            "coordenada_bodega": "P2-C4-F1"
        }
    ]
}
````

## File: Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs
````csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor
{
    static string s_ShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
    
    static string s_ReadmeSourceDirectory = "Assets/TutorialInfo";

    const float k_Space = 16f;

    static ReadmeEditor()
    {
        EditorApplication.delayCall += SelectReadmeAutomatically;
    }

    static void RemoveTutorial()
    {
        if (EditorUtility.DisplayDialog("Remove Readme Assets",
            
            $"All contents under {s_ReadmeSourceDirectory} will be removed, are you sure you want to proceed?",
            "Proceed",
            "Cancel"))
        {
            if (Directory.Exists(s_ReadmeSourceDirectory))
            {
                FileUtil.DeleteFileOrDirectory(s_ReadmeSourceDirectory);
                FileUtil.DeleteFileOrDirectory(s_ReadmeSourceDirectory + ".meta");
            }
            else
            {
                Debug.Log($"Could not find the Readme folder at {s_ReadmeSourceDirectory}");
            }

            var readmeAsset = SelectReadme();
            if (readmeAsset != null)
            {
                var path = AssetDatabase.GetAssetPath(readmeAsset);
                FileUtil.DeleteFileOrDirectory(path + ".meta");
                FileUtil.DeleteFileOrDirectory(path);
            }

            AssetDatabase.Refresh();
        }
    }

    static void SelectReadmeAutomatically()
    {
        if (!SessionState.GetBool(s_ShowedReadmeSessionStateName, false))
        {
            var readme = SelectReadme();
            SessionState.SetBool(s_ShowedReadmeSessionStateName, true);

            if (readme && !readme.loadedLayout)
            {
                LoadLayout();
                readme.loadedLayout = true;
            }
        }
    }

    static void LoadLayout()
    {
        var assembly = typeof(EditorApplication).Assembly;
        var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
        var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
    }

    static Readme SelectReadme()
    {
        var ids = AssetDatabase.FindAssets("Readme t:Readme");
        if (ids.Length == 1)
        {
            var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

            Selection.objects = new UnityEngine.Object[] { readmeObject };

            return (Readme)readmeObject;
        }
        else
        {
            Debug.Log("Couldn't find a readme");
            return null;
        }
    }

    protected override void OnHeaderGUI()
    {
        var readme = (Readme)target;
        Init();

        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

        GUILayout.BeginHorizontal("In BigTitle");
        {
            if (readme.icon != null)
            {
                GUILayout.Space(k_Space);
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
            }
            GUILayout.Space(k_Space);
            GUILayout.BeginVertical();
            {

                GUILayout.FlexibleSpace();
                GUILayout.Label(readme.title, TitleStyle);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        var readme = (Readme)target;
        Init();

        foreach (var section in readme.sections)
        {
            if (!string.IsNullOrEmpty(section.heading))
            {
                GUILayout.Label(section.heading, HeadingStyle);
            }

            if (!string.IsNullOrEmpty(section.text))
            {
                GUILayout.Label(section.text, BodyStyle);
            }

            if (!string.IsNullOrEmpty(section.linkText))
            {
                if (LinkLabel(new GUIContent(section.linkText)))
                {
                    Application.OpenURL(section.url);
                }
            }

            GUILayout.Space(k_Space);
        }

        if (GUILayout.Button("Remove Readme Assets", ButtonStyle))
        {
            RemoveTutorial();
        }
    }

    bool m_Initialized;

    GUIStyle LinkStyle
    {
        get { return m_LinkStyle; }
    }

    [SerializeField]
    GUIStyle m_LinkStyle;

    GUIStyle TitleStyle
    {
        get { return m_TitleStyle; }
    }

    [SerializeField]
    GUIStyle m_TitleStyle;

    GUIStyle HeadingStyle
    {
        get { return m_HeadingStyle; }
    }

    [SerializeField]
    GUIStyle m_HeadingStyle;

    GUIStyle BodyStyle
    {
        get { return m_BodyStyle; }
    }

    [SerializeField]
    GUIStyle m_BodyStyle;

    GUIStyle ButtonStyle
    {
        get { return m_ButtonStyle; }
    }

    [SerializeField]
    GUIStyle m_ButtonStyle;

    void Init()
    {
        if (m_Initialized)
            return;
        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;
        m_BodyStyle.richText = true;

        m_TitleStyle = new GUIStyle(m_BodyStyle);
        m_TitleStyle.fontSize = 26;

        m_HeadingStyle = new GUIStyle(m_BodyStyle);
        m_HeadingStyle.fontStyle = FontStyle.Bold;
        m_HeadingStyle.fontSize = 18;

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;

        // Match selection color which works nicely for both light and dark skins
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

        m_ButtonStyle = new GUIStyle(EditorStyles.miniButton);
        m_ButtonStyle.fontStyle = FontStyle.Bold;

        m_Initialized = true;
    }

    bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, LinkStyle);
    }
}
````

## File: Assets/TutorialInfo/Scripts/Readme.cs
````csharp
using System;
using UnityEngine;

public class Readme : ScriptableObject
{
    public Texture2D icon;
    public string title;
    public Section[] sections;
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading, text, linkText, url;
    }
}
````

## File: ProjectSettings/ProjectVersion.txt
````
m_EditorVersion: 2022.3.62f3
m_EditorVersionWithRevision: 2022.3.62f3 (96770f904ca7)
````

## File: Assets/_ProyectoBodega/Scripts/DashboardUI.cs
````csharp
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

    public void IniciarTurnoNocturno()
    {
        if (panelMenuModulos != null) { panelMenuModulos.SetActive(false); }
        
        GameObject menuPrincipal = GameObject.Find("Canvas_MenuPrincipal");
        if (menuPrincipal != null) menuPrincipal.SetActive(false);

        Transform hud = transform.Find("HUD_Operativo");
        if (hud != null) hud.gameObject.SetActive(true);

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
        tipoAlertaActual = TipoAlertaActiva.BateriaCriticaAMR;
        MostrarEmergencia(mostrar);
        
        if (mostrar && textoDetalleEmergencia != null)
        {
            string mensaje = "╔════════════════════════════════════════════════════════════════════════════════════════╗\n" +
                             "║             ANOMALÍA PREDICTIVA: VIABILIDAD OPERATIVA COMPROMETIDA                     ║\n" +
                             $"║  Retorno estimado: {bateriaProyectada:F1}% (±{varianzaMargen:F1}% tolerancia térmica)  |  Límite seguro: 20.0%            ║\n" +
                             "╠════════════════════════════════════════════════════════════════════════════════════════╣\n" +
                             "║  MATRIZ DE TRADE-OFF GERENCIAL (DECISIÓN SUPERVISOR - INDUSTRIA 5.0):                  ║\n" +
                             "║                                                                                        ║\n" +
                             "║  • [OPCIÓN A: FORZAR OVERRIDE]                                                         ║\n" +
                             "║    Consumo motor: +30% | Aceleración: -50% | Degradación térmica de celda: ALTA.       ║\n" +
                             "║                                                                                        ║\n" +
                             "║  • [OPCIÓN B: ACTIVAR RELEVO 24/7]                                                     ║\n" +
                             "║    Retirar AMR-01 a carga inductiva. Desplegar AMR-02 (80%) para asumir el pedido.     ║\n" +
                             "╚════════════════════════════════════════════════════════════════════════════════════════╝";
            
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
            string mensaje;
            if (escenario == RobotManagerEVE.EscenarioHorizonte.EscenarioA_InicioTurno)
            {
                mensaje = "╔════════════════════════════════════════════════════════════════════════════════════════╗\n" +
                          "║       ANALIZADOR HORIZONTE [ESCENARIO A - INICIO TURNO]: VIABILIDAD SLA OK             ║\n" +
                          $"║  Reloj: Hora {horaActual:F1}/8 (Restan {horasRestantes:F1}h) | Lote: {nuevosPedidos} ped ({pesoLoteKg:F0} kg) | Déficit: -{deficitSoC:F1}% SoC   ║\n" +
                          "║  Dictamen: Tiempo suficiente antes del amanecer, pero demanda (192.5%) supera el 60%  ║\n" +
                          "║  nominal por unidad. Flota ideal: 3 AMRs (2 activos + 1 soporte/relevo inductivo).    ║\n" +
                          "╠════════════════════════════════════════════════════════════════════════════════════════╣\n" +
                          "║  DECISIÓN GERENCIAL DE HORIZONTE TEMPRANO (INDUSTRIA 5.0):                             ║\n" +
                          "║                                                                                        ║\n" +
                          "║  • [OPCIÓN A: DIFERIR EXCEDENTE / MANTENER 2 AMRs]                                     ║\n" +
                          "║    Limitar operación al SoC actual sin habilitar el 3er AMR de relevo rotativo.        ║\n" +
                          "║                                                                                        ║\n" +
                          "║  • [OPCIÓN B: APROBAR FLOTA DE 3 AMRs (2 ACTIVOS + 1 RELEVO)]                          ║\n" +
                          "║    Cumplir 100% del SLA (0% Carry-over) con ciclos de carga de oportunidad 1C.         ║\n" +
                          "╚════════════════════════════════════════════════════════════════════════════════════════╝";
            }
            else if (escenario == RobotManagerEVE.EscenarioHorizonte.EscenarioB_Madrugada)
            {
                mensaje = "╔════════════════════════════════════════════════════════════════════════════════════════╗\n" +
                          "║      ANALIZADOR HORIZONTE [ESCENARIO B - MADRUGADA]: RIESGO MEDIO DE CORTE SLA         ║\n" +
                          $"║  Reloj: Hora {horaActual:F1}/8 (Restan {horasRestantes:F1}h) | Rezago proyectado: {porcentajeCarryOver:F0}% | Déficit: -{deficitSoC:F1}% SoC ║\n" +
                          $"║  Dictamen: Tasa de absorción actual (10 ped/h) dejará {porcentajeCarryOver:F0}% del lote sin entregar     ║\n" +
                          "║  al corte (06:00 AM). Incrementar a 4 AMRs o autorizar Carry-over al turno diurno.    ║\n" +
                          "╠════════════════════════════════════════════════════════════════════════════════════════╣\n" +
                          "║  DECISIÓN GERENCIAL DE MADRUGADA (TRADE-OFF SLA vs. CAPEX):                            ║\n" +
                          "║                                                                                        ║\n" +
                          $"║  • [OPCIÓN A: AUTORIZAR CARRY-OVER ({porcentajeCarryOver:F0}% REZAGO AL TURNO DIURNO)]                    ║\n" +
                          "║    Mantener 2 AMRs actuales hasta las 06:00 AM y transferir el saldo pendiente.        ║\n" +
                          "║                                                                                        ║\n" +
                          $"║  • [OPCIÓN B: INCREMENTAR FLOTA A {amrRequeridos} AMRs OPERATIVOS]                                  ║\n" +
                          "║    Duplicar tasa de absorción (20 ped/h) para eliminar el Carry-over antes de 06:00 AM.║\n" +
                          "╚════════════════════════════════════════════════════════════════════════════════════════╝";
            }
            else
            {
                mensaje = "╔════════════════════════════════════════════════════════════════════════════════════════╗\n" +
                          "║       ANALIZADOR HORIZONTE [ESCENARIO C - FINAL TURNO]: COLAPSO LOGÍSTICO / SLA        ║\n" +
                          $"║  Reloj: Hora {horaActual:F1}/8 (Restan {horasRestantes:F1}h) | Déficit Crítico LiFePO4: -{deficitSoC:F1}% SoC             ║\n" +
                          $"║  Dictamen: Faltan 30 min y celdas en reserva. Capacidad actual colapsa ({porcentajeCarryOver:F0}% rezago).   ║\n" +
                          $"║  Se requieren {amrRequeridos} AMRs de emergencia para mantener HBF o diferir el 100% del lote.      ║\n" +
                          "╠════════════════════════════════════════════════════════════════════════════════════════╣\n" +
                          "║  DECISIÓN GERENCIAL DE EMERGENCIA (CIERRE DE TURNO NOCTURNO):                          ║\n" +
                          "║                                                                                        ║\n" +
                          "║  • [OPCIÓN A: DIFERIR LOTE COMPLETO A TURNO DIURNO]                                    ║\n" +
                          "║    Proteger SLA actual y piso térmico del 20% LiFePO4 en las 2 unidades en pista.      ║\n" +
                          "║                                                                                        ║\n" +
                          $"║  • [OPCIÓN B: APROBAR {amrRequeridos} AMRs DE EMERGENCIA (+1 BAHÍA TMS)]                             ║\n" +
                          "║    Despliegue táctico inmediato para absorber las 42.5 t preservando el HBF.           ║\n" +
                          "╚════════════════════════════════════════════════════════════════════════════════════════╝";
            }

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
    }

    private void InicializarPanelesColapsables()
    {
        if (panelManifiestoRect == null)
        {
            var go = GameObject.Find("UI_Dashboard_Corporativo/HUD_Operativo/Manifiesto");
            if (go != null) panelManifiestoRect = go.GetComponent<RectTransform>();
        }
        if (panelFlotaRect == null)
        {
            var go = GameObject.Find("UI_Dashboard_Corporativo/HUD_Operativo/Panel_Flota");
            if (go != null) panelFlotaRect = go.GetComponent<RectTransform>();
        }
        if (panelTerminalRect == null)
        {
            var go = GameObject.Find("UI_Dashboard_Corporativo/HUD_Operativo/Panel_Terminal_EVE");
            if (go != null) panelTerminalRect = go.GetComponent<RectTransform>();
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
        rt.sizeDelta = new Vector2(560f, 190f);
        rt.anchoredPosition = new Vector2(0f, 120f);

        Image bg = panelPopupSLA.GetComponent<Image>();
        bg.color = new Color(0.06f, 0.09f, 0.14f, 0.96f);

        GameObject goTitulo = new GameObject("Txt_Titulo_Popup_SLA", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        goTitulo.transform.SetParent(panelPopupSLA.transform, false);
        RectTransform rtTitulo = goTitulo.GetComponent<RectTransform>();
        rtTitulo.anchorMin = new Vector2(0f, 1f);
        rtTitulo.anchorMax = new Vector2(1f, 1f);
        rtTitulo.pivot = new Vector2(0.5f, 1f);
        rtTitulo.sizeDelta = new Vector2(-120f, 34f);
        rtTitulo.anchoredPosition = new Vector2(-40f, -12f);
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
        txtDetallePopupSLA.alignment = TextAlignmentOptions.TopLeft;
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
}
````

## File: Assets/_ProyectoBodega/Scripts/DronMantenimiento.cs
````csharp
using System;
using System.Collections;
using UnityEngine;

public class DronMantenimiento : MonoBehaviour
{
    [Header("Topografía Base Aérea")]
    public Vector3 posicionBaseTecho = new Vector3(-32.0f, 10.0f, -38.0f);
    public Quaternion rotacionBase = Quaternion.identity;

    [Header("Parámetros de Intervención")]
    public float alturaAcopleSobreObstaculo = 1.76f;

    [Header("Componentes Visuales")]
    public DronePropellers controlHelices;

    [Header("Estado Operativo")]
    public bool ocupado = false;

    void Awake()
    {
        if (controlHelices == null)
        {
            controlHelices = GetComponentInChildren<DronePropellers>();
        }

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
            controlHelices.rotationAxis = new Vector3(0f, 0f, 1f);
        }
    }

    /// <summary>
    /// Corrutina de despeje autónomo con perfil de vuelo ortogonal en "U" (12.0 segundos totales):
    /// - Etapa 1A (2.0s): Tránsito horizontal a cota de techo Y=10m desde la base hasta el cenit del obstáculo.
    /// - Etapa 1B (2.5s): Descenso vertical puro (X/Z fijos) desde el cenit hasta posAcople (+1.76m sobre base del obstáculo).
    /// - Etapa 2  (1.5s): Acople magnético (SetParent), desactivación de BoxCollider, elevación vertical inicial (+1.0m) y reanudación temprana WMS.
    /// - Etapa 3A (3.5s): Ascenso vertical puro con carga hasta el techo (posCenit Y=10m).
    /// - Etapa 3B (2.5s): Tránsito horizontal con carga hasta la base aérea (-32, 10, -38), desemparentar y ocultar.
    /// </summary>
    public IEnumerator EjecutarMisionDespeje(GameObject obstaculo, Action onDespejado = null, float duracionTotal = 12.0f)
    {
        if (obstaculo == null) yield break;

        ocupado = true;
        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
        }

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        Vector3 posObs = obstaculo.transform.position;
        Vector3 posCenit = new Vector3(posObs.x, posicionBaseTecho.y, posObs.z);
        Vector3 posAcople = new Vector3(posObs.x, posObs.y + alturaAcopleSobreObstaculo, posObs.z);

        Collider colObstaculo = obstaculo.GetComponent<Collider>();

        // -------------------------------------------------------------
        // ETAPA 1A (2.0s): Tránsito horizontal en techo hasta posCenit
        // -------------------------------------------------------------
        const float durEtapa1A = 2.0f;
        Vector3 posInicio1A = transform.position;
        float dist1A = Vector3.Distance(posInicio1A, posCenit);
        float vel1A = dist1A > 0.001f ? (dist1A / durEtapa1A) : 5f;

        Vector3 dir1A = (posCenit - posInicio1A);
        dir1A.y = 0f;
        Quaternion rotMirarCenit = dir1A.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir1A) : rotacionBase;

        float t1a = 0f;
        while (t1a < durEtapa1A)
        {
            float dt = Time.deltaTime;
            t1a += dt;
            float factor = Mathf.Clamp01(t1a / durEtapa1A);

            transform.position = Vector3.MoveTowards(transform.position, posCenit, vel1A * dt);
            Quaternion rotObjetivo = factor < 0.75f ? rotMirarCenit : rotacionBase;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 5f);

            yield return null;
        }
        transform.position = posCenit;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // ETAPA 1B (2.5s): Descenso vertical puro (X/Z fijos) hacia posAcople
        // -------------------------------------------------------------
        const float durEtapa1B = 2.5f;
        float t1b = 0f;
        while (t1b < durEtapa1B)
        {
            float dt = Time.deltaTime;
            t1b += dt;
            float factor = Mathf.Clamp01(t1b / durEtapa1B);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            float yActual = Mathf.Lerp(posCenit.y, posAcople.y, smooth);
            transform.position = new Vector3(posObs.x, yActual, posObs.z);
            transform.rotation = rotacionBase;

            yield return null;
        }
        transform.position = posAcople;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // ETAPA 2 (1.5s): Acople, desactivación de collider, elevación vertical 1.0m y reanudación temprana
        // -------------------------------------------------------------
        const float durEtapa2 = 1.5f;
        if (colObstaculo != null)
        {
            colObstaculo.enabled = false;
        }

        obstaculo.transform.SetParent(transform, true);

        Vector3 posAcopleInicio = transform.position;
        Vector3 posAcopleElevado = posAcopleInicio + Vector3.up * 1.0f;

        float t2 = 0f;
        while (t2 < durEtapa2)
        {
            float dt = Time.deltaTime;
            t2 += dt;
            float factor = Mathf.Clamp01(t2 / durEtapa2);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(posAcopleInicio, posAcopleElevado, smooth);
            transform.rotation = rotacionBase;

            yield return null;
        }
        transform.position = posAcopleElevado;
        transform.rotation = rotacionBase;

        // Reanudación temprana del AMR: el obstáculo ya está elevado 1m y su collider apagado
        onDespejado?.Invoke();

        // -------------------------------------------------------------
        // ETAPA 3A (3.5s): Ascenso vertical puro con carga hasta el techo (posCenit)
        // -------------------------------------------------------------
        const float durEtapa3A = 3.5f;
        Vector3 posInicio3A = transform.position;
        float t3a = 0f;
        while (t3a < durEtapa3A)
        {
            float dt = Time.deltaTime;
            t3a += dt;
            float factor = Mathf.Clamp01(t3a / durEtapa3A);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            float yActual = Mathf.Lerp(posInicio3A.y, posCenit.y, smooth);
            transform.position = new Vector3(posObs.x, yActual, posObs.z);
            transform.rotation = rotacionBase;

            yield return null;
        }
        transform.position = posCenit;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // ETAPA 3B (2.5s): Tránsito horizontal en techo hasta la base (-32, 10, -38)
        // -------------------------------------------------------------
        const float durEtapa3B = 2.5f;
        Vector3 posInicio3B = transform.position;
        float dist3B = Vector3.Distance(posInicio3B, posicionBaseTecho);
        float vel3B = dist3B > 0.001f ? (dist3B / durEtapa3B) : 5f;

        Vector3 dir3B = (posicionBaseTecho - posInicio3B);
        dir3B.y = 0f;
        Quaternion rotMirarBase = dir3B.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir3B) : rotacionBase;

        float t3b = 0f;
        while (t3b < durEtapa3B)
        {
            float dt = Time.deltaTime;
            t3b += dt;
            float factor = Mathf.Clamp01(t3b / durEtapa3B);

            transform.position = Vector3.MoveTowards(transform.position, posicionBaseTecho, vel3B * dt);
            Quaternion rotObjetivo = factor < 0.75f ? rotMirarBase : rotacionBase;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, Time.deltaTime * 5f);

            yield return null;
        }
        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;

        // -------------------------------------------------------------
        // FINALIZACIÓN: Desemparentar obstáculo, restaurar collider y ocultar
        // -------------------------------------------------------------
        if (obstaculo != null)
        {
            obstaculo.transform.SetParent(null, true);
            if (colObstaculo != null)
            {
                colObstaculo.enabled = true;
            }
            obstaculo.SetActive(false);
        }

        ocupado = false;
    }

    /// <summary>
    /// Corrutina de rescate aéreo de 5 etapas para un AMR colapsado (&lt; 3% BMS):
    /// - Etapa 1A (2.0s): Tránsito horizontal a cota Y=10.0m hasta el cenit del AMR apagado.
    /// - Etapa 1B (2.0s): Descenso vertical puro hasta acoplarse sobre el chasis (+1.15m).
    /// - Etapa 2  (2.5s): Acople magnético (SetParent) y elevación vertical hasta Y=8.5m.
    /// - Etapa 3  (3.0s): Tránsito horizontal aéreo a Y=8.5m hasta el cenit del Pad Inductivo destino.
    /// - Etapa 4  (2.0s): Descenso vertical hasta posar al AMR en posPadDestino, desemparentar y reactivar en Pad.
    /// - Etapa 5  (2.0s): Ascenso vertical a Y=10.0m y retorno a la base aérea (-32, 10, -38).
    /// </summary>
    public IEnumerator EjecutarRescateAMRAbortado(RobotAMR amrApagado, Vector3 posPadDestino, Action onRescateCompletado = null, Action onElevacionCompletada = null)
    {
        if (amrApagado == null) yield break;

        while (ocupado)
        {
            yield return null;
        }

        ocupado = true;
        if (controlHelices != null)
        {
            controlHelices.isEngineOn = true;
        }

        Vector3 posAMR = amrApagado.transform.position;
        Vector3 posCenitAMR = new Vector3(posAMR.x, posicionBaseTecho.y, posAMR.z);
        Vector3 posAcopleAMR = new Vector3(posAMR.x, posAMR.y + 1.15f, posAMR.z);
        Vector3 posElevadoAMR = new Vector3(posAMR.x, 8.5f, posAMR.z);
        Vector3 posCenitPad85 = new Vector3(posPadDestino.x, 8.5f, posPadDestino.z);
        Vector3 posDescensoPad = new Vector3(posPadDestino.x, posPadDestino.y + 1.15f, posPadDestino.z);
        Vector3 posCenitPadTecho = new Vector3(posPadDestino.x, posicionBaseTecho.y, posPadDestino.z);

        // Etapa 1A (2.0s): Vuelo a Y = 10.0m hasta el cenit del AMR apagado
        yield return StartCoroutine(MoverTramoDron(transform.position, posCenitAMR, 2.0f, true));

        // Etapa 1B (2.0s): Descenso vertical puro hasta Y_amr + 1.15m
        yield return StartCoroutine(MoverTramoDron(posCenitAMR, posAcopleAMR, 2.0f, false));

        // Etapa 2 (2.5s): Emparentar AMR apagado y elevar verticalmente hasta Y = 8.5m
        amrApagado.transform.SetParent(transform, true);
        yield return StartCoroutine(MoverTramoDron(posAcopleAMR, posElevadoAMR, 2.5f, false));
        onElevacionCompletada?.Invoke();

        // Etapa 3 (3.0s): Tránsito horizontal a Y = 8.5m hasta el cenit de posPadDestino
        yield return StartCoroutine(MoverTramoDron(posElevadoAMR, posCenitPad85, 3.0f, true));

        // Etapa 4 (2.0s): Descenso vertical hasta posar al AMR en posPadDestino, desemparentar y finalizar rescate
        yield return StartCoroutine(MoverTramoDron(posCenitPad85, posDescensoPad, 2.0f, false));

        amrApagado.transform.SetParent(null, true);
        amrApagado.FinalizarRescateEnPad(posPadDestino);
        onRescateCompletado?.Invoke();

        // Etapa 5 (2.0s): Ascenso a Y = 10.0m y retorno a posicionBaseTecho
        yield return StartCoroutine(MoverTramoDron(posDescensoPad, posCenitPadTecho, 0.7f, false));
        yield return StartCoroutine(MoverTramoDron(posCenitPadTecho, posicionBaseTecho, 1.3f, true));

        transform.position = posicionBaseTecho;
        transform.rotation = rotacionBase;
        ocupado = false;
    }

    private IEnumerator MoverTramoDron(Vector3 origen, Vector3 destino, float duracion, bool rotarHaciaDestino)
    {
        float dur = Mathf.Max(0.05f, duracion);
        Vector3 dirPlana = destino - origen;
        dirPlana.y = 0f;
        Quaternion rotMirar = dirPlana.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dirPlana) : rotacionBase;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float factor = Mathf.Clamp01(t / dur);
            float smooth = Mathf.SmoothStep(0f, 1f, factor);

            transform.position = Vector3.Lerp(origen, destino, smooth);
            if (rotarHaciaDestino)
            {
                Quaternion rotObj = factor < 0.75f ? rotMirar : rotacionBase;
                transform.rotation = Quaternion.Slerp(transform.rotation, rotObj, Time.deltaTime * 5f);
            }
            else
            {
                transform.rotation = rotacionBase;
            }
            yield return null;
        }

        transform.position = destino;
        transform.rotation = rotacionBase;
    }
}
````

## File: Assets/_ProyectoBodega/Scripts/GeneradorTablaManifiesto.cs
````csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;

public class GeneradorTablaManifiesto : MonoBehaviour
{
    [Header("Configuración de UI")]
    [SerializeField] private Transform contenedorFilas; // Content_Filas dentro del ScrollRect
    [SerializeField] private GameObject filaPrefab;     // Prefab de fila operativa
    [SerializeField] private ScrollRect scrollManifiesto;

    private GameObject filaResumenGO;
    private TextMeshProUGUI[] columnasResumenLote;
    private int contadorCorrelativoPallet = 8491;
    private int filasOperativasInstanciadas = 0;

    private void ResolverContenedorYScroll()
    {
        if (contenedorFilas == null || contenedorFilas == transform || contenedorFilas.name == "Manifiesto")
        {
            Transform content = transform.Find("Scroll_Manifiesto/Viewport/Content_Filas");
            if (content != null)
            {
                contenedorFilas = content;
            }
            else if (contenedorFilas == null)
            {
                contenedorFilas = transform;
            }
        }

        if (scrollManifiesto == null)
        {
            scrollManifiesto = GetComponentInChildren<ScrollRect>(true);
        }
    }

    public void LimpiarTablaStandby()
    {
        ResolverContenedorYScroll();

        for (int i = contenedorFilas.childCount - 1; i >= 0; i--)
        {
            Transform child = contenedorFilas.GetChild(i);
            if (child.name == "Header_Manifiesto" || child.name == "Fila_Titulos")
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        filaResumenGO = null;
        columnasResumenLote = null;
        contadorCorrelativoPallet = 8491;
        filasOperativasInstanciadas = 0;

        if (DashboardUI.Instance != null && DashboardUI.Instance.textosEstadoManifiesto != null)
        {
            DashboardUI.Instance.textosEstadoManifiesto.Clear();
        }
    }

    // Mantiene compatibilidad sin destruir filas previas durante el turno
    public void PoblarTabla(List<Pedido> pedidosJSON, int totalPalletsLote = 0)
    {
        int nuevosPallets = totalPalletsLote > 0 ? totalPalletsLote : (pedidosJSON != null ? pedidosJSON.Count : 0);
        AgregarLoteAManifiesto(pedidosJSON, nuevosPallets, nuevosPallets);
    }

    /// <summary>
    /// Agrega un lote nuevo de forma acumulativa en orden FIFO sin destruir el historial de pedidos anteriores.
    /// </summary>
    public void AgregarLoteAManifiesto(List<Pedido> plantillaBase, int nuevosPallets, int totalPalletsEnColaWMS)
    {
        ResolverContenedorYScroll();
        if (filaPrefab == null || nuevosPallets <= 0) return;

        int filasAAgregar = Mathf.Min(3, nuevosPallets);
        for (int i = 0; i < filasAAgregar; i++)
        {
            Pedido plantilla = (plantillaBase != null && plantillaBase.Count > 0)
                ? plantillaBase[i % plantillaBase.Count]
                : null;
            InstanciarFilaOperativa(plantilla);
        }

        ActualizarResumenColaEnVivo(totalPalletsEnColaWMS);
        ForzarLayoutYScroll(indiceObjetivo: Mathf.Max(0, filasOperativasInstanciadas - 1));
    }

    /// <summary>
    /// Garantiza que exista una fila operativa instantiated para el índice secuencial de misión y enfoca el scroll en ella.
    /// </summary>
    public int AsegurarFilaOperativaParaMision(int indiceSecuencial, Pedido plantilla, int palletsRestantesEnCola)
    {
        ResolverContenedorYScroll();
        if (DashboardUI.Instance == null || DashboardUI.Instance.textosEstadoManifiesto == null)
        {
            return indiceSecuencial;
        }

        int guard = 0;
        while (indiceSecuencial >= DashboardUI.Instance.textosEstadoManifiesto.Count && guard++ < 8)
        {
            InstanciarFilaOperativa(plantilla);
        }

        ActualizarResumenColaEnVivo(palletsRestantesEnCola);
        ForzarLayoutYScroll(indiceObjetivo: indiceSecuencial);
        return indiceSecuencial;
    }

    private void InstanciarFilaOperativa(Pedido plantilla)
    {
        if (filaPrefab == null || contenedorFilas == null) return;

        int numeroPallet = contadorCorrelativoPallet++;
        GameObject nuevaFila = Instantiate(filaPrefab, contenedorFilas);
        nuevaFila.name = $"Fila_PAL_{numeroPallet}";
        AsegurarAlturaFila(nuevaFila);

        TextMeshProUGUI[] columnas = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();
        if (columnas.Length >= 5)
        {
            columnas[0].text = $"PAL-{numeroPallet}";
            columnas[1].text = plantilla != null && !string.IsNullOrEmpty(plantilla.nombre_producto) ? plantilla.nombre_producto : "Carga Industrial";
            columnas[2].text = plantilla != null && !string.IsNullOrEmpty(plantilla.codigo_sku) ? plantilla.codigo_sku : "SKU-STD";
            float peso = plantilla != null && plantilla.peso_kg > 0f ? plantilla.peso_kg : 420f;
            columnas[3].text = peso.ToString("F0") + " kg";

            for (int c = 0; c < 4; c++)
            {
                if (columnas[c] == null) continue;
                columnas[c].color = Color.white;
                columnas[c].fontSize = 11f;
            }

            columnas[4].text = "PENDIENTE";
            columnas[4].color = new Color(0.94f, 0.27f, 0.27f, 1f);
            columnas[4].fontSize = 11f;

            if (DashboardUI.Instance != null && DashboardUI.Instance.textosEstadoManifiesto != null)
            {
                DashboardUI.Instance.textosEstadoManifiesto.Add(columnas[4]);
            }
        }

        filasOperativasInstanciadas++;
        if (filaResumenGO != null)
        {
            filaResumenGO.transform.SetAsLastSibling();
        }
    }

    private void AsegurarAlturaFila(GameObject fila)
    {
        if (fila == null) return;
        LayoutElement le = fila.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = fila.AddComponent<LayoutElement>();
            RectTransform rt = fila.GetComponent<RectTransform>();
            float alto = rt != null && rt.sizeDelta.y > 10f ? rt.sizeDelta.y : 44f;
            le.minHeight = alto;
            le.preferredHeight = alto;
            le.flexibleHeight = 0f;
        }
    }

    private int ContarFilasPendientesOEnTransito()
    {
        if (DashboardUI.Instance == null || DashboardUI.Instance.textosEstadoManifiesto == null) return 0;

        int activas = 0;
        var lista = DashboardUI.Instance.textosEstadoManifiesto;
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i] == null) continue;
            string estado = lista[i].text;
            if (estado == "PENDIENTE" || estado == "EN TRÁNSITO")
            {
                activas++;
            }
        }
        return activas;
    }

    public void ActualizarResumenColaEnVivo(int palletsRestantesEnCola)
    {
        ResolverContenedorYScroll();
        int sinInstanciar = Mathf.Max(0, palletsRestantesEnCola - ContarFilasPendientesOEnTransito());

        if (sinInstanciar > 0)
        {
            if (filaResumenGO == null && filaPrefab != null && contenedorFilas != null)
            {
                filaResumenGO = Instantiate(filaPrefab, contenedorFilas);
                filaResumenGO.name = "Fila_Resumen_Lote";
                AsegurarAlturaFila(filaResumenGO);
                columnasResumenLote = filaResumenGO.GetComponentsInChildren<TextMeshProUGUI>();
            }

            if (filaResumenGO != null)
            {
                filaResumenGO.SetActive(true);
                filaResumenGO.transform.SetAsLastSibling();
            }

            if (columnasResumenLote != null && columnasResumenLote.Length >= 5)
            {
                columnasResumenLote[0].text = $"+{sinInstanciar} EN COLA WMS";
                columnasResumenLote[1].text = "Lote ERP";
                columnasResumenLote[2].text = "CONSOLIDADO";
                columnasResumenLote[3].text = $"{sinInstanciar * 420f:F0} kg";
                for (int c = 0; c < 4; c++)
                {
                    if (columnasResumenLote[c] == null) continue;
                    columnasResumenLote[c].color = Color.white;
                    columnasResumenLote[c].fontSize = 11f;
                }
                columnasResumenLote[4].text = "COLA WMS";
                columnasResumenLote[4].color = new Color(0.22f, 0.74f, 0.97f, 1f);
                columnasResumenLote[4].fontSize = 11f;
            }
        }
        else if (filaResumenGO != null)
        {
            filaResumenGO.SetActive(false);
        }
    }

    private void ForzarLayoutYScroll(int indiceObjetivo)
    {
        Canvas.ForceUpdateCanvases();
        if (contenedorFilas is RectTransform contentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (scrollManifiesto != null && DashboardUI.Instance != null && DashboardUI.Instance.textosEstadoManifiesto != null)
        {
            int total = Mathf.Max(1, DashboardUI.Instance.textosEstadoManifiesto.Count);
            if (total <= 1)
            {
                scrollManifiesto.verticalNormalizedPosition = 1f;
            }
            else
            {
                float norm = 1f - Mathf.Clamp01((float)indiceObjetivo / (total - 1));
                scrollManifiesto.verticalNormalizedPosition = norm;
            }
        }
    }
}
````

## File: Assets/_ProyectoBodega/Scripts/ModelosLogistica.cs
````csharp
using System.Collections.Generic;

[System.Serializable]
public class Pedido
{
    public string id_pallet;
    public string nombre_producto;
    public string codigo_sku;
    public float peso_kg;
    public string coordenada_bodega;
}

[System.Serializable]
public class Manifiesto
{
    public string id_lote;
    public int total_pallets;
    public float peso_total_kg;
    public List<Pedido> pedidos;
}
````

## File: Assets/_ProyectoBodega/Scripts/TelemetriaAMR.cs
````csharp
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class TelemetriaAMR : MonoBehaviour
{
    private RobotAMR robot;
    private NavMeshAgent agente;

    [Header("UI Text References")]
    public TextMeshProUGUI txtEstado;
    public TextMeshProUGUI txtPayload;
    public TextMeshProUGUI txtETA;

    [Header("Elevación Dinámica Píldora")]
    public float alturaLocalVacio = 1.05f;
    public float alturaLocalConCarga = 2.35f;

    [Header("Throttling Settings")]
    private float tiempoProximoRefrescoETA = 0f;
    private const float IntervaloRefrescoETA = 0.15f; // ~6.6 Hz de refresco
    private float ultimoETAValor = -1f;
    private bool ultimoEstadoETAPendiente = false;
    private string textoETACache = "ETA: --";
    private int ultimoPorcentajeRecargaMostrado = -1;

    void Awake()
    {
        robot = GetComponentInParent<RobotAMR>();
        agente = GetComponentInParent<NavMeshAgent>();
        AplicarAlturaInicial();
    }

    void OnEnable()
    {
        if (robot != null)
        {
            robot.OnEstadoCambiado += ActualizarTelemetriaEstadoYPayload;
        }
        RefrescarTextoPildora();
    }

    void OnDisable()
    {
        if (robot != null)
        {
            robot.OnEstadoCambiado -= ActualizarTelemetriaEstadoYPayload;
        }
    }

    void Start()
    {
        AplicarAlturaInicial();
        if (txtPayload != null) txtPayload.gameObject.SetActive(false);
        if (txtETA != null) txtETA.gameObject.SetActive(false);
        RefrescarTextoPildora();
    }

    private void AplicarAlturaInicial()
    {
        Vector3 pos = transform.localPosition;
        pos.y = alturaLocalVacio;
        transform.localPosition = pos;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }

        bool conCarga = EsConCargaFisica();
        float yMeta = conCarga ? alturaLocalConCarga : alturaLocalVacio;
        Vector3 pos = transform.localPosition;
        pos.y = Mathf.Lerp(pos.y, yMeta, Time.deltaTime * 8f);
        transform.localPosition = pos;
    }

    void Update()
    {
        if (robot == null) return;

        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            if (textoETACache != "ETA: --" || ultimoETAValor != 0f || ultimoEstadoETAPendiente)
            {
                textoETACache = "ETA: --";
                ultimoETAValor = 0f;
                ultimoEstadoETAPendiente = false;
                RefrescarTextoPildora();
            }
            return;
        }

        // Throttling para cálculo y renderizado de ETA (~6-7 Hz para eliminar GC pressure a 60 FPS)
        if (Time.time >= tiempoProximoRefrescoETA)
        {
            tiempoProximoRefrescoETA = Time.time + IntervaloRefrescoETA;
            ActualizarETAThrottled();
        }
    }

    private bool EsConCargaFisica()
    {
        return robot != null && (robot.TieneCargaFisica || robot.estadoActual == RobotAMR.EstadoRobot.Transportando || robot.estadoActual == RobotAMR.EstadoRobot.Entregando);
    }

    private void ActualizarTelemetriaEstadoYPayload(RobotAMR.EstadoRobot nuevoEstado)
    {
        ultimoPorcentajeRecargaMostrado = -1;
        RefrescarTextoPildora();
    }

    private void ActualizarETAThrottled()
    {
        if (agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            if (textoETACache != "ETA: --")
            {
                textoETACache = "ETA: --";
                RefrescarTextoPildora();
            }
            ultimoETAValor = 0f;
            ultimoEstadoETAPendiente = false;
            return;
        }

        bool requiereRefresco = false;

        if (agente.pathPending)
        {
            if (!ultimoEstadoETAPendiente || textoETACache != "ETA: --")
            {
                textoETACache = "ETA: --";
                ultimoEstadoETAPendiente = true;
                ultimoETAValor = -1f;
                requiereRefresco = true;
            }
        }
        else if (agente.remainingDistance > 0.5f && agente.velocity.magnitude > 0.1f)
        {
            float eta = agente.remainingDistance / agente.velocity.magnitude;
            float etaRedondeado = Mathf.Round(eta * 10f) / 10f; // Redondeo a 1 decimal

            if (Mathf.Abs(etaRedondeado - ultimoETAValor) >= 0.1f || ultimoEstadoETAPendiente)
            {
                ultimoETAValor = etaRedondeado;
                ultimoEstadoETAPendiente = false;
                textoETACache = $"ETA: {etaRedondeado:F1}s";
                requiereRefresco = true;
            }
        }
        else
        {
            if (ultimoETAValor != 0f || ultimoEstadoETAPendiente || textoETACache != "ETA: --")
            {
                textoETACache = "ETA: --";
                ultimoETAValor = 0f;
                ultimoEstadoETAPendiente = false;
                requiereRefresco = true;
            }
        }

        if (robot != null && robot.estadoActual == RobotAMR.EstadoRobot.Recargando)
        {
            int pct = Mathf.RoundToInt(robot.bateriaActual);
            if (pct != ultimoPorcentajeRecargaMostrado)
            {
                ultimoPorcentajeRecargaMostrado = pct;
                requiereRefresco = true;
            }
        }

        if (requiereRefresco)
        {
            RefrescarTextoPildora();
        }
    }

    private void RefrescarTextoPildora()
    {
        if (txtEstado == null) return;

        string id = (robot != null && !string.IsNullOrEmpty(robot.idRobot)) ? robot.idRobot : "AMR";
        RobotAMR.EstadoRobot estado = robot != null ? robot.estadoActual : RobotAMR.EstadoRobot.Inactivo;

        float peso = 0f;
        bool conCarga = EsConCargaFisica();
        if (conCarga && robot.PedidoActual != null)
        {
            peso = robot.PedidoActual.peso_kg;
        }

        if (conCarga && peso > 0f)
        {
            txtEstado.text = $"{id} • <color=#FBBF24>{peso:F0} kg</color> | {textoETACache}";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Inactivo)
        {
            txtEstado.text = $"{id} • <color=#94A3B8>RESERVA</color>";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Recargando)
        {
            bool enMovimientoHaciaPad = agente != null && agente.enabled && agente.isOnNavMesh && agente.velocity.magnitude > 0.15f;
            txtEstado.text = enMovimientoHaciaPad
                ? $"{id} • <color=#38BDF8>RETORNO PAD</color> | {textoETACache}"
                : $"{id} • <color=#38BDF8>CARGA 1C ({robot.bateriaActual:F0}%)</color>";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Interrumpido)
        {
            txtEstado.text = $"{id} • <color=#EF4444>ALERTA</color>";
            return;
        }

        if (estado == RobotAMR.EstadoRobot.Apagado)
        {
            txtEstado.text = $"{id} • <color=#EF4444>APAGADO &lt;3%</color>";
            return;
        }

        txtEstado.text = $"{id} • <color=#38BDF8>{ObtenerEstadoCorto(estado)}</color> | {textoETACache}";
    }

    private static string ObtenerEstadoCorto(RobotAMR.EstadoRobot estado)
    {
        switch (estado)
        {
            case RobotAMR.EstadoRobot.En_Transito: return "TRÁNSITO";
            case RobotAMR.EstadoRobot.Extrayendo: return "EXTRAYENDO";
            case RobotAMR.EstadoRobot.Transportando: return "CARGADO";
            case RobotAMR.EstadoRobot.Entregando: return "DESCARGA";
            default: return estado.ToString().ToUpperInvariant();
        }
    }
}
````

## File: Assets/StreamingAssets/manifiesto.json
````json
{
    "pedidos": [
        {
            "id_pallet": "PAL-8491",
            "nombre_producto": "CRubia Aguila (Lata 330ml)",
            "codigo_sku": "AGU-RUB-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C1-F1"
        },
        {
            "id_pallet": "PAL-8492",
            "nombre_producto": "CNegra ClubColombia (Botella 330ml)",
            "codigo_sku": "CLU-NEG-330",
            "peso_kg": 850,
            "coordenada_bodega": "P1-C2-F1"
        },
        {
            "id_pallet": "PAL-8493",
            "nombre_producto": "CEspecial Oktoberfest (Barril 50L)",
            "codigo_sku": "OKT-BAR-50L",
            "peso_kg": 920,
            "coordenada_bodega": "P2-C4-F1"
        }
    ]
}
````

## File: README.md
````markdown
# Sistema Logístico AMR — Gemelo Digital de Despacho Táctico Nocturno

[![Unity Version](https://img.shields.io/badge/Unity-2022.3.62f3%20LTS-black?style=flat&logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%2014.0.12-blue?style=flat)](https://unity.com/)
[![License](https://img.shields.io/badge/Standard-Industria%205.0%20%2F%20ISO%203691--4-green?style=flat)]()
[![Build Target](https://img.shields.io/badge/Platform-StandaloneLinux64%20%7C%20Windows64-lightgrey?style=flat)]()
[![Documentation](https://img.shields.io/badge/Norma-APA%207ma%20Edici%C3%B3n-orange?style=flat)]()

Entorno interactivo industrial y Producto Mínimo Viable (MVP) de un **Gemelo Digital** para la optimización del turno nocturno de despacho logístico (18:00 - 06:00) en el sector de bebidas de consumo masivo. Desarrollado en **Unity 2022.3 LTS**, el sistema simula la orquestación autónoma de flotas robóticas (AMR/EVE) bajo estándares **DPO (Distribution Process Optimization)**, desacoplamiento estricto de datos JSON e interacción supervisada basada en **Industria 5.0**.

---

##  Arquitectura y Ecosistema de Planta (Lore 100% vs. MVP 20%)

El sistema se concibe conceptualmente como parte de un macroproceso automatizado de 5 microservicios independientes:
1. **Módulo 1: Recepción y Lavado de Envases** *(Restringido en MVP)*
2. **Módulo 2: Llenado y Sellado de Producto** *(Restringido en MVP)*
3. **Módulo 3: Paletizado Automatizado** *(Restringido en MVP)*
4. **Módulo 4: Bodegaje Inteligente** *(Implementado en MVP)*
5. **Módulo 5: Despacho Táctico Nocturno y TMS** *(Implementado en MVP)*

---

##  Características Principales de Ingeniería

* **Desacoplamiento Estricto (Data-Driven Design):** Cero dependencias rígidas en código fuente. Las misiones de carga y especificaciones de SKU se extraen en tiempo de ejecución desde `Assets/StreamingAssets/manifiesto.json`.
* **Física y Batería Predictiva Realista:** El drenaje de las celdas LiFePO4 cruza distancia ortogonal real con el peso del payload (`kg`) y un factor de dispersión térmica.
* **Seguridad Industrial ISO 3691-4 (Cat. 3):** Detección volumétrica por LiDAR frontal y parada de emergencia a $\le 0.85\text{ m}$ con protocolo *Docking Mute* en zonas de carga.
* **Trade-Off Gerencial Industria 5.0:** El operario humano resuelve el dilema energético ante sobrecargas críticas ($920\text{ kg}$):
  * **Forzar Override:** Continúa la entrega asumiendo degradación física.
  * **Activar Relevo 24/7:** Despliega a `AMR-02` (80% batería) y retira a `AMR-01` a recarga inductiva.
* **Cero Basura en Memoria (Zero-GC Optimization):** Uso de `StringBuilder` para el scroll de terminal, eventos en C# desacoplados y throttling de telemetría para garantizar estabilidad en rendimiento.

---

##  Stack Tecnológico y Dependencias

* **Motor:** Unity 2022.3.62f3 LTS (URP - Universal Render Pipeline 14.0.12)
* **Lenguaje:** C# (.NET Standard 2.1)
* **Paquetes Principales:**
  * `com.unity.ai.navigation` (1.1.7): Mallas dinámicas de navegación para AMRs terrestres.
  * `com.unity.cinemachine` (3.1.7): Arquitectura cinemática multicámara por estados robóticos.
  * `com.unity.textmeshpro` (3.0.9): Renderizado tipográfico SDF en HUDs y SCADA.
  * `com.unity.inputsystem` (1.14.2): Procesamiento de periféricos.

---

##  Estructura del Repositorio

```text
├── Assets/
│   ├── Art/                # Fuentes tipográficas SDF, texturas e iconos corporativos
│   ├── Prefabs/            # Fila_Dato_Prefab, módulos de fábrica y estanterías
│   ├── Scenes/
│   │   └── Bodega_TurnoNocturno.unity  # Escena operacional activa (Index 0)
│   ├── Scripts/            # Los 9 scripts de arquitectura C#
│   └── StreamingAssets/
│       └── manifiesto.json # Contrato de datos externo de pedidos
├── Manuales/               # Documentación oficial bajo norma APA 7ma Edición (PDFs)
└── ProjectSettings/        # Configuraciones de compilación y capas del motor
````

## File: Assets/_ProyectoBodega/Scripts/DirectorCinematico.cs
````csharp
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
            if (vcam3_AMR_Hombro != null) vcam3_AMR_Hombro.Target.TrackingTarget = robotAMR.transform;
            if (!focoIncidenteActivo)
            {
                if (vcam4_Emergencia != null) vcam4_Emergencia.Target.TrackingTarget = robotAMR.transform;
                EvaluarEstadoAMR(robotAMR.estadoActual);
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

        var unicos = new HashSet<RobotAMR>();
        if (robotAMR != null) unicos.Add(robotAMR);
        if (managerEVE != null)
        {
            if (managerEVE.robotAMR_01 != null) unicos.Add(managerEVE.robotAMR_01);
            if (managerEVE.robotAMR_02 != null) unicos.Add(managerEVE.robotAMR_02);
            if (managerEVE.robotAMR_03 != null) unicos.Add(managerEVE.robotAMR_03);
        }

        foreach (RobotAMR amr in unicos)
        {
            if (amr == null) continue;
            amr.OnEstadoCambiado -= EvaluarEstadoAMR;
            if (suscribir) amr.OnEstadoCambiado += EvaluarEstadoAMR;
        }
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
        DesactivarPrioridades();

        if (amrRelevo != null)
        {
            robotAMR = amrRelevo;
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
            vcam2_EVE.Priority = 10;
            Debug.Log("<color=orange>Director:</color> Foco de incidente liberado. Retorno a VCam2 (EVE).");
        }
    }

    private void EnfocarEVE()
    {
        // Guarda de protección: Si hay un incidente crítico activo o un AMR apagado/interrumpido, VCam4 retiene el foco
        if (focoIncidenteActivo || HayIncidenteActivoEnFlota())
        {
            Debug.Log("<color=orange>Director:</color> Enfoque a EVE omitido: incidente crítico activo en VCam4.");
            return;
        }

        DesactivarPrioridades();
        if (vcam2_EVE != null) vcam2_EVE.Priority = 10;
        Debug.Log("<color=orange>Director:</color> Transición a VCam2 (EVE).");
    }

    private void EvaluarEstadoAMR(RobotAMR.EstadoRobot estado)
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

        DesactivarPrioridades();

        switch (estado)
        {
            case RobotAMR.EstadoRobot.En_Transito:
            case RobotAMR.EstadoRobot.Transportando:
            case RobotAMR.EstadoRobot.Extrayendo:
            case RobotAMR.EstadoRobot.Entregando:
                if (vcam3_AMR_Hombro != null)
                {
                    RobotAMR objetivoActivo = (managerEVE != null && managerEVE.robotEjecutor != null && managerEVE.robotEjecutor.estadoActual != RobotAMR.EstadoRobot.Apagado)
                        ? managerEVE.robotEjecutor
                        : robotAMR;
                    if (objetivoActivo != null)
                    {
                        vcam3_AMR_Hombro.Target.TrackingTarget = objetivoActivo.transform;
                    }
                    vcam3_AMR_Hombro.Priority = 10;
                }
                Debug.Log("<color=orange>Director:</color> Transición a VCam3 (AMR_Hombro).");
                break;
            case RobotAMR.EstadoRobot.Recargando:
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

    public void ActivarVistaGeneralFinDeTurno()
    {
        focoIncidenteActivo = false;
        DesactivarPrioridades();
        if (vcam1_Menu != null) vcam1_Menu.Priority = 10;
        Debug.Log("<color=orange>Director:</color> Fin de turno. Transición a VCam1 (Vista General / Menu).");
    }

    private int indiceCamaraSCADA = 0;

    /// <summary>
    /// Alterna manualmente entre las 5 vistas tácticas (General, AMR-01, AMR-02, AMR-03, Aérea EVE).
    /// </summary>
    public void CiclarCamaraSCADA()
    {
        AlternarCamaraManual();
    }

    public void AlternarCamaraManual()
    {
        focoIncidenteActivo = false;
        if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();

        RobotAMR amr01 = (managerEVE != null && managerEVE.robotAMR_01 != null) ? managerEVE.robotAMR_01 : GameObject.Find("Robot_AMR")?.GetComponent<RobotAMR>();
        RobotAMR amr02 = (managerEVE != null && managerEVE.robotAMR_02 != null) ? managerEVE.robotAMR_02 : GameObject.Find("Robot_AMR_02")?.GetComponent<RobotAMR>();
        RobotAMR amr03 = (managerEVE != null && managerEVE.robotAMR_03 != null) ? managerEVE.robotAMR_03 : GameObject.Find("Robot_AMR_03")?.GetComponent<RobotAMR>();

        var vistas = new (CinemachineCamera cam, RobotAMR targetAMR, string descripcion)[]
        {
            (vcam1_Menu, null, "CÁMARA: PANORÁMICA GENERAL (SCADA)"),
            (vcam3_AMR_Hombro, amr01, "CÁMARA: SEGUIMIENTO HOMBRO (AMR-01)"),
            (vcam3_AMR_Hombro, amr02, "CÁMARA: SEGUIMIENTO HOMBRO (AMR-02)"),
            (vcam3_AMR_Hombro, amr03, "CÁMARA: SEGUIMIENTO HOMBRO (AMR-03)"),
            (vcam2_EVE, null, "CÁMARA: ORQUESTADOR CENTRAL EVE (SCADA)")
        };

        for (int intento = 0; intento < vistas.Length; intento++)
        {
            indiceCamaraSCADA = (indiceCamaraSCADA + 1) % vistas.Length;
            var seleccion = vistas[indiceCamaraSCADA];
            if (seleccion.cam == null) continue;
            if (seleccion.cam == vcam3_AMR_Hombro && seleccion.targetAMR == null) continue;

            DesactivarPrioridades();
            if (seleccion.targetAMR != null)
            {
                robotAMR = seleccion.targetAMR;
                vcam3_AMR_Hombro.Target.TrackingTarget = seleccion.targetAMR.transform;
            }
            seleccion.cam.Priority = 20;

            Debug.Log($"<color=orange>Director [SCADA]:</color> {seleccion.descripcion}");
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[{seleccion.descripcion}]</color>");
            return;
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
````

## File: Assets/_ProyectoBodega/Scripts/ModuloDespachoTMS.cs
````csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuloDespachoTMS : MonoBehaviour
{
    public static ModuloDespachoTMS Instance { get; private set; }

    [Header("Bahías de Carga / Muelles (Sector Este)")]
    public Transform muelleCamion01;
    public Transform muelleCamion02;
    public Transform muelleCamion03;

    [Header("Compuertas Traseras (Sellado 3/3)")]
    public GameObject puertaCerradaCamion1;
    public GameObject puertaCerradaCamion2;
    public GameObject puertaCerradaCamion3;

    [Header("Trazabilidad LIFO por Camión")]
    private Stack<Pedido> matrizLIFOCamion1 = new Stack<Pedido>();
    private Stack<Pedido> matrizLIFOCamion2 = new Stack<Pedido>();
    private Stack<Pedido> matrizLIFOCamion3 = new Stack<Pedido>();

    private List<GameObject> palletsCamion1 = new List<GameObject>();
    private List<GameObject> palletsCamion2 = new List<GameObject>();
    private List<GameObject> palletsCamion3 = new List<GameObject>();

    private float pesoAcumuladoCamion1 = 0f;
    private float pesoAcumuladoCamion2 = 0f;
    private float pesoAcumuladoCamion3 = 0f;

    public const float CapacidadMaximaCamionKg = 12000f;
    public const int CapacidadMaximaPalletsCamion = 3;
    private const float EspaciadoSlotLIFOMetros = 1.60f;

    // Índices 1..3 correspondientes a Muelle_Camion_01..03
    private readonly int[] slotsReservados = new int[4];
    private readonly bool[] renovacionEnCurso = new bool[4];
    private readonly bool[] camionSellado = new bool[4];
    private bool reinicioConvoyEnCurso = false;
    private int camionTurnoActual = 1;

    public Stack<Pedido> MatrizLIFOCamion1 => matrizLIFOCamion1;
    public Stack<Pedido> MatrizLIFOCamion2 => matrizLIFOCamion2;
    public Stack<Pedido> MatrizLIFOCamion3 => matrizLIFOCamion3;

    public float PesoCamion1 => pesoAcumuladoCamion1;
    public float PesoCamion2 => pesoAcumuladoCamion2;
    public float PesoCamion3 => pesoAcumuladoCamion3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Auto-resolver referencias si no han sido asignadas en Inspector
        if (muelleCamion01 == null)
        {
            GameObject m1 = GameObject.Find("Muelle_Camion_01") ?? GameObject.Find("Zona_Camion");
            if (m1 != null) muelleCamion01 = m1.transform;
        }
        if (muelleCamion02 == null)
        {
            GameObject m2 = GameObject.Find("Muelle_Camion_02");
            if (m2 != null) muelleCamion02 = m2.transform;
        }
        if (muelleCamion03 == null)
        {
            GameObject m3 = GameObject.Find("Muelle_Camion_03");
            if (m3 != null) muelleCamion03 = m3.transform;
        }

        ResolverPuertasCerradas();
        AbrirTodasLasCompuertas();
    }

    private void ResolverPuertasCerradas()
    {
        if (puertaCerradaCamion1 == null && muelleCamion01 != null && muelleCamion01.parent != null)
        {
            Transform t = muelleCamion01.parent.Find("Carroceria_Trailer/Puerta_Trasera_Cerrada");
            if (t != null) puertaCerradaCamion1 = t.gameObject;
        }
        if (puertaCerradaCamion2 == null && muelleCamion02 != null && muelleCamion02.parent != null)
        {
            Transform t = muelleCamion02.parent.Find("Carroceria_Trailer/Puerta_Trasera_Cerrada");
            if (t != null) puertaCerradaCamion2 = t.gameObject;
        }
        if (puertaCerradaCamion3 == null && muelleCamion03 != null && muelleCamion03.parent != null)
        {
            Transform t = muelleCamion03.parent.Find("Carroceria_Trailer/Puerta_Trasera_Cerrada");
            if (t != null) puertaCerradaCamion3 = t.gameObject;
        }
    }

    private void ConfigurarPuertaCamion(int numeroCamion, bool cerrada)
    {
        GameObject puerta = null;
        switch (numeroCamion)
        {
            case 1: puerta = puertaCerradaCamion1; break;
            case 2: puerta = puertaCerradaCamion2; break;
            case 3: puerta = puertaCerradaCamion3; break;
        }

        if (puerta != null && puerta.activeSelf != cerrada)
        {
            puerta.SetActive(cerrada);
        }
    }

    private void AbrirTodasLasCompuertas()
    {
        ConfigurarPuertaCamion(1, false);
        ConfigurarPuertaCamion(2, false);
        ConfigurarPuertaCamion(3, false);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ReiniciarEstadoMuelles()
    {
        StopAllCoroutines();
        ResolverPuertasCerradas();
        AbrirTodasLasCompuertas();

        LimpiarListaPalletsCamion(palletsCamion1);
        LimpiarListaPalletsCamion(palletsCamion2);
        LimpiarListaPalletsCamion(palletsCamion3);

        matrizLIFOCamion1.Clear();
        matrizLIFOCamion2.Clear();
        matrizLIFOCamion3.Clear();

        pesoAcumuladoCamion1 = 0f;
        pesoAcumuladoCamion2 = 0f;
        pesoAcumuladoCamion3 = 0f;

        for (int i = 0; i < 4; i++)
        {
            slotsReservados[i] = 0;
            renovacionEnCurso[i] = false;
            camionSellado[i] = false;
        }
        reinicioConvoyEnCurso = false;
        camionTurnoActual = 1;
    }

    private static void LimpiarListaPalletsCamion(List<GameObject> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            if (lista[i] != null) Destroy(lista[i]);
        }
        lista.Clear();
    }

    private List<GameObject> ObtenerListaPallets(int numeroCamion)
    {
        switch (numeroCamion)
        {
            case 2: return palletsCamion2;
            case 3: return palletsCamion3;
            default: return palletsCamion1;
        }
    }

    public Transform ConsultarMuelleDestino(Pedido pedido, out int numeroCamion, out int slotLIFOProyectado)
    {
        numeroCamion = 0;

        // Rotación secuencial 1 -> 2 -> 3 respetando máximo 3 pallets (físicos o reservados) y camiones sellados
        for (int offset = 0; offset < 3; offset++)
        {
            int candidato = ((camionTurnoActual - 1 + offset) % 3) + 1;
            int ocupados = Mathf.Max(slotsReservados[candidato], ObtenerListaPallets(candidato).Count);
            if (!renovacionEnCurso[candidato] && !camionSellado[candidato] && ocupados < CapacidadMaximaPalletsCamion)
            {
                numeroCamion = candidato;
                break;
            }
        }

        if (numeroCamion == 0)
        {
            // Fallback seguro si todos estuvieran momentáneamente al límite 3/3 en simultáneo
            numeroCamion = ((camionTurnoActual - 1) % 3) + 1;
        }

        int baseOcupados = Mathf.Max(slotsReservados[numeroCamion], ObtenerListaPallets(numeroCamion).Count);
        int nuevaReserva = Mathf.Clamp(baseOcupados + 1, 1, CapacidadMaximaPalletsCamion);
        slotsReservados[numeroCamion] = nuevaReserva;
        slotLIFOProyectado = nuevaReserva;

        if (slotsReservados[numeroCamion] >= CapacidadMaximaPalletsCamion)
        {
            camionTurnoActual = (numeroCamion % 3) + 1;
        }
        else
        {
            camionTurnoActual = numeroCamion;
        }

        string idPallet = pedido != null ? pedido.id_pallet : "PAL-DESC";
        string logMensaje = $"<color=#38BDF8>[API MÓDULO 5 -> WMS]:</color> Ruta GPS validada. Asignando {idPallet} a Muelle_Camion_0{numeroCamion} (Slot LIFO #{slotLIFOProyectado}/{CapacidadMaximaPalletsCamion})";
        DashboardUI.Instance?.RegistrarLog(logMensaje);
        Debug.Log(logMensaje);

        return ObtenerTransformMuelle(numeroCamion);
    }

    public void CancelarReservaSlot(int numeroCamion, int slotReservado)
    {
        if (numeroCamion < 1 || numeroCamion > 3 || slotReservado <= 0) return;

        int entregados = ObtenerListaPallets(numeroCamion).Count;
        slotsReservados[numeroCamion] = Mathf.Max(entregados, slotsReservados[numeroCamion] - 1);
    }

    public Transform ObtenerTransformMuelle(int numeroCamion)
    {
        switch (numeroCamion)
        {
            case 1: return muelleCamion01;
            case 2: return muelleCamion02;
            case 3: return muelleCamion03;
            default: return muelleCamion01;
        }
    }

    public int ObtenerSlotLIFOProyectado(int numeroCamion)
    {
        if (numeroCamion < 1 || numeroCamion > 3) numeroCamion = 1;
        return Mathf.Clamp(Mathf.Max(ObtenerListaPallets(numeroCamion).Count, slotsReservados[numeroCamion]) + 1, 1, CapacidadMaximaPalletsCamion);
    }

    public Vector3 ObtenerCoordenadaSlotLIFO(int numeroCamion, int slotLIFOProyectado)
    {
        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        if (muelleTransform == null) return Vector3.zero;

        // Inversión LIFO física (Flatbed):
        // Slot 1 al fondo (Z = +1.60m), Slot 2 al centro (Z = 0m), Slot 3 cerca de rampa (Z = -1.60m)
        int slotClamped = Mathf.Clamp(slotLIFOProyectado, 1, CapacidadMaximaPalletsCamion);
        float zOffset = (2 - slotClamped) * EspaciadoSlotLIFOMetros;

        Transform sueloFalso = muelleTransform.parent != null ? muelleTransform.parent.Find("Suelo_Falso_NavMesh") : null;
        float ySuperficie = 1.813f;
        float zCentroCarga = muelleTransform.position.z;
        float xCentroCarga = muelleTransform.position.x;

        if (sueloFalso != null)
        {
            Collider colSuelo = sueloFalso.GetComponent<Collider>();
            ySuperficie = colSuelo != null ? colSuelo.bounds.max.y : (sueloFalso.position.y + 0.066f);
            zCentroCarga = sueloFalso.position.z;
            xCentroCarga = sueloFalso.position.x;
        }

        return new Vector3(xCentroCarga, ySuperficie, zCentroCarga + zOffset);
    }

    public Vector3 ObtenerCoordenadaSlotActivo(int numeroCamion)
    {
        int slotProyectado = ObtenerSlotLIFOProyectado(numeroCamion);
        return ObtenerCoordenadaSlotLIFO(numeroCamion, slotProyectado);
    }

    public void RegistrarEstibaEnCamion(int numeroCamion, Pedido pedido, GameObject palletFisico, int slotAsignado = 0)
    {
        if (pedido == null) return;
        if (numeroCamion < 1 || numeroCamion > 3) numeroCamion = 1;

        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        Stack<Pedido> pilaObjetivo;
        List<GameObject> listaPallets;
        float pesoAcumulado;

        switch (numeroCamion)
        {
            case 2:
                matrizLIFOCamion2.Push(pedido);
                pesoAcumuladoCamion2 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion2;
                listaPallets = palletsCamion2;
                pesoAcumulado = pesoAcumuladoCamion2;
                break;
            case 3:
                matrizLIFOCamion3.Push(pedido);
                pesoAcumuladoCamion3 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion3;
                listaPallets = palletsCamion3;
                pesoAcumulado = pesoAcumuladoCamion3;
                break;
            default:
                matrizLIFOCamion1.Push(pedido);
                pesoAcumuladoCamion1 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion1;
                listaPallets = palletsCamion1;
                pesoAcumulado = pesoAcumuladoCamion1;
                break;
        }

        if (palletFisico != null && muelleTransform != null)
        {
            listaPallets.Add(palletFisico);
            palletFisico.transform.SetParent(null, true);

            int indiceLIFO = slotAsignado > 0
                ? Mathf.Clamp(slotAsignado, 1, CapacidadMaximaPalletsCamion)
                : Mathf.Clamp(pilaObjetivo.Count, 1, CapacidadMaximaPalletsCamion);
            float zOffset = (2 - indiceLIFO) * EspaciadoSlotLIFOMetros;

            Transform sueloFalso = muelleTransform.parent != null ? muelleTransform.parent.Find("Suelo_Falso_NavMesh") : null;
            float ySuperficie = 1.813f;
            float zCentroCarga = muelleTransform.position.z;
            float xCentroCarga = muelleTransform.position.x;

            if (sueloFalso != null)
            {
                Collider colSuelo = sueloFalso.GetComponent<Collider>();
                ySuperficie = colSuelo != null ? colSuelo.bounds.max.y : (sueloFalso.position.y + 0.066f);
                zCentroCarga = sueloFalso.position.z;
                xCentroCarga = sueloFalso.position.x;
            }

            Vector3 posicionLIFO = new Vector3(
                xCentroCarga,
                ySuperficie,
                zCentroCarga + zOffset
            );

            palletFisico.transform.position = posicionLIFO;
            palletFisico.transform.rotation = muelleTransform.rotation;
        }

        int estibasActuales = listaPallets.Count;
        slotsReservados[numeroCamion] = Mathf.Max(slotsReservados[numeroCamion], estibasActuales);

        string logMatriz = $"<color=cyan>[MATRIZ LIFO CAMIÓN {numeroCamion}]:</color> Estiba {pedido.id_pallet} apilada en Posición #{estibasActuales}/{CapacidadMaximaPalletsCamion} (Peso acumulado: {pesoAcumulado:F0} kg / {CapacidadMaximaCamionKg:F0} kg)";
        DashboardUI.Instance?.RegistrarLog(logMatriz);
        Debug.Log(logMatriz);

        if (estibasActuales >= CapacidadMaximaPalletsCamion && !renovacionEnCurso[numeroCamion] && !camionSellado[numeroCamion])
        {
            slotsReservados[numeroCamion] = CapacidadMaximaPalletsCamion;
            StartCoroutine(SellarCamionYVerificarConvoy(numeroCamion, 2.5f));
        }
    }

    private IEnumerator SellarCamionYVerificarConvoy(int numeroCamion, float delay = 2.5f)
    {
        renovacionEnCurso[numeroCamion] = true;
        yield return new WaitForSeconds(delay);

        // Esperar hasta que ningún AMR permanezca sobre la rampa o cama de este camión (Z > -2.0f)
        while (AlgunAMREnMuelle(numeroCamion))
        {
            yield return new WaitForSeconds(0.25f);
        }

        // 1. Sellar visualmente las compuertas traseras del remolque lleno
        ConfigurarPuertaCamion(numeroCamion, true);
        camionSellado[numeroCamion] = true;

        // 2. Destruir los 3 pallets internos tras sellar para optimizar GPU/RAM, manteniendo slotsReservados en 3
        List<GameObject> listaPallets = ObtenerListaPallets(numeroCamion);
        LimpiarListaPalletsCamion(listaPallets);
        slotsReservados[numeroCamion] = CapacidadMaximaPalletsCamion;
        renovacionEnCurso[numeroCamion] = false;

        string msgSellado = $"<color=#10B981>[TMS MÓDULO 5]:</color> Muelle_Camion_0{numeroCamion} completo (3/3 estibas LIFO). Compuertas selladas; habilitando siguiente bahía.";
        DashboardUI.Instance?.RegistrarLog(msgSellado);
        Debug.Log(msgSellado);

        // 3. Si los 3 camiones ya quedaron llenos y sellados, despachar el convoy completo tras 4.0s
        if (camionSellado[1] && camionSellado[2] && camionSellado[3] && !reinicioConvoyEnCurso)
        {
            reinicioConvoyEnCurso = true;
            yield return new WaitForSeconds(4.0f);

            AbrirTodasLasCompuertas();

            matrizLIFOCamion1.Clear();
            matrizLIFOCamion2.Clear();
            matrizLIFOCamion3.Clear();
            pesoAcumuladoCamion1 = 0f;
            pesoAcumuladoCamion2 = 0f;
            pesoAcumuladoCamion3 = 0f;

            for (int i = 1; i <= 3; i++)
            {
                slotsReservados[i] = 0;
                renovacionEnCurso[i] = false;
                camionSellado[i] = false;
            }
            camionTurnoActual = 1;
            reinicioConvoyEnCurso = false;

            string msgConvoy = "<color=#38BDF8>[TMS MÓDULO 5]:</color> Convoy de 3 camiones despachado a ruta. 3 nuevos remolques vacíos posicionados en Muelles 01, 02 y 03.";
            DashboardUI.Instance?.RegistrarLog(msgConvoy);
            Debug.Log(msgConvoy);
        }
    }

    private bool AlgunAMREnMuelle(int numeroCamion)
    {
        Transform muelle = ObtenerTransformMuelle(numeroCamion);
        if (muelle == null) return false;

        float xMuelle = muelle.position.x;
        RobotAMR[] flota = Object.FindObjectsByType<RobotAMR>(FindObjectsSortMode.None);
        for (int i = 0; i < flota.Length; i++)
        {
            RobotAMR amr = flota[i];
            if (amr == null || !amr.gameObject.activeInHierarchy) continue;
            if (amr.estadoActual == RobotAMR.EstadoRobot.Apagado) continue;

            if (amr.transform.position.z > -2.0f && Mathf.Abs(amr.transform.position.x - xMuelle) < 4.5f)
            {
                return true;
            }
        }
        return false;
    }
}
````

## File: Assets/_ProyectoBodega/Scripts/RobotAMR.cs
````csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando, Recargando, Apagado }
    
    public event System.Action<EstadoRobot> OnEstadoCambiado;

    [Header("Identidad de Flota")]
    public string idRobot = "AMR-01";

    [Header("Telemetría Batería")]
    public float bateriaActual = 80.0f;
    public float coeficienteVacio = 0.00175f;
    public float coeficienteCarga = 0.00142f;
    public float consumoIdle = 0.0010f;

    [Header("Gestión de Pausa Preventiva y Relevo")]
    public EstadoRobot estadoPreInterrupcionBateria = EstadoRobot.Inactivo;
    public bool enPausaPreventivaBateria20 = false;
    public bool relevoDiferidoPostEntrega = false;

    [Header("Recarga Inductiva KUKA KMP 1500P (20% -> 80% en 1h = 1.0%/min)")]
    // KUKA KMP 1500P Datasheet: Charge from 20% to 80% within 1 hour (60% / 3600s = 0.0166667% / s)
    public float tasaRecargaPorSegundo = 60.0f / 3600.0f; // 0.0166667f %/s (1.0% por minuto real)
    public const float FactorCompresionTurno = 28800f / 180f; // 160x
    public float tiempoRecargaRealSegundos = 3840f;
    private float tasaRecargaSimulada => (80.0f - 20.0f) / (tiempoRecargaRealSegundos / FactorCompresionTurno); // 2.50f

    [Header("Throttling Telemetría")]
    private float tiempoProximoEnvioTelemetria = 0f;
    private const float IntervaloTelemetria = 0.15f; // ~6.6 Hz
    private float ultimaBateriaTransmitida = -1f;
    private EstadoRobot ultimoEstadoTransmitido = (EstadoRobot)(-1);

    [Header("Sensores LiDAR")]
    public LayerMask capaObstaculos = 1; // Default layer (bit 0) por defecto
    private float distanciaDeteccionAlerta = 1.65f;
    private float distanciaFrenadoEmergencia = 1.15f;
    private float distanciaMaxSensor = 4.0f;
    private bool obstaculoDetectado = false;
    private bool enParadaSeguridadLiDAR = false;
    private EstadoRobot estadoPrevioBloqueo = EstadoRobot.Inactivo;

    private float velocidadBase = 3.5f;
    private float aceleracionBase = 20.0f;
    private float coeficientePeso = 0.002f;

    [Header("Cinemática Industrial (ISO 3691-4)")]
    public const float RadioAgenteIndustrial = 0.60f;
    public const float VelocidadAngularIndustrial = 720.0f;

    [Header("Control de Tráfico Industrial (Mutex ISO 3691-4 & VDA 5050)")]
    private static readonly List<RobotAMR> flotaActiva = new List<RobotAMR>(3);
    private static readonly Dictionary<Transform, RobotAMR> zonasReservadasMutex = new Dictionary<Transform, RobotAMR>();
    private const float DistanciaCompuertaMutex = 3.5f;
    private const float RadioOcupacionFisicaMutex = 2.5f;
    private const float RadioBurbujaEntradaVDA = 2.50f;
    private const float RadioBurbujaSalidaVDA = 2.80f;
    private bool enEsperaMutex = false;
    private bool cediendoPasoAMR = false;
    private float tiempoDetenidoCediendo = 0f;
    private float tiempoInmunidadDespeje = 0f;

    [Header("Enrutamiento Ortogonal (VDA 5050)")]
    public const float ZAvenidaOeste = -32.0f;
    public const float ZAvenidaEste = -34.0f;
    public const float XPorticoSalidaNorte = 34.40f;
    public const float XPorticoEntradaSur = 32.60f;
    public const float ZUmbralPortico = -48.0f;
    public const float ZColectorSalaCarga = -51.80f;
    public const float ZPieRampaCamion = -2.0f;
    public const float DesfaseCarrilRack = 1.75f;
    public const float XCentroASRS = -21.20f;
    public const float DesfaseCarrilASRSCentro = 3.84f;
    public const float DesfaseFrontalAcopleVLM = 0.90f;
    public const float UmbralPasoWaypoint = 0.65f;
    private readonly Queue<Vector3> colaWaypoints = new Queue<Vector3>();
    private readonly List<Vector3> bufferNodosRuta = new List<Vector3>();

    [Header("Panel de Control (Solo lectura)")]
    public EstadoRobot estadoActual = EstadoRobot.Inactivo;
    /*El NavMeshAgent es el motor físico de Unity. Pero nota la variable Pedido. Tu cilindro físico no conoce el archivo JSON completo ni le interesa.
    Solo conoce la estructura de un pedido aislado que le entrega el Gestor, manteniendo el código limpio y modular.*/
    private NavMeshAgent agente;
    private Pedido pedidoActual; // Conoce la estructura, pero no toda la base de datos
    private bool modoDegradacionActivo = false;
    private int indiceFilaManifiesto = 0;

    public Pedido PedidoActual => pedidoActual;
    [HideInInspector]
    public bool colapsoBMSArmadoPostDescarga = false;

    public bool TieneCargaFisica =>
        palletActualGO != null ||
        estadoActual == EstadoRobot.Extrayendo ||
        estadoActual == EstadoRobot.Transportando ||
        estadoActual == EstadoRobot.Entregando;

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    public Transform puntoMontajeCarga;

    [Header("Activos 3D de Carga (Gemelo Físico)")]
    public GameObject prefabPalletCervezas;
    public GameObject prefabPalletBarriles;
    private Transform destinoActual;
    private GameObject palletActualGO;
    private GameObject stockOcultadoActual;

    public GameObject PalletActualGO => palletActualGO;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();

        if (agente != null)
        {
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.acceleration = aceleracionBase;
            agente.stoppingDistance = 0.0f;
            agente.autoBraking = true;
        }

        ActualizarJerarquiaNavegacion();
    }

    private void LimpiarPalletTemporal()
    {
        if (palletActualGO != null)
        {
            Destroy(palletActualGO);
            palletActualGO = null;
        }
        if (stockOcultadoActual != null)
        {
            stockOcultadoActual.SetActive(true);
            stockOcultadoActual = null;
        }
    }

    void OnEnable()
    {
        if (!flotaActiva.Contains(this))
        {
            flotaActiva.Add(this);
        }
    }

    void OnDisable()
    {
        flotaActiva.Remove(this);
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        colaWaypoints.Clear();
        LimpiarPalletTemporal();
        LiberarMutexDeRobot(this);
    }

    void OnDestroy()
    {
        flotaActiva.Remove(this);
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        colaWaypoints.Clear();
        LimpiarPalletTemporal();
        LiberarMutexDeRobot(this);
    }

    private int ObtenerIndiceUnidad()
    {
        if (!string.IsNullOrEmpty(idRobot))
        {
            if (idRobot.EndsWith("03")) return 3;
            if (idRobot.EndsWith("02")) return 2;
            if (idRobot.EndsWith("01")) return 1;
        }
        if (gameObject.name.Contains("03")) return 3;
        if (gameObject.name.Contains("02")) return 2;
        return 1;
    }

    public void ActualizarJerarquiaNavegacion()
    {
        if (agente == null || !agente.enabled) return;

        int offsetDesempate = Mathf.Max(0, ObtenerIndiceUnidad() - 1);

        switch (estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
            case EstadoRobot.Apagado:
                agente.avoidancePriority = 10 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                break;

            case EstadoRobot.Transportando:
                agente.avoidancePriority = 20 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;

            case EstadoRobot.En_Transito:
                agente.avoidancePriority = 50 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;

            case EstadoRobot.Recargando:
                agente.avoidancePriority = 80 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                break;

            case EstadoRobot.Inactivo:
            default:
                agente.avoidancePriority = 90 + offsetDesempate;
                agente.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
                break;
        }
    }

    public bool EstaEvacuandoZonaMuelle()
    {
        return (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Recargando) &&
               transform.position.z > -12.0f && transform.position.x > 18.0f;
    }

    private bool IntentarReservarZonaMutex(Transform zona)
    {
        if (zona == null) return true;

        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR propietario))
        {
            if (propietario == null || !propietario.gameObject.activeInHierarchy ||
                propietario.estadoActual == EstadoRobot.Inactivo ||
                propietario.estadoActual == EstadoRobot.Recargando ||
                propietario.estadoActual == EstadoRobot.Apagado)
            {
                zonasReservadasMutex.Remove(zona);
            }
            else if (propietario != this)
            {
                return false;
            }
        }

        Vector3 puntoRef = (agente != null && agente.enabled && agente.hasPath) ? agente.pathEndPosition : zona.position;
        Vector2 posRefXZ = new Vector2(puntoRef.x, puntoRef.z);
        bool reservandoMuelleEntrega = estadoActual == EstadoRobot.Transportando && zona == zonaTransferencia;

        for (int i = 0; i < flotaActiva.Count; i++)
        {
            RobotAMR otroRobot = flotaActiva[i];
            if (otroRobot == null || otroRobot == this || !otroRobot.gameObject.activeInHierarchy) continue;
            if (otroRobot.estadoActual == EstadoRobot.Inactivo || otroRobot.estadoActual == EstadoRobot.Apagado) continue;

            // Si estamos entrando a un muelle, esperar antes de la rampa mientras otro AMR ocupe la cama o rampa de ese muelle
            if (reservandoMuelleEntrega &&
                otroRobot.transform.position.z > -5.5f &&
                Mathf.Abs(otroRobot.transform.position.x - zona.position.x) < 3.2f)
            {
                return false;
            }

            Vector2 posOtroXZ = new Vector2(otroRobot.transform.position.x, otroRobot.transform.position.z);
            if (Vector2.Distance(posOtroXZ, posRefXZ) < RadioOcupacionFisicaMutex)
            {
                return false;
            }
        }

        zonasReservadasMutex[zona] = this;
        return true;
    }

    private void LiberarZonaMutex(Transform zona, RobotAMR propietario)
    {
        if (zona == null) return;
        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR actual) && actual == propietario)
        {
            zonasReservadasMutex.Remove(zona);
        }
    }

    private static void LiberarMutexDeRobot(RobotAMR robot)
    {
        if (robot == null || zonasReservadasMutex.Count == 0) return;

        List<Transform> clavesALiberar = null;
        foreach (var kvp in zonasReservadasMutex)
        {
            if (kvp.Value == null || kvp.Value == robot)
            {
                if (clavesALiberar == null) clavesALiberar = new List<Transform>();
                clavesALiberar.Add(kvp.Key);
            }
        }

        if (clavesALiberar != null)
        {
            for (int i = 0; i < clavesALiberar.Count; i++)
            {
                zonasReservadasMutex.Remove(clavesALiberar[i]);
            }
        }
    }

    private void GestionarCompuertaMutex()
    {
        if (agente == null || !agente.enabled || !agente.isOnNavMesh) return;
        if (enParadaSeguridadLiDAR || cediendoPasoAMR || estadoActual == EstadoRobot.Interrumpido || estadoActual == EstadoRobot.Apagado) return;

        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando || estadoActual == EstadoRobot.Recargando)
        {
            Transform objetivo = (estadoActual == EstadoRobot.En_Transito)
                ? destinoActual
                : (estadoActual == EstadoRobot.Transportando ? zonaTransferencia : null);

            // Liberar cualquier cerrojo previo (o de muelle al descender bajo Z < -5.5f) si ya no es el objetivo activo
            if (zonasReservadasMutex.Count > 0)
            {
                List<Transform> obsoletas = null;
                Vector2 posActualXZ = new Vector2(transform.position.x, transform.position.z);
                foreach (var kvp in zonasReservadasMutex)
                {
                    if (kvp.Value == this && kvp.Key != null && kvp.Key != objetivo)
                    {
                        bool esMuelleDesocupado = kvp.Key.position.z > -4.0f && transform.position.z < -5.5f;
                        Vector2 posZonaXZ = new Vector2(kvp.Key.position.x, kvp.Key.position.z);
                        if (esMuelleDesocupado || Vector2.Distance(posActualXZ, posZonaXZ) > RadioOcupacionFisicaMutex)
                        {
                            if (obsoletas == null) obsoletas = new List<Transform>();
                            obsoletas.Add(kvp.Key);
                        }
                    }
                }
                if (obsoletas != null)
                {
                    for (int i = 0; i < obsoletas.Count; i++) zonasReservadasMutex.Remove(obsoletas[i]);
                }
            }

            if (objetivo != null && agente != null && !agente.pathPending)
            {
                bool enDistanciaFinal = colaWaypoints.Count == 0 &&
                                        agente.remainingDistance <= DistanciaCompuertaMutex &&
                                        DistanciaPlanaADestinoAgente() <= DistanciaCompuertaMutex + 0.25f;

                bool enAproximacionPreRampa = estadoActual == EstadoRobot.Transportando &&
                                              objetivo == zonaTransferencia &&
                                              colaWaypoints.Count <= 1 &&
                                              transform.position.z > -11.0f &&
                                              transform.position.z < -5.5f &&
                                              Mathf.Abs(transform.position.x - objetivo.position.x) < 2.5f;

                if (enDistanciaFinal || enAproximacionPreRampa)
                {
                    if (!IntentarReservarZonaMutex(objetivo))
                    {
                        if (!enEsperaMutex)
                        {
                            enEsperaMutex = true;
                            agente.isStopped = true;
                            agente.velocity = Vector3.zero;
                        }
                    }
                    else if (enEsperaMutex)
                    {
                        enEsperaMutex = false;
                        agente.isStopped = false;
                    }
                }
                else if (enEsperaMutex)
                {
                    enEsperaMutex = false;
                    agente.isStopped = false;
                }
            }
        }
        else
        {
            enEsperaMutex = false;
        }
    }

    private Vector3 AjustarPuntoNavMesh(Vector3 punto)
    {
        if (NavMesh.SamplePosition(punto, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return punto;
    }

    public static bool EsEstacionTransferenciaFrontalASRS(Transform rackReferencia, Vector3 posicion)
    {
        if (rackReferencia != null)
        {
            if (rackReferencia.name == "P1-C1-F1" ||
                rackReferencia.name == "P1-C2-F1" ||
                rackReferencia.name == "P2-C4-F1" ||
                rackReferencia.name == "Punto_Acople_AMR" ||
                rackReferencia.name == "Estacion_Entrega_Rodillos" ||
                rackReferencia.name == "Estacion_PD_Rodillos_01" ||
                rackReferencia.name == "Base_Metalica_01")
            {
                return true;
            }

            Transform cursor = rackReferencia;
            while (cursor != null)
            {
                if (cursor.name == "ASRS_Estante_Heroe" || cursor.name == "Estacion_PD_Rodillos_01")
                {
                    return true;
                }
                cursor = cursor.parent;
            }
        }

        return Mathf.Abs(posicion.x - XCentroASRS) < 3.0f &&
               posicion.z <= -4.0f &&
               posicion.z >= -31.8f;
    }

    private static Vector3 ResolverPuntoAcopleFrontalASRS(Transform rackReferencia)
    {
        if (rackReferencia != null)
        {
            Transform cursor = rackReferencia;
            while (cursor != null)
            {
                if (cursor.name == "ASRS_Estante_Heroe")
                {
                    Transform acopleHijo = cursor.Find("Ascensor_VLM/Punto_Acople_AMR");
                    if (acopleHijo != null)
                    {
                        return new Vector3(acopleHijo.position.x, 0.0f, acopleHijo.position.z);
                    }
                    break;
                }
                cursor = cursor.parent;
            }
        }

        GameObject goAcople = GameObject.Find("Punto_Acople_AMR");
        if (goAcople != null)
        {
            Vector3 pos = goAcople.transform.position;
            return new Vector3(pos.x, 0.0f, pos.z);
        }

        return new Vector3(XCentroASRS, 0.0f, -30.57f);
    }

    private static Vector3 MuestrearNavMeshOCandidato(Vector3 candidato, float radioMax)
    {
        if (NavMesh.SamplePosition(candidato, out NavMeshHit hit, radioMax, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return candidato;
    }

    public float ResolverEjeCarrilX(Vector3 posicion, out Vector3 puntoCarrilNavMesh, Transform rackReferencia = null)
    {
        // Si la coordenada está en la rampa o interior del Flatbed del camión (Z > -1.0m),
        // muestrear directamente sobre el NavMesh del camión sin aplicar DesfaseCarrilRack
        if (posicion.z > -1.0f)
        {
            puntoCarrilNavMesh = MuestrearNavMeshOCandidato(posicion, 2.5f);
            return puntoCarrilNavMesh.x;
        }

        if (rackReferencia == null && destinoActual != null &&
            Vector2.Distance(new Vector2(posicion.x, posicion.z), new Vector2(destinoActual.position.x, destinoActual.position.z)) < 0.25f)
        {
            rackReferencia = destinoActual;
        }

        if (EsEstacionTransferenciaFrontalASRS(rackReferencia, posicion))
        {
            Vector3 candidatoFrontalPD = ResolverPuntoAcopleFrontalASRS(rackReferencia);
            puntoCarrilNavMesh = MuestrearNavMeshOCandidato(candidatoFrontalPD, 2.5f);
            return puntoCarrilNavMesh.x;
        }

        if (rackReferencia != null)
        {
            Collider colliderRack = rackReferencia.GetComponent<Collider>();
            if (colliderRack != null)
            {
                bool esASRS = rackReferencia.name.ToUpperInvariant().Contains("ASRS") ||
                              Mathf.Abs(colliderRack.bounds.center.x - XCentroASRS) < 1.5f;

                float xRack;
                if (esASRS)
                {
                    bool robotAlOeste = transform.position.x <= XCentroASRS;
                    xRack = robotAlOeste
                        ? (XCentroASRS - DesfaseCarrilASRSCentro)
                        : (XCentroASRS + DesfaseCarrilASRSCentro);
                }
                else
                {
                    // Determina si la cara frontal apunta al Oeste (ej. P1-C1) o Este (ej. P1-C2)
                    if (colliderRack.bounds.center.x < -9.0f || colliderRack.bounds.center.x > 0.0f)
                    {
                        xRack = colliderRack.bounds.min.x - DesfaseCarrilRack; // Pasillo Oeste
                    }
                    else
                    {
                        xRack = colliderRack.bounds.max.x + DesfaseCarrilRack; // Pasillo Este
                    }
                }

                float zRack = esASRS
                    ? Mathf.Clamp(posicion.z, colliderRack.bounds.min.z, colliderRack.bounds.max.z)
                    : posicion.z;
                Vector3 candidatoRack = new Vector3(xRack, 0.0f, zRack);
                puntoCarrilNavMesh = MuestrearNavMeshOCandidato(candidatoRack, 2.5f);
                return puntoCarrilNavMesh.x;
            }
        }

        Vector3 baseNav = posicion;
        if (NavMesh.SamplePosition(posicion, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            baseNav = hit.position;
        }

        float dx = baseNav.x - posicion.x;
        float xCarril = baseNav.x;
        if (Mathf.Abs(dx) > 0.15f)
        {
            xCarril = baseNav.x + Mathf.Sign(dx) * DesfaseCarrilRack;
        }

        Vector3 candidato = new Vector3(xCarril, baseNav.y, baseNav.z);
        if (NavMesh.SamplePosition(candidato, out NavMeshHit hitCarril, 2.0f, NavMesh.AllAreas))
        {
            puntoCarrilNavMesh = hitCarril.position;
        }
        else
        {
            puntoCarrilNavMesh = baseNav;
        }

        return puntoCarrilNavMesh.x;
    }

    private static void AgregarNodoRuta(List<Vector3> nodos, Vector3 candidato, bool esDestinoFinal = false)
    {
        if (nodos.Count == 0)
        {
            nodos.Add(candidato);
            return;
        }

        Vector3 ultimo = nodos[nodos.Count - 1];
        Vector2 ultimoXZ = new Vector2(ultimo.x, ultimo.z);
        Vector2 candXZ = new Vector2(candidato.x, candidato.z);

        if (Vector2.Distance(ultimoXZ, candXZ) > 0.25f)
        {
            nodos.Add(candidato);
        }
        else if (esDestinoFinal && nodos.Count > 1)
        {
            nodos[nodos.Count - 1] = candidato;
        }
    }

    private void ConstruirNodosManhattan(Vector3 origen, Vector3 destino, List<Vector3> nodos, Transform rackOrigen = null, Transform rackDestino = null)
    {
        nodos.Clear();

        float xOrigen = ResolverEjeCarrilX(origen, out Vector3 origenNav, rackOrigen);
        float xDestino = ResolverEjeCarrilX(destino, out Vector3 destinoNav, rackDestino);

        bool origenEnSalaCarga = origen.z < ZUmbralPortico;
        bool destinoEnSalaCarga = destino.z < ZUmbralPortico;
        bool origenEnCamion = origen.z > -1.0f;
        bool destinoEnCamion = destino.z > -1.0f;
        bool origenEnZonaMuelle = !origenEnSalaCarga && !destinoEnCamion && origen.z > -4.5f && origen.x > 18.0f;
        float xCarrilEvacuacion = origenEnZonaMuelle ? (xOrigen - 2.20f) : xOrigen;

        // Caso 1: Ambos puntos dentro de la Sala de Carga
        if (origenEnSalaCarga && destinoEnSalaCarga)
        {
            AgregarNodoRuta(nodos, origenNav);
            if (Mathf.Abs(origenNav.x - destinoNav.x) > 0.25f && Mathf.Abs(origenNav.z - destinoNav.z) > 0.25f)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(destinoNav.x, destinoNav.y, origenNav.z)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
            return;
        }

        // Caso 2: Ambos puntos en el mismo pasillo / muelle de la nave principal (sin cruzar pórtico)
        if (!origenEnSalaCarga && !destinoEnSalaCarga && Mathf.Abs(xOrigen - xDestino) <= 0.5f)
        {
            AgregarNodoRuta(nodos, origenNav);
            if ((origenEnCamion || origenEnZonaMuelle) && !destinoEnCamion)
            {
                if (origen.z > ZPieRampaCamion)
                {
                    AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
                }
                if (destino.z < -7.0f)
                {
                    AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, -4.80f)));
                    AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xCarrilEvacuacion, 0.0f, -6.50f)));
                }
            }
            else if (!origenEnCamion && destinoEnCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, ZPieRampaCamion)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
            return;
        }

        float xSalidaAvenida = origenEnSalaCarga ? XPorticoSalidaNorte : xCarrilEvacuacion;
        float xEntradaAvenida = destinoEnSalaCarga ? XPorticoEntradaSur : xDestino;

        // Selección de carril direccional en la Avenida Transversal:
        // -32.0f para flujos hacia el Oeste (Izquierda), -34.0f para flujos hacia el Este (Derecha)
        float zAvenida = (xEntradaAvenida < xSalidaAvenida) ? ZAvenidaOeste : ZAvenidaEste;

        AgregarNodoRuta(nodos, origenNav);

        // Si parte desde el camión o pie de rampa, bajar recto y desviarse por el carril lateral izquierdo de evacuación
        if ((origenEnCamion || origenEnZonaMuelle) && !destinoEnCamion)
        {
            if (origen.z > ZPieRampaCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
            }
            if (origenEnZonaMuelle)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, -4.80f)));
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xCarrilEvacuacion, 0.0f, -6.50f)));
            }
        }
        else if (origenEnCamion)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, 0.0f, ZPieRampaCamion)));
        }

        // 1. Salida hacia la Avenida Principal (vía carril colector frontal Z = -51.80m en Sala de Carga)
        if (origenEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(origenNav.x, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, 0.0f, zAvenida)));
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xCarrilEvacuacion, 0.0f, zAvenida)));
        }

        // 2. Transición por la Avenida y Entrada al Destino (vía carril colector Z = -51.80m hacia Pad)
        if (destinoEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, 0.0f, zAvenida)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(destinoNav.x, 0.0f, ZColectorSalaCarga)));
            AgregarNodoRuta(nodos, destinoNav, true);
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, zAvenida)));
            // Waypoint de alineación perpendicular al pie de la rampa antes de ingresar al slot LIFO del camión
            if (destinoEnCamion)
            {
                AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, 0.0f, ZPieRampaCamion)));
            }
            AgregarNodoRuta(nodos, destinoNav, true);
        }
    }

    private void TrazarRutaOrtogonal(Vector3 destinoFinal, Transform rackDestino = null)
    {
        if (agente == null) return;

        colaWaypoints.Clear();
        if (rackDestino == null && destinoActual != null &&
            Vector2.Distance(new Vector2(destinoFinal.x, destinoFinal.z), new Vector2(destinoActual.position.x, destinoActual.position.z)) < 0.25f)
        {
            rackDestino = destinoActual;
        }
        ConstruirNodosManhattan(transform.position, destinoFinal, bufferNodosRuta, null, rackDestino);

        for (int i = 1; i < bufferNodosRuta.Count; i++)
        {
            colaWaypoints.Enqueue(bufferNodosRuta[i]);
        }

        if (colaWaypoints.Count > 0)
        {
            Vector3 primerWaypoint = colaWaypoints.Dequeue();
            agente.autoBraking = (colaWaypoints.Count == 0);
            agente.SetDestination(primerWaypoint);
        }
        else
        {
            ResolverEjeCarrilX(destinoFinal, out Vector3 destinoNav, rackDestino);
            agente.autoBraking = true;
            agente.SetDestination(destinoNav);
        }
    }

    private float DistanciaPlanaADestinoAgente()
    {
        if (agente == null) return float.MaxValue;
        Vector2 posXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 destXZ = new Vector2(agente.destination.x, agente.destination.z);
        return Vector2.Distance(posXZ, destXZ);
    }

    private bool AvanzarColaWaypoints()
    {
        if (agente == null || !agente.enabled || !agente.isOnNavMesh || enParadaSeguridadLiDAR || enEsperaMutex || cediendoPasoAMR || estadoActual == EstadoRobot.Interrumpido || estadoActual == EstadoRobot.Apagado) return false;
        if (colaWaypoints.Count == 0) return false;

        if (!agente.pathPending &&
            agente.remainingDistance <= UmbralPasoWaypoint &&
            DistanciaPlanaADestinoAgente() <= UmbralPasoWaypoint + 0.25f)
        {
            Vector3 siguienteWaypoint = colaWaypoints.Dequeue();
            agente.autoBraking = (colaWaypoints.Count == 0);
            agente.SetDestination(siguienteWaypoint);
            return true;
        }
        return false;
    }

    private float CalcularDistanciaNavMesh(Vector3 origen, Vector3 destino, Transform rackOrigen = null, Transform rackDestino = null)
    {
        List<Vector3> nodos = new List<Vector3>(8);
        ConstruirNodosManhattan(origen, destino, nodos, rackOrigen, rackDestino);

        if (nodos.Count > 1)
        {
            float distanciaTotal = 0f;
            for (int i = 0; i < nodos.Count - 1; i++)
            {
                Vector2 a = new Vector2(nodos[i].x, nodos[i].z);
                Vector2 b = new Vector2(nodos[i + 1].x, nodos[i + 1].z);
                distanciaTotal += Vector2.Distance(a, b);
            }
            return distanciaTotal;
        }

        Vector3 dif = destino - origen;
        dif.y = 0f;
        return dif.magnitude;
    }

    [Header("Batería Industrial LiFePO4")]
    public const float UmbralSeguridadLiFePO4 = 20.0f;

    [Header("Ruteo TMS")]
    public int numCamionAsignado = 1;
    public int slotLIFOAsignado = 0;
    public const float OffsetParadaRodillosCamion = 1.30f;

    public Vector3 ObtenerPosicionDescargaActiva()
    {
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null)
        {
            int slotObjetivo = slotLIFOAsignado > 0 ? slotLIFOAsignado : tms.ObtenerSlotLIFOProyectado(numCamionAsignado);
            return tms.ObtenerCoordenadaSlotLIFO(numCamionAsignado, slotObjetivo);
        }
        return zonaTransferencia != null ? zonaTransferencia.position : transform.position;
    }

    public Vector3 ObtenerPosicionParadaDescargaActiva()
    {
        Vector3 posSlot = ObtenerPosicionDescargaActiva();
        return new Vector3(posSlot.x, posSlot.y, posSlot.z - OffsetParadaRodillosCamion);
    }

    private void LiberarReservaSlotTMS()
    {
        if (slotLIFOAsignado <= 0) return;

        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if (tms != null)
        {
            tms.CancelarReservaSlot(numCamionAsignado, slotLIFOAsignado);
        }
        slotLIFOAsignado = 0;
    }

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        // Si la zona de transferencia o el slot LIFO no han sido resueltos aún, consultar al TMS
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        if ((zonaTransferencia == null || slotLIFOAsignado <= 0) && tms != null)
        {
            zonaTransferencia = tms.ConsultarMuelleDestino(pedido, out numCamionAsignado, out slotLIFOAsignado);
        }

        float distanciaIda = CalcularDistanciaNavMesh(transform.position, destino.position, null, destino);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            Vector3 destinoSlotCamion = ObtenerPosicionParadaDescargaActiva();
            distanciaVuelta = CalcularDistanciaNavMesh(destino.position, destinoSlotCamion, destino, null);
        }

        // Proyección del consumo: ida vacío + vuelta cargado con ruido térmico máximo (1.15f)
        float gastoIda = distanciaIda * coeficienteVacio;
        float gastoVuelta = distanciaVuelta * coeficienteCarga * (pedido.peso_kg / 100f) * 1.15f;
        float gastoProyectadoTotal = gastoIda + gastoVuelta;

        bateriaRestanteEstimada = bateriaActual - gastoProyectadoTotal;

        return bateriaRestanteEstimada >= UmbralSeguridadLiFePO4;
    }

    public void AsignarMision(Pedido nuevoPedido)
    {
        Transform destino = null;
        if (nuevoPedido != null && !string.IsNullOrEmpty(nuevoPedido.coordenada_bodega))
        {
            GameObject go = GameObject.Find(nuevoPedido.coordenada_bodega);
            if (go != null) destino = go.transform;
        }
        AsignarMision(nuevoPedido, destino);
    }

    public void AsignarMision(Pedido nuevoPedido, Transform destinoFisico)
    {
        AsignarMision(nuevoPedido, destinoFisico, -1);
    }

    public void AsignarMision(Pedido nuevoPedido, Transform destinoFisico, int indiceFilaSecuencial)
    {
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
        if (estadoActual == EstadoRobot.Recargando || estadoActual == EstadoRobot.Apagado)
        {
            Debug.LogWarning($"[{idRobot}]: Llamada a AsignarMision rechazada — unidad en estado {estadoActual}.");
            return;
        }
        if (estadoActual != EstadoRobot.Inactivo) return;

        // CRI-05: Guardia de Nulidad estricta para evitar NullReferenceException
        if (destinoFisico == null)
        {
            string idPallet = nuevoPedido != null ? nuevoPedido.id_pallet : "NULO";
            string coord = nuevoPedido != null ? nuevoPedido.coordenada_bodega : "DESCONOCIDA";
            Debug.LogError($"[ALERTA WMS] Destino físico nulo recibido para pedido {idPallet} ({coord}). Misión abortada.");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ERROR WMS] Ubicación {coord} no localizada en bodega. Misión abortada.</color>");
            return;
        }

        pedidoActual = nuevoPedido;
        indiceFilaManifiesto = indiceFilaSecuencial >= 0 ? indiceFilaSecuencial : (managerEVE != null ? managerEVE.indicePedidoActual : 0);

        if (!CalcularViabilidadOperativa(pedidoActual, destinoFisico, out float bateriaProyectada))
        {
            destinoActual = destinoFisico;
            if (managerEVE != null) managerEVE.amrEnAlertaBateria20 = this;
            estadoPreInterrupcionBateria = EstadoRobot.Inactivo;
            enPausaPreventivaBateria20 = false;
            Debug.Log("<color=#10B981>[FLUJO]: 6. Pedido 3 evaluado. Emergencia de Batería (Ind. 5.0) disparada. Esperando decisión del supervisor.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ALERTA PREDICTIVA] Misión inviable. Retorno proyectado: {bateriaProyectada:F1}% (Umbral LiFePO4: {UmbralSeguridadLiFePO4:F0}%). Detención preventiva.</color>");
            DashboardUI.Instance?.MostrarEmergencia(true, bateriaProyectada, 3.2f);
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            if (agente != null) agente.isStopped = true;
            return;
        }

        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "EN TRÁNSITO", Color.yellow);
        destinoActual = destinoFisico;
        enEsperaMutex = false;
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        agente.radius = RadioAgenteIndustrial;
        agente.angularSpeed = VelocidadAngularIndustrial;
        agente.speed = velocidadBase;
        agente.acceleration = aceleracionBase;

        TrazarRutaOrtogonal(destinoActual.position); // Da la orden de moverse por rieles VDA 5050
        Debug.Log("<color=cyan>AMR Desplegado:</color> Viajando a " + destinoFisico.name + " para buscar " + pedidoActual.codigo_sku);
        DashboardUI.Instance?.RegistrarLog("AMR Desplegado: Viajando a " + destinoFisico.name);
    }

    public void EnviarACargaInductiva(Vector3 posicionPad)
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        // Limpieza estricta de corrutinas y memoria de la misión abortada para evitar extracciones fantasma
        StopAllCoroutines();
        LiberarReservaSlotTMS();
        LimpiarPalletTemporal();
        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        estadoActual = EstadoRobot.Recargando;

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = true;

        if (agente != null)
        {
            if (!agente.enabled) agente.enabled = true;
            ActualizarJerarquiaNavegacion();
            agente.updateRotation = true;
            agente.isStopped = false;
            agente.ResetPath();
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = velocidadBase;
            agente.acceleration = aceleracionBase;
            TrazarRutaOrtogonal(posicionPad);
        }
        else
        {
            ActualizarJerarquiaNavegacion();
        }

        OnEstadoCambiado?.Invoke(estadoActual);
        Debug.Log($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de carga en {posicionPad}");
        DashboardUI.Instance?.RegistrarLog($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de recarga.");
    }

    public void InducirInterrupcionPreventivaBateria(float nuevaBateria)
    {
        estadoPreInterrupcionBateria = estadoActual;
        enPausaPreventivaBateria20 = true;
        bateriaActual = nuevaBateria;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        estadoActual = EstadoRobot.Interrumpido;
        ActualizarJerarquiaNavegacion();
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    public void ReanudarTrasRelevoDiferido()
    {
        relevoDiferidoPostEntrega = true;
        enPausaPreventivaBateria20 = false;
        estadoActual = EstadoRobot.Transportando;
        ActualizarJerarquiaNavegacion();
        if (agente != null && agente.enabled && agente.isOnNavMesh)
        {
            agente.isStopped = false;
            agente.speed = velocidadBase * 0.65f;
        }
        OnEstadoCambiado?.Invoke(estadoActual);
    }

    public Pedido ForzarApagadoCriticoBMS()
    {
        colapsoBMSArmadoPostDescarga = false;
        Pedido pedidoHuerfano = pedidoActual;
        bateriaActual = 2.4f;

        if (pedidoHuerfano != null)
        {
            DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REASIGNADO (<3%)", new Color(0.98f, 0.45f, 0.09f));
        }

        StopAllCoroutines();
        LiberarReservaSlotTMS();
        LimpiarPalletTemporal();
        if (palletActualGO != null)
        {
            Destroy(palletActualGO);
            palletActualGO = null;
        }
        LiberarMutexDeRobot(this);

        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        cediendoPasoAMR = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        if (agente != null)
        {
            if (agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = true;
                agente.ResetPath();
                agente.velocity = Vector3.zero;
            }
            agente.enabled = false;
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = false;

        estadoActual = EstadoRobot.Apagado;
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);

        return pedidoHuerfano;
    }

    public void FinalizarRescateEnPad(Vector3 posPad)
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = true;

        transform.position = posPad;
        transform.rotation = Quaternion.identity;

        if (agente != null)
        {
            agente.enabled = true;
            agente.Warp(posPad);
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        estadoActual = EstadoRobot.Recargando;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
    }

    public void EjecutarOverride()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        DirectorCinematico.Instance?.LiberarFocoIncidente(this);
        enEsperaMutex = false;

        modoDegradacionActivo = true;
        coeficienteCarga *= 1.3f;

        if (enPausaPreventivaBateria20)
        {
            enPausaPreventivaBateria20 = false;
            if (agente != null)
            {
                agente.speed = velocidadBase * 0.5f;
                agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
            }

            EstadoRobot estadoRestaurado = (estadoPreInterrupcionBateria != EstadoRobot.Inactivo && estadoPreInterrupcionBateria != EstadoRobot.Interrumpido)
                ? estadoPreInterrupcionBateria
                : (TieneCargaFisica ? EstadoRobot.Transportando : EstadoRobot.En_Transito);

            estadoActual = estadoRestaurado;
            ActualizarJerarquiaNavegacion();

            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = false;
            }

            OnEstadoCambiado?.Invoke(estadoActual);
            DashboardUI.Instance?.RegistrarLog($"<color=orange>[OVERRIDE EJECUTADO] {idRobot} reanuda misión ({estadoActual}) en caliente. Degradación térmica activa: Velocidad -50%, Aceleración -50%, Consumo +30%.</color>");
            Debug.Log($"<color=orange>[OVERRIDE EJECUTADO] {idRobot} reanuda misión ({estadoActual}) en caliente. Degradación térmica activa.</color>");
            return;
        }

        // Flujo si estaba Inactivo antes de iniciar ruta
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        if (agente != null)
        {
            agente.isStopped = false;
            agente.speed = velocidadBase * 0.5f;
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
        }

        if (destinoActual != null)
        {
            TrazarRutaOrtogonal(destinoActual.position);
        }

        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.RegistrarLog($"<color=orange>[OVERRIDE EJECUTADO] {idRobot}: Protocolo de contingencia forzado. Modo de degradación térmica activo: Aceleración -50%, Consumo +30%.</color>");
    }

    public void AbortarMisionPorSeguridad()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        colapsoBMSArmadoPostDescarga = false;

        string idPallet = pedidoActual != null ? pedidoActual.id_pallet : "ORDEN";
        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "REPROGRAMADO", new Color(1.0f, 0.55f, 0.1f));

        DashboardUI.Instance?.RegistrarLog($"<color=#FFA500>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Celda protegida al {bateriaActual:F1}%. Orden reprogramada.");
        Debug.Log($"<color=orange>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Batería conservada al {bateriaActual:F1}%.");

        StopAllCoroutines();
        LiberarReservaSlotTMS();
        LimpiarPalletTemporal();
        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        if (agente != null)
        {
            agente.updateRotation = true;
            agente.autoBraking = true;
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        if (managerEVE != null)
        {
            managerEVE.NotificarMisionAbortada();
        }
    }

    public void ReanudarTrasDespejeLiDAR()
    {
        if (!enParadaSeguridadLiDAR) return;

        enParadaSeguridadLiDAR = false;
        obstaculoDetectado = false;
        estadoActual = estadoPrevioBloqueo;
        ActualizarJerarquiaNavegacion();

        if (agente != null)
        {
            agente.isStopped = enEsperaMutex;
        }

        OnEstadoCambiado?.Invoke(estadoActual);
    }

    private GameObject ConstruirPalletVisual(Pedido pedido)
    {
        GameObject rootPallet = new GameObject($"Pallet_{pedido.id_pallet}_{pedido.codigo_sku}");
        if (puntoMontajeCarga != null)
        {
            rootPallet.transform.SetParent(puntoMontajeCarga, false);
            rootPallet.transform.localPosition = Vector3.zero;
            rootPallet.transform.localRotation = Quaternion.identity;
        }
        else
        {
            rootPallet.transform.SetParent(transform, false);
            rootPallet.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            rootPallet.transform.localRotation = Quaternion.identity;
        }

        Transform padreMontaje = puntoMontajeCarga != null ? puntoMontajeCarga : transform;
        rootPallet.transform.SetParent(padreMontaje, false);
        rootPallet.transform.localScale = Vector3.one;

        // Selección dinámica por SKU / Peso (920 kg o barril 50L -> PalletBarriles; 850 kg -> PalletCervezas)
        bool esBarrilCritico = pedido != null &&
            (pedido.peso_kg >= 900f ||
             (!string.IsNullOrEmpty(pedido.codigo_sku) &&
              (pedido.codigo_sku.Contains("50L") || pedido.codigo_sku.Contains("BAR"))));

        GameObject prefabElegido = esBarrilCritico ? prefabPalletBarriles : prefabPalletCervezas;

        if (prefabElegido != null)
        {
            GameObject modeloInstanciado = Instantiate(prefabElegido, rootPallet.transform, false);
            modeloInstanciado.name = prefabElegido.name;
            modeloInstanciado.transform.localPosition = Vector3.zero;
            modeloInstanciado.transform.localRotation = Quaternion.identity;
            modeloInstanciado.transform.localScale = Vector3.one;

            // 1. REGLA CRÍTICA LiDAR: Deshabilitar y destruir cualquier Collider en raíz o hijos
            Collider[] colliders = modeloInstanciado.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                    Destroy(colliders[i]);
                }
            }

            // 2. Corrección matemática de Pivote (Centrado X/Z = 0 y Base Inferior Y_min = 0.000m)
            //    + Optimización GPU para mallas densas (ej. PalletBarriles 1.37M tris -> sin ShadowCaster)
            Renderer[] renderers = modeloInstanciado.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds boundsMundo = renderers[0].bounds;
                for (int i = 0; i < renderers.Length; i++)
                {
                    boundsMundo.Encapsulate(renderers[i].bounds);

                    MeshFilter mf = renderers[i].GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 50000)
                    {
                        renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderers[i].receiveShadows = false;
                    }
                }

                Vector3 puntoBaseCentroMundo = new Vector3(boundsMundo.center.x, boundsMundo.min.y, boundsMundo.center.z);
                Vector3 desfaseLocal = rootPallet.transform.InverseTransformPoint(puntoBaseCentroMundo);
                modeloInstanciado.transform.localPosition -= desfaseLocal;
            }

            return rootPallet;
        }

        // Fallback seguro (primitivas) si algún prefab no estuviera asignado en el Inspector
        // 1. Base de madera oscura (Estiba oficial: 1.00m x 0.15m x 1.20m)
        GameObject baseMadera = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseMadera.name = "Base_Madera";
        baseMadera.transform.SetParent(rootPallet.transform, false);
        baseMadera.transform.localPosition = new Vector3(0f, 0.075f, 0f);
        baseMadera.transform.localScale = new Vector3(1.00f, 0.15f, 1.20f);

        // REGLA CRÍTICA DE FÍSICA: Eliminar cualquier Collider para no interferir con LiDAR ni NavMesh
        Collider colBase = baseMadera.GetComponent<Collider>();
        if (colBase != null) Destroy(colBase);

        Renderer rendBase = baseMadera.GetComponent<Renderer>();
        if (rendBase != null)
        {
            rendBase.material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            rendBase.material.color = new Color(0.35f, 0.20f, 0.10f); // Tono madera oscura
        }

        // 2. Bloque de carga
        GameObject bloqueCarga = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bloqueCarga.name = "Bloque_Carga";
        bloqueCarga.transform.SetParent(rootPallet.transform, false);
        bloqueCarga.transform.localPosition = new Vector3(0f, 0.80f, 0f);
        bloqueCarga.transform.localScale = new Vector3(0.96f, 1.45f, 1.16f);

        // REGLA CRÍTICA DE FÍSICA: Eliminar Collider del bloque de carga
        Collider colCarga = bloqueCarga.GetComponent<Collider>();
        if (colCarga != null) Destroy(colCarga);

        Renderer rendCarga = bloqueCarga.GetComponent<Renderer>();
        if (rendCarga != null)
        {
            rendCarga.material = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            if (pedido.peso_kg >= 900f || pedido.codigo_sku.Contains("50L"))
            {
                // Pedido crítico 920 kg: Rojo industrial distintivo
                rendCarga.material.color = new Color(0.85f, 0.15f, 0.10f);
            }
            else
            {
                // Pedidos estándar 850 kg: Color ámbar cervecero dorado
                rendCarga.material.color = new Color(0.85f, 0.55f, 0.12f);
            }
        }

        return rootPallet;
    }

    IEnumerator ProcesoExtraccion()
    {
        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        // Fase 0 (0.25s): Micro-alineación ortogonal exacta al Punto_Acople_AMR (-21.20f, -30.57f, 0° Norte)
        Vector3 posAcopleObjetivo = new Vector3(-21.20f, transform.position.y, -30.57f);
        Vector3 posInicialAMR = transform.position;
        Quaternion rotInicialAMR = transform.rotation;
        const float durAlineacionASRS = 0.25f;
        float tAlineacion = 0f;
        while (tAlineacion < durAlineacionASRS)
        {
            tAlineacion += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tAlineacion / durAlineacionASRS));
            transform.position = Vector3.Lerp(posInicialAMR, posAcopleObjetivo, s);
            transform.rotation = Quaternion.Slerp(rotInicialAMR, Quaternion.identity, s);
            yield return null;
        }
        transform.position = posAcopleObjetivo;
        transform.rotation = Quaternion.identity;

        // Ocultar temporalmente el Pallet_Stock del slot en el estante jefe y posicionar el pallet dinámico en ese slot
        stockOcultadoActual = null;
        if (destinoActual != null)
        {
            Transform stockHijo = destinoActual.Find("Pallet_Stock");
            if (stockHijo != null && stockHijo.gameObject.activeSelf)
            {
                stockOcultadoActual = stockHijo.gameObject;
                stockOcultadoActual.SetActive(false);
            }
        }

        palletActualGO = ConstruirPalletVisual(pedidoActual);
        Transform padreMontaje = puntoMontajeCarga != null ? puntoMontajeCarga : transform;
        Vector3 metaLocal = puntoMontajeCarga != null ? Vector3.zero : new Vector3(0f, 0.6f, 0f);

        if (palletActualGO != null && destinoActual != null)
        {
            palletActualGO.transform.SetParent(null, true);
            Vector3 posSlot = destinoActual.position;
            palletActualGO.transform.position = posSlot;
            palletActualGO.transform.rotation = Quaternion.identity;

            GameObject goPlat = GameObject.Find("Plataforma_Ascensor");
            Transform platAscensor = goPlat != null ? goPlat.transform : null;

            const float xCintaCentral = -21.20f;
            const float zAscensor = -28.00f;
            const float yAcoplePalletAMR = 0.52186f;
            const float yBasePlataforma = 0.45186f;
            float yPlataformaNivel = Mathf.Max(yBasePlataforma, posSlot.y - 0.07f);

            // Etapa 1 (0.7s): Empuje lateral del pallet hacia la Cinta_Central_Rodillos de su nivel
            // mientras la Plataforma_Ascensor se posiciona verticalmente en ese nivel
            const float durEtapa1 = 0.7f;
            Vector3 posCinta = new Vector3(xCintaCentral, posSlot.y, posSlot.z);
            Vector3 posPlatInicio = platAscensor != null ? platAscensor.position : new Vector3(xCintaCentral, yBasePlataforma, zAscensor);
            Vector3 posPlatNivel = new Vector3(xCintaCentral, yPlataformaNivel, zAscensor);

            float t1 = 0f;
            while (t1 < durEtapa1)
            {
                t1 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t1 / durEtapa1));
                if (palletActualGO != null)
                {
                    palletActualGO.transform.position = Vector3.Lerp(posSlot, posCinta, s);
                }
                if (platAscensor != null)
                {
                    platAscensor.position = Vector3.Lerp(posPlatInicio, posPlatNivel, s);
                }
                yield return null;
            }

            // Etapa 2 (1.2s): Avance longitudinal por la Cinta_Central_Rodillos (-Z) hasta ingresar al Ascensor_VLM
            const float durEtapa2 = 1.2f;
            Vector3 posEnAscensor = new Vector3(xCintaCentral, posSlot.y, zAscensor);
            float t2 = 0f;
            while (t2 < durEtapa2)
            {
                t2 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t2 / durEtapa2));
                if (palletActualGO != null)
                {
                    palletActualGO.transform.position = Vector3.Lerp(posCinta, posEnAscensor, s);
                }
                yield return null;
            }

            // Etapa 3 (1.1s): Descenso vertical sincronizado del Ascensor_VLM y el pallet hasta la cota de acople del AMR
            const float durEtapa3 = 1.1f;
            Vector3 posAscensorBase = new Vector3(xCintaCentral, yAcoplePalletAMR, zAscensor);
            Vector3 posPlatBase = new Vector3(xCintaCentral, yBasePlataforma, zAscensor);
            float t3 = 0f;
            while (t3 < durEtapa3)
            {
                t3 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t3 / durEtapa3));
                if (palletActualGO != null)
                {
                    palletActualGO.transform.position = Vector3.Lerp(posEnAscensor, posAscensorBase, s);
                }
                if (platAscensor != null)
                {
                    platAscensor.position = Vector3.Lerp(posPlatNivel, posPlatBase, s);
                }
                yield return null;
            }

            // Etapa 4 (0.9s): Transferencia final por Estacion_Entrega_Rodillos hacia Punto_Montaje_Carga del AMR
            const float durEtapa4 = 0.9f;
            float t4 = 0f;
            while (t4 < durEtapa4)
            {
                t4 += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t4 / durEtapa4));
                if (palletActualGO != null)
                {
                    Vector3 metaMundo = puntoMontajeCarga != null
                        ? puntoMontajeCarga.position
                        : (transform.position + new Vector3(0f, 0.52186f, 0f));
                    palletActualGO.transform.position = Vector3.Lerp(posAscensorBase, metaMundo, s);
                }
                yield return null;
            }

            if (palletActualGO != null)
            {
                palletActualGO.transform.SetParent(padreMontaje, false);
                palletActualGO.transform.localPosition = metaLocal;
                palletActualGO.transform.localRotation = Quaternion.identity;
            }
        }

        if (agente != null)
        {
            agente.isStopped = false;
        }

        // Liberar reserva del rack extraído al iniciar retorno hacia muelle
        LiberarZonaMutex(destinoActual, this);

        // Consultar muelle de destino únicamente si aún no hay reserva activa para esta misión
        if (zonaTransferencia == null || slotLIFOAsignado <= 0)
        {
            if (managerEVE != null && managerEVE.gestorTMS != null)
            {
                Transform muelleDestino = managerEVE.gestorTMS.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out slotLIFOAsignado);
                if (muelleDestino != null) zonaTransferencia = muelleDestino;
            }
            else if (ModuloDespachoTMS.Instance != null)
            {
                Transform muelleDestino = ModuloDespachoTMS.Instance.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out slotLIFOAsignado);
                if (muelleDestino != null) zonaTransferencia = muelleDestino;
            }
        }
        
        if (zonaTransferencia != null)
        {
            enEsperaMutex = false;
            estadoActual = EstadoRobot.Transportando;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase - (pedidoActual.peso_kg * coeficientePeso * 2f));

            Vector3 destinoParadaCamion = ObtenerPosicionParadaDescargaActiva();
            TrazarRutaOrtogonal(destinoParadaCamion);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a posición de parada en Camión (" + zonaTransferencia.name + $" en {destinoParadaCamion}).");
            DashboardUI.Instance?.RegistrarLog("AMR Retornando: Llevando estiba a " + zonaTransferencia.name);
        }
        else
        {
            Debug.LogError("AMR Error: Falta asignar la Zona de Transferencia en el Inspector.");
            DashboardUI.Instance?.RegistrarLog("AMR Error: Falta Zona de Transferencia.");
        }
    }

    IEnumerator ProcesoEntrega()
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        estadoActual = EstadoRobot.Entregando;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        DashboardUI.Instance?.ActualizarEstadoManifiesto(indiceFilaManifiesto, "ENTREGADO", Color.green);
        Debug.Log("<color=magenta>LIFO:</color> Descargando estiba en Slot Flatbed del Camión por rodillos frontales.");
        DashboardUI.Instance?.RegistrarLog("LIFO: Descarga frontal por rodillos en camión.");

        if (agente != null)
        {
            agente.isStopped = true;
            agente.ResetPath();
            agente.velocity = Vector3.zero;
        }

        // a) 0.20s de asentamiento del chasis en la posición de parada y alineación a 0° Norte (Quaternion.identity)
        Vector3 posParada = ObtenerPosicionParadaDescargaActiva();
        Vector3 posInicialAMR = transform.position;
        Quaternion rotInicialAMR = transform.rotation;
        Vector3 posFinalAMR = new Vector3(posParada.x, transform.position.y, posParada.z);
        const float durAsentamientoChasis = 0.20f;
        float tChasis = 0f;
        while (tChasis < durAsentamientoChasis)
        {
            tChasis += Time.deltaTime;
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tChasis / durAsentamientoChasis));
            transform.position = Vector3.Lerp(posInicialAMR, posFinalAMR, s);
            transform.rotation = Quaternion.Slerp(rotInicialAMR, Quaternion.identity, s);
            yield return null;
        }
        transform.position = posFinalAMR;
        transform.rotation = Quaternion.identity;

        // b) 0.85s: Deslizamiento frontal por rodillos del pallet hacia destinoSlotLIFO mientras el AMR permanece inmóvil
        ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
        Vector3 destinoSlotLIFO = ObtenerPosicionDescargaActiva();
        Quaternion rotacionSlot = zonaTransferencia != null ? zonaTransferencia.rotation : Quaternion.identity;

        GameObject palletEntregado = palletActualGO;
        palletActualGO = null;
        stockOcultadoActual = null;

        if (palletEntregado != null)
        {
            palletEntregado.transform.SetParent(null, true);
            Vector3 posInicioDeslizamiento = palletEntregado.transform.position;
            Quaternion rotInicioDeslizamiento = palletEntregado.transform.rotation;

            const float duracionDeslizamiento = 0.85f;
            float tDesliz = 0f;
            while (tDesliz < duracionDeslizamiento)
            {
                tDesliz += Time.deltaTime;
                float factor = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tDesliz / duracionDeslizamiento));
                if (palletEntregado != null)
                {
                    palletEntregado.transform.position = Vector3.Lerp(posInicioDeslizamiento, destinoSlotLIFO, factor);
                    palletEntregado.transform.rotation = Quaternion.Slerp(rotInicioDeslizamiento, rotacionSlot, factor);
                }
                yield return null;
            }

            if (palletEntregado != null)
            {
                palletEntregado.transform.position = destinoSlotLIFO;
                palletEntregado.transform.rotation = rotacionSlot;
            }

            if (tms != null)
            {
                tms.RegistrarEstibaEnCamion(numCamionAsignado, pedidoActual, palletEntregado, slotLIFOAsignado);
            }
            else if (managerEVE != null)
            {
                managerEVE.RegistrarEntregaLIFO(pedidoActual, palletEntregado);
            }
            slotLIFOAsignado = 0;
        }
        else
        {
            yield return new WaitForSeconds(0.85f);
        }

        // c) 0.90s de retroceso recto de despegue con chasis vacío (1.20 m hacia -transform.forward con agente.updateRotation = false)
        const float duracionRetroceso = 0.90f;
        const float distanciaRetroceso = 1.20f;
        if (agente != null)
        {
            bool prevUpdateRotation = agente.updateRotation;
            agente.updateRotation = false;

            Vector3 dirRetroceso = -transform.forward;
            dirRetroceso.y = 0f;
            if (dirRetroceso.sqrMagnitude < 0.001f) dirRetroceso = Vector3.back;
            dirRetroceso.Normalize();

            float velocidadRetroceso = distanciaRetroceso / duracionRetroceso;
            float tRetroceso = 0f;
            while (tRetroceso < duracionRetroceso)
            {
                float dt = Time.deltaTime;
                tRetroceso += dt;
                agente.Move(dirRetroceso * (velocidadRetroceso * dt));
                yield return null;
            }

            agente.updateRotation = prevUpdateRotation;
            agente.isStopped = false;
        }
        else
        {
            yield return new WaitForSeconds(duracionRetroceso);
        }

        if (colapsoBMSArmadoPostDescarga)
        {
            colapsoBMSArmadoPostDescarga = false;
            LiberarMutexDeRobot(this);
            pedidoActual = null;
            destinoActual = null;
            zonaTransferencia = null;

            if (agente != null)
            {
                agente.speed = velocidadBase;
                agente.acceleration = modoDegradacionActivo ? Mathf.Max(1.0f, aceleracionBase * 0.5f) : aceleracionBase;
            }

            if (managerEVE != null)
            {
                managerEVE.FinalizarEntregaYEjecutarColapsoBMS(this);
                yield break;
            }
        }

        if (relevoDiferidoPostEntrega)
        {
            relevoDiferidoPostEntrega = false;
            LiberarMutexDeRobot(this);
            pedidoActual = null;
            destinoActual = null;
            zonaTransferencia = null;

            if (managerEVE != null)
            {
                managerEVE.palletsEnColaWMS = Mathf.Max(0, managerEVE.palletsEnColaWMS - 1);
                managerEVE.palletsCompletadosTurno++;
                FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(managerEVE.palletsEnColaWMS);
            }

            if (agente != null)
            {
                agente.speed = velocidadBase;
                agente.acceleration = modoDegradacionActivo ? Mathf.Max(1.0f, aceleracionBase * 0.5f) : aceleracionBase;
            }

            Vector3 posPad = (managerEVE != null) ? managerEVE.ObtenerPosicionPadPorUnidad(this) : transform.position;
            EnviarACargaInductiva(posPad);
            yield break;
        }

        LiberarMutexDeRobot(this);
        estadoActual = EstadoRobot.Inactivo;
        ActualizarJerarquiaNavegacion();
        OnEstadoCambiado?.Invoke(estadoActual);

        agente.speed = velocidadBase;
        if (modoDegradacionActivo)
        {
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase * 0.5f);
        }
        else
        {
            agente.acceleration = aceleracionBase;
        }

        if (managerEVE != null)
        {
            managerEVE.ConfirmarEntregaExitosa(this);
        }
    }

    private bool EsObjetivoDeAcoplamiento(Transform hitTransform)
    {
        if (hitTransform == null) return false;

        // Si viaja hacia una estantería en En_Transito
        if (estadoActual == EstadoRobot.En_Transito && destinoActual != null)
        {
            if (hitTransform == destinoActual || 
                hitTransform.IsChildOf(destinoActual) || 
                destinoActual.IsChildOf(hitTransform))
            {
                return true;
            }

            if (EsEstacionTransferenciaFrontalASRS(destinoActual, destinoActual.position) &&
                EsEstacionTransferenciaFrontalASRS(hitTransform, hitTransform.position))
            {
                return true;
            }
        }
        // Si retorna hacia la zona de transferencia / camión en Transportando
        else if (estadoActual == EstadoRobot.Transportando && zonaTransferencia != null)
        {
            if (hitTransform == zonaTransferencia || 
                hitTransform.IsChildOf(zonaTransferencia) || 
                zonaTransferencia.IsChildOf(hitTransform))
            {
                return true;
            }
        }

        return false;
    }

    private bool EsObstaculoISORegistrado(Transform hitTransform)
    {
        if (hitTransform == null || managerEVE == null || managerEVE.obstaculoPasilloISO == null) return false;
        Transform raizISO = managerEVE.obstaculoPasilloISO.transform;
        return hitTransform == raizISO || hitTransform.IsChildOf(raizISO);
    }

    private void ProcesarSensoresLiDAR()
    {
        if (enParadaSeguridadLiDAR) return;

        Vector3 origen = transform.position + Vector3.up * 0.5f;
        obstaculoDetectado = false;

        Vector3[] direcciones = new Vector3[]
        {
            transform.forward,
            Quaternion.Euler(0, -15, 0) * transform.forward,
            Quaternion.Euler(0, 15, 0) * transform.forward
        };

        foreach (var dir in direcciones)
        {
            if (Physics.Raycast(origen, dir, out RaycastHit hit, distanciaMaxSensor, capaObstaculos))
            {
                // 1. Filtrar propio chasis y componentes hijos (punto de carga, telemetría, estibas montadas)
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
                    continue;
                }

                // 2. Filtrar suelo de la bodega
                if (hit.transform.name.Contains("Suelo") || hit.point.y <= 0.35f)
                {
                    Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
                    continue;
                }

                // 3. Protocolo Docking Mute (ISO 3691-4): Acoplamiento con objetivo legítimo
                if (EsObjetivoDeAcoplamiento(hit.transform))
                {
                    // Visualización en amarillo: Acoplamiento legítimo en curso sin penalizar velocidad
                    Debug.DrawLine(origen, hit.point, Color.yellow);
                    continue;
                }

                // 4. Obstáculo imprevisto
                // Parada Cat. 3 solo con el AMR en movimiento y ante el obstáculo ISO registrado por EVE.
                // Evita disparos en reposo (spawn frente a P1-C1-F2) y que EVE desactive infraestructura estática (racks).
                bool amrEnMovimiento = estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando;
                if (hit.distance <= distanciaFrenadoEmergencia && !enParadaSeguridadLiDAR && amrEnMovimiento && EsObstaculoISORegistrado(hit.transform))
                {
                    Debug.Log("<color=#10B981>[FLUJO]: 4. Emergencia LiDAR (Ind. 4.0) disparada correctamente.</color>");
                    enParadaSeguridadLiDAR = true;
                    estadoPrevioBloqueo = estadoActual;
                    obstaculoDetectado = true;
                    Debug.DrawLine(origen, hit.point, Color.red);

                    if (agente != null)
                    {
                        agente.isStopped = true;
                        agente.velocity = Vector3.zero;
                    }

                    estadoActual = EstadoRobot.Interrumpido;
                    ActualizarJerarquiaNavegacion();
                    OnEstadoCambiado?.Invoke(estadoActual);

                    if (managerEVE == null) managerEVE = FindFirstObjectByType<RobotManagerEVE>();
                    managerEVE?.GestionarObstruccionAutonomaISO(this, hit.collider.gameObject);
                    break;
                }
                else if (hit.distance < distanciaDeteccionAlerta)
                {
                    // Zona de advertencia crítica: reducción preventiva de velocidad
                    obstaculoDetectado = true;
                    Debug.DrawLine(origen, hit.point, Color.red);
                }
                else
                {
                    // Obstáculo detectado en zona lejana (amarillo-naranja de aviso)
                    Debug.DrawLine(origen, hit.point, new Color(1f, 0.6f, 0f));
                }
            }
            else
            {
                Debug.DrawRay(origen, dir * distanciaMaxSensor, Color.cyan);
            }
        }

        // Gestión activa de velocidad preventiva según LiDAR
        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            float velocidadNominal = ObtenerVelocidadNominal();
            if (obstaculoDetectado)
            {
                // Campo de protección LiDAR activo: 40% de velocidad nominal
                agente.speed = velocidadNominal * 0.4f;
            }
            else
            {
                agente.speed = velocidadNominal;
            }
        }
    }

    private float ObtenerVelocidadNominal()
    {
        if (estadoActual == EstadoRobot.En_Transito)
        {
            return velocidadBase;
        }
        else if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
        {
            return Mathf.Max(0.5f, velocidadBase - (pedidoActual.peso_kg * coeficientePeso));
        }
        return velocidadBase;
    }

    private int ObtenerPrioridadVDA(RobotAMR r)
    {
        switch (r.estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
                return 4;
            case EstadoRobot.Transportando:
                return 3;
            case EstadoRobot.En_Transito:
                return 2;
            case EstadoRobot.Recargando:
                return 1;
            default:
                return 0;
        }
    }

    private bool DebeCederPasoA(RobotAMR otro)
    {
        if (otro == null) return false;

        // Regla 1: Nunca ceder el paso por burbuja a un AMR detenido esperando compuerta Mutex
        if (otro.enEsperaMutex) return false;

        // Regla 2: Quien evacúa la zona de muelle/rampas tiene prioridad absoluta ("Antes de entrar, deje salir")
        bool yoEvacuando = EstaEvacuandoZonaMuelle();
        bool otroEvacuando = otro.EstaEvacuandoZonaMuelle();
        if (yoEvacuando && !otroEvacuando) return false;
        if (!yoEvacuando && otroEvacuando) return true;

        int miPrio = ObtenerPrioridadVDA(this);
        int otroPrio = ObtenerPrioridadVDA(otro);
        if (otroPrio != miPrio) return otroPrio > miPrio;
        return ObtenerIndiceUnidad() > otro.ObtenerIndiceUnidad();
    }

    private void GestionarBurbujaProximidadVDA5050()
    {
        bool enMovimientoNavegable = estadoActual == EstadoRobot.En_Transito ||
                                     estadoActual == EstadoRobot.Transportando ||
                                     estadoActual == EstadoRobot.Recargando;

        if (!enMovimientoNavegable || agente == null || !agente.enabled || !agente.isOnNavMesh)
        {
            cediendoPasoAMR = false;
            tiempoDetenidoCediendo = 0f;
            return;
        }

        // Ventana de inmunidad temporal para despejar cruces tras activarse el watchdog anti-deadlock
        if (Time.time < tiempoInmunidadDespeje)
        {
            tiempoDetenidoCediendo = 0f;
            if (cediendoPasoAMR)
            {
                cediendoPasoAMR = false;
                if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
                {
                    agente.isStopped = false;
                }
            }
            return;
        }

        // Si ya está estacionado cargando en el Pad, no participa en evasión activa
        if (estadoActual == EstadoRobot.Recargando && colaWaypoints.Count == 0 && !agente.pathPending && agente.remainingDistance <= 0.60f)
        {
            cediendoPasoAMR = false;
            tiempoDetenidoCediendo = 0f;
            return;
        }

        Vector2 posMia = new Vector2(transform.position.x, transform.position.z);
        Vector2 dirMiMov = new Vector2(transform.forward.x, transform.forward.z);
        if (agente.desiredVelocity.sqrMagnitude > 0.01f)
        {
            dirMiMov = new Vector2(agente.desiredVelocity.x, agente.desiredVelocity.z);
        }
        if (dirMiMov.sqrMagnitude > 0.0001f) dirMiMov.Normalize();

        float umbral = cediendoPasoAMR ? RadioBurbujaSalidaVDA : RadioBurbujaEntradaVDA;
        bool debeCeder = false;
        RobotAMR otroEnConflicto = null;

        for (int i = 0; i < flotaActiva.Count; i++)
        {
            RobotAMR otro = flotaActiva[i];
            if (otro == null || otro == this || !otro.gameObject.activeInHierarchy) continue;
            if (otro.estadoActual == EstadoRobot.Inactivo || otro.estadoActual == EstadoRobot.Apagado) continue;

            // Ignorar a otro AMR si está estacionado en su Pad cargando (a menos que estemos justo en su área)
            if (otro.estadoActual == EstadoRobot.Recargando && otro.agente != null && otro.agente.enabled && otro.agente.isStopped &&
                Vector2.Distance(posMia, new Vector2(otro.transform.position.x, otro.transform.position.z)) >= 1.50f)
            {
                continue;
            }

            Vector2 posOtro = new Vector2(otro.transform.position.x, otro.transform.position.z);
            Vector2 haciaOtro = posOtro - posMia;
            float dist = haciaOtro.magnitude;

            if (dist < umbral)
            {
                Vector2 dirHaciaOtro = dist > 0.001f ? (haciaOtro / dist) : Vector2.zero;
                bool trayectoriasConvergen = Vector2.Dot(dirMiMov, dirHaciaOtro) > -0.15f || dist < 1.35f;

                if (trayectoriasConvergen && DebeCederPasoA(otro))
                {
                    debeCeder = true;
                    otroEnConflicto = otro;
                    break;
                }
            }
        }

        if (debeCeder)
        {
            if (!cediendoPasoAMR)
            {
                cediendoPasoAMR = true;
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }

            bool otroDetenido = otroEnConflicto != null &&
                                (otroEnConflicto.cediendoPasoAMR ||
                                 otroEnConflicto.enEsperaMutex ||
                                 (otroEnConflicto.agente != null && otroEnConflicto.agente.velocity.sqrMagnitude < 0.04f));

            if (otroDetenido)
            {
                tiempoDetenidoCediendo += Time.deltaTime;
                bool tienePrioridadDesempate = EstaEvacuandoZonaMuelle() ||
                                               (otroEnConflicto != null && ObtenerIndiceUnidad() < otroEnConflicto.ObtenerIndiceUnidad());
                float limiteWatchdog = tienePrioridadDesempate ? 1.2f : 1.8f;

                if (tiempoDetenidoCediendo >= limiteWatchdog)
                {
                    tiempoInmunidadDespeje = Time.time + 3.0f;
                    tiempoDetenidoCediendo = 0f;
                    cediendoPasoAMR = false;
                    if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
                    {
                        agente.isStopped = false;
                    }
                    Debug.Log($"<color=yellow>[VDA 5050 WATCHDOG]:</color> {idRobot} liberado por inmunidad de despeje (3.0s) ante bloqueo circular.");
                }
            }
            else
            {
                tiempoDetenidoCediendo = 0f;
            }
        }
        else
        {
            tiempoDetenidoCediendo = 0f;
            if (cediendoPasoAMR)
            {
                cediendoPasoAMR = false;
                if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
                {
                    agente.isStopped = false;
                }
            }
        }
    }

    void Update()
    {
        if (estadoActual == EstadoRobot.Apagado) return;

        if (colapsoBMSArmadoPostDescarga && !TieneCargaFisica && transform.position.z < -2.5f)
        {
            colapsoBMSArmadoPostDescarga = false;
            managerEVE?.EjecutarColapsoYRescateInmediato(this);
            return;
        }

        // Procesamiento activo del sensor LiDAR con Docking Mute (ISO 3691-4)
        ProcesarSensoresLiDAR();

        // Burbuja de proximidad inter-AMR VDA 5050 (2.5m Yield / 2.8m Resume)
        GestionarBurbujaProximidadVDA5050();

        // Transición fluida entre tramos ortogonales (VDA 5050) sin parada en esquinas intermedias
        bool cambioTramoWaypoint = AvanzarColaWaypoints();

        // Gestión de exclusión mutua (Mutex) de zona de acople a <= 3.5m en el tramo final
        if (!cambioTramoWaypoint)
        {
            GestionarCompuertaMutex();
        }

        // Consumo pasivo obligatorio por encendido de sistemas lógicos (blindado a 0% en parada de seguridad/interrupción/espera Mutex/cediendo paso)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && !cediendoPasoAMR && estadoActual != EstadoRobot.Interrumpido && estadoActual != EstadoRobot.Recargando)
        {
            bateriaActual -= consumoIdle * Time.deltaTime;
        }

        // Matemáticas de Batería (Termodinámica Inyectada)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && !cediendoPasoAMR && estadoActual != EstadoRobot.Interrumpido &&
            (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando) &&
            agente != null && agente.enabled)
        {
            float distanciaRecorrida = agente.velocity.magnitude * Time.deltaTime;
            if (distanciaRecorrida > 0)
            {
                float gasto = 0f;
                if (estadoActual == EstadoRobot.En_Transito)
                {
                    gasto = distanciaRecorrida * coeficienteVacio;
                }
                else if (estadoActual == EstadoRobot.Transportando && pedidoActual != null)
                {
                    // Inyección de Ruido Termodinámico: Oscilación orgánica (Perlin Noise) para fricción e ineficiencia de motor (0.98 a 1.15)
                    float factorDeRuido = Mathf.Lerp(0.98f, 1.15f, Mathf.PerlinNoise(Time.time * 0.5f, 0f));
                    gasto = distanciaRecorrida * coeficienteCarga * (pedidoActual.peso_kg / 100f) * factorDeRuido;
                }
                
                bateriaActual -= gasto;
            }
        }

        // CRI-04: Acotamiento de batería y corte crítico al 0%
        bateriaActual = Mathf.Max(0f, bateriaActual);
        if (bateriaActual <= 0f && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando))
        {
            if (agente != null && agente.enabled && agente.isOnNavMesh)
            {
                agente.isStopped = true;
                agente.velocity = Vector3.zero;
            }
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            DashboardUI.Instance?.RegistrarLog("<color=red><b>[FALLO CATASTRÓFICO]:</b> Celda agotada al 0% en pasillo. Unidad inmovilizada.</color>");
            Debug.LogError("[FALLO CATASTRÓFICO] Batería agotada al 0% en pasillo. NavMeshAgent inmovilizado.");
        }

        // Telemetría con throttling (6.6 Hz / 0.15s) y compuerta por delta (> 0.1% o cambio de estado)
        if (Time.time >= tiempoProximoEnvioTelemetria || Mathf.Abs(bateriaActual - ultimaBateriaTransmitida) > 0.1f || estadoActual != ultimoEstadoTransmitido)
        {
            tiempoProximoEnvioTelemetria = Time.time + IntervaloTelemetria;
            ultimaBateriaTransmitida = bateriaActual;
            ultimoEstadoTransmitido = estadoActual;
            DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
        }

        if (agente == null || !agente.enabled || !agente.isOnNavMesh) return;
        if (cambioTramoWaypoint || colaWaypoints.Count > 0) return;

        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!enEsperaMutex && !cediendoPasoAMR && !agente.pathPending && agente.remainingDistance <= 0.15f && DistanciaPlanaADestinoAgente() <= 0.25f)
            {
                estadoActual = EstadoRobot.Extrayendo;
                ActualizarJerarquiaNavegacion();
                OnEstadoCambiado?.Invoke(estadoActual);
                Debug.Log("<color=yellow>Destino Alcanzado:</color> Extrayendo estiba de " + pedidoActual.peso_kg + " kg.");
                DashboardUI.Instance?.RegistrarLog("Destino Alcanzado: Extrayendo estiba.");
                StartCoroutine(ProcesoExtraccion());
            }
        }
        else if (estadoActual == EstadoRobot.Transportando)
        {
            // CRI-01 y MED-04: Detección de arribo exacto a la posición de parada previa al Slot LIFO (Offset 1.30m)
            if (!enEsperaMutex && !cediendoPasoAMR && zonaTransferencia != null && !agente.pathPending && agente.remainingDistance <= 0.15f && DistanciaPlanaADestinoAgente() <= 0.25f)
            {
                StartCoroutine(ProcesoEntrega());
            }
        }
        else if (estadoActual == EstadoRobot.Recargando)
        {
            if (!agente.pathPending && agente.remainingDistance <= 0.5f && DistanciaPlanaADestinoAgente() <= 0.75f)
            {
                // Llegó al pad de inducción. Frenar y recargar progresivamente.
                agente.isStopped = true;
                // KUKA KMP 1500P Datasheet: Charge from 20% to 80% within 1 hour (60% / 3600s = 0.0166667% / s = 1.0% por minuto real)
                bateriaActual = Mathf.Min(80.0f, bateriaActual + (Time.deltaTime * tasaRecargaPorSegundo));
                DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);

                if (bateriaActual >= 80.0f)
                {
                    estadoActual = EstadoRobot.Inactivo;
                    ActualizarJerarquiaNavegacion();
                    OnEstadoCambiado?.Invoke(estadoActual);
                    DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
                }
            }
        }
    }
}
````

## File: Assets/_ProyectoBodega/Scripts/RobotManagerEVE.cs
````csharp
using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

public class RobotManagerEVE : MonoBehaviour
{
    public event System.Action OnMisionEvaluada;

    [Header("Waypoints de Vuelo")]
    public Transform[] waypoints;
    public float velocidadVuelo = 5f;
    private int indiceWaypointActual = 0;

    [Header("Arquitectura Energética WMS (IEC 61508)")]
    // EVE opera como Orquestador Maestro SIL-3 alimentado por Bus Aéreo PoE++ 48V DC continuo.
    // Disponibilidad 24/7 garantizada sin degradación química de celda, evitando Single Point of Failure.
    [Tooltip("Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada")]
    public string estadoAlimentacionWMS = "Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada";

    [Header("Confinamiento Aéreo Espacial (AABB)")]
    public const float LimiteXMin = -38.0f;
    public const float LimiteXMax = 52.0f;
    public const float AltitudMin = 9.2f;
    public const float AltitudMax = 10.5f;
    public const float LimiteZMin = -43.0f;
    public const float LimiteZMax = -7.5f;

    [Header("Integración TMS (Módulo 5)")]
    public ModuloDespachoTMS gestorTMS;

    [Header("Flota de Robots AMR")]
    public RobotAMR robotAMR_01;
    public RobotAMR robotAMR_02;
    public RobotAMR robotAMR_03;

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public RobotAMR amrEnAlertaBateria20;
    public int indicePedidoActual = 0;
    public bool turnoIniciado = false;
    private bool sesionOperativaIniciada = false;
    private bool relevo24_7Activo = false;
    private Dictionary<string, Transform> directorioBodega = new Dictionary<string, Transform>();

    [Header("Cola Acumulativa WMS en Vivo")]
    public int palletsEnColaWMS = 0;
    public int palletsCompletadosTurno = 0;
    public bool flotaEscaladaDual = false;
    private List<Pedido> plantillaPedidosFisicos = new List<Pedido>();
    private readonly Queue<Pedido> colaPrioritariaReasignacion = new Queue<Pedido>();
    private int indiceFilaManifiestoGlobal = 0;
    private float tiempoProximoDespachoPermitido = 0f;
    private Coroutine rutinaHeadwayDespacho;
    private const float HeadwaySalidaSalaCargaSegundos = 5.0f;

    [Header("Trazabilidad LIFO Camión")]
    private Stack<Pedido> matrizLIFOCamion = new Stack<Pedido>();
    private List<GameObject> palletsEntregados = new List<GameObject>();
    private float pesoAcumuladoCamion = 0f;
    private const float CapacidadMaximaCamionKg = 12000f;

    public Stack<Pedido> MatrizLIFOCamion => matrizLIFOCamion;
    public float PesoAcumuladoCamion => pesoAcumuladoCamion;

    [Header("Simulacion de Incertidumbre ISO 3691-4")]
    public GameObject obstaculoPasilloISO;
    public DronMantenimiento dronMantenimiento;
    private bool despachandoDespejeISO = false;

    public enum EscenarioHorizonte { EscenarioA_InicioTurno, EscenarioB_Madrugada, EscenarioC_FinalTurno }

    [Header("Simulador de Estrés de Flota (Industria 5.0 - O(1))")]
    public float horaTurnoActual = 1.0f;
    private bool alertaDemandaEnCurso = false;
    private bool enSaltoTemporal = false;
    private int indiceCicloDemoHorizonte = 0;

    void Awake()
    {
        if (gestorTMS == null) gestorTMS = FindFirstObjectByType<ModuloDespachoTMS>();
        if (dronMantenimiento == null) dronMantenimiento = FindFirstObjectByType<DronMantenimiento>();
        if (robotAMR_01 == null)
        {
            GameObject r1 = GameObject.Find("Robot_AMR");
            if (r1 != null) robotAMR_01 = r1.GetComponent<RobotAMR>();
        }
        if (robotAMR_02 == null)
        {
            GameObject r2 = GameObject.Find("Robot_AMR_02");
            if (r2 != null) robotAMR_02 = r2.GetComponent<RobotAMR>();
        }
        if (robotAMR_03 == null)
        {
            GameObject r3 = GameObject.Find("Robot_AMR_03");
            if (r3 != null) robotAMR_03 = r3.GetComponent<RobotAMR>();
        }
        if (robotEjecutor == null) robotEjecutor = robotAMR_01;

        CargarManifiesto();
    }

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            InyectarObstaculoPasilloISO();
        }

        if (turnoIniciado && !enSaltoTemporal && horaTurnoActual < 7.5f)
        {
            float deltaHoras = (Time.deltaTime * RobotAMR.FactorCompresionTurno) / 3600f;
            horaTurnoActual = Mathf.Min(7.5f, horaTurnoActual + deltaHoras);
        }

        if (Input.GetKeyDown(KeyCode.I) && turnoIniciado)
        {
            bool shiftPresionado = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shiftPresionado)
            {
                float[] horasDemo = new float[] { 1.5f, 5.0f, 7.5f };
                horaTurnoActual = horasDemo[indiceCicloDemoHorizonte];
                indiceCicloDemoHorizonte = (indiceCicloDemoHorizonte + 1) % horasDemo.Length;
                alertaDemandaEnCurso = false;
                ProyectarEstresDeFlota(50);
            }
            else if (!alertaDemandaEnCurso)
            {
                ProyectarEstresDeFlota(50);
            }
        }

        if (!turnoIniciado) return;
        MoverPorWaypoints();
    }

    public void IniciarTurno()
    {
        if (turnoIniciado) return;
        turnoIniciado = true;
        sesionOperativaIniciada = true;
        palletsEnColaWMS = 0;
        palletsCompletadosTurno = 0;
        flotaEscaladaDual = false;
        indicePedidoActual = 0;
        indiceFilaManifiestoGlobal = 0;
        colaPrioritariaReasignacion.Clear();
        relevo24_7Activo = false;
        tiempoProximoDespachoPermitido = 0f;
        if (rutinaHeadwayDespacho != null)
        {
            StopCoroutine(rutinaHeadwayDespacho);
            rutinaHeadwayDespacho = null;
        }

        Debug.Log("<color=#10B981>[FLUJO]: 1. Turno iniciado en STANDBY. Esperando ingesta JSON ERP.</color>");

        Transform hudTransform = DashboardUI.Instance != null ? DashboardUI.Instance.transform.Find("HUD_Operativo") : null;
        if (hudTransform != null)
        {
            hudTransform.gameObject.SetActive(true);
        }
        else
        {
            GameObject hud = GameObject.Find("HUD_Operativo");
            if (hud != null) hud.SetActive(true);
        }

        // Arranque en blanco / Standby real
        gestorTMS?.ReiniciarEstadoMuelles();
        FindFirstObjectByType<GeneradorTablaManifiesto>()?.LimpiarTablaStandby();
        DashboardUI.Instance?.ConfigurarReservaAMR02(false);
        DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);

        DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
            "<color=#38BDF8>TURNO INICIADO EN STANDBY</color>",
            "Use [+] Cargar JSON ERP para inyectar órdenes a la cola WMS.",
            4.5f
        );

        DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Turno iniciado en STANDBY. Esperando ingesta de lotes JSON ERP.");
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[WMS ENERGÍA]:</color> Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada.");
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        plantillaPedidosFisicos.Clear();
        directorioBodega.Clear();

        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            Manifiesto baseJson = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            if (baseJson != null && baseJson.pedidos != null)
            {
                foreach (var pedido in baseJson.pedidos)
                {
                    if (pedido == null) continue;
                    plantillaPedidosFisicos.Add(pedido);
                    if (!string.IsNullOrEmpty(pedido.coordenada_bodega) && !directorioBodega.ContainsKey(pedido.coordenada_bodega))
                    {
                        GameObject destino = GameObject.Find(pedido.coordenada_bodega);
                        if (destino != null) directorioBodega.Add(pedido.coordenada_bodega, destino.transform);
                    }
                }
            }

            Debug.Log($"<color=green>EVE:</color> Directorio físico cacheado ({plantillaPedidosFisicos.Count} puntos). Sistema listo en Standby.");
        }
        else
        {
            Debug.LogError("EVE Error crítico: No se encontró el Manifiesto base en la ruta: " + ruta);
        }

        // Arranque sin lote activo cargado
        manifiestoActual = new Manifiesto { pedidos = new List<Pedido>() };
        palletsEnColaWMS = 0;
        flotaEscaladaDual = false;
    }

    public void RegistrarEntregaLIFO(Pedido pedidoEntregado, GameObject palletFisico)
    {
        if (pedidoEntregado == null) return;

        matrizLIFOCamion.Push(pedidoEntregado);
        pesoAcumuladoCamion += pedidoEntregado.peso_kg;

        Transform zonaCamionTransform = null;
        GameObject zonaObj = GameObject.Find("Zona_Camion");
        if (zonaObj != null) zonaCamionTransform = zonaObj.transform;

        if (palletFisico != null)
        {
            palletsEntregados.Add(palletFisico);

            if (zonaCamionTransform != null)
            {
                // Desemparentar del AMR preservando escala de mundo
                palletFisico.transform.SetParent(null, true);

                // Cuadrícula LIFO: acomodar a lo largo del eje Z del camión (de fondo a frente)
                int indiceLIFO = matrizLIFOCamion.Count; // 1, 2, 3...
                float zOffset = (2 - indiceLIFO) * 1.6f;
                float ySuperficie = zonaCamionTransform.position.y + 1.55f;

                Vector3 posicionLIFO = new Vector3(
                    zonaCamionTransform.position.x,
                    ySuperficie,
                    zonaCamionTransform.position.z + zOffset
                );

                palletFisico.transform.position = posicionLIFO;
                palletFisico.transform.rotation = zonaCamionTransform.rotation;
            }
        }

        DashboardUI.Instance?.RegistrarLog($"<color=cyan>[MATRIZ LIFO]:</color> Estiba {pedidoEntregado.id_pallet} apilada en Posición #{matrizLIFOCamion.Count} (Peso acumulado: {pesoAcumuladoCamion:F0} kg / {CapacidadMaximaCamionKg:F0} kg)");
        Debug.Log($"<color=cyan>[MATRIZ LIFO]:</color> Estiba {pedidoEntregado.id_pallet} apilada en Posición #{matrizLIFOCamion.Count}. Peso acumulado: {pesoAcumuladoCamion:F0} kg.");
    }

    public void ConfirmarEntregaExitosa(RobotAMR robotEntregador = null)
    {
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        palletsCompletadosTurno++;

        FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(palletsEnColaWMS);

        if (palletsEnColaWMS > 0)
        {
            if (!enSaltoTemporal)
            {
                DespacharColaDisponible(robotEntregador);
            }
        }
        else if (ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
            Debug.Log("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
        }
    }

    public void NotificarMisionAbortada()
    {
        DashboardUI.Instance?.RegistrarLog("<color=orange>[WMS]:</color> Misión abortada. Avanzando cola WMS.");
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(palletsEnColaWMS);

        if (palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
        else if (ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada (Con Reprogramaciones). Unidades en espera de nuevas órdenes ERP.");
        }
    }

    private void FinalizarTurnoYRetornarACarga(string mensajeCierre)
    {
        turnoIniciado = false;
        Debug.Log(mensajeCierre);
        DashboardUI.Instance?.RegistrarLog(mensajeCierre);

        if (robotEjecutor != null && robotEjecutor.estadoActual != RobotAMR.EstadoRobot.Recargando)
        {
            bool esUnidad02 = (robotEjecutor == robotAMR_02) ||
                              (!string.IsNullOrEmpty(robotEjecutor.idRobot) && robotEjecutor.idRobot.EndsWith("02"));
            string nombrePad = esUnidad02 ? "Pad_Inductivo_02" : "Pad_Inductivo_01";
            Vector3 posPadFallback = esUnidad02 ? new Vector3(38.13f, 0.25886f, -54.66f) : new Vector3(28.86f, 0.25886f, -54.91f);

            GameObject goPad = GameObject.Find(nombrePad);
            Vector3 destinoPad = goPad != null ? goPad.transform.position : posPadFallback;
            robotEjecutor.EnviarACargaInductiva(destinoPad);
        }

        DirectorCinematico.Instance?.ActivarVistaGeneralFinDeTurno();
    }

    private int ContarMisionesEnVuelo()
    {
        int enVuelo = 0;
        if (EstaEnMisionActiva(robotAMR_01)) enVuelo++;
        if (EstaEnMisionActiva(robotAMR_02)) enVuelo++;
        if (EstaEnMisionActiva(robotAMR_03)) enVuelo++;
        return enVuelo;
    }

    private static bool EstaEnMisionActiva(RobotAMR robot)
    {
        if (robot == null) return false;
        return robot.estadoActual == RobotAMR.EstadoRobot.En_Transito ||
               robot.estadoActual == RobotAMR.EstadoRobot.Extrayendo ||
               robot.estadoActual == RobotAMR.EstadoRobot.Transportando ||
               robot.estadoActual == RobotAMR.EstadoRobot.Entregando ||
               robot.estadoActual == RobotAMR.EstadoRobot.Interrumpido;
    }

    private int ObtenerCuposMaximosPiso()
    {
        return flotaEscaladaDual ? 2 : 1;
    }

    private void DespacharColaDisponible(RobotAMR robotPreferido = null)
    {
        if (plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) return;
        if (palletsEnColaWMS <= 0) return;

        int iteracionesSeguridad = 4;
        int cuposMaximos = ObtenerCuposMaximosPiso();
        while (palletsEnColaWMS > ContarMisionesEnVuelo() && ContarMisionesEnVuelo() < cuposMaximos && iteracionesSeguridad-- > 0)
        {
            RobotAMR objetivo = SeleccionarSiguienteRobotLibre(robotPreferido);
            robotPreferido = null;

            if (objetivo == null) break;

            bool enSalaCarga = objetivo.transform.position.z < RobotAMR.ZUmbralPortico;
            if (enSalaCarga && Time.time < tiempoProximoDespachoPermitido)
            {
                if (rutinaHeadwayDespacho == null)
                {
                    rutinaHeadwayDespacho = StartCoroutine(EsperarHeadwayYDespachar());
                }
                break;
            }

            if (!DespacharPedidoARobot(objetivo))
            {
                break;
            }
        }
    }

    private IEnumerator EsperarHeadwayYDespachar()
    {
        while (Time.time < tiempoProximoDespachoPermitido)
        {
            yield return null;
        }
        rutinaHeadwayDespacho = null;
        if (!enSaltoTemporal && palletsEnColaWMS > 0)
        {
            DespacharColaDisponible();
        }
    }

    private RobotAMR SeleccionarSiguienteRobotLibre(RobotAMR preferido)
    {
        if (preferido != null && preferido.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            if (flotaEscaladaDual) return preferido;
            if (relevo24_7Activo && preferido == robotAMR_02) return preferido;
            if (!relevo24_7Activo && preferido == robotAMR_01) return preferido;
        }

        if (flotaEscaladaDual)
        {
            if (robotAMR_01 != null && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo) return robotAMR_01;
            if (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo) return robotAMR_02;
            if (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
                return robotAMR_03;
            }
            return null;
        }

        RobotAMR ejecutorActual = (relevo24_7Activo && robotAMR_02 != null)
            ? robotAMR_02
            : (robotAMR_01 != null ? robotAMR_01 : robotEjecutor);

        if (ejecutorActual != null && ejecutorActual.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            return ejecutorActual;
        }

        if (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo &&
            (robotAMR_01 == null || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Recargando || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Apagado))
        {
            return robotAMR_02;
        }

        if (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo &&
            (robotAMR_01 == null || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Recargando || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Apagado || robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido))
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
            return robotAMR_03;
        }

        return null;
    }

    private bool DespacharPedidoARobot(RobotAMR robot)
    {
        if (robot == null || plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) return false;
        if (robot.estadoActual != RobotAMR.EstadoRobot.Inactivo) return false;

        bool desdeColaReasignacion = colaPrioritariaReasignacion.Count > 0;
        Pedido pedido = desdeColaReasignacion
            ? colaPrioritariaReasignacion.Dequeue()
            : plantillaPedidosFisicos[Mathf.Abs(indicePedidoActual) % plantillaPedidosFisicos.Count];

        bool ok = DespacharPedidoEspecificoARobot(robot, pedido);
        if (ok && !desdeColaReasignacion)
        {
            indicePedidoActual++;
        }
        return ok;
    }

    private bool DespacharPedidoEspecificoARobot(RobotAMR robot, Pedido pedido)
    {
        if (robot == null || pedido == null) return false;
        if (robot.estadoActual != RobotAMR.EstadoRobot.Inactivo) return false;

        if (robot.transform.position.z < RobotAMR.ZUmbralPortico)
        {
            tiempoProximoDespachoPermitido = Time.time + HeadwaySalidaSalaCargaSegundos;
        }

        if (robot == robotAMR_03)
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
        }

        directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);
        if (destino == null && !string.IsNullOrEmpty(pedido.coordenada_bodega))
        {
            GameObject go = GameObject.Find(pedido.coordenada_bodega);
            if (go != null)
            {
                destino = go.transform;
                directorioBodega[pedido.coordenada_bodega] = destino;
            }
        }

        int numCamion = 1;
        int slot = 0;
        if (gestorTMS != null)
        {
            Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out numCamion, out slot);
            robot.zonaTransferencia = muelleDestino;
            robot.numCamionAsignado = numCamion;
            robot.slotLIFOAsignado = slot;
        }

        if (destino == null)
        {
            robot.slotLIFOAsignado = 0;
            if (gestorTMS != null && slot > 0) gestorTMS.CancelarReservaSlot(numCamion, slot);
            robot.AsignarMision(pedido, null, -1);
            return false;
        }

        int filaSecuencial = indiceFilaManifiestoGlobal++;
        var generadorTabla = FindFirstObjectByType<GeneradorTablaManifiesto>();
        if (generadorTabla != null)
        {
            filaSecuencial = generadorTabla.AsegurarFilaOperativaParaMision(filaSecuencial, pedido, palletsEnColaWMS);
        }

        robotEjecutor = robot;
        OnMisionEvaluada?.Invoke();
        robot.AsignarMision(pedido, destino, filaSecuencial);
        return true;
    }

    public Vector3 ObtenerPosicionPadPorUnidad(RobotAMR amr)
    {
        string nombrePad = "Pad_Inductivo_01";
        Vector3 fallback = new Vector3(28.00f, 0.25886f, -54.80f);

        if (amr != null)
        {
            bool es03 = (amr == robotAMR_03) ||
                        (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("03")) ||
                        amr.gameObject.name.Contains("03");
            bool es02 = !es03 && ((amr == robotAMR_02) ||
                        (!string.IsNullOrEmpty(amr.idRobot) && amr.idRobot.EndsWith("02")) ||
                        amr.gameObject.name.Contains("02"));

            if (es03)
            {
                nombrePad = "Pad_Inductivo_03";
                fallback = new Vector3(40.00f, 0.25886f, -54.80f);
            }
            else if (es02)
            {
                nombrePad = "Pad_Inductivo_02";
                fallback = new Vector3(34.00f, 0.25886f, -57.40f);
            }
        }

        GameObject goPad = GameObject.Find(nombrePad);
        if (goPad != null)
        {
            Vector3 p = goPad.transform.position;
            return new Vector3(p.x, 0.25886f, p.z);
        }
        return fallback;
    }

    private IEnumerator SecuenciaSaltoTemporal()
    {
        enSaltoTemporal = true;
        OnMisionEvaluada?.Invoke();
        Debug.Log("<color=#10B981>[FLUJO]: 2. Pedido 1 Entregado. Iniciando Time-Lapse Logístico (7 horas).</color>");
        DashboardUI.Instance?.RegistrarLog("<color=#F59E0B><b>[TIME-LAPSE LOGÍSTICO]:</b> Avanzando 7 horas de turno continuo (00:30 AM -> 05:30 AM). Simulando fatiga de flota...</color>");

        float duracion = 3.5f;
        DirectorCinematico.Instance?.EjecutarBarridoTimeLapse(duracion);
        DashboardUI.Instance?.MostrarOverlayTimeLapse(true);
        DashboardUI.Instance?.ActualizarProgresoTimeLapse(0f);

        RobotAMR amr = robotAMR_01 != null ? robotAMR_01 : robotEjecutor;
        if (amr != null)
        {
            float bateriaInicial = amr.bateriaActual;
            float bateriaObjetivo = 21.93f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                horaTurnoActual = Mathf.Lerp(1.0f, 7.5f, t);
                amr.bateriaActual = Mathf.Lerp(bateriaInicial, bateriaObjetivo, t);
                DashboardUI.Instance?.ActualizarBateria(amr.idRobot, amr.bateriaActual / 100f, amr.estadoActual);
                DashboardUI.Instance?.ActualizarProgresoTimeLapse(t);
                yield return null;
            }

            amr.bateriaActual = bateriaObjetivo;
            DashboardUI.Instance?.ActualizarBateria(amr.idRobot, amr.bateriaActual / 100f, amr.estadoActual);
        }
        else
        {
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
                horaTurnoActual = Mathf.Lerp(1.0f, 7.5f, t);
                DashboardUI.Instance?.ActualizarProgresoTimeLapse(t);
                yield return null;
            }
        }

        horaTurnoActual = 7.5f;
        enSaltoTemporal = false;
        DashboardUI.Instance?.ActualizarProgresoTimeLapse(1f);
        DashboardUI.Instance?.ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 7.5/8 (Bloque Final) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        yield return new WaitForSeconds(0.5f);
        DashboardUI.Instance?.MostrarOverlayTimeLapse(false);

        Debug.Log("<color=#10B981>[FLUJO]: 3. Time-Lapse completado. Continuando despacho de Cola WMS.</color>");
        DespacharColaDisponible();
    }

    public void AutorizarOverrideBateria()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        RobotAMR objetivo = (amrEnAlertaBateria20 != null && amrEnAlertaBateria20.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
            ? amrEnAlertaBateria20
            : ((robotEjecutor != null && robotEjecutor.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
                ? robotEjecutor
                : (robotAMR_01 != null && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                    ? robotAMR_01
                    : (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                        ? robotAMR_02
                        : (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                            ? robotAMR_03
                            : amrEnAlertaBateria20))));

        if (objetivo != null)
        {
            objetivo.EjecutarOverride();
        }
        else if (robotEjecutor != null)
        {
            robotEjecutor.EjecutarOverride();
        }

        amrEnAlertaBateria20 = null;
    }

    public void EjecutarRelevoFlota24_7()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        RobotAMR robotSaliente = (amrEnAlertaBateria20 != null && amrEnAlertaBateria20.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
            ? amrEnAlertaBateria20
            : ((robotEjecutor != null && robotEjecutor.estadoActual == RobotAMR.EstadoRobot.Interrumpido)
                ? robotEjecutor
                : (robotAMR_01 != null && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                    ? robotAMR_01
                    : (robotAMR_02 != null && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                        ? robotAMR_02
                        : (robotAMR_03 != null && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Interrumpido
                            ? robotAMR_03
                            : (robotAMR_01 != null ? robotAMR_01 : robotEjecutor)))));

        RobotAMR robotRelevo = null;
        if (robotAMR_02 != null && robotAMR_02 != robotSaliente && robotAMR_02.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_02;
        }
        else if (robotAMR_03 != null && robotAMR_03 != robotSaliente && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_03;
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true);
        }
        else if (robotAMR_01 != null && robotAMR_01 != robotSaliente && robotAMR_01.estadoActual == RobotAMR.EstadoRobot.Inactivo)
        {
            robotRelevo = robotAMR_01;
        }

        bool tieneCarga = robotSaliente != null && (robotSaliente.TieneCargaFisica || robotSaliente.estadoPreInterrupcionBateria == RobotAMR.EstadoRobot.Transportando);

        if (tieneCarga)
        {
            // Caso B: El AMR YA tiene el pallet encima -> Reanudar para terminar entrega y diferir recarga
            robotSaliente.ReanudarTrasRelevoDiferido();
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[RELEVO 24/7]:</color> {robotSaliente.idRobot} transportando estiba. Completará entrega en muelle antes de acoplarse al Pad.");

            // Desplegar inmediatamente la unidad de soporte libre para el siguiente pedido de la cola WMS
            if (robotRelevo != null)
            {
                DirectorCinematico.Instance?.RelevarRobotActivo(robotRelevo);
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotRelevo);
                relevo24_7Activo = (robotRelevo == robotAMR_02);
                robotEjecutor = robotRelevo;
                DespacharColaDisponible(robotRelevo);
            }
            else
            {
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotSaliente);
            }
        }
        else
        {
            // Caso A: El AMR aún NO tiene pallet físico encima -> Desviar inmediatamente a recargar
            Pedido pedidoInterrumpido = robotSaliente != null ? robotSaliente.PedidoActual : null;

            if (robotSaliente != null && robotSaliente != robotRelevo)
            {
                robotSaliente.enPausaPreventivaBateria20 = false;
                Vector3 destinoPad = ObtenerPosicionPadPorUnidad(robotSaliente);
                robotSaliente.EnviarACargaInductiva(destinoPad);
            }

            if (robotRelevo != null)
            {
                DirectorCinematico.Instance?.RelevarRobotActivo(robotRelevo);
                DirectorCinematico.Instance?.LiberarFocoIncidente(robotRelevo);
                relevo24_7Activo = (robotRelevo == robotAMR_02);
                robotEjecutor = robotRelevo;

                Pedido pedidoPendiente = pedidoInterrumpido;
                if (pedidoPendiente == null && plantillaPedidosFisicos != null && plantillaPedidosFisicos.Count > 0)
                {
                    pedidoPendiente = plantillaPedidosFisicos[Mathf.Abs(indicePedidoActual) % plantillaPedidosFisicos.Count];
                    indicePedidoActual++;
                }

                if (pedidoPendiente != null)
                {
                    DespacharPedidoEspecificoARobot(robotRelevo, pedidoPendiente);
                }
            }
            else
            {
                DirectorCinematico.Instance?.LiberarFocoIncidente(null);
            }
        }

        amrEnAlertaBateria20 = null;

        string idSaliente = robotSaliente != null ? robotSaliente.idRobot : "AMR";
        string idRelevo = robotRelevo != null ? robotRelevo.idRobot : "SOPORTE";
        string mensajeRelevo = $"[RELEVO 24/7]: Gestión completada para {idSaliente}. {idRelevo} activo en cola WMS.";
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8><b>{mensajeRelevo}</b></color>");
        Debug.Log($"<color=cyan>{mensajeRelevo}</color>");
    }

    public void SimularAlertaBateria20()
    {
        RobotAMR objetivo = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: false);
        if (objetivo == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA <20%]:</color> No hay AMRs disponibles en escena.");
            return;
        }

        amrEnAlertaBateria20 = objetivo;
        objetivo.InducirInterrupcionPreventivaBateria(20.8f);
        robotEjecutor = objetivo;
        DirectorCinematico.Instance?.EnfocarIncidenteCritico(objetivo.transform);
        DashboardUI.Instance?.RegistrarLog($"<color=red>[SIMULACIÓN SCADA <20%]:</color> Descarga preventiva inducida en {objetivo.idRobot} (20.8% SoC). Evaluando Override vs. Relevo 24/7.");
        DashboardUI.Instance?.MostrarEmergencia(true, 18.9f, 3.2f);
    }

    public void SimularColapsoBateria3()
    {
        RobotAMR[] candidatos = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        RobotAMR objetivoSinCarga = null;
        RobotAMR objetivoCargado = null;

        // Caso A (Inmediato): AMR activo en tránsito sin estiba a bordo
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null) continue;
            if (EstaEnMisionActiva(r) && !r.TieneCargaFisica && !r.colapsoBMSArmadoPostDescarga)
            {
                objetivoSinCarga = r;
                break;
            }
        }

        if (objetivoSinCarga != null)
        {
            EjecutarColapsoYRescateInmediato(objetivoSinCarga);
            return;
        }

        // Caso B (Diferido seguro): AMR con estiba física completa descarga antes del corte <3%
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null) continue;
            if (EstaEnMisionActiva(r) && r.TieneCargaFisica && !r.colapsoBMSArmadoPostDescarga)
            {
                objetivoCargado = r;
                break;
            }
        }

        if (objetivoCargado != null)
        {
            objetivoCargado.colapsoBMSArmadoPostDescarga = true;
            objetivoCargado.bateriaActual = 4.2f;
            DashboardUI.Instance?.ActualizarBateria(objetivoCargado.idRobot, objetivoCargado.bateriaActual / 100f, objetivoCargado.estadoActual);

            string msgDiferido = $"[BMS CRÍTICO]: Caída de tensión en {objetivoCargado.idRobot} (4.2%). Completando descarga de estiba en muelle antes de corte total (<3%) y extracción aérea.";
            DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>{msgDiferido}</b></color>");
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA("<color=#EF4444>BMS CRÍTICO (4.2%)</color>", msgDiferido, 4.5f);
            Debug.Log($"<color=orange>{msgDiferido}</color>");
            return;
        }

        RobotAMR fallback = BuscarAMRActivoOPisoParaSimulacion(soloEnMovimiento: true);
        if (fallback == null)
        {
            DashboardUI.Instance?.RegistrarLog("<color=orange>[SIMULACIÓN SCADA <3%]:</color> No hay AMRs disponibles en escena.");
            return;
        }

        if (fallback.colapsoBMSArmadoPostDescarga)
        {
            DashboardUI.Instance?.RegistrarLog($"<color=orange>[BMS CRÍTICO]:</color> {fallback.idRobot} ya tiene corte diferido armado tras descarga.");
            return;
        }

        EjecutarColapsoYRescateInmediato(fallback);
    }

    public void FinalizarEntregaYEjecutarColapsoBMS(RobotAMR objetivo)
    {
        palletsEnColaWMS = Mathf.Max(0, palletsEnColaWMS - 1);
        palletsCompletadosTurno++;
        FindFirstObjectByType<GeneradorTablaManifiesto>()?.ActualizarResumenColaEnVivo(palletsEnColaWMS);
        EjecutarColapsoYRescateInmediato(objetivo);
    }

    public void EjecutarColapsoYRescateInmediato(RobotAMR objetivo)
    {
        if (objetivo == null) return;

        Pedido pedidoHuerfano = objetivo.ForzarApagadoCriticoBMS();
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[COLAPSO CRÍTICO <3% BMS]:</b></color> {objetivo.idRobot} apagado en pasillo (2.4% SoC). Desplegando Dron de Mantenimiento y relevo físico AMR-03.");

        // Bloquea VCam4_Emergencia sobre el AMR colapsado desde el segundo 0 hasta completar izaje vertical (Etapa 2)
        DirectorCinematico.Instance?.EnfocarIncidenteCritico(objetivo.transform);

        RobotAMR relevo = (robotAMR_03 != null && robotAMR_03 != objetivo && robotAMR_03.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            ? robotAMR_03
            : SeleccionarSiguienteRobotLibre(null);

        if (relevo == robotAMR_03)
        {
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#FACC15>EN RELEVO ACTIVO</color>");
        }

        if (pedidoHuerfano != null)
        {
            palletsEnColaWMS = Mathf.Max(palletsEnColaWMS, 1);
            if (relevo != null && relevo.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                StartCoroutine(DespacharRelevoConHeadway(relevo, pedidoHuerfano));
            }
            else
            {
                colaPrioritariaReasignacion.Enqueue(pedidoHuerfano);
            }
        }
        else if (palletsEnColaWMS > 0 && relevo != null)
        {
            DespacharColaDisponible(relevo);
        }
        else if (palletsEnColaWMS == 0 && ContarMisionesEnVuelo() == 0)
        {
            flotaEscaladaDual = false;
            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Cola WMS completada. Unidades en espera de nuevas órdenes ERP.");
        }

        if (dronMantenimiento != null)
        {
            Vector3 posPadDestino = ObtenerPosicionPadPorUnidad(objetivo);
            StartCoroutine(dronMantenimiento.EjecutarRescateAMRAbortado(
                objetivo,
                posPadDestino,
                onRescateCompletado: () =>
                {
                    DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[RESCATE AÉREO COMPLETADO]:</color> {objetivo.idRobot} acoplado en Pad Inductivo (Recarga 1C iniciada).");
                },
                onElevacionCompletada: () =>
                {
                    RobotAMR seguimientoRelevo = (relevo != null && relevo.estadoActual != RobotAMR.EstadoRobot.Apagado)
                        ? relevo
                        : (EstaEnMisionActiva(robotAMR_03) ? robotAMR_03 : null);
                    DirectorCinematico.Instance?.LiberarFocoIncidente(seguimientoRelevo);
                }
            ));
        }
        else
        {
            DirectorCinematico.Instance?.LiberarFocoIncidente(relevo);
        }
    }

    private IEnumerator DespacharRelevoConHeadway(RobotAMR relevo, Pedido pedido)
    {
        if (relevo != null && relevo.transform.position.z < RobotAMR.ZUmbralPortico)
        {
            while (Time.time < tiempoProximoDespachoPermitido)
            {
                yield return null;
            }
        }
        DespacharPedidoEspecificoARobot(relevo, pedido);
    }

    private RobotAMR BuscarAMRActivoOPisoParaSimulacion(bool soloEnMovimiento)
    {
        RobotAMR[] candidatos = new RobotAMR[] { robotAMR_01, robotAMR_02, robotAMR_03 };
        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r == null) continue;
            if (r.estadoActual == RobotAMR.EstadoRobot.En_Transito || r.estadoActual == RobotAMR.EstadoRobot.Transportando)
            {
                return r;
            }
        }

        if (soloEnMovimiento)
        {
            for (int i = 0; i < candidatos.Length; i++)
            {
                RobotAMR r = candidatos[i];
                if (r != null && r.estadoActual != RobotAMR.EstadoRobot.Apagado && r.estadoActual != RobotAMR.EstadoRobot.Recargando)
                {
                    return r;
                }
            }
        }

        for (int i = 0; i < candidatos.Length; i++)
        {
            RobotAMR r = candidatos[i];
            if (r != null && r.estadoActual == RobotAMR.EstadoRobot.Inactivo)
            {
                return r;
            }
        }

        return robotAMR_01 != null ? robotAMR_01 : robotAMR_02;
    }

    void MoverPorWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform destino = waypoints[indiceWaypointActual];
        if (destino == null) return;

        // Vuelo matemático determinista acotado por AABB industrial (Zero-Physics)
        Vector3 siguientePos = Vector3.MoveTowards(transform.position, destino.position, velocidadVuelo * Time.deltaTime);
        siguientePos.x = Mathf.Clamp(siguientePos.x, LimiteXMin, LimiteXMax);
        siguientePos.y = Mathf.Clamp(siguientePos.y, AltitudMin, AltitudMax);
        siguientePos.z = Mathf.Clamp(siguientePos.z, LimiteZMin, LimiteZMax);
        transform.position = siguientePos;
        
        Vector3 direccion = (destino.position - transform.position).normalized;
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 3f);
        }

        if (Vector3.Distance(transform.position, destino.position) < 0.5f)
        {
            indiceWaypointActual = (indiceWaypointActual + 1) % waypoints.Length;
        }
    }

    public void InyectarObstaculoPasilloISO()
    {
        if (obstaculoPasilloISO != null)
        {
            obstaculoPasilloISO.transform.SetParent(null, true);
            Collider col = obstaculoPasilloISO.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            RobotAMR robot = robotEjecutor != null ? robotEjecutor : robotAMR_01;
            if (robot != null && (robot.estadoActual == RobotAMR.EstadoRobot.En_Transito || robot.estadoActual == RobotAMR.EstadoRobot.Transportando))
            {
                Vector3 dirFrente = robot.transform.forward;
                Vector3 posObstaculo = robot.transform.position + dirFrente * 3.10f;
                posObstaculo.y = 0.202f;

                obstaculoPasilloISO.transform.position = posObstaculo;
                obstaculoPasilloISO.transform.rotation = Quaternion.identity;
            }

            obstaculoPasilloISO.SetActive(true);
            DashboardUI.Instance?.RegistrarLog("<color=#F59E0B>[STRESS TEST]:</color> Obstaculo imprevisto detectado en corredor principal.");
        }
        else
        {
            Debug.LogWarning("[EVE]: Referencia obstaculoPasilloISO no asignada para el Stress Test.");
        }
    }

    public void GestionarObstruccionAutonomaISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        if (despachandoDespejeISO) return;
        StartCoroutine(ProtocoloDespejeAutonomoISO(robotAfectado, obstaculoDetectado));
    }

    public void ProyectarEstresDeFlota(int nuevosPedidos = 50)
    {
        if (alertaDemandaEnCurso) return;
        alertaDemandaEnCurso = true;

        const float deltaSoCPedido = 3.85f;
        const float pisoSeguridadLiFePO4 = RobotAMR.UmbralSeguridadLiFePO4; // 20.0f
        const float ventanaNominalUnidad = 60.0f; // 80.0% -> 20.0%
        const float pesoEstibaEstandarKg = 850.0f;

        float energiaRequerida = nuevosPedidos * deltaSoCPedido; // 192.5% para 50 pedidos

        float bateriaAMR01 = robotAMR_01 != null ? robotAMR_01.bateriaActual : 80.0f;
        float bateriaAMR02 = robotAMR_02 != null ? robotAMR_02.bateriaActual : 80.0f;
        float energiaDisponible = Mathf.Max(0f, bateriaAMR01 - pisoSeguridadLiFePO4) +
                                  Mathf.Max(0f, bateriaAMR02 - pisoSeguridadLiFePO4);

        float deficitSoC = Mathf.Max(0f, energiaRequerida - energiaDisponible);
        float pesoLoteKg = nuevosPedidos * pesoEstibaEstandarKg; // 42,500 kg

        const float duracionTurnoHoras = 8.0f;
        const float tiempoCicloSimSegundos = 4.50f;
        float tiempoCicloHorasReales = (tiempoCicloSimSegundos * RobotAMR.FactorCompresionTurno) / 3600f; // 0.20 h/pedido
        float tasaAbsorcionPorAMR = 1f / tiempoCicloHorasReales; // 5.0 ped/h
        float tasaAbsorcionFlotaActual = 2f * tasaAbsorcionPorAMR; // 10.0 ped/h

        float horasRestantes = Mathf.Max(0.5f, duracionTurnoHoras - horaTurnoActual);
        int pedidosAbsorbibles = Mathf.Clamp(Mathf.FloorToInt(horasRestantes * tasaAbsorcionFlotaActual), 0, nuevosPedidos);
        int pedidosRezagados = Mathf.Max(0, nuevosPedidos - pedidosAbsorbibles);
        float porcentajeCarryOver = (pedidosRezagados / (float)nuevosPedidos) * 100f;

        EscenarioHorizonte escenario;
        int amrRequeridos;
        string etiquetaEscenario;

        if (horaTurnoActual <= 3.0f)
        {
            escenario = EscenarioHorizonte.EscenarioA_InicioTurno;
            amrRequeridos = 3; // 2 activos + 1 relevo en carga inductiva 1C
            porcentajeCarryOver = 0f;
            etiquetaEscenario = "ESCENARIO A (INICIO TURNO)";
        }
        else if (horaTurnoActual <= 6.0f)
        {
            escenario = EscenarioHorizonte.EscenarioB_Madrugada;
            amrRequeridos = Mathf.Max(4, Mathf.CeilToInt(energiaRequerida / ventanaNominalUnidad));
            etiquetaEscenario = "ESCENARIO B (MADRUGADA)";
        }
        else
        {
            escenario = EscenarioHorizonte.EscenarioC_FinalTurno;
            amrRequeridos = Mathf.CeilToInt(energiaRequerida / ventanaNominalUnidad); // 4 unidades
            etiquetaEscenario = "ESCENARIO C (FINAL TURNO)";
        }

        Debug.Log($"<color=#F59E0B>[ANALIZADOR HORIZONTE O(1) - {etiquetaEscenario}]:</color> Hora {horaTurnoActual:F1}/8 (Restan {horasRestantes:F1}h). Lote: {nuevosPedidos} ped ({pesoLoteKg:F0} kg). Déficit LiFePO4: -{deficitSoC:F1}% SoC | Carry-over SLA: {porcentajeCarryOver:F0}% | Recomendación: {amrRequeridos} AMRs.");
        DashboardUI.Instance?.RegistrarLog($"<color=#F59E0B><b>[WMS HORIZONTE - {etiquetaEscenario}]:</b></color> Ráfaga de {nuevosPedidos} pedidos ({pesoLoteKg:F0} kg) en Hora {horaTurnoActual:F1}/8 (Restan {horasRestantes:F1}h).");
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444><b>[DICTAMEN O(1)]:</b></color> Déficit LiFePO4: -{deficitSoC:F1}% SoC | Rezago SLA (Carry-over): {porcentajeCarryOver:F0}%. Flota recomendada: {amrRequeridos} AMRs.");

        DashboardUI.Instance?.MostrarAlertaIncertidumbreDemanda(
            escenario, horaTurnoActual, horasRestantes, nuevosPedidos,
            deficitSoC, amrRequeridos, porcentajeCarryOver, pesoLoteKg
        );
    }

    public void NotificarResolucionAlertaDemanda()
    {
        alertaDemandaEnCurso = false;
    }

    public void IngestarManifiestoDesdeSO()
    {
        string dirInicial = Directory.Exists(Application.streamingAssetsPath)
            ? Application.streamingAssetsPath
            : Environment.GetFolderPath(Environment.SpecialFolder.Personal);

#if UNITY_EDITOR
        string rutaSeleccionada = UnityEditor.EditorUtility.OpenFilePanel("Cargar Lote ERP", dirInicial, "json");
        if (!string.IsNullOrEmpty(rutaSeleccionada))
        {
            ProcesarArchivoManifiesto(rutaSeleccionada);
        }
#elif UNITY_STANDALONE_LINUX
        StartCoroutine(EjecutarSelectorArchivoSOAsync(() =>
        {
            string archivoInicial = dirInicial.EndsWith("/") ? dirInicial : dirInicial + "/";
            var psi = new ProcessStartInfo
            {
                FileName = "zenity",
                Arguments = $"--file-selection --title=\"Cargar Lote ERP\" --filename=\"{archivoInicial}\" --file-filter=\"Archivos JSON (*.json) | *.json\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process proc = Process.Start(psi))
            {
                if (proc == null) return string.Empty;
                string salida = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return proc.ExitCode == 0 ? salida.Trim() : string.Empty;
            }
        }));
#elif UNITY_STANDALONE_WIN
        StartCoroutine(EjecutarSelectorArchivoSOAsync(() =>
        {
            string dirEscapado = dirInicial.Replace("'", "''");
            string psScript = $"Add-Type -AssemblyName System.Windows.Forms; $d = New-Object System.Windows.Forms.OpenFileDialog; $d.InitialDirectory = '{dirEscapado}'; $d.Filter = 'JSON (*.json)|*.json'; if ($d.ShowDialog() -eq 'OK') {{ Write-Output $d.FileName }}";
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -STA -Command \"{psScript}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using (Process proc = Process.Start(psi))
            {
                if (proc == null) return string.Empty;
                string salida = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                return proc.ExitCode == 0 ? salida.Trim() : string.Empty;
            }
        }));
#else
        DashboardUI.Instance?.MostrarPopUpRechazo("Selector de archivos no soportado en esta plataforma.");
#endif
    }

#if !UNITY_EDITOR && (UNITY_STANDALONE_LINUX || UNITY_STANDALONE_WIN)
    private IEnumerator EjecutarSelectorArchivoSOAsync(Func<string> accionSelector)
    {
        Task<string> tarea = Task.Run(() =>
        {
            try
            {
                return accionSelector();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EVE Selector SO]: {ex.Message}");
                return string.Empty;
            }
        });

        while (!tarea.IsCompleted)
        {
            yield return null;
        }

        string rutaSeleccionada = tarea.Result;
        if (!string.IsNullOrEmpty(rutaSeleccionada))
        {
            ProcesarArchivoManifiesto(rutaSeleccionada);
        }
    }
#endif

    public void ProcesarArchivoManifiesto(string rutaArchivo)
    {
        try
        {
            if (string.IsNullOrEmpty(rutaArchivo) || !File.Exists(rutaArchivo))
            {
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            string json = File.ReadAllText(rutaArchivo);
            if (string.IsNullOrWhiteSpace(json))
            {
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            Manifiesto lote = JsonUtility.FromJson<Manifiesto>(json);
            bool sinPedidos = lote == null || lote.pedidos == null || lote.pedidos.Count == 0;
            if (lote == null || (sinPedidos && lote.total_pallets <= 0))
            {
                DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                return;
            }

            if (sinPedidos)
            {
                if (manifiestoActual != null && manifiestoActual.pedidos != null && manifiestoActual.pedidos.Count > 0)
                {
                    lote.pedidos = new List<Pedido>(manifiestoActual.pedidos);
                }
                else
                {
                    DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                    return;
                }
            }
            else
            {
                bool tieneCampoValido = false;
                for (int i = 0; i < lote.pedidos.Count; i++)
                {
                    if (lote.pedidos[i] != null && !string.IsNullOrEmpty(lote.pedidos[i].id_pallet))
                    {
                        tieneCampoValido = true;
                        break;
                    }
                }
                if (!tieneCampoValido && lote.total_pallets <= 0)
                {
                    DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
                    return;
                }
            }

            EvaluarIngestaAutonoma(lote);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[EVE Ingesta JSON]: Error procesando '{rutaArchivo}': {ex.Message}");
            DashboardUI.Instance?.MostrarPopUpRechazo("Esquema inválido");
        }
    }

    public void EvaluarIngestaAutonoma(Manifiesto nuevoLote)
    {
        if (nuevoLote == null) return;

        if ((plantillaPedidosFisicos == null || plantillaPedidosFisicos.Count == 0) &&
            nuevoLote.pedidos != null && nuevoLote.pedidos.Count > 0)
        {
            plantillaPedidosFisicos = new List<Pedido>(nuevoLote.pedidos);
        }

        int nuevosPallets = (nuevoLote.total_pallets > 0)
            ? nuevoLote.total_pallets
            : (nuevoLote.pedidos != null ? nuevoLote.pedidos.Count : 0);

        int proyeccionCola = palletsEnColaWMS + nuevosPallets;

        float horasRestantesRaw = Mathf.Max(0.5f, 8.0f - horaTurnoActual);
        float horasRestantes = Mathf.Clamp(horasRestantesRaw, 4.5f, 5.0f);

        float capacidad1AMR = 40f * horasRestantes;
        float capacidad2AMRs = 80f * horasRestantes;

        if (proyeccionCola <= capacidad1AMR)
        {
            palletsEnColaWMS = proyeccionCola;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * 850f,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>()?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

            if (!flotaEscaladaDual)
            {
                DashboardUI.Instance?.ConfigurarReservaAMR02(true);
                DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(false);
            }

            if (sesionOperativaIniciada || turnoIniciado)
            {
                turnoIniciado = true;
                if (!enSaltoTemporal)
                {
                    DespacharColaDisponible();
                }
            }

            float tasaActual = flotaEscaladaDual ? 80f : 40f;
            float eta = proyeccionCola / tasaActual;
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#10B981>APROBADO (1 AMR)</color>",
                $"Lote: +{nuevosPallets} | Cola Total: {proyeccionCola} pallets (SLA: 40 pal/h)\nPiso: AMR-01 | Soporte: AMR-02 (Reserva 80%) | ETA: {eta:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[SLA ADMISIÓN]:</color> Aprobado +{nuevosPallets} pallets (Cola WMS: {proyeccionCola}). Operación 1 AMR (ETA: {eta:F2}h).");
        }
        else if (proyeccionCola <= capacidad2AMRs)
        {
            palletsEnColaWMS = proyeccionCola;
            flotaEscaladaDual = true;
            manifiestoActual = new Manifiesto
            {
                id_lote = nuevoLote.id_lote,
                total_pallets = palletsEnColaWMS,
                peso_total_kg = palletsEnColaWMS * 850f,
                pedidos = new List<Pedido>(plantillaPedidosFisicos)
            };

            RegistrarCoordenadasBodega(plantillaPedidosFisicos);
            FindFirstObjectByType<GeneradorTablaManifiesto>()?.AgregarLoteAManifiesto(plantillaPedidosFisicos, nuevosPallets, palletsEnColaWMS);

            DashboardUI.Instance?.ConfigurarReservaAMR02(false);
            DashboardUI.Instance?.ConfigurarEstadoSoporteAMR03(true, "<color=#38BDF8>SOPORTE / RELEVO 24/7 (80% SoC)</color>");

            if (sesionOperativaIniciada || turnoIniciado)
            {
                turnoIniciado = true;
                if (!enSaltoTemporal)
                {
                    DespacharColaDisponible();
                }
            }

            float eta = proyeccionCola / 80f;
            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#38BDF8>ESCALADO DUAL (2 AMRs + AMR-03)</color>",
                $"Cola Total: {proyeccionCola} pallets | SLA Dual: 80 pal/h\nPiso: AMR-01 + AMR-02 activos | Soporte: AMR-03 | ETA: {eta:F2} h",
                4.5f
            );
            DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8>[SLA ESCALAMIENTO]:</color> Despacho dual activado (AMR-01 + AMR-02 en piso, AMR-03 soporte). Cola WMS: {proyeccionCola} pallets (ETA: {eta:F2}h).");
        }
        else
        {
            int amrRequeridos = Mathf.CeilToInt(proyeccionCola / capacidad1AMR);

            DashboardUI.Instance?.RegistrarLog("Pedido rechazado, feedback adjuntado.");

            string feedback = $"[REPORTE SLA] Lote rechazado (+{nuevosPallets} pal | Proyección: {proyeccionCola} pal).\n" +
                              $"Techo actual (2 AMRs @ 80 pal/h en {horasRestantes:F1}h): {capacidad2AMRs:F0} pallets.\n" +
                              $"Flota requerida: {amrRequeridos} AMRs en piso + 1 AMR soporte ({amrRequeridos + 1} totales).";
            DashboardUI.Instance?.RegistrarFeedbackSupervisor(feedback);

            DashboardUI.Instance?.MostrarPopUpEjecutivoSLA(
                "<color=#EF4444>RECHAZADO</color>",
                $"Proyección ({proyeccionCola} pallets) excede techo SLA (2 AMRs).\nFlota requerida: {amrRequeridos} AMRs en piso. Feedback en Buzón.",
                4.5f
            );
        }
    }

    private void RegistrarCoordenadasBodega(List<Pedido> pedidos)
    {
        if (pedidos == null) return;
        foreach (var pedido in pedidos)
        {
            if (pedido != null && !string.IsNullOrEmpty(pedido.coordenada_bodega) && !directorioBodega.ContainsKey(pedido.coordenada_bodega))
            {
                GameObject destino = GameObject.Find(pedido.coordenada_bodega);
                if (destino != null) directorioBodega.Add(pedido.coordenada_bodega, destino.transform);
            }
        }
    }

    private IEnumerator ProtocoloDespejeAutonomoISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        despachandoDespejeISO = true;

        string idRobot = robotAfectado != null ? robotAfectado.idRobot : "AMR";
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[ISO 3691-4 CAT. 3]:</color> Bloqueo frontal LiDAR. {idRobot} en parada de proteccion. EVE gestionando despeje...");

        if (dronMantenimiento != null && obstaculoPasilloISO != null)
        {
            Action reanudar = () =>
            {
                Debug.Log("<color=#10B981>[FLUJO]: 5. Obstáculo despejado por EVE. Retomando Pedido 2.</color>");
                DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[WMS - VIA LIBRE]:</color> Obstaculo neutralizado por EVE. Autorizando reanudacion temprana de {idRobot}.");
                if (robotAfectado != null)
                {
                    robotAfectado.ReanudarTrasDespejeLiDAR();
                }
            };
            yield return StartCoroutine(dronMantenimiento.EjecutarMisionDespeje(obstaculoPasilloISO, reanudar));
        }
        else
        {
            yield return new WaitForSeconds(12.0f);
            if (obstaculoPasilloISO != null && obstaculoPasilloISO.activeSelf)
            {
                obstaculoPasilloISO.SetActive(false);
            }
            Debug.Log("<color=#10B981>[FLUJO]: 5. Obstáculo despejado por EVE. Retomando Pedido 2.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=#10B981>[WMS - VIA LIBRE]:</color> Obstaculo neutralizado por EVE. Autorizando reanudacion de {idRobot}.");
            if (robotAfectado != null)
            {
                robotAfectado.ReanudarTrasDespejeLiDAR();
            }
        }

        despachandoDespejeISO = false;
    }
}
````

## File: AGENTS.md
````markdown
# AGENTS.md

<!-- Funplay Unity MCP managed project skills -->
<!-- Funplay Unity MCP project skill versions: unity-mcp-workflow@1.0.10, unity-ui-composition@1.0.11 -->

# Funplay Unity MCP Project Guidance

This section is managed by Funplay MCP for Unity. Everything between the begin and end markers is regenerated on each sync; edit outside this block.

## Installed project skills

- `funplay-unity-mcp-workflow` v1.0.10 - Efficient workflow for using Unity MCP to edit, import, compile, inspect, and test Unity projects, including screenshot and Game View recording verification.
- `funplay-unity-ui-composition` v1.0.11 - Build and revise Unity UI, primarily uGUI mobile page prefabs, with framework-aware routing, Sprite import safety, text effects, localization and visual verification.

## Agent workflow rules

- Prefer project-local Funplay skills: `.codex/skills/` for Codex, `.opencode/skills/` for OpenCode, `.dsh/skills/` for DeepSeek Harness, `.agents/skills/` for Antigravity.
- Prefer structured MCP tools; use `execute_code` for project-specific gaps. For new snippets, include `using Funplay.Editor.Tools.Scripting;`, implement `IFunplayCommand`, and use `ctx.RegisterObjectCreation` / `RegisterObjectModification` / `DestroyObject` so changes participate in Undo automatically.
- Confirm the Unity project root, active scene, and real object/prefab/asset path before edits. Treat user-provided object names as hints, not paths.
- Inspect Unity objects through MCP before changing user-named scene or prefab targets. Carry the returned `instanceId` into follow-up calls (`find_method=by_id`) instead of re-resolving by name.
- Tool returns are structured JSON (`{success, message, data}` / `{success: false, code, error, data}`). Branch on `code`, not free-form text.
- Set component fields with `set_component_property(ies)` — it picks up `[SerializeField] private` fields and accepts Object references as `{"fileID": <instanceId>}` or `{"assetPath": "Assets/..."}`.
- For field-only prefab asset edits, use `set_prefab_property(ies)` with a verified `Assets/**/*.prefab` path. If it reports duplicate paths or components, retry only with an index from that response; use Prefab Mode for structural edits.
- Read editor state through dedicated tools (`get_selection`, `get_prefab_stage`, `get_tags`, `get_layers`, `get_build_settings`); use `execute_menu_item` before falling back to ad-hoc `execute_code`.
- Never edit `.unity`, `.prefab`, or `.asset` files with shell text tools or patches; use Unity MCP / Editor APIs for scenes, prefabs, and ScriptableObject assets.
- Save only the scene or prefab assets intentionally modified, then read back exact values.
- Prefer structured query/audit/edit tools in Core; low-frequency configuration and preview management remain in Full. Use `get_tool_capabilities` to check exposure; respect customized allowlists.
- `execute_code` refreshes by default; use skip_refresh only after verifying the intended assemblies are ready.
- In `execute_code`, null-guard every lookup and return explicit missing path/object/component messages; do not run self-healing fallback loops.
- For Unity object references, do not use `??=` for lazy rebinding; use explicit `if (field == null) field = Resolve();`.
- After edits use `prepare_editor` with the intended mode and a unique request_key; use the returned task_id with `get_task`, wait_seconds and after_revision until operation.status=ready and current_editor confirms readiness and mode, then check console errors. Honor poll_after_ms on unchanged results.
- `request_recompile` is rejected while Unity is in Play Mode. Call `exit_play_mode` first, then retry.
- On disconnect, retry the same operation status read by ID/key. Historical reload recovery or HTTP reachability alone does not establish current readiness.
- An interrupted arbitrary mutation has unknown outcome until exact object/asset readback confirms it; never automatically replay it.
- Avoid changing `Library/`, `Temp/`, `Logs/`, or `obj/`.

## Project

- Project root: `C:\Users\Aprendiz\Documents\ProyectosUnity\SistemaLogistico_AMR`
- Product name: `SistemaLogistico_AMR`

## Notes

- Re-run `Funplay > Project Skills` after changing selected skills or platforms.
<!-- /Funplay Unity MCP managed project skills -->
````

## File: Packages/manifest.json
````json
{
  "dependencies": {
    "com.gamebooom.unity.mcp": "https://github.com/FunplayAI/funplay-unity-mcp.git#v0.7.6",
    "com.unity.ai.navigation": "1.1.7",
    "com.unity.cinemachine": "3.1.7",
    "com.unity.ide.visualstudio": "2.0.29",
    "com.unity.inputsystem": "1.14.2",
    "com.unity.render-pipelines.universal": "14.0.12",
    "com.unity.test-framework": "1.1.33",
    "com.unity.textmeshpro": "3.0.9",
    "com.unity.toolchain.linux-x86_64": "2.0.11",
    "com.unity.toolchain.win-x86_64-linux-x86_64": "2.0.11",
    "com.unity.ugui": "1.0.0",
    "com.unity.modules.ai": "1.0.0",
    "com.unity.modules.androidjni": "1.0.0",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.assetbundle": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.cloth": "1.0.0",
    "com.unity.modules.director": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.particlesystem": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.screencapture": "1.0.0",
    "com.unity.modules.terrain": "1.0.0",
    "com.unity.modules.terrainphysics": "1.0.0",
    "com.unity.modules.tilemap": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0",
    "com.unity.modules.umbra": "1.0.0",
    "com.unity.modules.unityanalytics": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0",
    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
    "com.unity.modules.unitywebrequestaudio": "1.0.0",
    "com.unity.modules.unitywebrequesttexture": "1.0.0",
    "com.unity.modules.unitywebrequestwww": "1.0.0",
    "com.unity.modules.vehicles": "1.0.0",
    "com.unity.modules.video": "1.0.0",
    "com.unity.modules.vr": "1.0.0",
    "com.unity.modules.wind": "1.0.0",
    "com.unity.modules.xr": "1.0.0"
  }
}
````
