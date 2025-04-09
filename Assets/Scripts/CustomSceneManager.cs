// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.SceneManagement;
//
// public class CustomSceneManager : MonoBehaviour
// {
//     // Define your specific buttons
//     [Header("Scene Buttons")]
//     [SerializeField] private Button navigationButton;
//     [SerializeField] private Button sequenceButton;
//     [SerializeField] private Button arrowsButton;
//     
//     // Define scene names for each button
//     [Header("Scene Names")]
//     [SerializeField] private string navigationSceneName = "NavigationScene";
//     [SerializeField] private string sequenceSceneName = "SequenceScene";
//     [SerializeField] private string arrowsSceneName = "ArrowsScene";
//     
//     // Home/main scene name
//     [SerializeField] private string homeSceneName = "MainScene";
//     
//     private void Start()
//     {
//         SetupButtons();
//     }
//     
//     private void SetupButtons()
//     {
//         // Setup navigation button
//         if (navigationButton != null)
//         {
//             navigationButton.onClick.RemoveAllListeners();
//             navigationButton.onClick.AddListener(() => LoadScene(navigationSceneName));
//         }
//         
//         // Setup sequence button
//         if (sequenceButton != null)
//         {
//             sequenceButton.onClick.RemoveAllListeners();
//             sequenceButton.onClick.AddListener(() => LoadScene(sequenceSceneName));
//         }
//         
//         // Setup arrow button
//         if (arrowsButton != null)
//         {
//             arrowsButton.onClick.RemoveAllListeners();
//             arrowsButton.onClick.AddListener(() => LoadScene(arrowsSceneName));
//         }
//     }
//     
//     private void LoadScene(string sceneName)
//     {
//         SceneManager.LoadScene(sceneName);
//     }
//     
//     public void LoadHomeScene()
//     {
//         LoadScene(homeSceneName);
//     }
// }

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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

    [Header("Hover Settings")]
    [SerializeField] private float hoverActivationDelay = 1.0f; // Time in seconds to hover before activation

    // List of all scene buttons
    private List<Button> allButtons = new List<Button>();

    // Hover tracking
    private Button currentHoverButton = null;
    private float hoverStartTime = 0f;
    private bool isProcessingHover = false;
    
    private void Start()
    {
        // Initialize button list
        InitializeButtonList();
        
        // Setup button actions
        SetupButtons();
        
        // Setup hover handlers for all buttons
        SetupHoverHandlers();
    }
    
    private void Update()
    {
        // Check if we are currently hovering over a button
        if (currentHoverButton != null && isProcessingHover)
        {
            float hoverDuration = Time.time - hoverStartTime;

            // If hover duration exceeds our threshold, trigger the button
            if (hoverDuration >= hoverActivationDelay)
            {
                isProcessingHover = false; // Prevent repeated activation
                currentHoverButton.onClick.Invoke(); // Trigger the button click
            }
        }
    }
    
    private void InitializeButtonList()
    {
        // Add all buttons to the list
        if (navigationButton != null) allButtons.Add(navigationButton);
        if (sequenceButton != null) allButtons.Add(sequenceButton);
        if (arrowsButton != null) allButtons.Add(arrowsButton);
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
    
    private void SetupHoverHandlers()
    {
        // Add hover handlers to all buttons
        foreach (Button button in allButtons)
        {
            AddHoverHandlers(button);
        }
    }
    
    private void AddHoverHandlers(Button button)
    {
        // Get or add the EventTrigger component
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Add enter event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnPointerEnter(button, (PointerEventData)data); });
        trigger.triggers.Add(enterEntry);

        // Add exit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPointerExit(button, (PointerEventData)data); });
        trigger.triggers.Add(exitEntry);
    }
    
    private void OnPointerEnter(Button button, PointerEventData eventData)
    {
        // Set the currently hovered button
        currentHoverButton = button;
        hoverStartTime = Time.time;
        isProcessingHover = true;
    }
    
    private void OnPointerExit(Button button, PointerEventData eventData)
    {
        if (currentHoverButton == button)
        {
            currentHoverButton = null;
            isProcessingHover = false;
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