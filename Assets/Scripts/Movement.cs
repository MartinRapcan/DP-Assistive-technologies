using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    public enum NavigationType
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

    public enum InterfaceType
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
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float maxTorque = 20f;
    [SerializeField] private float stopTime = 0.5f;
    [SerializeField] private InterfaceType interfaceType = InterfaceType.None;
    [SerializeField] private GameObject monitor;
    [SerializeField] private NavigationType navigation = NavigationType.Manual;
    [SerializeField] private Rigidbody casterLeftRb;
    [SerializeField] private Rigidbody casterRightRb;
    [SerializeField] private Transform casterLeftMesh;
    [SerializeField] private Transform casterRightMesh;
    
    
    private Direction _direction = Direction.None;
    private Coroutine _decelerationCoroutine;
    private float _time;

    private void Start()
    {
        
        CameraSetup(); // Set up cameras based on interface type
        
        // Find the child object named "Frame"
        Transform frameTransform = transform.Find("Frame");

        if (frameTransform != null)
        {
            Rigidbody frameRigidbody = frameTransform.GetComponent<Rigidbody>();
            NavMeshAgent frameAgent = frameTransform.GetComponent<NavMeshAgent>();
            Navigation navigationScript = frameTransform.GetComponent<Navigation>();

            if (frameRigidbody != null)
            {
                // Set Rigidbody to kinematic or not based on navigation type
                frameRigidbody.isKinematic = (navigation == NavigationType.Auto);
            }
            else
            {
                Debug.LogWarning("Rigidbody not found on child object 'Frame'.");
            }
            
            if (frameAgent != null)
            {
                // Set NavMeshAgent to enabled or not based on navigation type
                frameAgent.enabled = (navigation == NavigationType.Auto);
                
            }
            else
            {
                Debug.LogWarning("NavMeshAgent not found on child object 'Frame'.");
            }
            
            // Remove the Navigation script if navigation is manual
            if (navigation == NavigationType.Manual && navigationScript != null)
            {
                Destroy(navigationScript);
                Debug.Log("Navigation script removed from 'Frame' due to manual navigation mode.");
            }
        }
        else
        {
            Debug.LogWarning("Child object 'Frame' not found.");
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
        switch (interfaceType)
        {
            case InterfaceType.DoubleCamera:
                cameraFront.enabled = isMovingForward;
                cameraBack.enabled = !isMovingForward;
                cameraTop.enabled = false;
                break;
            case InterfaceType.SingleCamera:
                cameraFront.enabled = true;
                cameraBack.enabled = false;
                cameraTop.enabled = false;
                break;
            case InterfaceType.TopCamera:
                cameraFront.enabled = false;
                cameraBack.enabled = false;
                cameraTop.enabled = true;
                break;
            case InterfaceType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FixedUpdate()
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
        HandleCameraChange(true);
    }

    public void ShouldMoveBackward()
    {
        StartMovement(Direction.Backward);
        HandleCameraChange(false);
    }

    public void ShouldMoveForwardRight()
    {
        StartMovement(Direction.ForwardRight);
        HandleCameraChange(true);
    }
    
    public void ShouldMoveForwardLeft()
    {
        StartMovement(Direction.ForwardLeft);
        HandleCameraChange(true);
    }
    
    public void ShouldMoveBackwardLeft()
    {
        StartMovement(Direction.BackwardLeft);
        HandleCameraChange(false);
    }
    
    public void ShouldMoveBackwardRight()
    {
        StartMovement(Direction.BackwardRight);
        HandleCameraChange(false);
    }

    private void StartMovement(Direction newDirection)
    {
        // Stop any ongoing deceleration when a new movement is initiated
        if (_decelerationCoroutine != null)
        {
            StopCoroutine(_decelerationCoroutine);
            _decelerationCoroutine = null;
        }

        _time = 0f; // Reset time
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
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torque);
    }

    private void TurnLeft()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torque);
    }
    
    private void MoveForward()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);

        // Apply torque to rotate wheels around their local right axis
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torque);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torque);
    }
    
    private void MoveBackward()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);

        // Apply torque to rotate wheels around their local right axis
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * -torque);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * -torque);
    }
    
    private void MoveForwardRight()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque * 1.6f);
        float torqueRight = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);

        // Apply torque to rotate wheels around their local right axis
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torque);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torqueRight);
        Debug.DrawRay(transform.position, leftWheelRigidbody.transform.right * torque, Color.red);
        Debug.DrawRay(transform.position, rightWheelRigidbody.transform.right * torqueRight, Color.red);
    }
    
    private void MoveForwardLeft()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque * 1.6f);
        float torqueLeft = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);

        // Apply torque to rotate wheels around their local right axis
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * torqueLeft);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * torque);
    }
    
    private void MoveBackwardLeft()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque * 1.6f);
        float torqueLeft = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);

        // Apply torque to rotate wheels around their local right axis
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * -torqueLeft);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * -torque);
    }
    
    public void MoveBackwardRight()
    {
        float torque = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque * 1.6f);
        float torqueRight = Mathf.Clamp(3f * TrackTime(), 0f, maxTorque);

        // Apply torque to rotate wheels around their local right axis
        leftWheelRigidbody.AddTorque(leftWheelRigidbody.transform.right * -torque);
        rightWheelRigidbody.AddTorque(rightWheelRigidbody.transform.right * -torqueRight);
        Debug.DrawRay(transform.position, leftWheelRigidbody.transform.right * torque, Color.red);
        Debug.DrawRay(transform.position, rightWheelRigidbody.transform.right * torqueRight, Color.red);
    }
    
    private void RotateWheels(float angle)
    {
        // Rotate the wheels by the specified angle around the Y-axis
        leftWheelRigidbody.transform.rotation = Quaternion.Euler(0, angle, 0);
        rightWheelRigidbody.transform.rotation = Quaternion.Euler(0, angle, 0);
    }
    
    void RotateCaster()
    {
        casterLeftMesh.Rotate(-Vector3.right, casterLeftRb.angularVelocity.magnitude);
        casterRightMesh.Rotate(Vector3.right, casterRightRb.angularVelocity.magnitude);
    }
}
