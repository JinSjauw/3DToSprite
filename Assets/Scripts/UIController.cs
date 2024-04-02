using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private AnimationCapture capturer;
    [SerializeField] private Shader unlitShader;
    [SerializeField] private GameObject anchorPoint;
    [SerializeField] private List<AnimationClip> animationClips;
    [SerializeField] private GameObject spritePreview;

    private Animation animationPlayer;
    
    private bool isPreviewingSprite;
    private bool isPreviewingAnimation;
    private bool isDragging;
    
    private ListView animationsListView;
    private Label debugLabel;
    
    private Label selectedAnimation;
    private FloatField selectedField;
    
    private string meshURL;
    
    //Input Fields
    private IntegerField frameAmount;
    
    private IntegerField cellSizeX;
    private IntegerField cellSizeY;

    private FloatField rotationX;
    private FloatField rotationY;

    private FloatField scaleX;
    private FloatField scaleY;
    private FloatField scaleZ;
    
    //Drag Handlers - Rotation
    private VisualElement dragRotX;
    private VisualElement dragRotY;
    
    //Drag Handlers - Scale
    private VisualElement dragScaleX;
    private VisualElement dragScaleY;
    private VisualElement dragScaleZ;
    
    //Preview Button's
    private Button buttonAnimationPreview;
    private Button buttonSpritePreview;

    private int frames;
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
        selectedAnimation = root.Q<Label>("SelectedAnimationTxt");

        frameAmount = root.Q<IntegerField>("FrameAmount");
        
        cellSizeX = root.Q<IntegerField>("CellSizeX");
        cellSizeY = root.Q<IntegerField>("CellSizeY");
        
        rotationX = root.Q<FloatField>("RotationX");
        rotationY = root.Q<FloatField>("RotationY");
        
        scaleX = root.Q<FloatField>("ScaleX");
        scaleY = root.Q<FloatField>("ScaleY");
        scaleZ = root.Q<FloatField>("ScaleZ");

        //Drag Handlers
        dragRotX = root.Q<VisualElement>("DragRotX");
        dragRotY = root.Q<VisualElement>("DragRotY");
        
        dragScaleX = root.Q<VisualElement>("DragScaleX");
        dragScaleY = root.Q<VisualElement>("DragScaleY");
        dragScaleZ = root.Q<VisualElement>("DragScaleZ");
        
        //Bottom Bar
        Button buttonLoadMesh = root.Q<Button>("ButtonLoadMesh");
        buttonAnimationPreview = root.Q<Button>("ButtonPreviewAnimation");
        buttonSpritePreview = root.Q<Button>("ButtonPreviewSprite");

        debugLabel = root.Q<Label>("DebugText");

        saveButton.clicked += () => SaveProjectData();
        loadButton.clicked += () => LoadProjectData();
        exportButton.clicked += () => Pixelate();
        
        buttonLoadMesh.clicked += () => LoadMesh();
        buttonAnimationPreview.clicked += () => PreviewAnimation();
        buttonSpritePreview.clicked += () => PreviewSprite();
        
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

        if (projectData == null) return;
        
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
    
    private void CaptureMouse(MouseDownEvent evt)
    {
        Debug.Log("Mouse down on: " + evt.target);
        evt.target.CaptureMouse();
        isDragging = true;
    }

    private void ReleaseMouse(MouseUpEvent evt)
    {
        Debug.Log("Mouse down up: " + evt.target);
        evt.target.ReleaseMouse();
        isDragging = false;
    }

    #region Helper Functions

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

    private float Mod(float value, float modulo)
    {
        return ((value % modulo) + modulo) % modulo;
    }
    
    #endregion

    #region Callbacks
    private void RegisterFieldCallbacks()
    {
        frameAmount.RegisterCallback<ChangeEvent<int>>(evt =>
        {
            frames = evt.newValue;
        });
        
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
            rotation.x = Mod(evt.newValue, 360);
            rotationX.value = rotation.x;
            ApplyRotation();
        });
        
        rotationY.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            rotation.y = evt.newValue;
            ApplyRotation();
        });
        
        dragRotX.RegisterCallback<MouseDownEvent>(CaptureMouse);
        dragRotX.RegisterCallback<MouseUpEvent>(ReleaseMouse);
        dragRotX.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if(!isDragging) return;
            float value = rotation.x;
            value += evt.mouseDelta.x * 0.75f;
            rotation.x = Mod(value, 360);
            rotationX.value = rotation.x;
            ApplyRotation();
        });
        
        dragRotY.RegisterCallback<MouseDownEvent>(CaptureMouse);
        dragRotY.RegisterCallback<MouseUpEvent>(ReleaseMouse);
        dragRotY.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if(!isDragging) return;
            float value = rotation.y;
            value += evt.mouseDelta.x * 0.75f;
            rotation.y = Mod(value, 360);
            rotationY.value = rotation.y;
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
        
        dragScaleX.RegisterCallback<MouseDownEvent>(CaptureMouse);
        dragScaleX.RegisterCallback<MouseUpEvent>(ReleaseMouse);
        dragScaleX.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if(!isDragging) return;
            scale.x += evt.mouseDelta.x * 0.15f;
            scaleX.value = scale.x;
            ApplyScale();
        });
        
        dragScaleY.RegisterCallback<MouseDownEvent>(CaptureMouse);
        dragScaleY.RegisterCallback<MouseUpEvent>(ReleaseMouse);
        dragScaleY.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if(!isDragging) return;
            scale.y += evt.mouseDelta.x * 0.15f;
            scaleY.value = scale.y;
            ApplyScale();
        });
        
        dragScaleZ.RegisterCallback<MouseDownEvent>(CaptureMouse);
        dragScaleZ.RegisterCallback<MouseUpEvent>(ReleaseMouse);
        dragScaleZ.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if(!isDragging) return;
            scale.z += evt.mouseDelta.x * 0.15f;
            scaleZ.value = scale.z;
            ApplyScale();
        });
        
        //ListVew
        animationsListView.RegisterCallback<PointerLeaveEvent>(evt =>
        {
            animationsListView.ClearSelection();

            if (animationPlayer != null && !isPreviewingAnimation)
            {
                animationPlayer.Stop();
                //animationPlayer.clip = null;
            }
            
        });
    }
    
    private void PostLoad(AnimationClip[] animations, string url)
    {
        UpdateAnimationsList(animations);
        meshURL = url;
        //Apply settings
        ApplyRotation();
        ApplyScale();

        if (animationPlayer == null)
        {
            animationPlayer = anchorPoint.GetComponent<Animation>();
        }
        
        animationPlayer.clip = animations[0];
        
        //Go through all materials and set shader to unlit
        Renderer[] renderers = anchorPoint.GetComponentsInChildren<Renderer>();
        Debug.Log("Finding renderers: " + renderers.Length);
        foreach (var renderer in renderers)
        {
            Debug.Log("Finding shader");
            renderer.sharedMaterial.shader = unlitShader;
        }
    }

    #endregion
    
    #region UI Functions
    
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
        animationsListView.itemsChosen += SampleAnimation;
        animationsListView.selectionChanged += SampleAnimation;
    }
    private void UpdateAnimationsList(AnimationClip[] animations)
    {
        animationClips = animations.ToList();
        
        Action<VisualElement, int> bindItem = (e, a) => (e as Label).text = animationClips[a].name;
        
        animationsListView.itemsSource = animationClips;
        animationsListView.bindItem = bindItem;
        animationsListView.RefreshItems();
    }
    private void SampleAnimation(IEnumerable<object> obj)
    {
        if (!obj.Any())
        {
            Debug.Log("No Animations Selected!");
            return;
        }
        
        AnimationClip clipToPlay = (AnimationClip)obj.First();
        selectedAnimation.text = "Selected Anim: " + clipToPlay.name;
        
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
    }
    private void LoadMesh()
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
    private void PreviewAnimation()
    {
        isPreviewingAnimation = !isPreviewingAnimation;
        buttonAnimationPreview.text = isPreviewingAnimation ? "Stop Preview" : "Preview Animation";

        if (animationPlayer == null) return;
        if(animationPlayer.clip == null) return;
        
        if (isPreviewingAnimation)
        {
            animationPlayer.Play();
        }
        else
        {
            animationPlayer.Stop();
        }
    }
    private void PreviewSprite()
    {
        isPreviewingSprite = !isPreviewingSprite;
        buttonSpritePreview.text = isPreviewingSprite ? "Stop Preview" : "Preview Animation";

        spritePreview.SetActive(isPreviewingSprite);
    }
    private void Pixelate()
    {
        if (animationPlayer != null)
        {
            animationPlayer.Stop();
            capturer.Pixelate(animationPlayer.clip, anchorPoint, frames, cellSize);
        }
        else
        {
            Debug.Log("No Animation Component!");
        }
    }
    
    #endregion

}
