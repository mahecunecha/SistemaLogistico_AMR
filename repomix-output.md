This file is a merged representation of the entire codebase, combined into a single document by Repomix.

# File Summary

## Purpose
This file contains a packed representation of the entire repository's contents.
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
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Files are sorted by Git change count (files with more changes are at the bottom)

# Directory Structure
```
Editor/
  OrphanAssetFinder.cs
  OrphanAssetFinder.cs.meta
  RestructureProject.cs
  RestructureProject.cs.meta
DashboardUI.cs
DashboardUI.cs.meta
DirectorCinematico.cs
DirectorCinematico.cs.meta
DronePropellers.cs
DronePropellers.cs.meta
Editor.meta
GeneradorTablaManifiesto.cs
GeneradorTablaManifiesto.cs.meta
ModelosLogistica.cs
ModelosLogistica.cs.meta
ModuloDespachoTMS.cs
ModuloDespachoTMS.cs.meta
RobotAMR.cs
RobotAMR.cs.meta
RobotManagerEVE.cs
RobotManagerEVE.cs.meta
TelemetriaAMR.cs
TelemetriaAMR.cs.meta
```

# Files

## File: Editor/OrphanAssetFinder.cs
```csharp
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
```

## File: Editor/OrphanAssetFinder.cs.meta
```
fileFormatVersion: 2
guid: ea0b62a3f7f03a6ec8862303598b6cd2
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: Editor/RestructureProject.cs
```csharp
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
```

## File: Editor/RestructureProject.cs.meta
```
fileFormatVersion: 2
guid: 8e04022ff51bf91819c52d0bd9b287c5
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: DashboardUI.cs
```csharp
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Telemetría y Manifiesto")]
    public Image barraBateriaRelleno;
    public List<TextMeshProUGUI> textosEstadoManifiesto = new List<TextMeshProUGUI>();

    [Header("Alertas")]
    public GameObject panelEmergencia;
    [Tooltip("Asigna aquí el texto del detalle de emergencia (TMP o Legacy).")]
    [SerializeField] private Component textoDetalleEmergencia;

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

    public void OnOverridePresionado()
    {
        MostrarEmergencia(false);
        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>();
        RobotAMR robot = (eve != null && eve.robotEjecutor != null) ? eve.robotEjecutor : UnityEngine.Object.FindObjectOfType<RobotAMR>();
        if (robot != null)
        {
            robot.EjecutarOverride();
        }
    }

    public void OnRelevoPresionado()
    {
        MostrarEmergencia(false);
        FindFirstObjectByType<RobotManagerEVE>()?.EjecutarRelevoFlota24_7();
    }

    public void OnAbortarMisionPresionado()
    {
        MostrarEmergencia(false);
        RobotManagerEVE eve = FindFirstObjectByType<RobotManagerEVE>();
        RobotAMR robot = (eve != null && eve.robotEjecutor != null) ? eve.robotEjecutor : UnityEngine.Object.FindObjectOfType<RobotAMR>();
        if (robot != null)
        {
            robot.AbortarMisionPorSeguridad();
        }
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
                slot.txtEstado.text = FormatearEstadoVisual(estado);
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
            default: return estado.ToString().ToUpper();
        }
    }

    public void ActualizarBateria(float porcentaje)
    {
        ActualizarBateria("AMR-01", porcentaje, RobotAMR.EstadoRobot.Inactivo);
    }

    public void ActualizarEstadoManifiesto(int indice, string nuevoEstado, Color color)
    {
        if (indice >= 0 && indice < textosEstadoManifiesto.Count && textosEstadoManifiesto[indice] != null)
        {
            textosEstadoManifiesto[indice].text = nuevoEstado;
            textosEstadoManifiesto[indice].color = color;
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
```

## File: DashboardUI.cs.meta
```
fileFormatVersion: 2
guid: de85286332684c08295c0112a8a3ded3
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: DirectorCinematico.cs
```csharp
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
```

## File: DirectorCinematico.cs.meta
```
fileFormatVersion: 2
guid: fa955a2a5beb9ed42a8ab87944b00541
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: DronePropellers.cs
```csharp
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
```

## File: DronePropellers.cs.meta
```
fileFormatVersion: 2
guid: a60b6c2f773a72f4995047a9428bd705
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: Editor.meta
```
fileFormatVersion: 2
guid: b18bf4b757fe08c1e86fd088ac6ef456
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: GeneradorTablaManifiesto.cs
```csharp
using UnityEngine;
using TMPro; // Necesario para TextMeshPro
using System.Collections.Generic;

public class GeneradorTablaManifiesto : MonoBehaviour
{
    [Header("Configuración de UI")]
    [SerializeField] private Transform contenedorFilas; // Aquí arrastrarás tu panel 'Manifiesto'
    [SerializeField] private GameObject filaPrefab;     // Aquí arrastrarás tu Prefab azul

    // Función pública que llamaremos cuando el JSON termine de leerse
    public void PoblarTabla(List<Pedido> pedidosJSON)
    {
        // 1. Limpieza de seguridad: Borrar filas viejas si el JSON se recarga
        // (Preservando Header_Manifiesto y Fila_Titulos en la parte superior)
        for (int i = contenedorFilas.childCount - 1; i >= 0; i--)
        {
            Transform child = contenedorFilas.GetChild(i);
            if (child.name == "Header_Manifiesto" || child.name == "Fila_Titulos")
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        DashboardUI.Instance.textosEstadoManifiesto.Clear();

        // 2. Crear una fila nueva por cada pedido en el JSON
        foreach (Pedido pedido in pedidosJSON)
        {
            // Clonamos el prefab dentro del contenedor
            GameObject nuevaFila = Instantiate(filaPrefab, contenedorFilas);

            // Obtenemos los 5 textos de esa nueva fila
            TextMeshProUGUI[] columnas = nuevaFila.GetComponentsInChildren<TextMeshProUGUI>();

            // Asignamos la información del JSON a cada columna respectiva
            columnas[0].text = pedido.id_pallet;
            columnas[1].text = pedido.nombre_producto; // Asegúrate de que esta variable exista en tu clase Pedido
            columnas[2].text = pedido.codigo_sku;
            columnas[3].text = pedido.peso_kg.ToString() + " kg";
            
            // Lógica visual para el estado
            columnas[4].text = "PENDIENTE";
            columnas[4].color = Color.red; // Resalta en rojo para indicar que el robot aún no lo mueve
            
            DashboardUI.Instance.textosEstadoManifiesto.Add(columnas[4]);
        }
    }
}
```

