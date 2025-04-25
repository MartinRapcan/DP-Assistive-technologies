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
    [Header("UI References")] [SerializeField]
    private TMP_Text interactionCountText;

    [SerializeField] private RawImage cameraView;
    [SerializeField] private Camera displayCamera;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button toggleButton;

    // Remove this field since we'll access the text component directly from the button
    // [SerializeField] private TMP_Text toggleButtonText;

    [Header("Hover Settings")] [SerializeField]
    private float hoverActivationDelay = 1.0f; // Time in seconds for hover activation

    [Header("Scene Name for Files")] [SerializeField]
    private string sceneName = "";

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
    private float _totalTime = 0f;
    private bool _dataSaved = false;

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

        // Check if tracking has ended to show ONLY the toggle button
        if (hasStarted && hasEnded && toggleButton != null && !toggleButton.gameObject.activeSelf)
        {
            // Calculate total time directly when needed
            _totalTime = Time.time - _startTime;
            // Debug.Log($"Tracking ended at: {Time.time}, Total time: {_totalTime}, Tracking started at: {_startTime}");

            toggleButton.gameObject.SetActive(true);

            // Save data when the toggle button first appears
            if (!_dataSaved)
            {
                SaveToJSON();
                SaveToPNG();
                _dataSaved = true;
            }
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
    private void TogglePanel()
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

        if (speed == 1 && interactionType != Direction.Stop.ToString())
        {
            key = interactionType + "Slow";
        }
        else if (speed == 2 && interactionType != Direction.Stop.ToString())
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
        var minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        var seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    // Update camera view on canvas
    private void UpdateCameraView()
    {
        if (displayCamera != null && cameraView != null)
        {
            // Make sure the camera renders to its assigned texture
            displayCamera.Render();

            // Assign the camera's render texture to the UI RawImage
            cameraView.texture = displayCamera.targetTexture;
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
        _totalTime = 0f;
        _dataSaved = false;

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

    // // Save camera view as PNG
    // public void SaveToPNG()
    // {
    //     if (displayCamera == null) return;
    //
    //     try
    //     {
    //         // Use the existing RenderTexture already assigned to the camera
    //         RenderTexture currentRT = displayCamera.targetTexture;
    //     
    //         if (currentRT == null)
    //         {
    //             Debug.LogError("No RenderTexture assigned to camera");
    //             return;
    //         }
    //     
    //         // Make sure the camera renders to the current texture
    //         displayCamera.Render();
    //     
    //         // Read the pixels from the render texture
    //         Texture2D screenshot = new Texture2D(currentRT.width, currentRT.height, TextureFormat.RGBA32, false);
    //         RenderTexture.active = currentRT;
    //         screenshot.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
    //         screenshot.Apply();
    //     
    //         // Reset the active render texture
    //         RenderTexture.active = null;
    //     
    //         // Save as PNG
    //         byte[] bytes = screenshot.EncodeToPNG();
    //         string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //         string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"screenshot_{timestamp}_{sceneName}.png");
    //         System.IO.File.WriteAllBytes(filePath, bytes);
    //     
    //         // Cleanup
    //         Destroy(screenshot);
    //     
    //         Debug.Log($"Camera view saved to: {filePath}");
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"Failed to save camera view: {e.Message}");
    //     }
    // }

    [System.Serializable]
    public class InteractionDataToSave
    {
        public int totalCollisions;
        public float totalTime;
        public string formattedTime;
        public string[] interactionTypes;
        public int[] interactionCounts;
    }


    // // Save interaction data as JSON
    // public void SaveToJSON()
    // {
    //     try
    //     {
    //         // Convert dictionary to arrays for serialization
    //         string[] types = new string[_interactionCounts.Count];
    //         int[] counts = new int[_interactionCounts.Count];
    //
    //         int index = 0;
    //         foreach (var kvp in _interactionCounts)
    //         {
    //             types[index] = kvp.Key;
    //             counts[index] = kvp.Value;
    //             index++;
    //         }
    //
    //         // Create the data object
    //         InteractionDataToSave data = new InteractionDataToSave
    //         {
    //             totalCollisions = _numberOfCollisions,
    //             totalTime = _totalTime,
    //             formattedTime = FormatTime(_totalTime),
    //             interactionTypes = types,
    //             interactionCounts = counts
    //         };
    //
    //         // Convert to JSON
    //         string json = UnityEngine.JsonUtility.ToJson(data, true);
    //
    //         // Create a filename with timestamp
    //         string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //         string filePath = System.IO.Path.Combine(Application.persistentDataPath, $"interactions_{timestamp}_{sceneName}.json");
    //
    //         // Write to file
    //         System.IO.File.WriteAllText(filePath, json);
    //
    //         Debug.Log($"Interaction data saved to: {filePath}");
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"Failed to save interaction data: {e.Message}");
    //     }
    // }

    // Save camera view as PNG
    public void SaveToPNG()
    {
        if (displayCamera == null) return;

        try
        {
            // Use the existing RenderTexture already assigned to the camera
            RenderTexture currentRT = displayCamera.targetTexture;

            if (currentRT == null)
            {
                Debug.LogError("No RenderTexture assigned to camera");
                return;
            }

            // Make sure the camera renders to the current texture
            displayCamera.Render();

            // Read the pixels from the render texture
            Texture2D screenshot = new Texture2D(currentRT.width, currentRT.height, TextureFormat.RGBA32, false);
            RenderTexture.active = currentRT;
            screenshot.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
            screenshot.Apply();

            // Reset the active render texture
            RenderTexture.active = null;

            // Encode to PNG (still on main thread as it requires Unity objects)
            byte[] bytes = screenshot.EncodeToPNG();

            // Create filename
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath,
                $"screenshot_{timestamp}_{sceneName}.png");

            // Write to disk on a background thread
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    System.IO.File.WriteAllBytes(filePath, bytes);
                    Debug.Log($"Camera view saved to: {filePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to save camera view in background thread: {e.Message}");
                }
            });

            // Cleanup
            Destroy(screenshot);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save camera view: {e.Message}");
        }
    }

    // Save interaction data as JSON
    public void SaveToJSON()
    {
        try
        {
            // Convert dictionary to arrays for serialization
            string[] types = new string[_interactionCounts.Count];
            int[] counts = new int[_interactionCounts.Count];

            int index = 0;
            foreach (var kvp in _interactionCounts)
            {
                types[index] = kvp.Key;
                counts[index] = kvp.Value;
                index++;
            }

            // Create the data object
            InteractionDataToSave data = new InteractionDataToSave
            {
                totalCollisions = _numberOfCollisions,
                totalTime = _totalTime,
                formattedTime = FormatTime(_totalTime),
                interactionTypes = types,
                interactionCounts = counts
            };

            // Convert to JSON on main thread
            string json = UnityEngine.JsonUtility.ToJson(data, true);

            // Create a filename with timestamp
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath,
                $"interactions_{timestamp}_{sceneName}.json");

            // Write to file on background thread
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    System.IO.File.WriteAllText(filePath, json);
                    Debug.Log($"Interaction data saved to: {filePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to save interaction data in background thread: {e.Message}");
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save interaction data: {e.Message}");
        }
    }
}