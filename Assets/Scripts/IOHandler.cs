using System;
using System.IO;
using GLTFast;
using SFB;
using UnityEditor;
using UnityEngine;

public static class IOHandler
{
    public static void SerializeProjectData(ProjectData data)
    {
        var extensionFilter = new[]
        {
            new ExtensionFilter("JSON", "json"),
        };
        
        var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "ProjectData", extensionFilter);
        
        if(path.Length <= 0) return;
        
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
        Debug.Log(path);

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
        
    }

    public static ProjectData DeserializeProjectData()
    {
        //open the filebrowser
        ProjectData projectData = new ProjectData();

        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "json", false);
        if (paths.Length <= 0) return null;
        
        Debug.Log("Path: " + paths[0]);
        string json = File.ReadAllText(paths[0]);
        projectData = JsonUtility.FromJson<ProjectData>(json);
        
        //projectData = JsonUtility.FromJson<ProjectData>()
        //Have to get the json string back from a file stream;
        return projectData;
    }
    
    public static void ImportMesh(Transform anchorPoint, Action<AnimationClip[], string> onSuccess)
    {
        Debug.Log("Import Mesh");
        var paths = StandaloneFileBrowser.OpenFilePanel("Select .GLB", "", "glb", false);
        if (paths.Length > 0) {
            Debug.Log("Path: " + paths[0]);
            
            LoadFromMemory(paths[0], anchorPoint, onSuccess);
        }
    }

    public static void ImportMesh(string path, Transform anchorPoint, Action<AnimationClip[], string> onSuccess)
    {
        LoadFromMemory(path, anchorPoint, onSuccess);
    }
    
    private static async void LoadFromMemory(string path, Transform anchorPoint, Action<AnimationClip[], string> onSuccess)
    {
        if (path.Length <= 0)
        {
            return;
        }
        
        byte[] data = File.ReadAllBytes(path);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri("file://" + path));
        //debugLabel.text = "Progress: " + success;
        
        if (success)
        {
            success = await gltf.InstantiateMainSceneAsync(anchorPoint);
            onSuccess(gltf.GetAnimationClips(), path);
        }
    }
    
    public static void SaveCapture(Texture2D diffuseMap, Texture2D normalMap)
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save Files", Application.dataPath, "SpriteSheet", "");

        var diffusePath2 = string.Format("{0}{1}.{2}", path, "DiffuseMap", ".png");
        var normalPath2 = string.Format("{0}{1}.{2}", path, "NormalMap", ".png");
        
        File.WriteAllBytes(diffusePath2, diffuseMap.EncodeToPNG());
        File.WriteAllBytes(normalPath2, normalMap.EncodeToPNG());

        Debug.Log("DiffuseMap: " + diffusePath2 + " NormalMap: " + normalPath2);
        
        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
        
    }
}