## File: GeneradorTablaManifiesto.cs.meta
```
fileFormatVersion: 2
guid: 8d33dd4984a76f4e185e4b40d64bf7a4
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: ModelosLogistica.cs
```csharp
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
    public List<Pedido> pedidos;
}
```

## File: ModelosLogistica.cs.meta
```
fileFormatVersion: 2
guid: fedf791e237fa794599eb6c25ce31a3a
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: ModuloDespachoTMS.cs
```csharp
using System.Collections.Generic;
using UnityEngine;

public class ModuloDespachoTMS : MonoBehaviour
{
    public static ModuloDespachoTMS Instance { get; private set; }

    [Header("Bahías de Carga / Muelles (Sector Este)")]
    public Transform muelleCamion01;
    public Transform muelleCamion02;
    public Transform muelleCamion03;

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
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Transform ConsultarMuelleDestino(Pedido pedido, out int numeroCamion, out int slotLIFOProyectado)
    {
        numeroCamion = 1;
        if (pedido != null)
        {
            // Regla forzada temporal (Llenado Concentrado LIFO):
            // Todos los pedidos del manifiesto.json se envían al Camión 1 (muelleCamion01)
            // para apilar las 3 estibas en los Slots 1, 2 y 3 del Camión 1.
            numeroCamion = 1;

            /* Regla de ruteo TMS Módulo 5 (Desactivada temporalmente para validación LIFO):
            // Pedido 1 (AGU-RUB-330): Camión 1 (Ruta Nacional)
            // Pedido 2 (CLU-NEG-330): Camión 2 (Ruta Metropolitana)
            // Pedido 3 (OKT-BAR-50L, 920 kg): Camión 3 (Ruta Express / Carga Pesada)
            if (pedido.codigo_sku == "AGU-RUB-330" || pedido.id_pallet == "PAL-8491")
            {
                numeroCamion = 1;
            }
            else if (pedido.codigo_sku == "CLU-NEG-330" || pedido.id_pallet == "PAL-8492")
            {
                numeroCamion = 2;
            }
            else if (pedido.codigo_sku == "OKT-BAR-50L" || pedido.id_pallet == "PAL-8493" || pedido.peso_kg >= 900f)
            {
                numeroCamion = 3;
            }
            */
        }

        switch (numeroCamion)
        {
            case 1:
                slotLIFOProyectado = matrizLIFOCamion1.Count + 1;
                break;
            case 2:
                slotLIFOProyectado = matrizLIFOCamion2.Count + 1;
                break;
            case 3:
                slotLIFOProyectado = matrizLIFOCamion3.Count + 1;
                break;
            default:
                slotLIFOProyectado = 1;
                break;
        }

        string idPallet = pedido != null ? pedido.id_pallet : "PAL-DESC";
        string logMensaje = $"<color=#38BDF8>[API MÓDULO 5 -> WMS]:</color> Ruta GPS validada. Asignando {idPallet} a Muelle_Camion_0{numeroCamion}";
        DashboardUI.Instance?.RegistrarLog(logMensaje);
        Debug.Log(logMensaje);

        return ObtenerTransformMuelle(numeroCamion);
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

    public void RegistrarEstibaEnCamion(int numeroCamion, Pedido pedido, GameObject palletFisico)
    {
        if (pedido == null) return;

        Transform muelleTransform = ObtenerTransformMuelle(numeroCamion);
        Stack<Pedido> pilaObjetivo;
        List<GameObject> listaPallets;
        float pesoAcumulado;

        switch (numeroCamion)
        {
            case 1:
                matrizLIFOCamion1.Push(pedido);
                pesoAcumuladoCamion1 += pedido.peso_kg;
                pilaObjetivo = matrizLIFOCamion1;
                listaPallets = palletsCamion1;
                pesoAcumulado = pesoAcumuladoCamion1;
                break;
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

            int indiceLIFO = pilaObjetivo.Count;
            float zOffset = (indiceLIFO - 2) * 1.6f;

            // Calibración al ras del Suelo_Falso_NavMesh del camión (maxY = 1.813f en lugar del techo 3.23f)
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

        string logMatriz = $"<color=cyan>[MATRIZ LIFO CAMIÓN {numeroCamion}]:</color> Estiba {pedido.id_pallet} apilada en Posición #{pilaObjetivo.Count} (Peso acumulado: {pesoAcumulado:F0} kg / {CapacidadMaximaCamionKg:F0} kg)";
        DashboardUI.Instance?.RegistrarLog(logMatriz);
        Debug.Log(logMatriz);
    }
}
```

