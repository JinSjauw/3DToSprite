using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using GLTFast;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject spawnPoint;
    private Label debugLabel;
    
    private void OnEnable()
    {
        //Screen.fullScreenMode = FullScreenMode.Windowed;
        
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button buttonImportMesh = root.Q<Button>("ButtonImportMesh");
        Button buttonImportAnimation = root.Q<Button>("ButtonImportAnimation");
        Button buttonSampleAnimation = root.Q<Button>("ButtonSampleAnimation");
        Button buttonCrunch = root.Q<Button>("ButtonCrunch");
        Button buttonPlay = root.Q<Button>("ButtonPlay");

        debugLabel = root.Q<Label>("DebugText");
        
        buttonImportMesh.clicked += () => ImportMesh();
        buttonImportAnimation.clicked += () => ImportAnimation();
        buttonSampleAnimation.clicked += () => SampleAnimation();
        buttonCrunch.clicked += () => Crunch();
        buttonPlay.clicked += () => Play();
    }

    private void ImportMesh()
    {
        Debug.Log("Import Mesh");
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", "glb", false);
        if (paths.Length > 0) {
            Debug.Log("Path: " + paths[0]);
            
            //Load in GTLF ASSET
            /*GltfAsset gltfAsset;

            if (spawnPoint.AddComponent<GltfAsset>())
            {
                gltfAsset = spawnPoint.GetComponent<GltfAsset>();
                gltfAsset.Url = "file://" + paths[0];
                debugLabel.text = "GtlfAsset Loaded! on: " + gltfAsset.gameObject.name + "\n";
                debugLabel.text += "Path:" + paths[0];
            }
            */
            debugLabel.text = "Loading Model from URI";
            
            spawnPoint.transform.localScale *= .05f;
            LoadFromMemory(paths[0]);
            //StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
    
    /*private IEnumerator OutputRoutine(string url) {
        var loader = new WWW(url);
        yield return loader;
        //output.texture = loader.texture;
    }*/

    private async void LoadFromMemory(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri("file://" + path));
        debugLabel.text = "Progress: " + success;
        if (success)
        {
            debugLabel.text = "success? : " + gltf.LoadingDone;
            var material = gltf.GetMaterial();
            debugLabel.text = "Material: " + material;
            //Need to write an customInstantiator
            success = await gltf.InstantiateMainSceneAsync(spawnPoint.transform);
        }
    }
    
    private void ImportAnimation()
    {
        Debug.Log("Import Animation");
    }
    
    private void SampleAnimation()
    {
        Debug.Log("Sample Animation");
    }

    private void Crunch()
    {
        Debug.Log("Crunch");
    }

    private void Play()
    {
        Debug.Log("Play");
    }
}
