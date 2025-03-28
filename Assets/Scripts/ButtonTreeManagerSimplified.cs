using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverButtonTreeManagerSimplified : MonoBehaviour
{
    [Header("Movement Reference")] [SerializeField]
    private Movement movement;

    [Header("Button References")] [SerializeField]
    private Button middleButton;

    [Header("Second Layer Buttons")] [SerializeField]
    private Button forwardButton; // Second layer - Forward
    [SerializeField] private Button backwardButton; // Second layer - Backward
    [SerializeField] private Button leftButton; // Second layer - Left
    [SerializeField] private Button rightButton; // Second layer - Right
    [SerializeField] private Button stopButton; // Second layer - Stop

    [Header("Third Layer Buttons - Forward Path")] [SerializeField]
    private Button forwardLow;
    [SerializeField] private Button forwardHigh;

    [Header("Third Layer Buttons - Backward Path")] [SerializeField]
    private Button backwardLow;
    [SerializeField] private Button backwardHigh;

    [Header("Third Layer Buttons - Left Path")] [SerializeField]
    private Button leftLow;
    [SerializeField] private Button leftHigh;
    
    [Header("Third Layer Buttons - Right Path")] [SerializeField]
    private Button rightLow;
    [SerializeField] private Button rightHigh;

    [Header("Visual Settings")] [SerializeField]
    private Color validSequenceColor = Color.green;

    [SerializeField] private Color invalidSequenceColor = Color.red;
    [SerializeField] private float initialOpacity = 0.1f;
    [SerializeField] private float activeOpacity = 1.0f;

    [Header("Hover Settings")] [SerializeField]
    private float hoverActivationDelay = 1.0f; // Time in seconds to hover before activation

    // Track current sequence state
    private bool middleButtonActivated = false;
    private Button currentSecondLayerButton;
    private Button finalSelectedButton;

    // Lists to organize buttons by layer
    private List<Button> secondLayerButtons = new List<Button>();
    private List<Button> thirdLayerButtons = new List<Button>();

    // Dictionary for valid connections
    private Dictionary<Button, List<Button>> validConnections = new Dictionary<Button, List<Button>>();

    // Dictionary to map buttons to their movement functions
    private Dictionary<Button, System.Action> buttonActions = new Dictionary<Button, System.Action>();

    // Store original colors
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    // Keep track of the currently activated button (for movement)
    private static Button activeMovementButton = null;

    // Hover tracking
    private Button currentHoverButton = null;
    private float hoverStartTime = 0f;
    private bool isProcessingHover = false;

    private void Start()
    {
        // Initialize button lists
        InitializeButtonLists();

        // Set up valid connections between buttons
        SetupValidConnections();

        // Map buttons to movement actions
        SetupButtonActions();

        // Store original colors and set initial opacity
        StoreOriginalColorsAndSetOpacity();

        // Setup hover handlers for all buttons
        SetupHoverHandlers();

        // Initialize with a clean state
        ResetSelectionCompletelyToInitial();
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

    private void InitializeButtonLists()
    {
        // Add second layer buttons
        secondLayerButtons.Add(forwardButton);
        secondLayerButtons.Add(backwardButton);
        secondLayerButtons.Add(leftButton);
        secondLayerButtons.Add(rightButton);
        secondLayerButtons.Add(stopButton);

        // Add third layer buttons
        // Forward path
        thirdLayerButtons.Add(forwardLow);
        thirdLayerButtons.Add(forwardHigh);

        // Backward path
        thirdLayerButtons.Add(backwardLow);
        thirdLayerButtons.Add(backwardHigh);

        // Left path
        thirdLayerButtons.Add(leftLow);
        thirdLayerButtons.Add(leftHigh);

        // Right path
        thirdLayerButtons.Add(rightLow);
        thirdLayerButtons.Add(rightHigh);
    }

    private void SetupValidConnections()
    {
        // Define valid connections for each second layer button
        validConnections[forwardButton] = new List<Button> { forwardLow, forwardHigh };
        validConnections[backwardButton] = new List<Button> { backwardLow, backwardHigh };
        validConnections[leftButton] = new List<Button> { leftLow, leftHigh };
        validConnections[rightButton] = new List<Button> { rightLow, rightHigh };
        // Stop button has no third layer connections
        validConnections[stopButton] = new List<Button>();
    }

    private void SetupButtonActions()
    {
        // Map each button to its corresponding movement action

        // Forward path actions
        buttonActions[forwardLow] = () =>
        {
            SetVelocity(75f);
            movement.ShouldMoveForward();
        };
        buttonActions[forwardHigh] = () =>
        {
            SetVelocity();
            movement.ShouldMoveForward();
        };

        // Backward path actions
        buttonActions[backwardLow] = () =>
        {
            SetVelocity(75f);
            movement.ShouldMoveBackward();
        };
        buttonActions[backwardHigh] = () =>
        {
            SetVelocity();
            movement.ShouldMoveBackward();
        };

        // Left path actions
        buttonActions[leftLow] = () =>
        {
            SetRotation(20f);
            movement.ShouldTurnLeft();
        };
        buttonActions[leftHigh] = () =>
        {
            SetRotation();
            movement.ShouldTurnLeft();
        };

        // Right path actions
        buttonActions[rightLow] = () =>
        {
            SetRotation(20f);
            movement.ShouldTurnRight();
        };
        buttonActions[rightHigh] = () =>
        {
            SetRotation();
            movement.ShouldTurnRight();
        };

        // Stop action directly in second layer
        buttonActions[stopButton] = () =>
        {
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
        // Store middle button color
        Image middleImage = middleButton.GetComponent<Image>();
        originalColors[middleButton] = middleImage.color;

        // Set middle button to low opacity initially
        Color middleColor = middleImage.color;
        middleColor.a = initialOpacity;
        middleImage.color = middleColor;

        // Store colors and set opacity for second layer
        foreach (Button button in secondLayerButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            originalColors[button] = buttonImage.color;

            // Set low opacity
            Color color = buttonImage.color;
            color.a = initialOpacity;
            buttonImage.color = color;
        }

        // Store colors and set opacity for third layer
        foreach (Button button in thirdLayerButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            originalColors[button] = buttonImage.color;

            // Set low opacity
            Color color = buttonImage.color;
            color.a = initialOpacity;
            buttonImage.color = color;
        }
    }

    private void SetupHoverHandlers()
    {
        // Add hover handlers to middle button
        AddHoverHandlers(middleButton);

        // Add hover handlers to second layer buttons
        foreach (Button button in secondLayerButtons)
        {
            AddHoverHandlers(button);
        }

        // Add hover handlers to third layer buttons
        foreach (Button button in thirdLayerButtons)
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
        // Debug.Log("Pointer entered: " + button.name + " - Is interactable: " + IsButtonInteractable(button));

        // Check if this button should be interactive in the current state
        if (!IsButtonInteractable(button))
        {
            return;
        }

        // Set the currently hovered button
        currentHoverButton = button;
        hoverStartTime = Time.time;
        isProcessingHover = true;

        // You could add visual feedback here to show the button is being hovered
        // For example, a slight glow or outline
        // Debug.Log("Started hover processing for: " + button.name);
    }

    private void OnPointerExit(Button button, PointerEventData eventData)
    {
        if (currentHoverButton == button)
        {
            currentHoverButton = null;
            isProcessingHover = false;
        }

        // Remove any hover visual feedback here
    }

    private bool IsButtonInteractable(Button button)
    {
        // Middle button is always interactable at the start
        if (button == middleButton)
        {
            return true; // Allow hovering on middle button anytime
        }

        // Second layer buttons are interactable if middle button was activated
        if (secondLayerButtons.Contains(button) && middleButtonActivated)
        {
            return true; // Allow hovering on any second layer button if middle was activated
        }

        // Third layer buttons are interactable if they are valid children of the current second layer button
        if (thirdLayerButtons.Contains(button) && currentSecondLayerButton != null)
        {
            return validConnections[currentSecondLayerButton].Contains(button);
        }

        return false;
    }

    private void ActivateButton(Button button)
    {
        // Determine which layer the button belongs to and handle accordingly
        if (button == middleButton)
        {
            OnMiddleButtonActivated();
        }
        else if (secondLayerButtons.Contains(button))
        {
            // Special case for Stop button - immediate action
            if (button == stopButton)
            {
                OnStopButtonActivated();
            }
            else
            {
                OnSecondLayerButtonActivated(button);
            }
        }
        else if (thirdLayerButtons.Contains(button))
        {
            OnThirdLayerButtonActivated(button);
        }
    }

    private void OnMiddleButtonActivated()
    {
        // If there's already an active movement button, we need to handle it
        Button previousActiveButton = activeMovementButton;

        // Reset the current second layer selection regardless of whether there's an active button
        currentSecondLayerButton = null;

        // If we have an active button, don't modify it - just reset the navigation state
        if (previousActiveButton != null)
        {
            // Reset only the navigation part without affecting the active movement
            ResetSelectionPreservingMovement();
        }
        else
        {
            // If no active button, do a complete reset
            ResetSelectionCompletelyToInitial();
        }

        // Make middle button fully visible when activated
        SetButtonOpacity(middleButton, activeOpacity);

        // Make second layer buttons visible
        foreach (Button button in secondLayerButtons)
        {
            SetButtonOpacity(button, activeOpacity);
        }

        // Set state flag for valid sequence
        middleButtonActivated = true;

        // If we had an active movement button before, make sure it stays visible and green
        if (previousActiveButton != null)
        {
            SetButtonColor(previousActiveButton, validSequenceColor);
            SetButtonOpacity(previousActiveButton, activeOpacity);
        }
    }

    private void OnStopButtonActivated()
    {
        // Check if this is a valid sequence (middle button must be activated first)
        if (!middleButtonActivated)
        {
            // Debug.LogWarning("Trying to activate stop button without activating middle button first.");
            ResetSelectionCompletelyToInitial();
            return;
        }

        // Execute the stop action immediately
        if (buttonActions.ContainsKey(stopButton))
        {
            buttonActions[stopButton].Invoke();
        }

        // If there's an active movement button, change its color and opacity
        if (activeMovementButton != null)
        {
            // Reset color of previously active button
            SetButtonColor(activeMovementButton, originalColors[activeMovementButton]);
            SetButtonOpacity(activeMovementButton, initialOpacity);
            activeMovementButton = null;
        }

        // Briefly highlight the stop button to give feedback
        SetButtonColor(stopButton, validSequenceColor);
        SetButtonOpacity(stopButton, activeOpacity);

        // Start a coroutine to reset after showing feedback
        StartCoroutine(ResetAfterStopButtonFeedback(0.1f));
    }

    IEnumerator ResetAfterStopButtonFeedback(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Reset everything to initial state
        ResetSelectionCompletelyToInitial();
    }

    private void OnSecondLayerButtonActivated(Button activatedButton)
    {
        // Check if this is a valid sequence (middle button must be activated first)
        if (!middleButtonActivated)
        {
            // Debug.LogWarning("Trying to activate second layer without activating middle button first.");
            ResetSelectionCompletelyToInitial();
            return;
        }

        // If we're switching from one second layer button to another, handle cleanup
        if (currentSecondLayerButton != null && currentSecondLayerButton != activatedButton)
        {
            // Debug.Log("Switching from " + currentSecondLayerButton.name + " to " + activatedButton.name);

            // Hide all third layer buttons from previous selection
            foreach (Button childButton in validConnections[currentSecondLayerButton])
            {
                SetButtonOpacity(childButton, initialOpacity);
            }
        }

        // Set the current second layer button
        currentSecondLayerButton = activatedButton;

        // Highlight the selected second layer button
        foreach (Button button in secondLayerButtons)
        {
            if (button == activatedButton)
            {
                SetButtonOpacity(button, activeOpacity);
            }
            else
            {
                SetButtonOpacity(button, initialOpacity);
            }
        }

        // Show only valid child buttons of this second layer button
        foreach (Button childButton in validConnections[activatedButton])
        {
            SetButtonOpacity(childButton, activeOpacity);
        }
    }

    private void OnThirdLayerButtonActivated(Button activatedButton)
    {
        // Verify the entire sequence is valid
        bool isValidSequence = middleButtonActivated &&
                               currentSecondLayerButton != null &&
                               validConnections[currentSecondLayerButton].Contains(activatedButton);

        if (isValidSequence)
        {
            // Check if we're clicking on the already active button
            bool isReactivatingCurrentButton = (activatedButton == activeMovementButton);

            // If there's already an active button, deactivate it first (unless it's the same button)
            if (activeMovementButton != null && !isReactivatingCurrentButton)
            {
                // Reset color of previously active button
                SetButtonColor(activeMovementButton, originalColors[activeMovementButton]);
                SetButtonOpacity(activeMovementButton, initialOpacity);

                // Stop previous movement
                movement.ShouldStopMoving();
            }

            // Valid sequence - change color to green and keep only necessary buttons
            finalSelectedButton = activatedButton;

            // Execute the action associated with this button, but only if it's a new button
            // or we want to reactivate the same movement
            if (buttonActions.ContainsKey(activatedButton) &&
                (!isReactivatingCurrentButton || activeMovementButton == null))
            {
                buttonActions[activatedButton].Invoke();
            }

            // Set this as the active button
            activeMovementButton = activatedButton;

            // Change color of the final button to green
            SetButtonColor(finalSelectedButton, validSequenceColor);

            // Ensure the selected button stays fully visible
            SetButtonOpacity(finalSelectedButton, activeOpacity);

            // Reset sequence state
            middleButtonActivated = false;
            currentSecondLayerButton = null;

            // Set the middle button opacity back to initial value
            SetButtonOpacity(middleButton, initialOpacity);

            // Set all other buttons to low opacity except the final selected button
            foreach (Button button in secondLayerButtons)
            {
                SetButtonOpacity(button, initialOpacity);
            }

            foreach (Button button in thirdLayerButtons)
            {
                if (button != finalSelectedButton)
                {
                    SetButtonOpacity(button, initialOpacity);
                }
            }
        }
        else
        {
            // Invalid sequence - change color to red temporarily and reset the whole state
            SetButtonColor(activatedButton, invalidSequenceColor);

            // Disable hover processing during the error feedback
            isProcessingHover = false;
            currentHoverButton = null;

            // Debug.Log("Invalid sequence detected. Disabling hover temporarily.");

            // Start a coroutine to show the red color briefly, then reset everything
            StartCoroutine(InvalidSequenceReset(activatedButton, 1.0f));
        }
    }

    IEnumerator InvalidSequenceReset(Button button, float delay)
    {
        // First show the red color
        yield return new WaitForSeconds(delay);

        // Then reset the whole state including opacity
        ResetSelectionCompletelyToInitial();
    }

    private void SetButtonOpacity(Button button, float opacity)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color color = buttonImage.color;
            color.a = opacity;
            buttonImage.color = color;
        }
        
        // Text[] textComponents = button.GetComponentsInChildren<Text>();
        // foreach (Text textComponent in textComponents)
        // {
        //     Color textColor = textComponent.color;
        //     textColor.a = opacity;
        //     textComponent.color = textColor;
        // }
    }

    private void SetButtonColor(Button button, Color newColor)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Preserve the alpha value
            newColor.a = buttonImage.color.a;
            buttonImage.color = newColor;
        }
    }

    private void ResetSelectionCompletelyToInitial()
    {
        // Stop any active movement
        movement.ShouldStopMoving();
        activeMovementButton = null;

        // Reset the current selections
        currentSecondLayerButton = null;
        finalSelectedButton = null;
        middleButtonActivated = false;

        // Reset hover state
        currentHoverButton = null;
        isProcessingHover = false;

        // Reset all button colors to original and set all to initial opacity
        foreach (var kvp in originalColors)
        {
            Button button = kvp.Key;
            Color originalColor = kvp.Value;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color color = originalColor;
                color.a = initialOpacity; // Reset to initial opacity
                buttonImage.color = color;
            }
        }
    }

    // Reset selection while preserving current movement
    private void ResetSelectionPreservingMovement()
    {
        // Do not stop movement
        // Do not reset activeMovementButton

        // Reset the current selections for navigation
        currentSecondLayerButton = null;
        finalSelectedButton = null;
        middleButtonActivated = false;

        // Reset hover state
        currentHoverButton = null;
        isProcessingHover = false;

        // Reset all button colors to original and set all to initial opacity
        // except for the active movement button
        foreach (var kvp in originalColors)
        {
            Button button = kvp.Key;

            // Skip the currently active movement button
            if (button == activeMovementButton)
            {
                continue;
            }

            Color originalColor = kvp.Value;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color color = originalColor;
                color.a = initialOpacity; // Reset to initial opacity
                buttonImage.color = color;
            }
        }
    }

    // Public method to reset everything - can be called externally
    public void ResetNavigator()
    {
        ResetSelectionCompletelyToInitial();
    }
}