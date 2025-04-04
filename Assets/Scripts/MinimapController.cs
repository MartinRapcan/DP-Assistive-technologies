using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    
    [Header("Hover Settings")] [SerializeField]
    private float hoverTimeToConfirm = 5f;
    [SerializeField] private float hoverRadiusThreshold = 20f;
    [SerializeField] private float buttonHoverTime = 0.1f;
    
    [Header("Navigation Script")] [SerializeField]
    private Navigation navigation;

    private bool _isMinimapActive = false;
    private Button _cornerButton;
    private Button _closeButton;
    
    private Coroutine _cornerHoverCoroutine;
    private Coroutine _closeHoverCoroutine;
    private Coroutine _positionHoverCoroutine;
    
    private Vector3 _lastHoverPosition;
    private Vector3 _confirmedNavigationPosition;
    private float _hoverPositionTimer = 0f;
    private bool _isTrackingHoverPosition = false;
    private bool _isNavigationConfirmed = false;
    
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
        
        // Add hover handlers
        AddHoverHandlers(_cornerButton);
        AddHoverHandlers(_closeButton);
    }
    
    // Handle the minimap hit event
    // Handle the minimap hit event
    private void HandleMinimapHit(Vector3 position, bool isLocalPosition)
    {
        Vector3 currentPosition = isLocalPosition ? position : position;
        
        // If we're not already tracking hover position and minimap is active, start tracking
        if (!_isTrackingHoverPosition && _isMinimapActive && minimapMain.activeSelf && !navigationConfirm.activeSelf)
        {
            _lastHoverPosition = currentPosition;
            _hoverPositionTimer = 0f;
            _isTrackingHoverPosition = true;
            
            // Start the position hover coroutine if not already running
            if (_positionHoverCoroutine == null)
            {
                _positionHoverCoroutine = StartCoroutine(TrackHoverPosition());
            }
            
            Debug.Log($"Started tracking hover position: {_lastHoverPosition}");
        }
        // If we are tracking hover position, check if the current position is still within threshold
        else if (_isTrackingHoverPosition && _isMinimapActive)
        {
            float distance = Vector3.Distance(_lastHoverPosition, currentPosition);
            
            // If moved too far, reset the timer and update the last position
            if (distance > hoverRadiusThreshold)
            {
                _lastHoverPosition = currentPosition;
                _hoverPositionTimer = 0f;
                Debug.Log($"Hover position reset due to movement. New position: {_lastHoverPosition}");
            }
            // Otherwise, current position is valid, will be handled by coroutine
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
            if (_hoverPositionTimer >= hoverTimeToConfirm && !_isNavigationConfirmed)
            {
                // Save the confirmed position
                _confirmedNavigationPosition = _lastHoverPosition;
                _isNavigationConfirmed = true;
                
                // Show navigation confirmation button
                ShowNavigationConfirm();
                
                Debug.Log($"Navigation position confirmed at: {_confirmedNavigationPosition} after {hoverTimeToConfirm} seconds");
                
                // Stop tracking
                _isTrackingHoverPosition = false;
                break;
            }
            
            // Wait for the next check interval
            yield return new WaitForSeconds(positionCheckInterval);
        }
        
        _positionHoverCoroutine = null;
    }
    
    // Show navigation confirmation UI
    private void ShowNavigationConfirm()
    {
        navigationConfirm.SetActive(true);
        
        // If you have a confirm button in the navigationConfirm object
        Button confirmButton = navigationConfirm.GetComponentInChildren<Button>();
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => { ConfirmNavigation(); });
        }
    }
    
    // Handle when the ray exits the minimap
    private void HandleMinimapExit()
    {
        // Your logic when the ray is no longer hitting the minimap...
    }
    
    private void AddHoverHandlers(Button button)
    {
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
            // Cancel any existing coroutine
            if (_closeHoverCoroutine != null)
            {
                StopCoroutine(_closeHoverCoroutine);
            }
            // Start hover timer
            _closeHoverCoroutine = StartCoroutine(ButtonHoverTimer(CloseMinimap, buttonHoverTime));
        }
    }
    
    private void OnPointerExit(Button button, PointerEventData data)
    {
        if (button == _cornerButton)
        {
            // Cancel hover timer
            if (_cornerHoverCoroutine != null)
            {
                StopCoroutine(_cornerHoverCoroutine);
                _cornerHoverCoroutine = null;
            }
        }
        else if (button == _closeButton)
        {
            // Cancel hover timer
            if (_closeHoverCoroutine != null)
            {
                StopCoroutine(_closeHoverCoroutine);
                _closeHoverCoroutine = null;
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
        Debug.Log("Minimap opened");
    }
    
    private void CloseMinimap()
    {
        _isMinimapActive = false;
        minimapMain.SetActive(false);
        minimapCorner.SetActive(true);
        minimapClose.SetActive(false);
        navigationConfirm.SetActive(false);
        Debug.Log("Minimap closed");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
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
    }
}