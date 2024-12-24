using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public enum Direction
    {
        Forward,
        Backward,
        Left,
        Right,
        Stop,
        None
    }

    [SerializeField]
    private Rigidbody leftWheelRigidbody;
    [SerializeField]
    private Rigidbody rightWheelRigidbody;

    [SerializeField]
    private Transform frameTransform;

    [SerializeField]
    private Camera cameraFront;
    [SerializeField]
    private Camera cameraBack;

    [SerializeField]
    private RenderTexture renderTexture;

    private Direction _direction = Direction.None;
    private Coroutine _decelerationCoroutine;

    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float maxTorque = 20f;
    [SerializeField] private float stopTime = 2f;
    [SerializeField] private float decelerationRate = 0.95f; // The rate at which velocity decreases
    [SerializeField] private float stopThreshold = 0.1f; // Minimum velocity to consider as stopped

    private void Start()
    {
        cameraFront.targetTexture = renderTexture;
        cameraBack.targetTexture = renderTexture;
    }

    private void Update()
    {
        Debug.Log(_direction);
        // Call movement logic based on current direction
        switch (_direction)
        {
            case Direction.Forward:
                MoveForward();
                break;
            case Direction.Backward:
                MoveBackward();
                break;
            case Direction.Left:
                TurnLeft();
                break;
            case Direction.Right:
                TurnRight();
                break;
            case Direction.Stop:
                StopMoving();
                break;
            case Direction.None:
                break;
        }
    }

    // Public methods to change direction
    public void ShouldTurnRight() => StartMovement(Direction.Right);
    public void ShouldStopMoving() => StartMovement(Direction.Stop);
    public void ShouldTurnLeft() => StartMovement(Direction.Left);
    public void ShouldMoveForward()
    {
        StartMovement(Direction.Forward);
        cameraFront.enabled = true;
        cameraBack.enabled = false;
    }

    public void ShouldMoveBackward()
    {
        StartMovement(Direction.Backward);
        cameraFront.enabled = false;
        cameraBack.enabled = true;
    }

    private void StartMovement(Direction newDirection)
    {
        // Stop any ongoing deceleration when a new movement is initiated
        if (_decelerationCoroutine != null)
        {
            StopCoroutine(_decelerationCoroutine);
            _decelerationCoroutine = null;
        }

        _direction = newDirection; // Set new direction
    }

    private void StopMoving()
    {
        // Start deceleration coroutine if not already running
        if (_decelerationCoroutine == null)
        {
            _decelerationCoroutine = StartCoroutine(DecelerateWheels());
        }
    }

    private IEnumerator DecelerateWheels()
    {
        float startTime = Time.time;
        Vector3 initialVelocityLeft = leftWheelRigidbody.velocity;
        Vector3 initialVelocityRight = rightWheelRigidbody.velocity;

        while (Time.time - startTime < stopTime)
        {
            float lerpFactor = (Time.time - startTime) / stopTime;

            // Lerp between current velocity and zero over stopTime duration
            leftWheelRigidbody.velocity = Vector3.Lerp(initialVelocityLeft, Vector3.zero, lerpFactor);
            rightWheelRigidbody.velocity = Vector3.Lerp(initialVelocityRight, Vector3.zero, lerpFactor);

            leftWheelRigidbody.angularVelocity = Vector3.Lerp(leftWheelRigidbody.angularVelocity, Vector3.zero, lerpFactor);
            rightWheelRigidbody.angularVelocity = Vector3.Lerp(rightWheelRigidbody.angularVelocity, Vector3.zero, lerpFactor);

            yield return null;
        }

        // Ensure we stop completely after lerp finishes
        leftWheelRigidbody.velocity = Vector3.zero;
        rightWheelRigidbody.velocity = Vector3.zero;
        leftWheelRigidbody.angularVelocity = Vector3.zero;
        rightWheelRigidbody.angularVelocity = Vector3.zero;

        _direction = Direction.None;
    }


    private void TurnRight()
    {
        leftWheelRigidbody.AddTorque(Vector3.up * Mathf.Clamp(10f * Time.time, 0f, maxTorque));
    }

    private void TurnLeft()
    {
        rightWheelRigidbody.AddTorque(Vector3.up * Mathf.Clamp(-10f * Time.time, -maxTorque, 0f));
    }

    private void MoveForward()
    {
        var force = frameTransform.forward * Mathf.Clamp(10f * Time.time, 0f, maxForce);
        leftWheelRigidbody.AddForce(force);
        rightWheelRigidbody.AddForce(force);
    }

    private void MoveBackward()
    {
        var force = -frameTransform.forward * Mathf.Clamp(10f * Time.time, 0f, maxForce);
        leftWheelRigidbody.AddForce(force);
        rightWheelRigidbody.AddForce(force);
    }
}
