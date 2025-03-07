using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    private enum NavigationType
    {
        Auto,
        Manual
    }

    public enum Direction
    {
        Forward,
        Backward,
        Left,
        Right,
        Stop,
        None,
        ForwardRight,
        ForwardLeft,
        BackwardLeft,
        BackwardRight
    }

    private enum InterfaceType
    {
        DoubleCamera,
        SingleCamera,
        TopCamera,
        None
    }

    [SerializeField] private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Transform frameTransform;
    [SerializeField] private Camera cameraFront;
    [SerializeField] private Camera cameraBack;
    [SerializeField] private Camera cameraTop;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private float maxTorque = 20f;
    [SerializeField] private float stopTime = 2f;
    [SerializeField] private InterfaceType interfaceType = InterfaceType.None;
    [SerializeField] private GameObject monitor;
    [SerializeField] private NavigationType navigation = NavigationType.Manual;
    
    [SerializeField] private InteractionsCounter interactionsCounter;

    private Direction _direction = Direction.None;
    private Coroutine _decelerationCoroutine;
    private float _time;
    
    private void Awake() // Or use Start()
    {
        interactionsCounter.InitializeInteractionTypes();
    }
    
    private void Start()
    {
        CameraSetup();
        SetupFrame();
    }

    private void SetupFrame()
    {
        Transform frame = transform.Find("Frame");
        if (frame == null)
        {
            Debug.LogWarning("Child object 'Frame' not found.");
            return;
        }

        Rigidbody frameRigidbody = frame.GetComponent<Rigidbody>();
        NavMeshAgent frameAgent = frame.GetComponent<NavMeshAgent>();
        Navigation navigationScript = frame.GetComponent<Navigation>();

        if (frameRigidbody != null)
            frameRigidbody.isKinematic = (navigation == NavigationType.Auto);
        if (frameAgent != null)
            frameAgent.enabled = (navigation == NavigationType.Auto);
        if (navigation == NavigationType.Manual && navigationScript != null)
        {
            Destroy(navigationScript);
            Debug.Log("Navigation script removed from 'Frame' due to manual navigation mode.");
        }
    }

    private void CameraSetup()
    {
        if (interfaceType == InterfaceType.None)
        {
            monitor.SetActive(false);
            return;
        }

        cameraFront.targetTexture = renderTexture;
        cameraBack.targetTexture = renderTexture;
        cameraTop.targetTexture = renderTexture;
        HandleCameraChange(true);
    }

    private void HandleCameraChange(bool isMovingForward)
    {
        cameraFront.enabled = interfaceType == InterfaceType.SingleCamera ||
                              (interfaceType == InterfaceType.DoubleCamera && isMovingForward);
        cameraBack.enabled = interfaceType == InterfaceType.DoubleCamera && !isMovingForward;
        cameraTop.enabled = interfaceType == InterfaceType.TopCamera;
    }

    private void FixedUpdate()
    {
        switch (_direction)
        {
            case Direction.Forward:
                Move(Direction.Forward);
                break;
            case Direction.Backward:
                Move(Direction.Backward);
                break;
            case Direction.Left:
                MoveLeftOrRight(Direction.Left);
                break;
            case Direction.Right:
                MoveLeftOrRight(Direction.Right);
                break;
            case Direction.Stop:
                StopMoving();
                break;
            case Direction.ForwardRight:
                Move(Direction.ForwardRight);
                break;
            case Direction.ForwardLeft:
                Move(Direction.ForwardLeft);
                break;
            case Direction.BackwardLeft:
                Move(Direction.BackwardLeft);
                break;
            case Direction.BackwardRight:
                Move(Direction.BackwardRight);
                break;
            case Direction.None:
                break;
        }
    }
    
    private void HandleMovementAndInteraction(Direction direction)
    {
        if (interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
        {
            interactionsCounter.SetInteractionType(direction);
        }
        StartMovement(direction);
    }

    public void ShouldMoveForward() => HandleMovementAndInteraction(Direction.Forward);
    public void ShouldMoveBackward() => HandleMovementAndInteraction(Direction.Backward);
    public void ShouldTurnLeft() => HandleMovementAndInteraction(Direction.Left);
    public void ShouldTurnRight() => HandleMovementAndInteraction(Direction.Right);
    public void ShouldStopMoving(bool isClicked = false)
    {
        if (isClicked)
        {
            interactionsCounter.SetInteractionType(Direction.Stop);
        }
        StopMoving();
    }

    public void ShouldMoveForwardRight() => HandleMovementAndInteraction(Direction.ForwardRight);
    public void ShouldMoveForwardLeft() => HandleMovementAndInteraction(Direction.ForwardLeft);
    public void ShouldMoveBackwardLeft() => HandleMovementAndInteraction(Direction.BackwardLeft);
    public void ShouldMoveBackwardRight() => HandleMovementAndInteraction(Direction.BackwardRight);

    public void StartMovement(Direction newDirection)
    {
        if (_decelerationCoroutine != null)
        {
            StopCoroutine(_decelerationCoroutine);
            _decelerationCoroutine = null;
        }

        _time = 0f;
        _direction = newDirection;
        HandleCameraChange(newDirection == Direction.Forward || newDirection == Direction.ForwardRight ||
                           newDirection == Direction.ForwardLeft);
    }

    private void StopMoving()
    {
        _decelerationCoroutine ??= StartCoroutine(DecelerateWheels());
    }

    private IEnumerator DecelerateWheels()
    {
        var startTime = Time.time;
        var initialVelocityLeft = leftWheelRigidbody.velocity;
        var initialVelocityRight = rightWheelRigidbody.velocity;
        
        // Get the frame's rigidbody
        var frameRb = frameTransform.GetComponent<Rigidbody>();
        var initialFrameVelocity = frameRb ? frameRb.velocity : Vector3.zero;
        var initialFrameAngularVelocity = frameRb ? frameRb.angularVelocity : Vector3.zero;

        while (Time.time - startTime < stopTime)
        {
            Debug.Log("Decelerating");
            var lerpFactor = (Time.time - startTime) / stopTime;
            leftWheelRigidbody.velocity = Vector3.Lerp(initialVelocityLeft, Vector3.zero, lerpFactor);
            rightWheelRigidbody.velocity = Vector3.Lerp(initialVelocityRight, Vector3.zero, lerpFactor);
            leftWheelRigidbody.angularVelocity =
                Vector3.Lerp(leftWheelRigidbody.angularVelocity, Vector3.zero, lerpFactor);
            rightWheelRigidbody.angularVelocity =
                Vector3.Lerp(rightWheelRigidbody.angularVelocity, Vector3.zero, lerpFactor);
            yield return null;
        }

        leftWheelRigidbody.velocity = Vector3.zero;
        rightWheelRigidbody.velocity = Vector3.zero;
        leftWheelRigidbody.angularVelocity = Vector3.zero;
        rightWheelRigidbody.angularVelocity = Vector3.zero;
        
        if (frameRb && !frameRb.isKinematic)
        {
            frameRb.velocity = Vector3.zero;
            frameRb.angularVelocity = Vector3.zero;
        }
        
        _direction = Direction.None;
    }

    private void Move(Direction direction)
    {
        var reverse = direction is Direction.BackwardLeft or Direction.BackwardRight or Direction.Backward;
        var torque = Mathf.Clamp(2f * (_time += Time.deltaTime), 0f, reverse ? maxTorque / 2 : maxTorque);

        // Adjust torque multipliers for diagonal movements
        const float torqueMultiplier = 1.1f;

        var rightTorque = torque * (direction is Direction.ForwardLeft or Direction.BackwardLeft
            ? torqueMultiplier
            : 1);
        var leftTorque = torque * (direction is Direction.ForwardRight or Direction.BackwardRight
            ? torqueMultiplier
            : 1);

        // Apply torque correctly
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right *
                                     (reverse ? -leftTorque : leftTorque)); // Flipped sign
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right *
                                      (reverse ? -rightTorque : rightTorque)); // Flipped sign
    }

    // Dedicated function for pure left/right movement
    private void MoveLeftOrRight(Direction direction)
    {
        var torque = Mathf.Clamp(3f * (_time += Time.deltaTime), 0f, maxTorque);

        if (direction == Direction.Left)
        {
            rightWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torque); // Flipped sign
            leftWheelRigidbody.AddTorque(Vector3.zero); // Right wheel stays still
        }
        else if (direction == Direction.Right)
        {
            leftWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torque); // Flipped sign
            rightWheelRigidbody.AddTorque(Vector3.zero); // Left wheel stays still
        }
    }
}