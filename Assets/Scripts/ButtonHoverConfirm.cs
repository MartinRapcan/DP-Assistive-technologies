using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverConfirm : MonoBehaviour
{
    [SerializeField] private GameObject sliderCanvas;
    [SerializeField] private Slider hoverTimeSlider;
    [SerializeField] private Camera mainCamera; // Main camera in the scene
    [SerializeField] private ExpandUI expandUI;

    private float _hoverStartTime;
    private bool _isHovering = false;
    private bool _hasDurationCompleted = false;
    private RectTransform _buttonRect;
    private string _buttonTag;
    private float _maxHoverDuration;
    private float _distanceFromCamera;

    // Static variable to track which button is currently active
    private static string _currentActiveButtonTag = "";
    private static ButtonHoverConfirm _currentActiveButton = null;

    // make key value pair for button tag and expandedButtonType
    private readonly Dictionary<string, ExpandedButtonType> _buttonExpandedType =
        new Dictionary<string, ExpandedButtonType>();

    private bool _isActive = false;
    
    // Add fields for button color control
    [SerializeField] private Color activeColor = Color.green;
    private Color _originalColor;
    private Image _buttonImage;

    private void Start()
    {
        _maxHoverDuration = GlobalConfig.instance.maxHoverDuration;
        _distanceFromCamera = GlobalConfig.instance.distanceFromCamera;

        _buttonExpandedType.Add("ExpandUpButton", ExpandedButtonType.Up);
        _buttonExpandedType.Add("ExpandDownButton", ExpandedButtonType.Down);
        _buttonExpandedType.Add("ExpandLeftButton", ExpandedButtonType.Left);
        _buttonExpandedType.Add("ExpandRightButton", ExpandedButtonType.Right);

        _buttonRect = GetComponent<RectTransform>();
        _buttonTag = tag;

        // Get the button image component
        _buttonImage = GetComponent<Image>();
        if (_buttonImage != null)
        {
            _originalColor = _buttonImage.color;
        }
        else
        {
            Debug.LogWarning("Button image component is missing on " + gameObject.name);
        }

        // Disable the slider canvas initially
        if (sliderCanvas != null)
            sliderCanvas.SetActive(false);

        // Make sure the slider is properly referenced
        if (hoverTimeSlider == null)
            Debug.LogError("Hover time slider reference is missing!");

        // Configure the slider max value
        if (hoverTimeSlider != null)
            hoverTimeSlider.maxValue = _maxHoverDuration;
    }

    private void Update()
    {
        // Update the slider value while hovering
        if (_isHovering && !_hasDurationCompleted)
        {
            float hoverDuration = Time.time - _hoverStartTime;

            if (hoverTimeSlider != null)
                hoverTimeSlider.value = Mathf.Min(hoverDuration, _maxHoverDuration);

            // If we've reached max duration, hide the indicator
            if (hoverDuration >= _maxHoverDuration)
            {
                _hasDurationCompleted = true;
                if (_buttonExpandedType.ContainsKey(_buttonTag))
                {
                    switch (_buttonTag)
                    {
                        case "ExpandUpButton":
                            expandUI.expandedButtonType = ExpandedButtonType.Up;
                            break;
                        case "ExpandDownButton":
                            expandUI.expandedButtonType = ExpandedButtonType.Down;
                            break;
                        case "ExpandLeftButton":
                            expandUI.expandedButtonType = ExpandedButtonType.Left;
                            break;
                        case "ExpandRightButton":
                            expandUI.expandedButtonType = ExpandedButtonType.Right;
                            break;
                        default:
                            expandUI.expandedButtonType = ExpandedButtonType.None;
                            break;
                    }
                }

                HideSliderCanvas();
                if (!_isActive && (IsMovementButton(_buttonTag) || _buttonTag == "IdleButton"))
                {
                    // Deactivate current active button if exists
                    if (_currentActiveButton != null && _currentActiveButton != this)
                    {
                        _currentActiveButton.DeactivateButton();
                    }
                    
                    // Activate this button
                    ButtonActive();
                }
            }
        }
    }

    // Called when the pointer enters the button
    public void OnPointerEnter(BaseEventData baseEventData)
    {
        // Special handling for IdleButton - activate immediately on hover
        if (_buttonTag == "IdleButton")
        {
            // Activate the idle button immediately
            _isHovering = false;
            _hasDurationCompleted = false;
            HideSliderCanvas(); // Hide any slider that might be showing
            
            // Only activate if not already active
            if (!_isActive)
            {
                // Deactivate current active button if exists
                if (_currentActiveButton != null && _currentActiveButton != this)
                {
                    _currentActiveButton.DeactivateButton();
                }
                
                // Activate idle button
                ButtonActive();
            }
            return;
        }
        
        // check if the button tag is already expanded
        if (_buttonExpandedType.ContainsKey(_buttonTag) &&
            expandUI.expandedButtonType == _buttonExpandedType[_buttonTag])
        {
            return;
        }
        
        // If this is the currently active button, no need to show loading effect
        if (_currentActiveButton == this && _isActive)
        {
            return;
        }
        
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
                new Vector3(screenPos.x, screenPos.y, _distanceFromCamera)
            );

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
        _hasDurationCompleted = false; // Reset this flag when hiding
        _isHovering = false;
        
        // We don't stop the movement here anymore
        // Only hide the slider canvas
        HideSliderCanvas();
    }

    private void ButtonActive()
    {
        _isActive = true;
        
        // Change button color to indicate active state
        if (_buttonImage != null)
        {
            _buttonImage.color = activeColor;
        }
        
        // Handle the IdleButton specially
        if (_buttonTag == "IdleButton")
        {
            // Stop any currently active button (except itself)
            if (_currentActiveButton != null && _currentActiveButton != this)
            {
                _currentActiveButton.DeactivateButton();
                _currentActiveButton = null;
                _currentActiveButtonTag = "";
            }
            
            // Set this as the current active button
            _currentActiveButtonTag = _buttonTag;
            _currentActiveButton = this;
            
            // Stop all movement
            expandUI.movement.ShouldStopMoving();
            return;
        }
        
        _currentActiveButtonTag = _buttonTag;
        _currentActiveButton = this;
        
        expandUI.movement.SetMaxVelocity(_buttonTag is "ForwardHigh" or "BackwardHigh" ? 150f : 75f);

        switch (_buttonTag)
        {
            case "TurnRight":
                expandUI.movement.ShouldTurnRight();
                break;
            case "TurnLeft":
                expandUI.movement.ShouldTurnLeft();
                break;
            
            case "ForwardRight":
                expandUI.movement.ShouldMoveForwardRight();
                break;
            case "ForwardLeft":
                expandUI.movement.ShouldMoveForwardLeft();
                break;
            
            case "BackwardRight":
                expandUI.movement.ShouldMoveBackwardRight();
                break;
            case "BackwardLeft":
                expandUI.movement.ShouldMoveBackwardLeft();
                break;
            
            case "ForwardHigh" or "ForwardLow": 
                expandUI.movement.ShouldMoveForward();
                break;
            case "BackwardHigh" or "BackwardLow":
                expandUI.movement.ShouldMoveBackward();
                break;
        }
    }
    
    // New method to deactivate the button
    private void DeactivateButton()
    {
        _isActive = false;
        
        // Restore original button color
        if (_buttonImage != null)
        {
            _buttonImage.color = _originalColor;
        }
        
        expandUI.movement.ShouldStopMoving();
    }
    
    // Helper method to check if this is a movement button
    private bool IsMovementButton(string tag)
    {
        // Return true if this is not an expand button and not the idle button
        return !_buttonExpandedType.ContainsKey(tag) && tag != "IdleButton";
    }

    // Add a public method to stop all active buttons (useful for a stop button if needed)
    public static void StopAllActiveButtons()
    {
        if (_currentActiveButton != null)
        {
            _currentActiveButton.DeactivateButton();
            _currentActiveButton = null;
            _currentActiveButtonTag = "";
        }
    }
    
    // Reset all button colors in case something goes wrong
    private void OnDisable()
    {
        if (_isActive)
        {
            DeactivateButton();
        }
    }

    private void HideSliderCanvas()
    {
        if (sliderCanvas != null)
        {
            hoverTimeSlider.value = 0f;
            sliderCanvas.SetActive(false);
        }
    }
}