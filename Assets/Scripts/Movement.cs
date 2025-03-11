using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Rigidbody frameRb;
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
    [SerializeField] private float maxVelocity = 300f;
    [SerializeField] private float maxRotation = 60f;

    // GlobalConfig
    [SerializeField] private GlobalConfig globalConfig;
    private NavigationType navigation => globalConfig.navigationType;
    private InterfaceType interfaceType => globalConfig.interfaceType;

    public Direction direction { get; set; } = Direction.None;
    private Coroutine _decelerationCoroutine;
    private float _time;
    private float _currentVelocity = 0f;
    private JointMotor _leftMotor;
    private JointMotor _rightMotor;
    private readonly float _force = 500f;

    private void Start()
    {
        interactionsCounter.InitializeInteractionTypes();
        CameraSetup();

        // Configure and apply motors
        _leftMotor = leftHinge.motor;
        _rightMotor = leftHinge.motor;

        leftHinge.useMotor = true;
        rightHinge.useMotor = true;
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

    public float GetSpeed()
    {
        // Velocity magnitude in meters per second
        float velocityMs = frameRb.velocity.magnitude;

        // Convert to kilometers per hour (m/s * 3.6 = km/h)
        float velocityKmh = Mathf.Round(velocityMs * 3.6f * 10f) / 10f;

        // Log the result
        return velocityKmh;
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
        // _decelerationCoroutine ??= StartCoroutine(DecelerateWheels());
        _leftMotor.targetVelocity = 0f;
        _rightMotor.targetVelocity = 0f;
        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;
        
        frameRb.velocity = Vector3.zero;
        frameRb.angularVelocity = Vector3.zero;
        
        direction = Direction.None;
        
    }

    // private IEnumerator DecelerateWheels()
    // {
    //     var startTime = Time.time;
    //     var initialVelocityLeft = leftWheelRigidbody.velocity;
    //     var initialVelocityRight = rightWheelRigidbody.velocity;
    //     
    //     // Get the frame's rigidbody
    //     var frameRb = frameTransform.GetComponent<Rigidbody>();
    //     var initialFrameVelocity = frameRb ? frameRb.velocity : Vector3.zero;
    //     var initialFrameAngularVelocity = frameRb ? frameRb.angularVelocity : Vector3.zero;
    //
    //     while (Time.time - startTime < stopTime)
    //     {
    //         Debug.Log("Decelerating");
    //         var lerpFactor = (Time.time - startTime) / stopTime;
    //         leftWheelRigidbody.velocity = Vector3.Lerp(initialVelocityLeft, Vector3.zero, lerpFactor);
    //         rightWheelRigidbody.velocity = Vector3.Lerp(initialVelocityRight, Vector3.zero, lerpFactor);
    //         leftWheelRigidbody.angularVelocity =
    //             Vector3.Lerp(leftWheelRigidbody.angularVelocity, Vector3.zero, lerpFactor);
    //         rightWheelRigidbody.angularVelocity =
    //             Vector3.Lerp(rightWheelRigidbody.angularVelocity, Vector3.zero, lerpFactor);
    //         yield return null;
    //     }
    //
    //     leftWheelRigidbody.velocity = Vector3.zero;
    //     rightWheelRigidbody.velocity = Vector3.zero;
    //     leftWheelRigidbody.angularVelocity = Vector3.zero;
    //     rightWheelRigidbody.angularVelocity = Vector3.zero;
    //     
    //     if (frameRb && !frameRb.isKinematic)
    //     {
    //         frameRb.velocity = Vector3.zero;
    //         frameRb.angularVelocity = Vector3.zero;
    //     }
    //     
    //     direction = Direction.None;
    // }

    // private IEnumerator DecelerateWheels()
    // {
    //     var startTime = Time.time;
    //
    //     // Get initial velocity
    //     float initialVelocity = Math.Min(leftHinge.motor.targetVelocity, rightHinge.motor.targetVelocity);
    //
    //     // Apply to motors
    //     JointMotor leftMotor = leftHinge.motor;
    //     JointMotor rightMotor = rightHinge.motor;
    //     leftMotor.targetVelocity = initialVelocity;
    //     rightMotor.targetVelocity = initialVelocity;
    //     leftHinge.motor = leftMotor;
    //     rightHinge.motor = rightMotor;
    //
    //     // Get the frame's initial velocities
    //     var initialFrameVelocity = frameRb ? frameRb.velocity : Vector3.zero;
    //     var initialFrameAngularVelocity = frameRb ? frameRb.angularVelocity : Vector3.zero;
    //
    //     while (Time.time - startTime < stopTime)
    //     {
    //         Debug.Log("Decelerating");
    //         var t = (Time.time - startTime) / stopTime; // 0 to 1 over stopTime
    //         var smoothFactor = Mathf.SmoothStep(0f, 1f, t); // Smooth S-curve from 0 to 1
    //
    //         // Smoothly decelerate motor velocities to 0
    //         float currentVelocity = Mathf.Lerp(initialVelocity, 0f, smoothFactor);
    //
    //         // Apply to motors
    //         leftMotor = leftHinge.motor;
    //         rightMotor = rightHinge.motor;
    //         leftMotor.targetVelocity = currentVelocity;
    //         rightMotor.targetVelocity = currentVelocity;
    //         leftHinge.motor = leftMotor;
    //         rightHinge.motor = rightMotor;
    //
    //         // Smoothly decelerate frame Rigidbody (if not kinematic)
    //         if (frameRb && !frameRb.isKinematic)
    //         {
    //             // Reduce frame velocity and angular velocity with the same smooth curve
    //             frameRb.velocity = Vector3.Lerp(initialFrameVelocity, Vector3.zero, smoothFactor);
    //             frameRb.angularVelocity = Vector3.Lerp(initialFrameAngularVelocity, Vector3.zero, smoothFactor);
    //         }
    //
    //         yield return null;
    //     }
    //
    //     // Ensure everything stops completely
    //     JointMotor finalLeftMotor = leftHinge.motor;
    //     JointMotor finalRightMotor = rightHinge.motor;
    //     finalLeftMotor.targetVelocity = 0f;
    //     finalRightMotor.targetVelocity = 0f;
    //     leftHinge.motor = finalLeftMotor;
    //     rightHinge.motor = finalRightMotor;
    //
    //     _currentVelocity = 0f;
    //     leftWheelRigidbody.velocity = Vector3.zero;
    //     rightWheelRigidbody.velocity = Vector3.zero;
    //
    //     if (frameRb && !frameRb.isKinematic)
    //     {
    //         frameRb.velocity = Vector3.zero;
    //         frameRb.angularVelocity = Vector3.zero;
    //     }
    //
    //     direction = Direction.None;
    // }

    // private IEnumerator DecelerateWheels()
    // {
    //     var startTime = Time.time;
    //
    //     // Get initial velocity
    //     float initialVelocity = Math.Min(leftHinge.motor.targetVelocity, rightHinge.motor.targetVelocity);
    //     
    //     // Apply to motors
    //     JointMotor leftMotor = leftHinge.motor;
    //     JointMotor rightMotor = rightHinge.motor;
    //     leftMotor.targetVelocity = initialVelocity;
    //     rightMotor.targetVelocity = initialVelocity;
    //
    //     leftHinge.motor = leftMotor;
    //     rightHinge.motor = rightMotor;
    //     
    //     frameRb.angularVelocity = Vector3.zero;
    //
    //     // Get the frame's Rigidbody
    //     var initialFrameVelocity = frameRb ? frameRb.velocity : Vector3.zero;
    //     var initialFrameAngularVelocity = frameRb ? frameRb.angularVelocity : Vector3.zero;
    //
    //     while (Time.time - startTime < stopTime)
    //     {
    //         Debug.Log("Decelerating");
    //         var t = (Time.time - startTime) / stopTime; // 0 to 1 over stopTime
    //         var smoothFactor = Mathf.SmoothStep(0f, 1f, t); // Smooth S-curve from 0 to 1
    //
    //         // Smoothly decelerate motor velocities to 0
    //         float currentVelocity = Mathf.Lerp(initialVelocity, 0f, smoothFactor);
    //         
    //         // Apply to motors
    //         leftMotor = leftHinge.motor;
    //         rightMotor = rightHinge.motor;
    //         leftMotor.targetVelocity = currentVelocity;
    //         rightMotor.targetVelocity = currentVelocity;
    //
    //         leftHinge.motor = leftMotor;
    //         rightHinge.motor = rightMotor;
    //         
    //         // Smoothly decelerate frame Rigidbody (if not kinematic)
    //         if (frameRb && !frameRb.isKinematic)
    //         {
    //             // Reduce frame velocity and angular velocity with the same smooth curve
    //             frameRb.velocity = Vector3SmoothStep(initialFrameVelocity, Vector3.zero, t);
    //             frameRb.angularVelocity = Vector3.zero; // No angular velocity
    //         }
    //
    //         yield return null;
    //     }
    //
    //     // Ensure everything stops completely
    //     JointMotor finalLeftMotor = leftHinge.motor;
    //     JointMotor finalRightMotor = rightHinge.motor;
    //     finalLeftMotor.targetVelocity = 0f;
    //     finalRightMotor.targetVelocity = 0f;
    //     leftHinge.motor = finalLeftMotor;
    //     rightHinge.motor = finalRightMotor;
    //     
    //     _currentVelocity = 0f;
    //     
    //     leftWheelRigidbody.velocity = Vector3.zero;
    //     rightWheelRigidbody.velocity = Vector3.zero;
    //
    //     if (frameRb && !frameRb.isKinematic)
    //     {
    //         frameRb.velocity = Vector3.zero;
    //         frameRb.angularVelocity = Vector3.zero;
    //     }
    //
    //     direction = Direction.None;
    // }

    // Custom Vector3 SmoothStep implementation (since Unity doesn't provide it natively)
    private Vector3 Vector3SmoothStep(Vector3 from, Vector3 to, float t)
    {
        t = Mathf.SmoothStep(0f, 1f, t); // Apply smooth curve
        return new Vector3(
            Mathf.Lerp(from.x, to.x, t),
            Mathf.Lerp(from.y, to.y, t),
            Mathf.Lerp(from.z, to.z, t)
        );
    }

    private void Move(Direction direction)
    {
        // Determine if moving in reverse
        bool reverse = direction is Direction.BackwardLeft or Direction.BackwardRight or Direction.Backward;

        // Target velocities based on direction (degrees per second)
        float targetVelocity = maxVelocity * (reverse ? -1 : 1);

        var forceRight = direction is Direction.BackwardRight or Direction.ForwardRight ? _force * 0.8f : _force;
        var forceLeft = direction is Direction.BackwardLeft or Direction.ForwardLeft ? _force * 0.8f : _force;

        // Smoothly adjust current velocities toward targets
        _currentVelocity = Mathf.MoveTowards(_currentVelocity, targetVelocity, _force * Time.deltaTime);

        Debug.Log(
            $"Left velocity: {_currentVelocity * (direction is Direction.ForwardLeft or Direction.BackwardLeft ? 0.8f : 1)}");
        Debug.Log(
            $"Right velocity: {_currentVelocity * (direction is Direction.ForwardRight or Direction.BackwardRight ? 0.8f : 1)}");

        _leftMotor.targetVelocity =
            _currentVelocity * (direction is Direction.ForwardLeft or Direction.BackwardLeft ? 0.8f : 1);
        _rightMotor.targetVelocity =
            _currentVelocity * (direction is Direction.ForwardRight or Direction.BackwardRight ? 0.8f : 1);
        _leftMotor.force = forceLeft;
        _rightMotor.force = forceRight;

        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;
    }

    // private void Move(Direction direction)
    // {
    //     // Determine if moving in reverse
    //     var reverse = direction is Direction.BackwardLeft or Direction.BackwardRight or Direction.Backward;
    //
    //     // Calculate base torque (clamping as before)
    //     var torque = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, reverse ? maxTorque / 2 : maxTorque);
    //
    //     // Adjust torque multipliers for diagonal movements
    //     const float torqueMultiplier = 1.2f;
    //
    //     // Calculate torque for each wheel based on direction
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
    //     // Configure motors
    //     JointMotor leftMotor = leftHinge.motor;
    //     JointMotor rightMotor = rightHinge.motor;
    //
    //     // Convert torque to target velocity (degrees per second)
    //     // Assuming torque correlates to rotational speed; adjust scaling as needed
    //     float velocityScale = 100f; // Tune this to match your previous AddTorque feel
    //     leftMotor.targetVelocity = (reverse ? -leftTorque : leftTorque) * velocityScale;
    //     rightMotor.targetVelocity = (reverse ? -rightTorque : rightTorque) * velocityScale;
    //
    //     // Set motor force (similar to maxTorque in your original setup)
    //     leftMotor.force = maxTorque;  // Use your existing maxTorque value
    //     rightMotor.force = maxTorque;
    //
    //     // Apply motors to hinges
    //     leftHinge.motor = leftMotor;
    //     rightHinge.motor = rightMotor;
    //
    //     // Enable motor
    //     leftHinge.useMotor = true;
    //     rightHinge.useMotor = true;
    // }

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
    // private void MoveLeftOrRight(Direction direction)
    // {
    //     var torque = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, maxTorque);
    //
    //     if (direction == Direction.Left)
    //     {
    //         rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torque);
    //         leftWheelRigidbody.angularVelocity = Vector3.zero;
    //     }
    //     else if (direction == Direction.Right)
    //     {
    //         leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torque);
    //         rightWheelRigidbody.angularVelocity = Vector3.zero;
    //     }
    // }

    private void MoveLeftOrRight(Direction direction)
    {
        // Calculate base velocity (replacing torque)
        var velocity = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, maxRotation);

        // Get HingeJoint components
        JointMotor leftMotor = leftHinge.motor;
        JointMotor rightMotor = rightHinge.motor;

        if (direction == Direction.Left)
        {
            // Turn left: drive right wheel forward, left wheel stopped
            rightMotor.targetVelocity = velocity; // Right wheel moves forward
            leftMotor.targetVelocity = -velocity; // Left wheel moves backward
        }
        else if (direction == Direction.Right)
        {
            // Turn right: drive left wheel forward, right wheel stopped
            leftMotor.targetVelocity = velocity; // Left wheel moves forward
            rightMotor.targetVelocity = -velocity; // Right wheel moves backward
        }

        // Apply motor settings
        leftMotor.force = 1000f; // High force to enforce velocity (tune if needed)
        rightMotor.force = 1000f;
        leftHinge.motor = leftMotor;
        rightHinge.motor = rightMotor;
    }
}