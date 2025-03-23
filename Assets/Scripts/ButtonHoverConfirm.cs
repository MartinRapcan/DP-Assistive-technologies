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

    // make key value pair for button tag and expandedButtonType
    private readonly Dictionary<string, ExpandedButtonType> _buttonExpandedType =
        new Dictionary<string, ExpandedButtonType>();

    private bool _isActive = false;

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
                if (!_isActive && !_buttonExpandedType.ContainsKey(_buttonTag))
                {
                    ButtonActive();
                }
            }
        }
    }

    // Called when the pointer enters the button
    public void OnPointerEnter(BaseEventData baseEventData)
    {
        // check if the button tag is already expanded
        if (_buttonExpandedType.ContainsKey(_buttonTag) &&
            expandUI.expandedButtonType == _buttonExpandedType[_buttonTag])
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
        if (_isActive)
        {
            _isActive = false;
            expandUI.movement.ShouldStopMoving();
        }
        
        HideSliderCanvas();
    }

    private void ButtonActive()
    {
        _isActive = true;
        expandUI.movement.SetMaxVelocity(_buttonTag is "ForwardHigh" or "BackwardHigh" ? 300f : 150f);

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

    private void HideSliderCanvas()
    {
        if (sliderCanvas != null)
        {
            hoverTimeSlider.value = 0f;
            sliderCanvas.SetActive(false);
        }
    }
}