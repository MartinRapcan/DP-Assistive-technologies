using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class InteractionsCounter : MonoBehaviour
{
    [System.Serializable] // Make serializable for JsonUtility
    public class InteractionData
    {
        public string direction;
        public int amount;
    }
    
    [System.Serializable] // Make serializable for JsonUtility
    public class OutputData
    {
        public int numberOfCollisions;
        public List<InteractionData> interactionTypes;
    }
    
    public struct InteractionType
    {
        public Direction Type;
        public int Amount;
    }
    
    public bool hasStarted
    {
        get;
        // Getter for _hasStarted
        set;
        // Setter for _hasStarted
    } = false;
    
    private bool _hasEnded = false;

    public bool hasEnded
    {
        get { return _hasEnded; }
        set
        {
            if (_hasEnded == value) return;
            _hasEnded = value;
            if (_hasEnded)
            {
                OnEndTriggered?.Invoke();
            }
        }
    }
    
    // Event to notify when the process ends
    public event Action OnEndTriggered;

    private int _numberOfCollisions = 0;
    private Dictionary<string, InteractionType> _interactionTypes;
    
    [Header("Screenshot Camera")]
    [SerializeField] private Camera environmentCamera;
    
    public void InitializeInteractionTypes()
    {
        _interactionTypes = new Dictionary<string, InteractionType>();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == Direction.None) continue; // Skip "None" if needed
            _interactionTypes[dir.ToString()] = new InteractionType { Type = dir, Amount = 0 };
        }
    }
    
    public void SetInteractionType(Direction direction)
    {
        var key = direction.ToString();
        _interactionTypes[key] = new InteractionType 
        { 
            Type = _interactionTypes[key].Type, 
            Amount = _interactionTypes[key].Amount + 1 
        };
    }

    public void IncrementNumberOfCollisions()
    {
        _numberOfCollisions++;
    }
    
    // Method to output data to a JSON file
    private void OutputJsonAndStop()
    {
        try
        {
            // Create a serializable output data structure
            OutputData outputData = new OutputData
            {
                numberOfCollisions = _numberOfCollisions,
                interactionTypes = new List<InteractionData>()
            };

            // Convert dictionary to list for serialization
            foreach (var kvp in _interactionTypes)
            {
                outputData.interactionTypes.Add(new InteractionData 
                { 
                    direction = kvp.Key, 
                    amount = kvp.Value.Amount 
                });
            }

            // Convert to JSON
            string json = JsonUtility.ToJson(outputData, true);

            // Get the desktop path for the current user
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Set the path where the file will be saved on the Desktop
            string filePath = Path.Combine(desktopPath, "interactionData.json");

            // Write the JSON string to a file
            File.WriteAllText(filePath, json);

            // Output message to the console
            Debug.Log("JSON file saved at: " + filePath);
            
            // Save a screenshot
            SaveCameraImage();
            
            // Make sure to call this on the main thread
            StartCoroutine(QuitAfterDelay());
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving JSON: " + e.Message);
        }
    }
    
    private void SaveCameraImage()
    {
        if (environmentCamera == null)
        {
            Debug.LogError("Camera is not assigned!");
            return;
        }

        // Set up RenderTexture
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        environmentCamera.targetTexture = renderTexture;
        environmentCamera.Render();

        // Convert to Texture2D
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // Reset Camera targetTexture
        environmentCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Convert to PNG
        byte[] bytes = texture.EncodeToPNG();
        Destroy(texture);

        // Save to Desktop
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "Screenshot.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Screenshot saved to: {filePath}");
    }

    private System.Collections.IEnumerator QuitAfterDelay()
    {
        // Wait for a frame to ensure logging happens
        yield return null;
        
        // Exit Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }


    // Subscribe to the event when the script is enabled
    private void OnEnable()
    {
        // Subscribe to the OnEndTriggered event to call OutputJsonAndStop when the end is triggered
        OnEndTriggered += OutputJsonAndStop;
    }

    // Unsubscribe to the event when the script is disabled
    private void OnDisable()
    {
        OnEndTriggered -= OutputJsonAndStop;
    }
}
