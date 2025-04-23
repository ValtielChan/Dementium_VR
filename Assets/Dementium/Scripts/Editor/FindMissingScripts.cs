using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<FindMissingScripts>("Find Missing Scripts");
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in Scene"))
        {
            FindMissingScriptsInScene();
        }
    }

    private void FindMissingScriptsInScene()
    {
        int missingCount = 0;
        
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.scene.isLoaded)
            {
                Component[] components = obj.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingCount++;
                        string path = GetGameObjectPath(obj.transform);
                        Debug.Log($"Script manquant sur l'objet: {path}", obj);
                        Selection.activeObject = obj;
                    }
                }
            }
        }
        
        Debug.Log($"Recherche terminée. {missingCount} scripts manquants trouvés.");
    }

    private string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}