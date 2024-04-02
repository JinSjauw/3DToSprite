using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationCapture : MonoBehaviour
{
    [SerializeField] private AnimationClip sourceClip;
    [SerializeField] private GameObject animationTarget;
    [SerializeField] private int frameRate;
    [SerializeField] private Camera captureCamera;
    [SerializeField] private Shader viewSpaceNormal;
    [SerializeField] private Vector2Int cellSize = new Vector2Int(100, 100);
    
    // Start is called before the first frame update
    public void Pixelate(AnimationClip clip, GameObject target, int frames, Vector2Int size)
    {
     Debug.Log("Amount of frames" + (int)(sourceClip.length * frameRate));
     sourceClip = clip;
     animationTarget = target;
     frameRate = frames;
     cellSize = size;
     
     StartCoroutine(CaptureAnimation(IOHandler.SaveCapture));
    }

    private IEnumerator CaptureAnimation(Action<Texture2D, Texture2D> onComplete)
    {
        int numberOfFrames = (int)(sourceClip.length * frameRate);
        var cellCount = Mathf.CeilToInt(Mathf.Sqrt(numberOfFrames));
        
        Vector2Int atlasSize = new Vector2Int(cellSize.x * cellCount, cellSize.y * cellCount);
        Vector2Int atlasPos = new Vector2Int(0, atlasSize.y - cellSize.y);
        
        Texture2D diffuseMap = new Texture2D(atlasSize.x, atlasSize.y, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        ClearAtlas(diffuseMap, Color.clear);

        Texture2D normalMap = new Texture2D(atlasSize.x, atlasSize.y, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        ClearAtlas(normalMap, new Color(0.5f, 0.5f, 1.0f, 0.0f));
        
        RenderTexture rtFrame = new RenderTexture(cellSize.x, cellSize.y, 24, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Point,
            antiAliasing = 1,
            hideFlags = HideFlags.HideAndDontSave
        };

        Shader normalShader = viewSpaceNormal;
        
        captureCamera.targetTexture = rtFrame;
        Color cachedCameraColor = captureCamera.backgroundColor;
        try
        {
            for (int currentFrame = 0; currentFrame < numberOfFrames; currentFrame++)
            {
                float currentTime = (currentFrame / (float)numberOfFrames) * sourceClip.length;
                sourceClip.SampleAnimation(animationTarget, currentTime);
                yield return null;
            
                captureCamera.backgroundColor = Color.clear;
                captureCamera.Render();
                Graphics.SetRenderTarget(rtFrame);
                diffuseMap.ReadPixels(new Rect(0, 0, rtFrame.width, rtFrame.height), atlasPos.x, atlasPos.y);
                diffuseMap.Apply();

                captureCamera.backgroundColor = new Color(0.5f, 0.5f, 1.0f, 0.0f);
                captureCamera.RenderWithShader(normalShader, "");
                //Override materials of target
                Graphics.SetRenderTarget(rtFrame);
                normalMap.ReadPixels(new Rect(0, 0, rtFrame.width, rtFrame.height), atlasPos.x, atlasPos.y);
                normalMap.Apply();
            
                atlasPos.x += cellSize.x;

                if ((currentFrame + 1) % cellCount == 0)
                {
                    atlasPos.x = 0;
                    atlasPos.y -= cellSize.y;
                }
            }
            onComplete.Invoke(diffuseMap, normalMap);
        }
        finally
        {
            Graphics.SetRenderTarget(null);
            captureCamera.targetTexture = null;
            captureCamera.backgroundColor = cachedCameraColor;
            DestroyImmediate(rtFrame);
        }
    }
    
    private void ClearAtlas(Texture2D texture, Color color)
    {
        var pixels = new Color[texture.width * texture.height];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    /*private void SaveCapture(Texture2D diffuseMap, Texture2D normalMap)
    {
        var fileName = Path.GetFileNameWithoutExtension("CharacterTest");
        var directory = Application.dataPath;
        var diffusePath = string.Format("{0}/{1}{2}.{3}", directory, fileName, "DiffuseMap", "png");
        var normalPath = string.Format("{0}/{1}{2}.{3}", directory, fileName, "NormalMap", "png");

        File.WriteAllBytes(diffusePath, diffuseMap.EncodeToPNG());
        File.WriteAllBytes(normalPath, normalMap.EncodeToPNG());

        Debug.Log("DiffuseMap: " + diffusePath + " NormalMap: " + normalPath);
        
        AssetDatabase.Refresh();
    }*/

    private void CreatePreviewMaterial(Texture2D diffuseMap, Texture2D normalMap)
    {
        
    }
}
