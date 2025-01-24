using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OptimizeMaterials : EditorWindow
{
    private string folderPath = "Assets";

    [MenuItem("Tools/Optimize Materials")]
    public static void ShowWindow()
    {
        GetWindow<OptimizeMaterials>("Optimize Materials");
    }

    private void OnGUI()
    {
        GUILayout.Label("Optimize Materials", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Folder Path:", folderPath);
        if (GUILayout.Button("Select Folder"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    folderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogError("Please select a folder inside the Assets directory.");
                }
            }
        }

        if (GUILayout.Button("Optimize"))
        {
            OptimizeMaterialsInFolder(folderPath);
        }
    }

    private void OptimizeMaterialsInFolder(string path)
    {
        var materialMap = new Dictionary<Texture2D, Material>();
        var duplicateMaterials = new List<Material>();

        // Étape 1 : Trouver tous les matériaux dans le dossier
        var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { path });
        foreach (var guid in materialGuids)
        {
            var materialPath = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material == null) continue;

            // Identifier la texture principale utilisée par le matériau
            var mainTexture = material.mainTexture as Texture2D;

            if (mainTexture == null) continue;

            if (materialMap.ContainsKey(mainTexture))
            {
                duplicateMaterials.Add(material);
                ReplaceMaterialInScene(material, materialMap[mainTexture]);
            }
            else
            {
                materialMap[mainTexture] = material;
            }
        }

        // Étape 2 : Supprimer les matériaux en double
        foreach (var material in duplicateMaterials)
        {
            var materialAssetPath = AssetDatabase.GetAssetPath(material);
            AssetDatabase.DeleteAsset(materialAssetPath);
            Debug.Log($"Deleted duplicate material: {materialAssetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Material Optimization Complete.");
    }

    private void ReplaceMaterialInScene(Material oldMaterial, Material newMaterial)
    {
        var renderers = FindObjectsOfType<Renderer>();

        foreach (var renderer in renderers)
        {
            bool materialUpdated = false;
            var materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == oldMaterial)
                {
                    materials[i] = newMaterial;
                    materialUpdated = true;
                }
            }

            if (materialUpdated)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
                Debug.Log($"Replaced material in renderer: {renderer.name}");
            }
        }
    }
}
