using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    [Header("Rigidbody Components")] [SerializeField]
    private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Rigidbody frameRb;
    [SerializeField] private Rigidbody leftCasterRb;
    [SerializeField] private Rigidbody rightCasterRb;

    [Header("Navigation")] [SerializeField]
    private NavMeshAgent navMeshAgent;

    [Header("Wheelchair Motors")] [SerializeField]
    private HingeJoint leftHinge;
    [SerializeField] private HingeJoint rightHinge;

    [Header("Navigation Settings")] [SerializeField]
    private float _maxRotationSpeed = 40f; // Maximum wheel rotation speed
    [SerializeField] private float _rotationSpeed = 4.0f;
    [SerializeField] private float _rotationThreshold = 0.5f;
    [SerializeField] private float _initialRotationAngle = 0f;
    [SerializeField] private float stopTime = 0.1f;
    [SerializeField] private float _maxMovementSpeed = 150f; // Maximum move speed
    
    public NavigationState navigationState { get; set; } = NavigationState.Stationary;
    private JointMotor _leftMotor;
    private JointMotor _rightMotor;
    private bool _hasPath = false;
    private bool _isMoving = false;
    private LineRenderer _lineRenderer;
    private readonly List<GameObject> _cornerMarkers = new List<GameObject>(); // Stores current markers
    private List<Vector3> _pathPoints;
    private float _currentVelocity = 0f;
    private float? _brakingDistance = null;
    private Coroutine _decelerationCoroutine;
    private float _stopDistanceThreshold = 0.08f; // Distance to stop at// Time to stop the wheels

    private void Start()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.02f;
        _lineRenderer.endWidth = 0.02f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.green;
        _lineRenderer.endColor = Color.green;

        // Configure and apply motors
        _leftMotor = leftHinge.motor;
        _rightMotor = leftHinge.motor;

        leftHinge.useMotor = true;
        rightHinge.useMotor = true;
    }

    private void Update()
    {
        if (_hasPath)
        {
            DrawPath();
        }

        Debug.DrawRay(frameRb.position, frameRb.transform.forward * 20f, Color.red);

        navMeshAgent.Warp(frameRb.position);
    }

    private void FixedUpdate()
    {
        if (_hasPath && _pathPoints.Count > 0 && _decelerationCoroutine == null)
        {
            switch (navigationState)
            {
                case NavigationState.Rotating:
                    RotateTowardsCurrentPoint();
                    break;
                case NavigationState.Moving:
                    MoveTowardsCurrentPoint();
                    break;
            }
        }
    }

    private void RotateTowardsCurrentPoint()
    {
        _isMoving = true;
        Vector3 targetPoint = _pathPoints.First();
        Vector3 directionToTarget = targetPoint - frameRb.position;
        directionToTarget.y = 0; // Ignore height difference

        if (directionToTarget.magnitude < 0.1f)
        {
            navigationState = NavigationState.Moving;
            return; // Already at the point
        }

        // Calculate the desired rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Calculate the angle between the current and target rotation
        float angleToTarget = Quaternion.Angle(frameRb.rotation, targetRotation);

        // Check if we need to rotate
        if (angleToTarget < _rotationThreshold)
        {
            // Hard stop rotation when close enough
            _leftMotor.targetVelocity = 0;
            _rightMotor.targetVelocity = 0;
            leftHinge.motor = _leftMotor;
            rightHinge.motor = _rightMotor;
            frameRb.angularVelocity = Vector3.zero;
            _currentVelocity = 0f;

            navigationState = NavigationState.Moving;
            return;
        }

        // Use quaternion math for determining rotation direction
        Quaternion rotationDifference = targetRotation * Quaternion.Inverse(frameRb.rotation);
        rotationDifference.ToAngleAxis(out float angle, out Vector3 axis);

        // Normalize to -180 to 180 range
        if (angle > 180f)
        {
            angle -= 360f;
        }

        float turnDirection = Mathf.Sign(angle) * Mathf.Sign(Vector3.Dot(axis, Vector3.up));

        // Remember the initial angle when we start rotating (if not already set)
        if (_initialRotationAngle == 0 || angleToTarget > _initialRotationAngle)
        {
            _initialRotationAngle = angleToTarget;
        }

        // Calculate what percentage of the total rotation we've completed
        float rotationProgress = 1f - (angleToTarget / _initialRotationAngle);

        // Speed profile: maintain high speed for first 4/5, then decelerate in last 1/5
        float speedMultiplier;

        if (rotationProgress < 0.8f)
        {
            // First 4/5 of rotation - maintain high speed
            speedMultiplier = 1.0f;
        }
        else
        {
            // Last 1/5 of rotation - decelerate quickly
            // Map 0.8-1.0 range to 1.0-0.0 for deceleration
            float decelerationProgress = (rotationProgress - 0.8f) * 5f; // Rescale 0.8-1.0 to 0-1
            speedMultiplier = 1.0f - (decelerationProgress * decelerationProgress); // Quadratic slowdown
        }

        // Calculate target velocity with the new speed profile
        float targetVelocity = _maxRotationSpeed * speedMultiplier;

        // Smooth velocity change
        _currentVelocity = Mathf.MoveTowards(_currentVelocity, targetVelocity, _rotationSpeed * Time.deltaTime);

        // Debug.Log($"Angle: {angleToTarget:F2}, Progress: {rotationProgress:F2}, Speed Mult: {speedMultiplier:F2}");

        // Apply motor forces based on calculated direction
        if (turnDirection > 0)
        {
            // Rotate Right
            _leftMotor.targetVelocity = _currentVelocity;
            _rightMotor.targetVelocity = -_currentVelocity;
        }
        else
        {
            // Rotate Left
            _rightMotor.targetVelocity = _currentVelocity;
            _leftMotor.targetVelocity = -_currentVelocity;
        }

        // Apply motor force
        _leftMotor.force = 1000f;
        _rightMotor.force = 1000f;
        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;
    }

    private void MoveTowardsCurrentPoint()
    {
        Vector3 targetPoint = _pathPoints.First();
        Vector3 directionToTarget = targetPoint - frameRb.position;
        directionToTarget.y = 0; // Ignore height difference

        float distanceToTarget = directionToTarget.magnitude;
        // Debug.Log($"Distance to target: {distanceToTarget}");

        // **Stop if we reached the target**
        if (distanceToTarget < _stopDistanceThreshold)
        {
            _leftMotor.targetVelocity = 0;
            _rightMotor.targetVelocity = 0;
            leftHinge.motor = _leftMotor;
            rightHinge.motor = _rightMotor;
            _currentVelocity = 0f;
            _brakingDistance = null;

            // Remove the first point from the list
            _pathPoints.RemoveAt(0);

            // Stop moving or switch to next point
            navigationState = _pathPoints.Count > 0
                ? NavigationState.Rotating
                : NavigationState.Stationary;
            
            _isMoving = _pathPoints.Count > 0;
            return;
        }

        // **Acceleration / Deceleration Logic**
        _brakingDistance ??= distanceToTarget / 4;

        if (distanceToTarget > _brakingDistance)
        {
            // Accelerate
            _currentVelocity = Mathf.MoveTowards(_currentVelocity, _maxMovementSpeed, _maxMovementSpeed / 4f * Time.deltaTime);
        }
        else
        {
            // Decelerate
            _currentVelocity = Mathf.MoveTowards(_currentVelocity, _maxMovementSpeed  / 3f, _maxMovementSpeed / 2f * Time.deltaTime);
        }

        // **Apply motor speeds**
        _leftMotor.targetVelocity = _currentVelocity;
        _rightMotor.targetVelocity = _currentVelocity;

        // Apply motor force
        _leftMotor.force = 1000f;
        _rightMotor.force = 1000f;
        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;
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
        navigationState = NavigationState.Rotating;
    
        // Clear the coroutine reference
        _decelerationCoroutine = null;
    }

    public void SetDestination(Vector3 newDestination)
    {
        // Extract path points from NavMeshPath
        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(newDestination, path);

        // foreach (Vector3 corner in path.corners)
        // {
        //     Debug.Log($"Corner: {corner}");
        // }


        // Skip the first point (agent's current position)
        if (path.corners.Length > 1)
        {
            // Create array without the first point
            _pathPoints = new List<Vector3>();
            for (int i = 1; i < path.corners.Length; i++)
            {
                _pathPoints.Add(new Vector3(path.corners[i].x, 0f, path.corners[i].z));
            }

            _hasPath = true;
            if (navigationState != NavigationState.Stationary)
            {
                // Stop the current movement
                StopMoving();
            }
            else
            {
                navigationState = NavigationState.Rotating;
            }
        }
        else
        {
            // Handle case where destination is very close and only has one point
            // Debug.Log("Destination is very close - no path points to follow");
            _hasPath = false;
            navigationState = NavigationState.Stationary;
        }
    }

    private void DrawPath()
    {
        _lineRenderer.positionCount = _pathPoints.Count;
        // Set the positions of the line renderer offset y-axis slightly
        for (int i = 0; i < _pathPoints.Count; i++)
        {
            _lineRenderer.SetPosition(i, new Vector3(_pathPoints[i].x, 0.05f, _pathPoints[i].z));
        }

        // Debug.Log("Path: ");
        ClearCornerMarkers(); // Remove old markers

        for (int i = 0; i < _pathPoints.Count; i++)
        {
            // Debug.Log($"Point {i}: {_pathPoints[i]}");

            // Only spawn a marker when the _path changes direction
            if (i == 0 || i == _pathPoints.Count - 1 || IsDirectionChange(i))
            {
                SpawnMarker(_pathPoints[i]);
            }
        }
    }

    private bool IsDirectionChange(int index)
    {
        if (index <= 0 || index >= _pathPoints.Count - 1)
            return false;

        Vector3 previous = _pathPoints[index - 1];
        Vector3 current = _pathPoints[index];
        Vector3 next = _pathPoints[index + 1];

        Vector3 dir1 = (current - previous).normalized;
        Vector3 dir2 = (next - current).normalized;

        // Check if there is a significant direction change
        return Vector3.Dot(dir1, dir2) < 0.98f; // Adjust threshold if needed
    }

    private void SpawnMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = new Vector3(position.x, 0.05f, position.z); // Slightly above ground
        marker.transform.localScale = Vector3.one * 0.05f; // Small sphere
        marker.GetComponent<Renderer>().material.color = Color.red; // Red color
        Destroy(marker.GetComponent<Collider>()); // Remove unnecessary collider
        _cornerMarkers.Add(marker);
    }

    private void ClearCornerMarkers()
    {
        foreach (GameObject marker in _cornerMarkers)
        {
            Destroy(marker);
        }

        _cornerMarkers.Clear();
    }

    public void ClearDestination()
    {
        _hasPath = false;
        _lineRenderer.positionCount = 0;
        ClearCornerMarkers();
    }

    public void StopNavigation()
    {
        _leftMotor.targetVelocity = 0;
        _rightMotor.targetVelocity = 0;
        leftHinge.motor = _leftMotor;
        rightHinge.motor = _rightMotor;

        frameRb.angularVelocity = Vector3.zero;
        frameRb.velocity = Vector3.zero;

        leftCasterRb.angularVelocity = Vector3.zero;
        rightCasterRb.angularVelocity = Vector3.zero;

        leftCasterRb.velocity = Vector3.zero;
        rightCasterRb.velocity = Vector3.zero;

        _currentVelocity = 0f;

        navigationState = NavigationState.Stationary;

        ClearDestination();
    }
}