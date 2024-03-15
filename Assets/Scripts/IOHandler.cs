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
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
        Debug.Log(path);
        AssetDatabase.Refresh();
    }

    public static ProjectData LoadProjectData()
    {
        //open the filebrowser
        ProjectData projectData = new ProjectData();

        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "json", false);
        if (paths.Length > 0)
        {
            Debug.Log("Path: " + paths[0]);
            string json = File.ReadAllText(paths[0]);
            projectData = JsonUtility.FromJson<ProjectData>(json);
        }
        //projectData = JsonUtility.FromJson<ProjectData>()
        //Have to get the json string back from a file stream;
        return projectData;
    }
    
    public static void ImportMesh(Transform anchorPoint, Action<AnimationClip[], string> onSuccess)
    {
        Debug.Log("Import Mesh");
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "glb", false);
        if (paths.Length > 0) {
            Debug.Log("Path: " + paths[0]);
            //debugLabel.text = "Loading Model from URI";
            
            LoadFromMemory(paths[0], anchorPoint, onSuccess);
        }
    }

    public static void ImportMesh(string path, Transform anchorPoint, Action<AnimationClip[], string> onSuccess)
    {
        LoadFromMemory(path, anchorPoint, onSuccess);
    }
    
    private static async void LoadFromMemory(string path, Transform anchorPoint, Action<AnimationClip[], string> onSuccess)
    {
        byte[] data = File.ReadAllBytes(path);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri("file://" + path));
        
        //debugLabel.text = "Progress: " + success;
        
        if (success)
        {
            onSuccess(gltf.GetAnimationClips(), path);
            success = await gltf.InstantiateMainSceneAsync(anchorPoint);
        }
    }
    
    public static void SaveCapture(Texture2D diffuseMap, Texture2D normalMap)
    {
        var fileName = Path.GetFileNameWithoutExtension("CharacterTest");
        var directory = Application.dataPath;
        var diffusePath = string.Format("{0}/{1}{2}.{3}", directory, fileName, "DiffuseMap", "png");
        var normalPath = string.Format("{0}/{1}{2}.{3}", directory, fileName, "NormalMap", "png");

        File.WriteAllBytes(diffusePath, diffuseMap.EncodeToPNG());
        File.WriteAllBytes(normalPath, normalMap.EncodeToPNG());

        Debug.Log("DiffuseMap: " + diffusePath + " NormalMap: " + normalPath);
        
        AssetDatabase.Refresh();
    }
}
