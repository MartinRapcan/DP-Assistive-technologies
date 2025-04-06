using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InteractionTracker : MonoBehaviour
{
    // Dictionary to store interaction types and their counts
    private Dictionary<string, int> _interactionCounts = new Dictionary<string, int>();
    
    // References to UI elements (assign these in the inspector)
    [Header("UI References")]
    [SerializeField] private TMP_Text interactionCountText;
    [SerializeField] private RawImage cameraView;
    [SerializeField] private Camera displayCamera;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button toggleButton;
    
    // Remove this field since we'll access the text component directly from the button
    // [SerializeField] private TMP_Text toggleButtonText;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverActivationDelay = 1.0f; // Time in seconds for hover activation
    
    private int _numberOfCollisions = 0;
    private bool _isPanelVisible = false;
    private bool _buttonHovered = false;
    private float _hoverStartTime = 0f;
    private TMP_Text _toggleButtonText; // Private field to cache the button's text component
    
    // Tracking state properties
    public bool hasStarted { get; set; } = false;
    public bool hasEnded { get; set; } = false;
    
    // Time tracking
    private float _startTime = 0f;
    private float _endTime = 0f;
    private float _totalTime = 0f;
    
    private void Start()
    {
        // Only canvas should be active initially, everything else hidden
        if (canvas != null)
        {
            canvas.SetActive(true);
        }
        
        // Hide the panel initially
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        // Hide the toggle button initially (will show only when tracking ends)
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(false);
            
            // Set up toggle button click handler
            toggleButton.onClick.AddListener(TogglePanel);
            
            // Set up hover handling for the button
            SetupButtonHover(toggleButton);
            
            // Get the TextMeshPro component from the button's children
            _toggleButtonText = toggleButton.GetComponentInChildren<TMP_Text>();
            
            // Set initial button text
            if (_toggleButtonText != null)
            {
                _toggleButtonText.text = "Show";
            }
        }
        
        // Make sure camera view is hidden initially
        if (cameraView != null)
        {
            cameraView.gameObject.SetActive(false);
        }
        
        // Make sure interaction text is hidden initially
        if (interactionCountText != null)
        {
            interactionCountText.gameObject.SetActive(false);
        }
        
        // Initialize the dictionary
        _interactionCounts = new Dictionary<string, int>();
        
        // No automatic tracking start - will be triggered when hasStarted is set to true
    }

    private bool _wasStarted = false; // To track state change

    private void Update()
    {
        // Check if tracking has just started
        if (!_wasStarted && hasStarted)
        {
            _startTime = Time.time;
            _wasStarted = true;
        }
        
        // Check if tracking is in progress to update total time
        if (hasStarted && !hasEnded)
        {
            _totalTime = Time.time - _startTime;
        }
        
        // Check if tracking has ended to show ONLY the toggle button
        if (hasStarted && hasEnded && toggleButton != null && !toggleButton.gameObject.activeSelf)
        {
            toggleButton.gameObject.SetActive(true);
        }
        
        // Handle hover activation
        if (_buttonHovered && toggleButton != null && toggleButton.gameObject.activeSelf)
        {
            float hoverDuration = Time.time - _hoverStartTime;
            if (hoverDuration >= hoverActivationDelay)
            {
                _buttonHovered = false; // Reset hover to prevent multiple activations
                TogglePanel();
            }
        }
    }
    
    private void SetupButtonHover(Button button)
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
        enterEntry.callback.AddListener((data) => { OnButtonHoverEnter(); });
        trigger.triggers.Add(enterEntry);

        // Add exit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnButtonHoverExit(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnButtonHoverEnter()
    {
        _buttonHovered = true;
        _hoverStartTime = Time.time;
    }

    private void OnButtonHoverExit()
    {
        _buttonHovered = false;
    }
    
    // Toggle panel visibility and update button text
    public void TogglePanel()
    {
        _isPanelVisible = !_isPanelVisible;
        
        // Toggle panel visibility
        if (panel != null)
        {
            panel.SetActive(_isPanelVisible);
        }
        
        // Toggle text visibility
        if (interactionCountText != null)
        {
            interactionCountText.gameObject.SetActive(_isPanelVisible);
        }
        
        // Toggle camera view visibility
        if (cameraView != null)
        {
            cameraView.gameObject.SetActive(_isPanelVisible);
        }
        
        // Update toggle button text
        if (_toggleButtonText != null)
        {
            _toggleButtonText.text = _isPanelVisible ? "Hide" : "Show";
        }
        
        // If showing the panel, update the display
        if (_isPanelVisible)
        {
            UpdateCameraView();
            UpdateInteractionDisplay();
        }
    }
    
    // Method to track interaction with optional speed modifier
    // speed: 0 = default, 1 = Slow, 2 = Fast
    public void SetInteractionType(string interactionType, int speed = 0)
    {
        if (!hasStarted || hasEnded) return;
        
        // Determine key based on speed parameter
        string key = interactionType;
        
        if (speed == 1)
        {
            key = interactionType + "Slow";
        }
        else if (speed == 2)
        {
            key = interactionType + "Fast";
        }
        
        // Dictionary.TryGetValue approach - compatible with all C# versions
        int currentCount;
        if (_interactionCounts.TryGetValue(key, out currentCount))
        {
            // Key exists, increment count
            _interactionCounts[key] = currentCount + 1;
        }
        else
        {
            // Key doesn't exist, add with count 1
            _interactionCounts.Add(key, 1);
        }
    }
    
    // Increment collision counter
    public void IncrementNumberOfCollisions()
    {
        if (!hasStarted || hasEnded) return;
        _numberOfCollisions++;
    }
    
    // You can remove this method if you're setting hasStarted directly
    // or keep it as a convenience method
    public void StartTracking()
    {
        hasStarted = true;
        hasEnded = false;
    }
    
    // Method to end tracking and show ONLY the toggle button
    public void EndTracking()
    {
        if (hasStarted && !hasEnded)
        {
            hasEnded = true;
            _endTime = Time.time;
            _totalTime = _endTime - _startTime;
        }
        
        // Make sure ONLY toggle button is visible
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(true);
        }
        
        // Make sure panel remains hidden until toggled
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        // Make sure text remains hidden until toggled
        if (interactionCountText != null)
        {
            interactionCountText.gameObject.SetActive(false);
        }
        
        // Make sure camera view remains hidden until toggled
        if (cameraView != null)
        {
            cameraView.gameObject.SetActive(false);
        }
    }
    
    // Update text display with current interactions
    private void UpdateInteractionDisplay()
    {
        if (interactionCountText != null)
        {
            string displayText = $"Total Collisions: {_numberOfCollisions}\n";
            
            // Add the time information
            displayText += $"Time: {FormatTime(_totalTime)}\n";
            
            foreach (var interaction in _interactionCounts)
            {
                displayText += $"{interaction.Key}: {interaction.Value}\n";
            }
            
            interactionCountText.text = displayText;
        }
    }
    
    // Helper method to format time in minutes and seconds
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    // Update camera view on canvas
    private void UpdateCameraView()
    {
        if (displayCamera != null && cameraView != null)
        {
            // Create render texture
            int width = 512;
            int height = 512;
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            
            // Set camera to render to this texture
            displayCamera.targetTexture = renderTexture;
            
            // Assign the render texture to the UI RawImage
            cameraView.texture = renderTexture;
        }
    }
    
    // Reset all tracking data
    public void ResetTracker()
    {
        // Reset states
        hasStarted = false;
        hasEnded = false;
        _wasStarted = false;
        _isPanelVisible = false;
        
        // Reset time tracking
        _startTime = 0f;
        _endTime = 0f;
        _totalTime = 0f;
        
        // Clear data
        _interactionCounts.Clear();
        _numberOfCollisions = 0;
        
        // Hide everything except canvas
        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(false);
        }
        
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        if (interactionCountText != null)
        {
            interactionCountText.gameObject.SetActive(false);
        }
        
        if (cameraView != null)
        {
            cameraView.gameObject.SetActive(false);
        }
        
        // Reset toggle button text
        if (_toggleButtonText != null)
        {
            _toggleButtonText.text = "Show";
        }
    }
    
    // Get interaction count for a specific type
    public int GetInteractionCount(string interactionType)
    {
        if (_interactionCounts.ContainsKey(interactionType))
        {
            return _interactionCounts[interactionType];
        }
        return 0;
    }
    
    // Get total number of collisions
    public int GetTotalCollisions()
    {
        return _numberOfCollisions;
    }
    
    // Get dictionary of all interactions (for display in other scripts)
    public Dictionary<string, int> GetAllInteractions()
    {
        return _interactionCounts;
    }
}