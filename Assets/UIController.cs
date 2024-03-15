using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLTFast;
using SFB;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject anchorPoint;
    [SerializeField] private List<AnimationClip> animationClips;
    
    private ListView animationsListView;
    private Label debugLabel;
    private Animation animationPlayer;
    
    private void OnEnable()
    {
        //Screen.fullScreenMode = FullScreenMode.Windowed;
        
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        animationsListView = root.Q<ListView>("AnimationsList");
        Button buttonImportMesh = root.Q<Button>("ButtonImportMesh");
        /*Button buttonImportAnimation = root.Q<Button>("ButtonImportAnimation");
        Button buttonSampleAnimation = root.Q<Button>("ButtonSampleAnimation");*/
        Button buttonCrunch = root.Q<Button>("ButtonCrunch");
        //Button buttonPlay = root.Q<Button>("ButtonPlay");

        debugLabel = root.Q<Label>("DebugText");
        
        buttonImportMesh.clicked += () => ImportMesh();
        /*buttonImportAnimation.clicked += () => ImportAnimation();
        buttonSampleAnimation.clicked += () => SampleAnimation();*/
        buttonCrunch.clicked += () => Crunch();
        //buttonPlay.clicked += () => Play();
        
        //Init the animationsList;
        InitiateAnimationsList();
        anchorPoint.transform.localScale *= .05f;
    }
    
    private void InitiateAnimationsList()
    {
        Func<VisualElement> makeListItem = () => new Label();
        Action<VisualElement, int> bindItem = (e, a) => (e as Label).text = animationClips[a].name;

        const int itemHeight = 16;
        
        animationsListView.itemsSource = animationClips;
        animationsListView.fixedItemHeight = itemHeight;
        animationsListView.makeItem = makeListItem;
        animationsListView.bindItem = bindItem;
        
        animationsListView.selectionType = SelectionType.Single;
        animationsListView.itemsChosen += SampleListAnimation;
        //animationsListView.selectionChanged += SampleListAnimation;
    }

    private void UpdateAnimationsList(AnimationClip[] animations)
    {
        animationClips = animations.ToList();
        
        Action<VisualElement, int> bindItem = (e, a) => (e as Label).text = animationClips[a].name;
        
        animationsListView.itemsSource = animationClips;
        animationsListView.bindItem = bindItem;
        animationsListView.RefreshItems();
    }

    private void SampleListAnimation(IEnumerable<object> obj)
    {
        AnimationClip clipToPlay = (AnimationClip)obj.First();
        
        animationPlayer.clip = clipToPlay;
        animationPlayer.Play();
        
        Debug.Log(clipToPlay.name);
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

        if (anchorPoint.transform.childCount > 0)
        {
            Destroy(anchorPoint.transform.GetChild(0).gameObject);
        }
        
        if (animationPlayer != null)
        {
            Destroy(animationPlayer);
            animationPlayer = null;
        }
        
        byte[] data = File.ReadAllBytes(path);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri("file://" + path));
        
        debugLabel.text = "Progress: " + success;
        
        if (success)
        {
            debugLabel.text = "success? : " + gltf.LoadingDone;
            var material = gltf.GetMaterial();
            debugLabel.text = "Material: " + material;
            success = await gltf.InstantiateMainSceneAsync(anchorPoint.transform);

            animationPlayer = anchorPoint.GetComponent<Animation>();
            UpdateAnimationsList(gltf.GetAnimationClips());
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
