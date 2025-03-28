using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Rigidbody Components")]
    [SerializeField] private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Rigidbody frameRb;
    [SerializeField] private Rigidbody leftCasterRb;
    [SerializeField] private Rigidbody rightCasterRb;
    
    [Header("Camera Components")]
    [SerializeField] private Camera cameraFront;
    [SerializeField] private Camera cameraBack;
    [SerializeField] private Camera cameraTop;

    [Header("UI Components")]
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private GameObject monitor;
    [SerializeField] private InteractionsCounter interactionsCounter;
    
    [Header("Movement Settings")]
    [SerializeField] private float stopTime = 2f;
    [SerializeField] private float maxRotation = 40f;
    [SerializeField] private float maxVelocity = 150f;
    
    [Header("HingeJoint Components")]
    [SerializeField] private HingeJoint leftHinge;
    [SerializeField] private HingeJoint rightHinge;

    [Header("Global Configuration")]
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
    private float _backwardMaxVelocity;
    
    public void SetMaxVelocity(float velocity)
    {
        maxVelocity = velocity;
        _backwardMaxVelocity = -maxVelocity / 2f;
    }
    
    public void SetMaxRotation(float rotation)
    {
        maxRotation = rotation;
    }

    private void Start()
    {
        interactionsCounter.InitializeInteractionTypes();
        CameraSetup();

        // Configure and apply motors
        _leftMotor = leftHinge.motor;
        _rightMotor = leftHinge.motor;

        leftHinge.useMotor = true;
        rightHinge.useMotor = true;

        _backwardMaxVelocity = -maxVelocity / 2f;
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

    private void HandleMovementAndInteraction(Direction dir)
    {
        if (interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
        {
            interactionsCounter.SetInteractionType(dir);
        }

        StartMovement(dir);
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
        // Start deceleration coroutine if not already running
        _decelerationCoroutine ??= StartCoroutine(DecelerateWheels());
    
        // However, we still want to zero out physics velocities to prevent sliding
        frameRb.velocity = Vector3.zero;
        frameRb.angularVelocity = Vector3.zero;
        leftCasterRb.velocity = Vector3.zero;
        rightCasterRb.velocity = Vector3.zero;
    }
    
    private IEnumerator DecelerateWheels()
    {
        // Store initial velocities when deceleration starts
        float initialLeftVelocity = _leftMotor.targetVelocity;
        float initialRightVelocity = _rightMotor.targetVelocity;
    
        // Calculate deceleration rate per second
        float leftDecelerationRate = initialLeftVelocity / stopTime;
        float rightDecelerationRate = initialRightVelocity / stopTime;
    
        float elapsedTime = 0f;
    
        while (elapsedTime < stopTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / stopTime; // Normalized time (0 to 1)
        
            // Apply smoothed deceleration
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
        
            // Linearly interpolate from initial velocity to zero
            _leftMotor.targetVelocity = Mathf.Lerp(initialLeftVelocity, 0f, smoothT);
            _rightMotor.targetVelocity = Mathf.Lerp(initialRightVelocity, 0f, smoothT);
        
            // Apply the motor settings
            leftHinge.motor = _leftMotor;
            rightHinge.motor = _rightMotor;
        
            // Update current velocity for tracking
            _currentVelocity = Mathf.Lerp(_currentVelocity, 0f, smoothT);
        
            yield return null; // Wait for next frame
        }
    
        // Ensure final velocities are exactly zero
        _leftMotor.targetVelocity = 0f;
        _rightMotor.targetVelocity = 0f;
        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;
        _currentVelocity = 0f;
    
        // Reset direction once fully stopped
        direction = Direction.None;
    
        // Clear the coroutine reference
        _decelerationCoroutine = null;
    }

    private void Move(Direction dir)
    {
        // Determine if moving in reverse
        bool reverse = dir is Direction.BackwardLeft or Direction.BackwardRight or Direction.Backward;

        // Target velocities based on direction (degrees per second)
        float targetVelocity = reverse ? _backwardMaxVelocity : maxVelocity;
        
        var forceRight = dir is Direction.BackwardRight or Direction.ForwardRight ? _force * 0.8f : _force;
        var forceLeft = dir is Direction.BackwardLeft or Direction.ForwardLeft ? _force * 0.8f : _force;

        // Smoothly adjust current velocities toward targets
        _currentVelocity = Mathf.MoveTowards(_currentVelocity, targetVelocity, _force * Time.deltaTime);

        // Debug.Log(
        //    $"Left velocity: {_currentVelocity * (dir is Direction.ForwardLeft or Direction.BackwardLeft ? 0.8f : 1)}");
        // Debug.Log(
        //    $"Right velocity: {_currentVelocity * (dir is Direction.ForwardRight or Direction.BackwardRight ? 0.8f : 1)}");

        _leftMotor.targetVelocity =
            _currentVelocity * (dir is Direction.ForwardLeft or Direction.BackwardLeft ? 0.8f : 1);
        _rightMotor.targetVelocity =
            _currentVelocity * (dir is Direction.ForwardRight or Direction.BackwardRight ? 0.8f : 1);
        _leftMotor.force = forceLeft;
        _rightMotor.force = forceRight;

        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;
    }

    private void MoveLeftOrRight(Direction dir)
    {
        // Calculate base velocity (replacing torque)
        var velocity = Mathf.Clamp(10f * (_time += Time.deltaTime), 0f, maxRotation);

        // Get HingeJoint components
        JointMotor leftMotor = leftHinge.motor;
        JointMotor rightMotor = rightHinge.motor;

        if (dir == Direction.Left)
        {
            // Turn left: drive right wheel forward, left wheel stopped
            rightMotor.targetVelocity = velocity; // Right wheel moves forward
            leftMotor.targetVelocity = -velocity; // Left wheel moves backward
        }
        else if (dir == Direction.Right)
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