using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArrowUIManager : MonoBehaviour
{
    [Header("Movement Reference")]
    [SerializeField] private Movement movement;

    [Header("Button References")]
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button backwardButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button stopButton;

    [Header("Visual Settings")]
    [SerializeField] private Color activeButtonColor = Color.green;
    [SerializeField] private Color inactiveButtonColor = Color.white;
    [SerializeField] private float initialOpacity = 0.1f;
    [SerializeField] private float activeOpacity = 1.0f;

    [Header("Hover Settings")]
    [SerializeField] private float hoverActivationDelay = 1.0f; // Time in seconds to hover before activation

    // List of all navigation buttons
    private List<Button> allButtons = new List<Button>();

    // Store original colors
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();
    
    // Store original text colors
    private Dictionary<TMP_Text, Color> originalTextColors = new Dictionary<TMP_Text, Color>();

    // Keep track of the currently activated button (for movement)
    private Button activeMovementButton = null;

    // Hover tracking
    private Button currentHoverButton = null;
    private float hoverStartTime = 0f;
    private bool isProcessingHover = false;

    private void Start()
    {
        // Initialize button list
        InitializeButtonList();

        // Setup mapping between buttons and movement actions
        SetupButtonActions();

        // Store original colors and set initial opacity
        StoreOriginalColorsAndSetOpacity();

        // Setup hover handlers for all buttons
        SetupHoverHandlers();

        // Initialize with all buttons at low opacity but interactable
        SetAllButtonsOpacity(initialOpacity);
    }

    private void Update()
    {
        // Check if we are currently hovering over a button
        if (currentHoverButton != null && isProcessingHover)
        {
            float hoverDuration = Time.time - hoverStartTime;

            // If hover duration exceeds our threshold, trigger the button
            if (hoverDuration >= hoverActivationDelay)
            {
                isProcessingHover = false; // Prevent repeated activation
                ActivateButton(currentHoverButton);
            }
        }
    }

    private void InitializeButtonList()
    {
        // Add all buttons to the list
        allButtons.Add(forwardButton);
        allButtons.Add(backwardButton);
        allButtons.Add(leftButton);
        allButtons.Add(rightButton);
        allButtons.Add(stopButton);
    }

    private Dictionary<Button, System.Action> buttonActions = new Dictionary<Button, System.Action>();

    private void SetupButtonActions()
    {
        // Map each button to its corresponding movement action
        buttonActions[forwardButton] = () =>
        {
            // movement._speed = 2;
            SetVelocity();
            movement.ShouldMoveForward();
        };

        buttonActions[backwardButton] = () =>
        {
            // movement._speed = 2;
            SetVelocity();
            movement.ShouldMoveBackward();
        };

        buttonActions[leftButton] = () =>
        {
            // movement._speed = 2;
            SetRotation();
            movement.ShouldTurnLeft();
        };

        buttonActions[rightButton] = () =>
        {
            // movement._speed = 2;
            SetRotation();
            movement.ShouldTurnRight();
        };

        buttonActions[stopButton] = () =>
        {
            // movement._speed = 0;
            movement.ShouldStopMoving();
        };
    }

    private void SetVelocity(float velocity = 150f)
    {
        movement.SetMaxVelocity(velocity);
    }
    
    private void SetRotation(float rotation = 40f)
    {
        movement.SetMaxRotation(rotation);
    }

    private void StoreOriginalColorsAndSetOpacity()
    {
        // Store colors for all buttons
        foreach (Button button in allButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                originalColors[button] = buttonImage.color;
            }
            
            // Store text colors
            TMP_Text[] textComponents = button.GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text textComponent in textComponents)
            {
                originalTextColors[textComponent] = textComponent.color;
            }
        }
    }

    private void SetupHoverHandlers()
    {
        // Add hover handlers to all buttons
        foreach (Button button in allButtons)
        {
            AddHoverHandlers(button);
        }
    }

    private void AddHoverHandlers(Button button)
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
        enterEntry.callback.AddListener((data) => { OnPointerEnter(button, (PointerEventData)data); });
        trigger.triggers.Add(enterEntry);

        // Add exit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnPointerExit(button, (PointerEventData)data); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnPointerEnter(Button button, PointerEventData eventData)
    {
        // Set the currently hovered button
        currentHoverButton = button;
        hoverStartTime = Time.time;
        isProcessingHover = true;
        
        // Highlight button slightly to show it's being hovered
        SetButtonOpacity(button, activeOpacity * 0.7f);
    }

    private void OnPointerExit(Button button, PointerEventData eventData)
    {
        if (currentHoverButton == button)
        {
            currentHoverButton = null;
            isProcessingHover = false;
        }

        // If this button was not the active one, return it to initial opacity
        if (button != activeMovementButton)
        {
            SetButtonOpacity(button, initialOpacity);
        }
    }

    private void ActivateButton(Button button)
    {
        // If there's already an active button, deactivate it first
        if (activeMovementButton != null)
        {
            // If it's the same button, no need to do anything
            if (activeMovementButton == button)
            {
                return;
            }
            
            // Reset color of previously active button
            SetButtonColor(activeMovementButton, inactiveButtonColor);
            SetButtonOpacity(activeMovementButton, initialOpacity);
        }

        // Special case for Stop button - immediate action and reset
        if (button == stopButton)
        {
            OnStopButtonActivated();
        }
        else
        {
            // Execute the action associated with this button
            if (buttonActions.ContainsKey(button))
            {
                buttonActions[button].Invoke();
            }

            // Set this as the active button
            activeMovementButton = button;

            // Change color of the activated button to green
            SetButtonColor(button, activeButtonColor);
            SetButtonOpacity(button, activeOpacity);
            
            // Make sure the button stays fully visible
            SetButtonOpacity(button, activeOpacity);
        }
    }

    private void OnStopButtonActivated()
    {
        // Execute the stop action
        if (buttonActions.ContainsKey(stopButton))
        {
            buttonActions[stopButton].Invoke();
        }

        // If there's an active movement button, change its color and opacity
        if (activeMovementButton != null)
        {
            // Reset color of previously active button
            SetButtonColor(activeMovementButton, inactiveButtonColor);
            SetButtonOpacity(activeMovementButton, initialOpacity);
            activeMovementButton = null;
        }

        // Briefly highlight the stop button to give feedback
        SetButtonColor(stopButton, activeButtonColor);
        SetButtonOpacity(stopButton, activeOpacity);

        // Start a coroutine to reset after showing feedback
        StartCoroutine(ResetAfterStopButtonFeedback(0.5f));
    }

    IEnumerator ResetAfterStopButtonFeedback(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Reset stop button opacity
        SetButtonOpacity(stopButton, initialOpacity);
        SetButtonColor(stopButton, inactiveButtonColor);
    }

    private void SetButtonOpacity(Button button, float opacity)
    {
        // Set opacity for the main button image
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = opacity;
            buttonImage.color = color;
        }
        
        // Set opacity for all child image components (like arrow images)
        Image[] childImages = button.GetComponentsInChildren<Image>();
        foreach (Image image in childImages)
        {
            // Skip the main button image as we already processed it
            if (image != buttonImage)
            {
                Color imgColor = image.color;
                imgColor.a = opacity * 5;
                image.color = imgColor;
            }
        }
        
        // Set opacity for all text components
        TMP_Text[] textComponents = button.GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text textComponent in textComponents)
        {
            Color textColor = textComponent.color;
            textColor.a = opacity * 5; // Text opacity is higher than button opacity for better visibility
            textComponent.color = textColor;
        }
    }

    private void SetButtonColor(Button button, Color newColor)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Preserve the alpha value
            float alpha = buttonImage.color.a;
            newColor.a = alpha;
            buttonImage.color = newColor;
        }
    }

    private void SetAllButtonsOpacity(float opacity)
    {
        foreach (Button button in allButtons)
        {
            SetButtonOpacity(button, opacity);
        }
    }

    // Public method to reset everything - can be called externally
    public void ResetNavigator()
    {
        // Stop any active movement
        movement.ShouldStopMoving();
        
        // Reset the active button
        if (activeMovementButton != null)
        {
            SetButtonColor(activeMovementButton, inactiveButtonColor);
            SetButtonOpacity(activeMovementButton, initialOpacity);
            activeMovementButton = null;
        }

        // Reset hover state
        currentHoverButton = null;
        isProcessingHover = false;

        // Reset all buttons to initial opacity
        SetAllButtonsOpacity(initialOpacity);
    }
}