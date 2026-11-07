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
