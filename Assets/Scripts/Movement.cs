using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    [SerializeField] private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Transform frameTransform;
    [SerializeField] private Camera cameraFront;
    [SerializeField] private Camera cameraBack;
    [SerializeField] private Camera cameraTop;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private float maxTorque = 20f;
    [SerializeField] private float stopTime = 2f;
    [SerializeField] private GameObject monitor;
    [SerializeField] private InteractionsCounter interactionsCounter;
    [SerializeField] private HingeJoint leftHinge;
    [SerializeField] private HingeJoint rightHinge;
    
    // GlobalConfig
    [SerializeField] private GlobalConfig globalConfig;
    private NavigationType navigation => globalConfig.navigationType;
    private InterfaceType interfaceType => globalConfig.interfaceType;
    
    public Direction direction { get; set; } = Direction.None;
    private Coroutine _decelerationCoroutine;
    private float _time;
    
    private void Start()
    {
        interactionsCounter.InitializeInteractionTypes();
        CameraSetup();
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
        switch (direction)
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
        direction = newDirection;
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
        
        direction = Direction.None;
    }
    
    private void Move(Direction direction)
    {
        // Determine if moving in reverse
        var reverse = direction is Direction.BackwardLeft or Direction.BackwardRight or Direction.Backward;

        // Calculate base torque (clamping as before)
        var torque = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, reverse ? maxTorque / 2 : maxTorque);

        // Adjust torque multipliers for diagonal movements
        const float torqueMultiplier = 1.2f;

        // Calculate torque for each wheel based on direction
        var rightTorque = torque * (direction is Direction.ForwardLeft or Direction.BackwardLeft
            ? torqueMultiplier
            : 1);
        var leftTorque = torque * (direction is Direction.ForwardRight or Direction.BackwardRight
            ? torqueMultiplier
            : 1);

        Debug.Log($"Left torque: {leftTorque}");
        Debug.Log($"Right torque: {rightTorque}");

        // Configure motors
        JointMotor leftMotor = leftHinge.motor;
        JointMotor rightMotor = rightHinge.motor;

        // Convert torque to target velocity (degrees per second)
        // Assuming torque correlates to rotational speed; adjust scaling as needed
        float velocityScale = 100f; // Tune this to match your previous AddTorque feel
        leftMotor.targetVelocity = (reverse ? -leftTorque : leftTorque) * velocityScale;
        rightMotor.targetVelocity = (reverse ? -rightTorque : rightTorque) * velocityScale;

        // Set motor force (similar to maxTorque in your original setup)
        leftMotor.force = maxTorque;  // Use your existing maxTorque value
        rightMotor.force = maxTorque;

        // Apply motors to hinges
        leftHinge.motor = leftMotor;
        rightHinge.motor = rightMotor;

        // Enable motor
        leftHinge.useMotor = true;
        rightHinge.useMotor = true;
    }

    // private void Move(Direction direction)
    // {
    //     var reverse = direction is Direction.BackwardLeft or Direction.BackwardRight or Direction.Backward;
    //     var torque = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, reverse ? maxTorque / 2 : maxTorque);
    //
    //     // Adjust torque multipliers for diagonal movements
    //     const float torqueMultiplier = 1.2f;
    //
    //     var rightTorque = torque * (direction is Direction.ForwardLeft or Direction.BackwardLeft
    //         ? torqueMultiplier
    //         : 1);
    //     var leftTorque = torque * (direction is Direction.ForwardRight or Direction.BackwardRight
    //         ? torqueMultiplier
    //         : 1);
    //     
    //     Debug.Log($"Left torque: {leftTorque}");
    //     Debug.Log($"Right torque: {rightTorque}");
    //
    //     // Apply torque correctly
    //     leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right *
    //                                  (reverse ? -leftTorque : leftTorque));
    //     rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right *
    //                                   (reverse ? -rightTorque : rightTorque));
    // }

    // Dedicated function for pure left/right movement
    private void MoveLeftOrRight(Direction direction)
    {
        var torque = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, maxTorque);

        if (direction == Direction.Left)
        {
            rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torque);
            leftWheelRigidbody.angularVelocity = Vector3.zero;
        }
        else if (direction == Direction.Right)
        {
            leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torque);
            rightWheelRigidbody.angularVelocity = Vector3.zero;
        }
    }
}