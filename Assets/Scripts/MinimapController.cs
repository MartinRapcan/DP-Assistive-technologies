using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("XR Ray")]
    [SerializeField] private CustomRayHover rayHover;
    [SerializeField] private Camera environmentCamera;

    [Header("UI Elements")]
    [SerializeField] private GameObject minimapMain;
    [SerializeField] private GameObject minimapCorner;
    [SerializeField] private GameObject minimapClose;
    [SerializeField] private GameObject navigationConfirm;
    
    [Header("Hover Settings")] 
    [SerializeField] private float hoverTimeToConfirm = 5f;
    [SerializeField] private float hoverRadiusThreshold = 20f;
    [SerializeField] private float buttonHoverTime = 0.1f;
    [SerializeField] private float positionCheckInterval = 0.1f;
    
    [Header("Navigation Script")] 
    [SerializeField] private Navigation navigation;

    private bool _isMinimapActive = false;
    private Button _cornerButton;
    private Button _closeButton;
    private Button _confirmButton;
    
    private Coroutine _cornerHoverCoroutine;
    private Coroutine _closeHoverCoroutine;
    private Coroutine _confirmHoverCoroutine;
    private Coroutine _positionHoverCoroutine;
    
    private Vector3 _lastHoverPosition;
    private Vector3 _confirmedNavigationPosition;
    private float _hoverPositionTimer = 0f;
    private bool _isTrackingHoverPosition = false;
    private bool _isNavigationConfirmed = false;
    
    private RectTransform _buttonRect;
    
    private void Start()
    {
        // Subscribe to the events
        if (rayHover != null)
        {
            rayHover.OnMinimapHit += HandleMinimapHit;
            rayHover.OnMinimapExit += HandleMinimapExit;
        }
        
        // Initialize the minimap UI elements
        if(!minimapMain || !minimapCorner || !minimapClose || !navigationConfirm)
        {
            Debug.LogError("Minimap UI elements are not assigned in the inspector.");
            return;
        }
        minimapMain.SetActive(false);
        minimapCorner.SetActive(true);
        minimapClose.SetActive(false);
        navigationConfirm.SetActive(false);
        
        // Get the RectTransform of the minimap
        _buttonRect = minimapMain.GetComponent<RectTransform>();
        
        // Get button components
        _cornerButton = minimapCorner.GetComponent<Button>();
        if (_cornerButton == null)
        {
            _cornerButton = minimapCorner.AddComponent<Button>();
        }
        
        _closeButton = minimapClose.GetComponent<Button>();
        if (_closeButton == null)
        {
            _closeButton = minimapClose.AddComponent<Button>();
        }
        
        _confirmButton = navigationConfirm.GetComponentInChildren<Button>();
        if (_confirmButton == null && navigationConfirm != null)
        {
            _confirmButton = navigationConfirm.AddComponent<Button>();
        }
        
        // Add hover handlers
        AddHoverHandlers(_cornerButton);
        AddHoverHandlers(_closeButton);
        AddHoverHandlers(_confirmButton);
    }
    
    // Handle the minimap hit event
    private void HandleMinimapHit(Vector3 position, bool isLocalPosition)
    {
        if(!isLocalPosition) return;
    
        // Always track hover position when minimap is active, even after confirmation has appeared
        if (_isMinimapActive && minimapMain.activeSelf)
        {
            // Check if current position is significantly different from last position
            float distance = Vector3.Distance(_lastHoverPosition, position);
        
            // If we're not tracking position yet or have moved too far, reset tracking
            if (!_isTrackingHoverPosition || distance > hoverRadiusThreshold)
            {
                _lastHoverPosition = position;
                _hoverPositionTimer = 0f;
            
                // Start tracking if not already
                if (!_isTrackingHoverPosition)
                {
                    _isTrackingHoverPosition = true;
                
                    // Start the position hover coroutine if not already running
                    if (_positionHoverCoroutine == null)
                    {
                        _positionHoverCoroutine = StartCoroutine(TrackHoverPosition());
                    }
                
                    // Debug.Log($"Started tracking hover position: {_lastHoverPosition}");
                }
                else
                {
                    // Debug.Log($"Hover position reset due to movement. New position: {_lastHoverPosition}");
                }
            }
        }
    }
    
    // Coroutine to track hover position and handle confirmation
    private IEnumerator TrackHoverPosition()
    {
        while (_isTrackingHoverPosition && _isMinimapActive)
        {
            // Increment the timer
            _hoverPositionTimer += positionCheckInterval;
        
            // Check if we've reached the confirmation time
            if (_hoverPositionTimer >= hoverTimeToConfirm)
            {
                // Save the confirmed position and show confirmation
                _confirmedNavigationPosition = _lastHoverPosition;
                ShowNavigationConfirm();
            
                // Update the confirm button if it already exists
                if (_confirmButton != null && navigationConfirm.activeSelf)
                {
                    // Update visual indicator or position of confirm button if needed
                    // This could be moving the confirm button to the hover position
                    // or updating a marker/indicator at the hover position
                }
            
                Debug.Log($"Navigation position updated to: {_confirmedNavigationPosition}");
            
                // Don't break out of the loop - let it continue
                // Just reset the timer to start checking for a new position
                _hoverPositionTimer = 0f;
            }
        
            // Wait for the next check interval
            yield return new WaitForSeconds(positionCheckInterval);
        }
    
        _positionHoverCoroutine = null;
    }
    
    // Show navigation confirmation UI
    private void ShowNavigationConfirm()
    {
        // Only activate the confirmation UI the first time
        if (navigationConfirm.activeSelf) return;
        navigationConfirm.SetActive(true);
        _isNavigationConfirmed = true;
        
        // Set up the confirm button if needed
        if (_confirmButton != null)
        {
            // Already set up in Start() with AddHoverHandlers
        }
    }
    
    // Confirm navigation and use the saved position
    private void ConfirmNavigation()
    {
        // Use the saved navigation position
        if (navigation != null)
        {
            // Convert to normalized coordinates (0-1 range)
            Vector2 normalizedPosition = new Vector2(
                (_confirmedNavigationPosition.x + _buttonRect.rect.width * 0.5f) / _buttonRect.rect.width,
                (_confirmedNavigationPosition.y + _buttonRect.rect.height * 0.5f) / _buttonRect.rect.height
            );
        
            // Debug.Log($"Hover Confirmed at Normalized Position: {normalizedPosition}");
        
            // Cast ray from environment camera using this normalized position
            CastRayFromEnvironmentCamera(normalizedPosition);
        }
        
        // Clean up
        navigationConfirm.SetActive(false);
        _isNavigationConfirmed = false;
        
        // Optionally close the minimap after navigation is confirmed
        CloseMinimap();
    }
    
    private void CastRayFromEnvironmentCamera(Vector2 normalizedPosition)
    {
        // Convert normalized position (0-1) to viewport position for the camera
        Ray ray = environmentCamera.ViewportPointToRay(new Vector3(normalizedPosition.x, normalizedPosition.y, 0));
        
        // Raycast down from the camera
        RaycastHit[] allHits = Physics.RaycastAll(ray);
        foreach (RaycastHit hitInfo in allHits)
        {
            if (!hitInfo.collider.CompareTag("Floor")) continue;
            
            navigation.SetDestination(hitInfo.point);
            break;
        }
    }
    
    // Handle when the ray exits the minimap
    private void HandleMinimapExit()
    {
        // Stop position tracking if we exit the minimap
        if (_isTrackingHoverPosition)
        {
            _isTrackingHoverPosition = false;
            _hoverPositionTimer = 0f;
            
            if (_positionHoverCoroutine != null)
            {
                StopCoroutine(_positionHoverCoroutine);
                _positionHoverCoroutine = null;
            }
            
            // Debug.Log("Stopped tracking hover position due to minimap exit");
        }
    }
    
    private void AddHoverHandlers(Button button)
    {
        if (button == null) return;
        
        // Get or add the EventTrigger component
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // Clear existing triggers to avoid duplicates
        trigger.triggers.Clear();

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
    
    private void OnPointerEnter(Button button, PointerEventData data)
    {
        if (button == _cornerButton && !_isMinimapActive)
        {
            // Debug.Log("Pointer entered corner button");
            // Cancel any existing coroutine
            if (_cornerHoverCoroutine != null)
            {
                StopCoroutine(_cornerHoverCoroutine);
            }
            // Start hover timer
            _cornerHoverCoroutine = StartCoroutine(ButtonHoverTimer(OpenMinimap, buttonHoverTime));
        }
        else if (button == _closeButton && _isMinimapActive)
        {
            // Debug.Log("Pointer entered close button");
            // Cancel any existing coroutine
            if (_closeHoverCoroutine != null)
            {
                StopCoroutine(_closeHoverCoroutine);
            }
            // Start hover timer
            _closeHoverCoroutine = StartCoroutine(ButtonHoverTimer(CloseMinimap, buttonHoverTime));
        }
        else if (button == _confirmButton && _isNavigationConfirmed)
        {
            // Debug.Log("Pointer entered confirm button");
            // Cancel any existing coroutine
            if (_confirmHoverCoroutine != null)
            {
                StopCoroutine(_confirmHoverCoroutine);
            }
            // Start hover timer
            _confirmHoverCoroutine = StartCoroutine(ButtonHoverTimer(ConfirmNavigation, buttonHoverTime));
        }
    }
    
    private void OnPointerExit(Button button, PointerEventData data)
    {
        if (button == _cornerButton)
        {
            // Debug.Log("Pointer exited corner button");
            // Cancel hover timer
            if (_cornerHoverCoroutine != null)
            {
                StopCoroutine(_cornerHoverCoroutine);
                _cornerHoverCoroutine = null;
            }
        }
        else if (button == _closeButton)
        {
            // Debug.Log("Pointer exited close button");
            // Cancel hover timer
            if (_closeHoverCoroutine != null)
            {
                StopCoroutine(_closeHoverCoroutine);
                _closeHoverCoroutine = null;
            }
        }
        else if (button == _confirmButton)
        {
            // Debug.Log("Pointer exited confirm button");
            // Cancel hover timer
            if (_confirmHoverCoroutine != null)
            {
                StopCoroutine(_confirmHoverCoroutine);
                _confirmHoverCoroutine = null;
            }
        }
    }
    
    private IEnumerator ButtonHoverTimer(System.Action action, float duration)
    {
        yield return new WaitForSeconds(duration);
        action?.Invoke();
    }
    
    private void OpenMinimap()
    {
        _isMinimapActive = true;
        minimapMain.SetActive(true);
        minimapCorner.SetActive(false);
        minimapClose.SetActive(true);
        navigationConfirm.SetActive(false);
        _isNavigationConfirmed = false;
        Debug.Log("Minimap opened");
    }
    
    private void CloseMinimap()
    {
        _isMinimapActive = false;
        minimapMain.SetActive(false);
        minimapCorner.SetActive(true);
        minimapClose.SetActive(false);
        navigationConfirm.SetActive(false);
        _isNavigationConfirmed = false;
        
        // Stop position tracking
        _isTrackingHoverPosition = false;
        
        if (_positionHoverCoroutine != null)
        {
            StopCoroutine(_positionHoverCoroutine);
            _positionHoverCoroutine = null;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (rayHover != null)
        {
            rayHover.OnMinimapHit -= HandleMinimapHit;
            rayHover.OnMinimapExit -= HandleMinimapExit;
        }
        
        // Stop any running coroutines
        if (_cornerHoverCoroutine != null)
        {
            StopCoroutine(_cornerHoverCoroutine);
        }
        if (_closeHoverCoroutine != null)
        {
            StopCoroutine(_closeHoverCoroutine);
        }
        if (_confirmHoverCoroutine != null)
        {
            StopCoroutine(_confirmHoverCoroutine);
        }
        if (_positionHoverCoroutine != null)
        {
            StopCoroutine(_positionHoverCoroutine);
        }
    }
}