## File: ModuloDespachoTMS.cs.meta
```
fileFormatVersion: 2
guid: 9ab0dae538c3e5542ba6de0990e262dd
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: RobotAMR.cs
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobotAMR : MonoBehaviour
{
    /* Máquina de Estados Finitos (FSM) para blindar el código contra errores lógicos, enum es una lista restrictiva.
    Le prohíbe a la variable estadoActual tener cualquier otro valor que no sea uno de esos tres.*/
    public enum EstadoRobot { Inactivo, En_Transito, Extrayendo, Transportando, Interrumpido, Entregando, Recargando }
    
    public event System.Action<EstadoRobot> OnEstadoCambiado;

    [Header("Identidad de Flota")]
    public string idRobot = "AMR-01";

    [Header("Telemetría Batería")]
    public float bateriaActual = 80.0f;
    public float coeficienteVacio = 0.0020f;
    public float coeficienteCarga = 0.00158f;
    public float consumoIdle = 0.0010f;

    [Header("Throttling Telemetría")]
    private float tiempoProximoEnvioTelemetria = 0f;
    private const float IntervaloTelemetria = 0.15f; // ~6.6 Hz
    private float ultimaBateriaTransmitida = -1f;
    private EstadoRobot ultimoEstadoTransmitido = (EstadoRobot)(-1);

    [Header("Sensores LiDAR")]
    public LayerMask capaObstaculos = 1; // Default layer (bit 0) por defecto
    private float distanciaDeteccionAlerta = 1.2f;
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

    [Header("Control de Tráfico Industrial (Mutex ISO 3691-4)")]
    private static readonly Dictionary<Transform, RobotAMR> zonasReservadasMutex = new Dictionary<Transform, RobotAMR>();
    private const float DistanciaCompuertaMutex = 3.5f;
    private const float RadioOcupacionFisicaMutex = 2.5f;
    private bool enEsperaMutex = false;

    [Header("Enrutamiento Ortogonal (VDA 5050)")]
    public const float ZAvenidaOeste = -32.0f;
    public const float ZAvenidaEste = -34.0f;
    public const float XPorticoSalidaNorte = 34.40f;
    public const float XPorticoEntradaSur = 32.60f;
    public const float ZUmbralPortico = -48.0f;
    public const float DesfaseCarrilRack = 0.46f;
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

    public Pedido PedidoActual => pedidoActual;

    [Header("Referencias (LIFO)")]
    public RobotManagerEVE managerEVE;
    public Transform zonaTransferencia;
    public Transform puntoMontajeCarga;
    private Transform destinoActual;
    private GameObject palletActualGO;

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

    void OnDisable()
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
    }

    void OnDestroy()
    {
        enEsperaMutex = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
    }

    public void ActualizarJerarquiaNavegacion()
    {
        if (agente == null) return;

        int offsetDesempate = ((!string.IsNullOrEmpty(idRobot) && idRobot.EndsWith("02")) || gameObject.name.Contains("02")) ? 1 : 0;

        switch (estadoActual)
        {
            case EstadoRobot.Extrayendo:
            case EstadoRobot.Entregando:
            case EstadoRobot.Interrumpido:
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

    private bool IntentarReservarZonaMutex(Transform zona)
    {
        if (zona == null) return true;

        if (zonasReservadasMutex.TryGetValue(zona, out RobotAMR propietario))
        {
            if (propietario == null || !propietario.gameObject.activeInHierarchy ||
                propietario.estadoActual == EstadoRobot.Inactivo || propietario.estadoActual == EstadoRobot.Recargando)
            {
                zonasReservadasMutex.Remove(zona);
            }
            else if (propietario != this)
            {
                return false;
            }
        }

        if (managerEVE != null)
        {
            RobotAMR otroRobot = (managerEVE.robotAMR_01 != null && managerEVE.robotAMR_01 != this) ? managerEVE.robotAMR_01 :
                                 ((managerEVE.robotAMR_02 != null && managerEVE.robotAMR_02 != this) ? managerEVE.robotAMR_02 : null);

            if (otroRobot != null && otroRobot.gameObject.activeInHierarchy && otroRobot.estadoActual != EstadoRobot.Inactivo)
            {
                Vector3 puntoRef = (agente != null && agente.hasPath) ? agente.pathEndPosition : zona.position;
                Vector2 posOtroXZ = new Vector2(otroRobot.transform.position.x, otroRobot.transform.position.z);
                Vector2 posRefXZ = new Vector2(puntoRef.x, puntoRef.z);

                if (Vector2.Distance(posOtroXZ, posRefXZ) < RadioOcupacionFisicaMutex)
                {
                    return false;
                }
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
        if (enParadaSeguridadLiDAR || estadoActual == EstadoRobot.Interrumpido) return;

        if (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando)
        {
            Transform objetivo = (estadoActual == EstadoRobot.En_Transito) ? destinoActual : zonaTransferencia;

            // Liberar cualquier cerrojo previo que ya no sea el objetivo activo si el robot ya salió del radio físico
            if (zonasReservadasMutex.Count > 0)
            {
                List<Transform> obsoletas = null;
                Vector2 posActualXZ = new Vector2(transform.position.x, transform.position.z);
                foreach (var kvp in zonasReservadasMutex)
                {
                    if (kvp.Value == this && kvp.Key != null && kvp.Key != objetivo)
                    {
                        Vector2 posZonaXZ = new Vector2(kvp.Key.position.x, kvp.Key.position.z);
                        if (Vector2.Distance(posActualXZ, posZonaXZ) > RadioOcupacionFisicaMutex)
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

            if (objetivo != null && agente != null && colaWaypoints.Count == 0 && !agente.pathPending)
            {
                if (agente.remainingDistance <= DistanciaCompuertaMutex &&
                    DistanciaPlanaADestinoAgente() <= DistanciaCompuertaMutex + 0.25f)
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

    public float ResolverEjeCarrilX(Vector3 posicion, out Vector3 puntoCarrilNavMesh)
    {
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

    private void ConstruirNodosManhattan(Vector3 origen, Vector3 destino, List<Vector3> nodos)
    {
        nodos.Clear();

        float xOrigen = ResolverEjeCarrilX(origen, out Vector3 origenNav);
        float xDestino = ResolverEjeCarrilX(destino, out Vector3 destinoNav);

        bool origenEnSalaCarga = origen.z < ZUmbralPortico;
        bool destinoEnSalaCarga = destino.z < ZUmbralPortico;

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
            AgregarNodoRuta(nodos, destinoNav, true);
            return;
        }

        float xSalidaAvenida = origenEnSalaCarga ? XPorticoSalidaNorte : xOrigen;
        float xEntradaAvenida = destinoEnSalaCarga ? XPorticoEntradaSur : xDestino;

        // Selección de carril direccional en la Avenida Transversal:
        // -32.0f para flujos hacia el Oeste (Izquierda), -34.0f para flujos hacia el Este (Derecha)
        float zAvenida = (xEntradaAvenida < xSalidaAvenida) ? ZAvenidaOeste : ZAvenidaEste;

        AgregarNodoRuta(nodos, origenNav);

        // 1. Salida hacia la Avenida Principal
        if (origenEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, origenNav.y, origenNav.z)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoSalidaNorte, origenNav.y, zAvenida)));
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xOrigen, origenNav.y, zAvenida)));
        }

        // 2. Transición por la Avenida y Entrada al Destino
        if (destinoEnSalaCarga)
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, destinoNav.y, zAvenida)));
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(XPorticoEntradaSur, destinoNav.y, destinoNav.z)));
            AgregarNodoRuta(nodos, destinoNav, true);
        }
        else
        {
            AgregarNodoRuta(nodos, AjustarPuntoNavMesh(new Vector3(xDestino, destinoNav.y, zAvenida)));
            AgregarNodoRuta(nodos, destinoNav, true);
        }
    }

    private void TrazarRutaOrtogonal(Vector3 destinoFinal)
    {
        if (agente == null) return;

        colaWaypoints.Clear();
        ConstruirNodosManhattan(transform.position, destinoFinal, bufferNodosRuta);

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
            ResolverEjeCarrilX(destinoFinal, out Vector3 destinoNav);
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
        if (agente == null || enParadaSeguridadLiDAR || enEsperaMutex || estadoActual == EstadoRobot.Interrumpido) return false;
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

    private float CalcularDistanciaNavMesh(Vector3 origen, Vector3 destino)
    {
        List<Vector3> nodos = new List<Vector3>(8);
        ConstruirNodosManhattan(origen, destino, nodos);

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

    private bool CalcularViabilidadOperativa(Pedido pedido, Transform destino, out float bateriaRestanteEstimada)
    {
        // Si la zona de transferencia no ha sido resuelta para este pedido, consultar al TMS
        if (zonaTransferencia == null)
        {
            ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
            if (tms != null)
            {
                zonaTransferencia = tms.ConsultarMuelleDestino(pedido, out numCamionAsignado, out _);
            }
        }

        float distanciaIda = CalcularDistanciaNavMesh(transform.position, destino.position);
        float distanciaVuelta = 0f;
        if (zonaTransferencia != null)
        {
            distanciaVuelta = CalcularDistanciaNavMesh(destino.position, zonaTransferencia.position);
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
        /* Regla de seguridad: Solo acepta misiones si está inactivo
        el return funciona como una pared que protege a la mision actual*/
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
        
        if (!CalcularViabilidadOperativa(pedidoActual, destinoFisico, out float bateriaProyectada))
        {
            destinoActual = destinoFisico;
            Debug.Log("<color=#10B981>[FLUJO]: 6. Pedido 3 evaluado. Emergencia de Batería (Ind. 5.0) disparada. Esperando decisión del supervisor.</color>");
            DashboardUI.Instance?.RegistrarLog($"<color=red>[ALERTA PREDICTIVA] Misión inviable. Retorno proyectado: {bateriaProyectada:F1}% (Umbral LiFePO4: {UmbralSeguridadLiFePO4:F0}%). Detención preventiva.</color>");
            DashboardUI.Instance?.MostrarEmergencia(true, bateriaProyectada, 3.2f);
            estadoActual = EstadoRobot.Interrumpido;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);
            if (agente != null) agente.isStopped = true;
            return;
        }

        DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "EN TRÁNSITO", Color.yellow);
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
        enParadaSeguridadLiDAR = false;
        enEsperaMutex = false;
        obstaculoDetectado = false;
        colaWaypoints.Clear();
        LiberarMutexDeRobot(this);
        pedidoActual = null;
        destinoActual = null;
        zonaTransferencia = null;

        estadoActual = EstadoRobot.Recargando;
        ActualizarJerarquiaNavegacion();

        if (agente != null)
        {
            agente.isStopped = false;
            agente.ResetPath();
            agente.radius = RadioAgenteIndustrial;
            agente.angularSpeed = VelocidadAngularIndustrial;
            agente.speed = velocidadBase;
            agente.acceleration = aceleracionBase;
            TrazarRutaOrtogonal(posicionPad);
        }

        OnEstadoCambiado?.Invoke(estadoActual);
        Debug.Log($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de carga en {posicionPad}");
        DashboardUI.Instance?.RegistrarLog($"<color=green>[CARGA INDUCTIVA]:</color> {gameObject.name} navegando a Pad de recarga.");
    }

    public void EjecutarOverride()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);
        enEsperaMutex = false;
        estadoActual = EstadoRobot.En_Transito;
        ActualizarJerarquiaNavegacion();
        if (agente != null) agente.isStopped = false;
        
        modoDegradacionActivo = true;
        if (agente != null) agente.acceleration = Mathf.Max(1.0f, agente.acceleration * 0.5f);
        coeficienteCarga *= 1.3f;

        if (destinoActual != null)
        {
            TrazarRutaOrtogonal(destinoActual.position);
        }
        
        OnEstadoCambiado?.Invoke(estadoActual);
        DashboardUI.Instance?.RegistrarLog("<color=orange>[OVERRIDE EJECUTADO] Protocolo de contingencia forzado. Modo de degradación térmica activo: Aceleración -50%, Consumo +30%.</color>");
    }

    public void AbortarMisionPorSeguridad()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        string idPallet = pedidoActual != null ? pedidoActual.id_pallet : "ORDEN";
        int indicePedido = managerEVE != null ? managerEVE.indicePedidoActual : 0;
        DashboardUI.Instance?.ActualizarEstadoManifiesto(indicePedido, "REPROGRAMADO", new Color(1.0f, 0.55f, 0.1f));

        DashboardUI.Instance?.RegistrarLog($"<color=#FFA500>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Celda protegida al {bateriaActual:F1}%. Orden reprogramada.");
        Debug.Log($"<color=orange>[DECISIÓN GERENCIAL]:</color> Misión {idPallet} abortada preventivamente. Batería conservada al {bateriaActual:F1}%.");

        StopAllCoroutines();
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

        // 1. Base de madera oscura (Estiba estándar: 1.1m x 0.15m x 0.9m)
        GameObject baseMadera = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseMadera.name = "Base_Madera";
        baseMadera.transform.SetParent(rootPallet.transform, false);
        baseMadera.transform.localPosition = new Vector3(0f, 0.075f, 0f);
        baseMadera.transform.localScale = new Vector3(1.1f, 0.15f, 0.9f);

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
        bloqueCarga.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        bloqueCarga.transform.localScale = new Vector3(1.0f, 0.6f, 0.85f);

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
            agente.velocity = Vector3.zero;
        }

        // Simulación de tiempo de extracción de carga calculada por hardware
        float tiempoExtraccion = Mathf.Lerp(1.2f, 3.8f, pedidoActual.peso_kg / 1000f) + Random.Range(-0.2f, 0.2f);
        yield return new WaitForSeconds(tiempoExtraccion);

        // CRI-03: Construcción e instanciación del gemelo físico del pallet sobre el punto de montaje
        palletActualGO = ConstruirPalletVisual(pedidoActual);

        if (agente != null)
        {
            agente.isStopped = false;
        }

        // Liberar reserva del rack extraído al iniciar retorno hacia muelle
        LiberarZonaMutex(destinoActual, this);

        // Consultar o validar muelle de destino en ModuloDespachoTMS al iniciar retorno
        if (managerEVE != null && managerEVE.gestorTMS != null)
        {
            Transform muelleDestino = managerEVE.gestorTMS.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out _);
            if (muelleDestino != null) zonaTransferencia = muelleDestino;
        }
        else if (ModuloDespachoTMS.Instance != null)
        {
            Transform muelleDestino = ModuloDespachoTMS.Instance.ConsultarMuelleDestino(pedidoActual, out numCamionAsignado, out _);
            if (muelleDestino != null) zonaTransferencia = muelleDestino;
        }
        
        if (zonaTransferencia != null)
        {
            enEsperaMutex = false;
            estadoActual = EstadoRobot.Transportando;
            ActualizarJerarquiaNavegacion();
            OnEstadoCambiado?.Invoke(estadoActual);

            agente.speed = ObtenerVelocidadNominal();
            agente.acceleration = Mathf.Max(1.0f, aceleracionBase - (pedidoActual.peso_kg * coeficientePeso * 2f));

            TrazarRutaOrtogonal(zonaTransferencia.position);
            Debug.Log("<color=cyan>AMR Retornando:</color> Llevando estiba a la Zona de Transferencia (" + zonaTransferencia.name + ").");
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

        DashboardUI.Instance?.ActualizarEstadoManifiesto(managerEVE.indicePedidoActual, "ENTREGADO", Color.green);
        Debug.Log("<color=magenta>LIFO:</color> Descargando estiba en Zona de Transferencia.");
        DashboardUI.Instance?.RegistrarLog("LIFO: Descargando estiba en Zona de Transferencia.");

        if (agente != null)
        {
            agente.isStopped = true;
            agente.velocity = Vector3.zero;
        }

        // Desacoplar pallet del AMR y transferirlo a la Matriz LIFO del Camión mediante ModuloDespachoTMS
        if (palletActualGO != null)
        {
            ModuloDespachoTMS tms = (managerEVE != null && managerEVE.gestorTMS != null) ? managerEVE.gestorTMS : ModuloDespachoTMS.Instance;
            if (tms != null)
            {
                tms.RegistrarEstibaEnCamion(numCamionAsignado, pedidoActual, palletActualGO);
            }
            else if (managerEVE != null)
            {
                managerEVE.RegistrarEntregaLIFO(pedidoActual, palletActualGO);
            }
            palletActualGO = null;
        }

        // Simulación temporal de desacople de carga
        yield return new WaitForSeconds(1.5f);

        if (agente != null)
        {
            agente.isStopped = false;
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
            managerEVE.ConfirmarEntregaExitosa();
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
                if (hit.distance <= 0.85f && !enParadaSeguridadLiDAR && amrEnMovimiento && EsObstaculoISORegistrado(hit.transform))
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

    void Update()
    {
        // Procesamiento activo del sensor LiDAR con Docking Mute (ISO 3691-4)
        ProcesarSensoresLiDAR();

        // Transición fluida entre tramos ortogonales (VDA 5050) sin parada en esquinas intermedias
        bool cambioTramoWaypoint = AvanzarColaWaypoints();

        // Gestión de exclusión mutua (Mutex) de zona de acople a <= 3.5m en el tramo final
        if (!cambioTramoWaypoint)
        {
            GestionarCompuertaMutex();
        }

        // Consumo pasivo obligatorio por encendido de sistemas lógicos (blindado a 0% en parada de seguridad/interrupción/espera Mutex)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido)
        {
            bateriaActual -= consumoIdle * Time.deltaTime;
        }

        // Matemáticas de Batería (Termodinámica Inyectada)
        if (!enParadaSeguridadLiDAR && !enEsperaMutex && estadoActual != EstadoRobot.Interrumpido && (estadoActual == EstadoRobot.En_Transito || estadoActual == EstadoRobot.Transportando))
        {
            float distanciaRecorrida = agente.velocity.magnitude * Time.deltaTime;
            if (distanciaRecorrida > 0)
            {
                float gasto = 0f;
                if (estadoActual == EstadoRobot.En_Transito)
                {
                    gasto = distanciaRecorrida * coeficienteVacio;
                }
                else if (estadoActual == EstadoRobot.Transportando)
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
            if (agente != null)
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

        if (cambioTramoWaypoint || colaWaypoints.Count > 0) return;

        if (estadoActual == EstadoRobot.En_Transito)
        {
            if (!enEsperaMutex && !agente.pathPending && agente.remainingDistance <= 1.0f && DistanciaPlanaADestinoAgente() <= 1.25f)
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
            // CRI-01 y MED-04: Detección estandarizada NavMesh y desacople temporal en corrutina
            if (!enEsperaMutex && zonaTransferencia != null && !agente.pathPending && agente.remainingDistance <= 1.0f && DistanciaPlanaADestinoAgente() <= 1.25f)
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
                const float TasaCargaInductivaPorSegundo = 0.05f; // Modela recarga lenta y estable LiFePO4
                bateriaActual = Mathf.Min(80.0f, bateriaActual + (Time.deltaTime * TasaCargaInductivaPorSegundo));
                DashboardUI.Instance?.ActualizarBateria(idRobot, bateriaActual / 100f, estadoActual);
            }
        }
    }
}
```

