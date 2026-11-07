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
