using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    // Define your specific buttons
    [Header("Scene Buttons")]
    [SerializeField] private Button navigationButton;
    [SerializeField] private Button sequenceButton;
    [SerializeField] private Button arrowsButton;
    
    // Define scene names for each button
    [Header("Scene Names")]
    [SerializeField] private string navigationSceneName = "NavigationScene";
    [SerializeField] private string sequenceSceneName = "SequenceScene";
    [SerializeField] private string arrowsSceneName = "ArrowsScene";
    
    // Home/main scene name
    [SerializeField] private string homeSceneName = "MainScene";
    
    private void Start()
    {
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        // Setup navigation button
        if (navigationButton != null)
        {
            navigationButton.onClick.RemoveAllListeners();
            navigationButton.onClick.AddListener(() => LoadScene(navigationSceneName));
        }
        
        // Setup sequence button
        if (sequenceButton != null)
        {
            sequenceButton.onClick.RemoveAllListeners();
            sequenceButton.onClick.AddListener(() => LoadScene(sequenceSceneName));
        }
        
        // Setup arrow button
        if (arrowsButton != null)
        {
            arrowsButton.onClick.RemoveAllListeners();
            arrowsButton.onClick.AddListener(() => LoadScene(arrowsSceneName));
        }
    }
    
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void LoadHomeScene()
    {
        LoadScene(homeSceneName);
    }
}
