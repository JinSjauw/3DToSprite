using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLTFast;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private AnimationCapture capturer;
    [SerializeField] private GameObject anchorPoint;
    [SerializeField] private List<AnimationClip> animationClips;
    [SerializeField] private GameObject spritePreview;
    private bool isPreviewing;
    
    private ListView animationsListView;
    private Label debugLabel;
    private Animation animationPlayer;

    private string meshURL;
    
    //Input Fields
    private IntegerField cellSizeX;
    private IntegerField cellSizeY;

    private FloatField rotationX;
    private FloatField rotationY;

    private FloatField scaleX;
    private FloatField scaleY;
    private FloatField scaleZ;
    
    private Vector2Int cellSize;
    private Vector2 rotation;
    private Vector3 scale = Vector3.one;
    
    private void Awake()
    {
        anchorPoint.GetComponent<Animation>();
    }
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        //Top Bar
        Button saveButton = root.Q<Button>("ButtonSave"); //Save URL to glb model + Transform settings + resolution --> JSON 
        Button loadButton = root.Q<Button>("ButtonLoad"); //Load JSON file
        Button exportButton = root.Q<Button>("ButtonExport"); //Export Normal Map.PNG or maybe Material
        
        //Middle Bar
        animationsListView = root.Q<ListView>("AnimationsList");
        cellSizeX = root.Q<IntegerField>("CellSizeX");
        cellSizeY = root.Q<IntegerField>("CellSizeY");
        rotationX = root.Q<FloatField>("RotationX");
        rotationY = root.Q<FloatField>("RotationY");
        scaleX = root.Q<FloatField>("ScaleX");
        scaleY = root.Q<FloatField>("ScaleY");
        scaleZ = root.Q<FloatField>("ScaleZ");
        
        //Bottom Bar
        Button buttonImportMesh = root.Q<Button>("ButtonImportMesh");
        Button buttonCrunch = root.Q<Button>("ButtonPixelate");
        Button buttonPreview = root.Q<Button>("ButtonPreviewSprite");

        debugLabel = root.Q<Label>("DebugText");

        saveButton.clicked += () => SaveProjectData();
        loadButton.clicked += () => LoadProjectData();
        exportButton.clicked += () => Pixelate();
        
        buttonImportMesh.clicked += () => ImportMesh();
        //buttonCrunch.clicked += () => Pixelate();
        buttonPreview.clicked += () => PreviewSprite();
        
        RegisterFieldCallbacks();
        InitiateAnimationsList();
    }

    private void SaveProjectData()
    {
        ProjectData projectData = new ProjectData();
        projectData.meshURL = meshURL;
        projectData.cellSize = cellSize;
        projectData.rotation = rotation;
        projectData.scale = scale;
        
        IOHandler.SerializeProjectData(projectData);
    }

    private void LoadProjectData()
    {
        ProjectData projectData = IOHandler.DeserializeProjectData();

        CleanPrevious();
        IOHandler.ImportMesh(projectData.meshURL, anchorPoint.transform, PostLoad);
        cellSize = projectData.cellSize;
        rotation = projectData.rotation;
        scale = projectData.scale;

        cellSizeX.value = cellSize.x;
        cellSizeY.value = cellSize.y;

        rotationX.value = rotation.x;
        rotationY.value = rotation.y;

        scaleX.value = scale.x;
        scaleY.value = scale.y;
        scaleZ.value = scale.z;
    }

    private void RegisterFieldCallbacks()
    {
        //Cell Size
        cellSizeX.RegisterCallback<ChangeEvent<int>>(evt =>
        {
            cellSize.x = evt.newValue;
        });
        
        cellSizeY.RegisterCallback<ChangeEvent<int>>(evt =>
        {
            cellSize.y = evt.newValue;
        });
        
        //Rotation
        rotationX.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            rotation.x = evt.newValue;
            ApplyRotation();
        });
        
        rotationY.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            rotation.y = evt.newValue;
            ApplyRotation();
        });
        
        //Scale
        scaleX.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            scale.x = evt.newValue;
            ApplyScale();
        });
        
        scaleY.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            scale.y = evt.newValue;
            ApplyScale();
        });
        
        scaleZ.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            scale.z = evt.newValue;
            ApplyScale();
        });    
        
        //ListVew
        animationsListView.RegisterCallback<PointerLeaveEvent>(evt =>
        {
            animationsListView.ClearSelection();

            if (animationPlayer != null)
            {
                animationPlayer.Stop();
                //animationPlayer.clip = null;
            }
            
        });
    }
    private void ApplyRotation()
    {
        Quaternion newRotation = Quaternion.AngleAxis(rotation.x, Vector3.right) * Quaternion.AngleAxis(rotation.y, Vector3.up);
        anchorPoint.transform.rotation = newRotation;
    }
    private void ApplyScale()
    {
        Debug.Log(scale);
        anchorPoint.transform.localScale = scale;
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
        animationsListView.selectionChanged += SampleListAnimation;
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
        if (!obj.Any())
        {
            Debug.Log("No Animations Selected!");
            return;
        }
        
        AnimationClip clipToPlay = (AnimationClip)obj.First();

        if (animationPlayer != null)
        {
            animationPlayer.clip = clipToPlay;
            animationPlayer.Play();
        }
        else if(anchorPoint.TryGetComponent(out Animation animation))
        {
            animationPlayer = animation;
            animationPlayer.clip = clipToPlay;
            animationPlayer.Play();
        }
        else
        {
            Debug.Log("Can't Find <Animation> component on anchorPoint");
        }
        
        Debug.Log(clipToPlay.name);
    }

    private void ImportMesh()
    {
        CleanPrevious();
        IOHandler.ImportMesh(anchorPoint.transform, PostLoad);
    }

    private void CleanPrevious()
    {
        if (anchorPoint.transform.childCount > 0)
        {
            Destroy(anchorPoint.transform.GetChild(0).gameObject);
        }
        
        if (anchorPoint.TryGetComponent(out Animation animationComponent))
        {
            Destroy(animationComponent);
            animationPlayer = null;
        }
    }
    
    private void PostLoad(AnimationClip[] animations, string url)
    {
        UpdateAnimationsList(animations);
        meshURL = url;
        //Apply settings
        ApplyRotation();
        ApplyScale();
    }
    
    private void PreviewSprite()
    {
        Debug.Log("Preview Sprite");
    }

    private void Pixelate()
    {
        if (animationPlayer != null)
        {
            animationPlayer.Stop();
            capturer.Pixelate(animationPlayer.clip, anchorPoint, 60, cellSize);
        }
        else
        {
            Debug.Log("No Animation Component!");
        }
        
        
    }
}
