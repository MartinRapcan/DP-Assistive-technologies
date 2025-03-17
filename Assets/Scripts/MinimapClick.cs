using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapClick : MonoBehaviour
{
    [SerializeField] private Camera environmentCamera; // Top-down camera above your scene
    [SerializeField] private Navigation navigation; // Reference to Navigation script
    [SerializeField] private Camera mainCamera; // Main camera in the scene
    private RectTransform _buttonRect;

    private void Start()
    {
        _buttonRect = GetComponent<RectTransform>();
    }

    public void OnPointerClick(BaseEventData baseEventData)
    {
        PointerEventData eventData = (PointerEventData) baseEventData;
        
        // if right click, return
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            navigation.ClearDestination();
            return;
        }
        
        // Convert UI click to normalized position (0-1) on the minimap
        Vector2 clickPosition = eventData.position;
        
        Debug.Log($"Click position: {clickPosition}");
        Debug.Log($"ButtonRect: {_buttonRect.position}, {_buttonRect.rect.width}, {_buttonRect.rect.height}");

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _buttonRect,
            clickPosition,
            mainCamera,
            out var localPosition
        );

        Debug.Log(localPosition);
        
        // Convert to normalized coordinates (0-1 range)
        Vector2 normalizedPosition = new Vector2(
            (localPosition.x + _buttonRect.rect.width * 0.5f) / _buttonRect.rect.width,
            (localPosition.y + _buttonRect.rect.height * 0.5f) / _buttonRect.rect.height
        );
        
        Debug.Log($"Normalized Position: {normalizedPosition}");
        
        // Cast ray from environment camera using this normalized position
        CastRayFromEnvironmentCamera(normalizedPosition);
    }

    private void CastRayFromEnvironmentCamera(Vector2 normalizedPosition)
    {
        // Convert normalized position (0-1) to viewport position for the camera
        // Note: In some cases you may need to flip Y depending on your UI orientation
        Ray ray = environmentCamera.ViewportPointToRay(new Vector3(normalizedPosition.x, normalizedPosition.y, 0));
        
        // Raycast down from the camera
        RaycastHit[] allHits = Physics.RaycastAll(ray);
        foreach (RaycastHit hitInfo in allHits)
        {
            Debug.Log(hitInfo.point);
            if (!hitInfo.collider.CompareTag("Floor")) continue;
            
            // Debug.Log($"Found floor in multiple hits at: {hitInfo.point}");
            navigation.SetDestination(hitInfo.point);
            break;
        }
    }
}