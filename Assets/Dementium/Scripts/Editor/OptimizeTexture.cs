using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class OptimizeTextures : EditorWindow
{
    private string folderPath = "Assets";

    [MenuItem("Tools/Optimize Textures and Materials")]
    public static void ShowWindow()
    {
        GetWindow<OptimizeTextures>("Optimize Textures");
    }

    private void OnGUI()
    {
        GUILayout.Label("Optimize Textures and Materials", EditorStyles.boldLabel);

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
            OptimizeTexturesAndMaterials(folderPath);
        }
    }

    private void OptimizeTexturesAndMaterials(string path)
    {
        var textureMap = new Dictionary<string, Texture2D>();
        var duplicateTextures = new List<string>();

        // Étape 1 : Trouver toutes les textures dans le dossier
        var textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
        foreach (var guid in textureGuids)
        {
            var texturePath = AssetDatabase.GUIDToAssetPath(guid);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            if (texture == null) continue;

            // Générer un hash pour la texture (comparaison basée sur le contenu)
            var hash = GetTextureHash(texture);

            if (textureMap.ContainsKey(hash))
            {
                duplicateTextures.Add(texturePath);
                ReplaceTextureInMaterials(texture, textureMap[hash]);
            }
            else
            {
                textureMap[hash] = texture;
            }
        }

        // Étape 2 : Supprimer les textures en double (et leurs fichiers .meta)
        foreach (var texturePath in duplicateTextures)
        {
            DeleteFileWithMeta(texturePath);
            Debug.Log($"Deleted duplicate texture: {texturePath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Optimization Complete.");
    }

    private string GetTextureHash(Texture2D texture)
    {
        var path = AssetDatabase.GetAssetPath(texture);
        var bytes = File.ReadAllBytes(path);
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hashBytes = md5.ComputeHash(bytes);
            return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    private void ReplaceTextureInMaterials(Texture2D oldTexture, Texture2D newTexture)
    {
        var materialGuids = AssetDatabase.FindAssets("t:Material");
        foreach (var guid in materialGuids)
        {
            var materialPath = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (material == null) continue;

            bool materialUpdated = false;
            var shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    if (material.GetTexture(propertyName) == oldTexture)
                    {
                        material.SetTexture(propertyName, newTexture);
                        materialUpdated = true;
                    }
                }
            }

            if (materialUpdated)
            {
                EditorUtility.SetDirty(material);
                Debug.Log($"Replaced texture in material: {materialPath}");
            }
        }
    }

    private void DeleteFileWithMeta(string assetPath)
    {
        string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log($"Deleted file: {fullPath}");
        }

        // Supprimer le fichier .meta associé
        string metaFilePath = fullPath + ".meta";
        if (File.Exists(metaFilePath))
        {
            File.Delete(metaFilePath);
            Debug.Log($"Deleted meta file: {metaFilePath}");
        }
    }
}
