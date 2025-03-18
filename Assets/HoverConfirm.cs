using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverConfirm : MonoBehaviour
{
    [SerializeField] private GameObject sliderCanvas;
    [SerializeField] private Slider hoverTimeSlider;
    [SerializeField] private Camera mainCamera; // Main camera in the scene
    [SerializeField] private float maxHoverDuration = 0.3f; // 300ms max duration
    [SerializeField] private float distanceFromCamera = 0.72f;
    
    private float _hoverStartTime;
    private bool _isHovering = false;
    private bool _hasDurationCompleted = false;
    private RectTransform _buttonRect;
    
    private void Start()
    {
        _buttonRect = GetComponent<RectTransform>();
        
        // Disable the slider canvas initially
        if (sliderCanvas != null)
            sliderCanvas.SetActive(false);
            
        // Make sure the slider is properly referenced
        if (hoverTimeSlider == null)
            Debug.LogError("Hover time slider reference is missing!");
            
        // Configure the slider max value
        if (hoverTimeSlider != null)
            hoverTimeSlider.maxValue = maxHoverDuration;
    }
    
    private void Update()
    {
        // Update the slider value while hovering
        if (_isHovering && !_hasDurationCompleted)
        {
            float hoverDuration = Time.time - _hoverStartTime;
            
            if (hoverTimeSlider != null)
                hoverTimeSlider.value = Mathf.Min(hoverDuration, maxHoverDuration);
                
            // If we've reached max duration, hide the indicator
            if (hoverDuration >= maxHoverDuration)
            {
                _hasDurationCompleted = true;
                HideSliderCanvas();
            }
        }
    }
    
    // Called when the pointer enters the button
    public void OnPointerEnter(BaseEventData baseEventData)
    {
        _isHovering = true;
        _hoverStartTime = Time.time;
        
        if (sliderCanvas != null && mainCamera != null)
        {
            // Reset slider
            if (hoverTimeSlider != null)
                hoverTimeSlider.value = 0;
            
            // Get button position in screen space
            Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, _buttonRect.position);
            
            // Convert to world position at specified distance from camera
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, distanceFromCamera)
            );
            
            Debug.Log($"World position: {worldPos}");
            
            // Position indicator with offset (adjust Z to position above button)
            sliderCanvas.transform.position = worldPos;
            
            // Make indicator face the camera
            sliderCanvas.transform.rotation = mainCamera.transform.rotation;
            
            // Show indicator
            sliderCanvas.SetActive(true);
        }
    }
    
    // Called when the pointer exits the button
    public void OnPointerExit(BaseEventData baseEventData)
    {
        HideSliderCanvas();
    }
    
    private void HideSliderCanvas()
    {
        _hasDurationCompleted = false; // Reset this flag when hiding
        _isHovering = false;
        
            if (sliderCanvas != null)
        {
            hoverTimeSlider.value = 0f;
            sliderCanvas.SetActive(false);
        }
    }
}