## File: RobotAMR.cs.meta
```
fileFormatVersion: 2
guid: a904ba43c8edac9fc8a5d87f79ae0ff5
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: RobotManagerEVE.cs
```csharp
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Base de Datos Local")]
    public Manifiesto manifiestoActual;
    public RobotAMR robotEjecutor;
    public int indicePedidoActual = 0;
    public bool turnoIniciado = false;
    private Dictionary<string, Transform> directorioBodega = new Dictionary<string, Transform>();

    [Header("Trazabilidad LIFO Camión")]
    private Stack<Pedido> matrizLIFOCamion = new Stack<Pedido>();
    private List<GameObject> palletsEntregados = new List<GameObject>();
    private float pesoAcumuladoCamion = 0f;
    private const float CapacidadMaximaCamionKg = 12000f;

    public Stack<Pedido> MatrizLIFOCamion => matrizLIFOCamion;
    public float PesoAcumuladoCamion => pesoAcumuladoCamion;

    [Header("Simulacion de Incertidumbre ISO 3691-4")]
    public GameObject obstaculoPasilloISO;
    private bool despachandoDespejeISO = false;

    void Awake()
    {
        if (gestorTMS == null) gestorTMS = FindFirstObjectByType<ModuloDespachoTMS>();
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

        if (!turnoIniciado) return;
        MoverPorWaypoints();
    }

    public void IniciarTurno()
    {
        if (turnoIniciado) return;
        turnoIniciado = true;
        Debug.Log("<color=#10B981>[FLUJO]: 1. Botón 'Iniciar Turno' presionado. Despachando Pedido 1.</color>");

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

        // Población sincronizada de la tabla del manifiesto tras activar el HUD
        FindObjectOfType<GeneradorTablaManifiesto>()?.PoblarTabla(manifiestoActual.pedidos);

        if (manifiestoActual != null && manifiestoActual.pedidos.Count > 0 && robotEjecutor != null)
        {
            var pedido = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

            if (gestorTMS != null)
            {
                Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
                robotEjecutor.zonaTransferencia = muelleDestino;
                robotEjecutor.numCamionAsignado = numCamion;
            }

            robotEjecutor.AsignarMision(pedido, destino);
        }
        DashboardUI.Instance?.RegistrarLog("<color=green>[WMS]:</color> Turno de despacho nocturno iniciado.");
        DashboardUI.Instance?.RegistrarLog("<color=#38BDF8>[WMS ENERGÍA]:</color> Bus Aéreo PoE++ 48V DC Activo — Disponibilidad 24/7 Garantizada.");
    }

    void CargarManifiesto()
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, "manifiesto.json");
        if (File.Exists(ruta))
        {
            string contenidoJson = File.ReadAllText(ruta);
            manifiestoActual = JsonUtility.FromJson<Manifiesto>(contenidoJson);
            
            directorioBodega.Clear();
            foreach(var pedido in manifiestoActual.pedidos)
            {
                if (!directorioBodega.ContainsKey(pedido.coordenada_bodega))
                {
                    GameObject destino = GameObject.Find(pedido.coordenada_bodega);
                    if (destino != null) directorioBodega.Add(pedido.coordenada_bodega, destino.transform);
                }
            }

            Debug.Log("<color=green>EVE:</color> Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
            DashboardUI.Instance?.RegistrarLog("EVE: Manifiesto cargado. Total de pallets: " + manifiestoActual.pedidos.Count);
        }
        else
        {
            Debug.LogError("EVE Error crítico: No se encontró el Manifiesto de Carga en la ruta: " + ruta);
            DashboardUI.Instance?.RegistrarLog("EVE Error: No se encontró el Manifiesto de Carga.");
        }
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

                // Cuadrícula LIFO: acomodar a lo largo del eje Z del camión
                int indiceLIFO = matrizLIFOCamion.Count; // 1, 2, 3...
                float zOffset = (indiceLIFO - 2) * 1.6f;
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

    public void ConfirmarEntregaExitosa()
    {
        indicePedidoActual++;
        if (indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            if (indicePedidoActual == 1)
            {
                StartCoroutine(SecuenciaSaltoTemporal());
                return;
            }

            DespacharPedido(indicePedidoActual);
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado. No hay más pedidos.");
            DashboardUI.Instance?.RegistrarLog("Operación Finalizada: Manifiesto completado.");
        }
    }

    public void NotificarMisionAbortada()
    {
        DashboardUI.Instance?.RegistrarLog("<color=orange>[WMS]:</color> Misión abortada. Avanzando cola de manifiesto.");
        indicePedidoActual++;
        if (manifiestoActual != null && indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            var pedido = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

            if (gestorTMS != null && robotEjecutor != null)
            {
                Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
                robotEjecutor.zonaTransferencia = muelleDestino;
                robotEjecutor.numCamionAsignado = numCamion;
            }

            OnMisionEvaluada?.Invoke();
            if (robotEjecutor != null)
            {
                robotEjecutor.AsignarMision(pedido, destino);
            }
        }
        else
        {
            Debug.Log("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado (Con Reprogramaciones).");
            DashboardUI.Instance?.RegistrarLog("<color=green>Operación Finalizada:</color> Manifiesto Nocturno completado (Con Reprogramaciones).");
        }
    }

    private void DespacharPedido(int indice)
    {
        if (manifiestoActual == null || indice >= manifiestoActual.pedidos.Count || robotEjecutor == null) return;

        var pedido = manifiestoActual.pedidos[indice];
        directorioBodega.TryGetValue(pedido.coordenada_bodega, out Transform destino);

        if (gestorTMS != null)
        {
            Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedido, out int numCamion, out int slot);
            robotEjecutor.zonaTransferencia = muelleDestino;
            robotEjecutor.numCamionAsignado = numCamion;
        }

        OnMisionEvaluada?.Invoke();
        robotEjecutor.AsignarMision(pedido, destino);
    }

    private IEnumerator SecuenciaSaltoTemporal()
    {
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
            float bateriaObjetivo = 22.20f;
            float tiempo = 0f;

            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracion);
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
                DashboardUI.Instance?.ActualizarProgresoTimeLapse(t);
                yield return null;
            }
        }

        DashboardUI.Instance?.ActualizarProgresoTimeLapse(1f);
        DashboardUI.Instance?.ActualizarBannerTurno("<b><color=#38BDF8>TURNO NOCTURNO:</color> Hora 7.5/8 (Bloque Final) | Química: <color=#F59E0B>LiFePO4</color> (80% - 20%)</b>");

        yield return new WaitForSeconds(0.5f);
        DashboardUI.Instance?.MostrarOverlayTimeLapse(false);

        Debug.Log("<color=#10B981>[FLUJO]: 3. Time-Lapse completado. Despachando Pedido 2. (PRESIONA 'O' AHORA PARA EMERGENCIA LiDAR)</color>");
        DespacharPedido(indicePedidoActual);
    }

    public void EjecutarRelevoFlota24_7()
    {
        DashboardUI.Instance?.MostrarEmergencia(false);

        // 1. Enviar evento de relevo al DirectorCinematico antes de alterar estados para desuscribir AMR-01 inmediatamente
        if (robotAMR_02 != null)
        {
            DirectorCinematico.Instance?.RelevarRobotActivo(robotAMR_02);
        }

        // 2. Desvincular AMR-01 nombrando a AMR-02 como nuevo robotEjecutor
        RobotAMR robotSaliente = robotAMR_01 != null ? robotAMR_01 : robotEjecutor;
        if (robotAMR_02 != null)
        {
            robotEjecutor = robotAMR_02;
        }

        // 3. Enviar AMR-01 a estación de carga inductiva (Pad_Inductivo_01) limpiando su estado
        if (robotSaliente != null && robotSaliente != robotAMR_02)
        {
            Vector3 destinoPad01 = new Vector3(-10.0f, 0.25f, -10.0f);
            Transform pad1 = GameObject.Find("Pad_Inductivo_01")?.transform;
            if (pad1 != null) destinoPad01 = pad1.position;

            robotSaliente.EnviarACargaInductiva(destinoPad01);
        }

        // 4. Asignar la misión pendiente al nuevo robotEjecutor (AMR-02)
        if (robotEjecutor != null && manifiestoActual != null && indicePedidoActual < manifiestoActual.pedidos.Count)
        {
            var pedidoPendiente = manifiestoActual.pedidos[indicePedidoActual];
            directorioBodega.TryGetValue(pedidoPendiente.coordenada_bodega, out Transform destino);

            if (gestorTMS != null)
            {
                Transform muelleDestino = gestorTMS.ConsultarMuelleDestino(pedidoPendiente, out int numCamion, out int slot);
                robotEjecutor.zonaTransferencia = muelleDestino;
                robotEjecutor.numCamionAsignado = numCamion;
            }

            robotEjecutor.AsignarMision(pedidoPendiente, destino);
        }

        // 5. Registrar en el Dashboard
        string mensajeRelevo = "[RELEVO 24/7]: AMR-01 acoplado a carga inductiva (Pad 01). AMR-02 desplegado para Pedido 3.";
        DashboardUI.Instance?.RegistrarLog($"<color=#38BDF8><b>{mensajeRelevo}</b></color>");
        Debug.Log($"<color=cyan>{mensajeRelevo}</color>");
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
            RobotAMR robot = robotEjecutor != null ? robotEjecutor : robotAMR_01;
            if (robot != null && (robot.estadoActual == RobotAMR.EstadoRobot.En_Transito || robot.estadoActual == RobotAMR.EstadoRobot.Transportando))
            {
                Vector3 dirFrente = robot.transform.forward;
                Vector3 posObstaculo = robot.transform.position + dirFrente * 2.2f;
                posObstaculo.y = robot.transform.position.y;

                obstaculoPasilloISO.transform.position = posObstaculo;
                if (dirFrente != Vector3.zero)
                {
                    obstaculoPasilloISO.transform.rotation = Quaternion.LookRotation(-dirFrente);
                }
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

    private IEnumerator ProtocoloDespejeAutonomoISO(RobotAMR robotAfectado, GameObject obstaculoDetectado)
    {
        despachandoDespejeISO = true;

        string idRobot = robotAfectado != null ? robotAfectado.idRobot : "AMR";
        DashboardUI.Instance?.RegistrarLog($"<color=#EF4444>[ISO 3691-4 CAT. 3]:</color> Bloqueo frontal a <0.85m. {idRobot} en parada de proteccion. EVE gestionando despeje...");

        yield return new WaitForSeconds(3.5f);

        // Solo se neutraliza el obstáculo ISO registrado (nunca racks u otra infraestructura estática)
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

        despachandoDespejeISO = false;
    }
}
```

## File: RobotManagerEVE.cs.meta
```
fileFormatVersion: 2
guid: 2119cc5f95d9681eb9873ddb1d3b2e34
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```

## File: TelemetriaAMR.cs
```csharp
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

    [Header("Throttling Settings")]
    private float tiempoProximoRefrescoETA = 0f;
    private const float IntervaloRefrescoETA = 0.15f; // ~6.6 Hz de refresco
    private float ultimoETAValor = -1f;
    private bool ultimoEstadoETAPendiente = false;

    void Awake()
    {
        robot = GetComponentInParent<RobotAMR>();
        agente = GetComponentInParent<NavMeshAgent>();
    }

    void OnEnable()
    {
        if (robot != null)
        {
            robot.OnEstadoCambiado += ActualizarTelemetriaEstadoYPayload;
        }
        ActualizarTelemetriaEstadoYPayload(robot != null ? robot.estadoActual : RobotAMR.EstadoRobot.Inactivo);
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
        ActualizarTelemetriaEstadoYPayload(robot != null ? robot.estadoActual : RobotAMR.EstadoRobot.Inactivo);
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }

    void Update()
    {
        if (robot == null || agente == null) return;

        // Throttling para cálculo y renderizado de ETA (~6-7 Hz para eliminar GC pressure a 60 FPS)
        if (Time.time >= tiempoProximoRefrescoETA)
        {
            tiempoProximoRefrescoETA = Time.time + IntervaloRefrescoETA;
            ActualizarETAThrottled();
        }
    }

    private void ActualizarTelemetriaEstadoYPayload(RobotAMR.EstadoRobot nuevoEstado)
    {
        if (txtEstado != null)
        {
            txtEstado.text = nuevoEstado.ToString();
        }

        if (txtPayload != null)
        {
            float peso = 0f;
            if (robot != null && (nuevoEstado == RobotAMR.EstadoRobot.Transportando || nuevoEstado == RobotAMR.EstadoRobot.Entregando))
            {
                if (robot.PedidoActual != null)
                {
                    peso = robot.PedidoActual.peso_kg;
                }
            }
            txtPayload.text = peso > 0 ? $"{peso:F0} kg" : "0 kg";
        }
    }

    private void ActualizarETAThrottled()
    {
        if (txtETA == null) return;

        if (agente.pathPending)
        {
            if (!ultimoEstadoETAPendiente)
            {
                txtETA.text = "ETA: --";
                ultimoEstadoETAPendiente = true;
                ultimoETAValor = -1f;
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
                txtETA.text = $"ETA: {etaRedondeado:F1}s";
            }
        }
        else
        {
            if (ultimoETAValor != 0f || ultimoEstadoETAPendiente)
            {
                txtETA.text = "ETA: --";
                ultimoETAValor = 0f;
                ultimoEstadoETAPendiente = false;
            }
        }
    }
}
```

## File: TelemetriaAMR.cs.meta
```
fileFormatVersion: 2
guid: 2261319aef7f2edf59eeec3e3922283a
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant:
```
