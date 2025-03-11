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