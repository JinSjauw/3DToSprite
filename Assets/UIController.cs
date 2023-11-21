using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button buttonImportMesh = root.Q<Button>("ButtonImportMesh");
        Button buttonImportAnimation = root.Q<Button>("ButtonImportAnimation");
        Button buttonSampleAnimation = root.Q<Button>("ButtonSampleAnimation");
        Button buttonCrunch = root.Q<Button>("ButtonCrunch");
        Button buttonPlay = root.Q<Button>("ButtonPlay");

        buttonImportMesh.clicked += () => ImportMesh();
        buttonImportAnimation.clicked += () => ImportAnimation();
        buttonSampleAnimation.clicked += () => SampleAnimation();
        buttonCrunch.clicked += () => Crunch();
        buttonPlay.clicked += () => Play();
    }

    private void ImportMesh()
    {
        Debug.Log("Import Mesh");
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
