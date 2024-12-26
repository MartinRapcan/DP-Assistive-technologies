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
        None,
        ForwardRight,
        ForwardLeft,
        BackwardLeft,
        BackwardRight
    }

    [SerializeField] private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Transform frameTransform;
    [SerializeField] private Camera cameraFront;
    [SerializeField] private Camera cameraBack;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float maxTorque = 20f;
    [SerializeField] private float stopTime = 2f;

    private Direction _direction = Direction.None;
    private Coroutine _decelerationCoroutine;
    private float _time;

    private void Start()
    {
        cameraFront.targetTexture = renderTexture;
        cameraBack.targetTexture = renderTexture;
    }

    private void Update()
    {
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
            case Direction.ForwardRight:
                MoveForwardRight();
                break;
            case Direction.ForwardLeft:
                MoveForwardLeft();
                break;
            case Direction.BackwardLeft:
                MoveBackwardLeft();
                break;
            case Direction.BackwardRight:
                MoveBackwardRight();
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

    public void ShouldMoveForwardRight()
    {
        StartMovement(Direction.ForwardRight);
        cameraFront.enabled = true;
        cameraBack.enabled = false;
    }
    
    public void ShouldMoveForwardLeft()
    {
        StartMovement(Direction.ForwardLeft);
        cameraFront.enabled = true;
        cameraBack.enabled = false;
    }
    
    public void ShouldMoveBackwardLeft()
    {
        StartMovement(Direction.BackwardLeft);
        cameraFront.enabled = false;
        cameraBack.enabled = true;
    }
    
    public void ShouldMoveBackwardRight()
    {
        StartMovement(Direction.BackwardRight);
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

        _time = Time.deltaTime;
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

    private float TrackTime()
    {
        _time += Time.deltaTime;
        return _time;
    }

    private void TurnRight()
    {
        leftWheelRigidbody.AddTorque(Vector3.up * Mathf.Clamp(5f * TrackTime(), 0f, maxTorque));
    }

    private void TurnLeft()
    {
        rightWheelRigidbody.AddTorque(Vector3.up * Mathf.Clamp(-5f * TrackTime(), -maxTorque, 0f));
    }

    private void MoveForward()
    {
        var force = frameTransform.forward * Mathf.Clamp(5f * TrackTime(), 0f, maxForce);
        var force2 = frameTransform.forward * Mathf.Clamp(TrackTime(), 0f, maxForce / 2);
        leftWheelRigidbody.AddForce(force);
        rightWheelRigidbody.AddForce(force);
    }

    private void MoveBackward()
    {
        var force = -frameTransform.forward * Mathf.Clamp(5f * TrackTime(), 0f, maxForce);
        var force2 = -frameTransform.forward * Mathf.Clamp(TrackTime(), 0f, maxForce / 2);
        leftWheelRigidbody.AddForce(force);
        rightWheelRigidbody.AddForce(force);
    }

    private void MoveForwardRight()
    {
        var forceLeft = frameTransform.forward * Mathf.Clamp(TrackTime(), 0f, maxForce / 2);
        var forceRight = frameTransform.forward * Mathf.Clamp(5f * TrackTime(), 0f, maxForce);
        leftWheelRigidbody.AddForce(forceLeft);
        rightWheelRigidbody.AddForce(forceRight);
    }
    
    private void MoveForwardLeft()
    {
        var forceLeft = frameTransform.forward * Mathf.Clamp(5f * TrackTime(), 0f, maxForce);
        var forceRight = frameTransform.forward * Mathf.Clamp(TrackTime(), 0f, maxForce / 2);
        leftWheelRigidbody.AddForce(forceLeft);
        rightWheelRigidbody.AddForce(forceRight);
    }
    
    private void MoveBackwardLeft()
    {
        var forceLeft = -frameTransform.forward * Mathf.Clamp(5f * TrackTime(), 0f, maxForce);
        var forceRight = -frameTransform.forward * Mathf.Clamp(TrackTime(), 0f, maxForce / 2);
        leftWheelRigidbody.AddForce(forceLeft);
        rightWheelRigidbody.AddForce(forceRight);
    }
    
    public void MoveBackwardRight()
    {
        var forceLeft = -frameTransform.forward * Mathf.Clamp(TrackTime(), 0f, maxForce / 2);
        var forceRight = -frameTransform.forward * Mathf.Clamp(5f * TrackTime(), 0f, maxForce);
        leftWheelRigidbody.AddForce(forceLeft);
        rightWheelRigidbody.AddForce(forceRight);
    }
}
