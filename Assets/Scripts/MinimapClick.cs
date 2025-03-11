using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Camera environmentCamera; // Top-down camera above your scene
    private RectTransform _buttonRect;

    [SerializeField] private GameObject prefab;

    private void Start()
    {
        _buttonRect = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Convert UI click to normalized position (0-1) on the minimap
        Vector2 clickPosition = eventData.position;
        Vector2 localPosition;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _buttonRect,
            clickPosition,
            eventData.pressEventCamera,
            out localPosition
        );

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
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Environment camera ray hit: {hit.collider.name} at point: {hit.point}");
            
            // Instantiate a prefab at the hit point
            Instantiate(prefab, hit.point + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }
    }
}