using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private Camera environmentCamera; // Top-down camera above your scene
    [SerializeField] private Navigation navigation; // Reference to Navigation script
    [SerializeField] private Camera mainCamera; // Main camera in the scene
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverTimeToConfirm = 5f; // Time needed to hover before confirming action
    [SerializeField] private float hoverRadiusThreshold = 20f; // Radius in pixels to consider "same position"
    
    private RectTransform _buttonRect;
    private bool _isHovering = false;
    private float _hoverTimer = 0f;
    private Vector2 _lastHoverPosition;
    private Coroutine _hoverCoroutine;

    private void Start()
    {
        _buttonRect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        _lastHoverPosition = eventData.position;
        
        // Start tracking hover
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
        }
        _hoverCoroutine = StartCoroutine(TrackHover(eventData.position));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        
        // Stop tracking hover
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
        _hoverTimer = 0f;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (_isHovering)
        {
            // Check if we've moved beyond the threshold
            if (Vector2.Distance(_lastHoverPosition, eventData.position) > hoverRadiusThreshold)
            {
                // Reset timer and update position
                _hoverTimer = 0f;
                _lastHoverPosition = eventData.position;
                
                // Restart tracking with new position
                if (_hoverCoroutine != null)
                {
                    StopCoroutine(_hoverCoroutine);
                }
                _hoverCoroutine = StartCoroutine(TrackHover(eventData.position));
            }
        }
    }

    private IEnumerator TrackHover(Vector2 initialPosition)
    {
        _hoverTimer = 0f;
        _lastHoverPosition = initialPosition;
        
        while (_isHovering && _hoverTimer < hoverTimeToConfirm)
        {
            _hoverTimer += Time.deltaTime;
            
            // Visual feedback could be added here (progress bar, color change, etc.)
            
            yield return null;
        }
        
        // If we completed the hover duration, trigger the action
        if (_isHovering)
        {
            ProcessHoverConfirmation(_lastHoverPosition);
        }
    }
    
    // Alternative approach using Update instead of coroutine for more responsive tracking
    private void Update()
    {
        if (_isHovering)
        {
            _hoverTimer += Time.deltaTime;
            
            if (_hoverTimer >= hoverTimeToConfirm)
            {
                ProcessHoverConfirmation(_lastHoverPosition);
                _hoverTimer = 0f;
                _isHovering = false; // Prevent multiple triggers
                
                // Reset after a short delay to allow new hover events
                StartCoroutine(ResetHoverAfterDelay(0.5f));
            }
        }
    }
    
    private IEnumerator ResetHoverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isHovering = true;
    }

    private void ProcessHoverConfirmation(Vector2 hoverPosition)
    {
        // Convert UI hover position to normalized position (0-1) on the minimap
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _buttonRect,
            hoverPosition,
            mainCamera,
            out var localPosition
        );
        
        // Convert to normalized coordinates (0-1 range)
        Vector2 normalizedPosition = new Vector2(
            (localPosition.x + _buttonRect.rect.width * 0.5f) / _buttonRect.rect.width,
            (localPosition.y + _buttonRect.rect.height * 0.5f) / _buttonRect.rect.height
        );
        
        Debug.Log($"Hover Confirmed at Normalized Position: {normalizedPosition}");
        
        // Cast ray from environment camera using this normalized position
        CastRayFromEnvironmentCamera(normalizedPosition);
        
        // Reset timer after action
        _hoverTimer = 0f;
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

    // For right-click functionality, you can add this method and call it from another input
    public void ClearDestination()
    {
        navigation.ClearDestination();
    }
}