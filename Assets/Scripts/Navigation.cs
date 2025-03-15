using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    [Header("Rigidbody Components")] 
    [SerializeField] private Rigidbody leftWheelRigidbody;
    [SerializeField] private Rigidbody rightWheelRigidbody;
    [SerializeField] private Rigidbody frameRb;
    [SerializeField] private Rigidbody leftCasterRb;
    [SerializeField] private Rigidbody rightCasterRb;

    [Header("Navigation")] [SerializeField]
    private NavMeshAgent navMeshAgent;

    [Header("Wheelchair Motors")] [SerializeField]
    private HingeJoint leftHinge;
    [SerializeField] private HingeJoint rightHinge;

    public NavigationState navigationState { get; set; } = NavigationState.Stationary;
    private JointMotor _leftMotor;
    private JointMotor _rightMotor;
    private bool _hasPath = false;
    private LineRenderer _lineRenderer;
    private readonly List<GameObject> _cornerMarkers = new List<GameObject>(); // Stores current markers
    private List<Vector3> _pathPoints;
    private float _currentVelocity = 0f;
    private float? _brakingDistance = null;

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
        if (_hasPath && _pathPoints.Count > 0)
        {
            switch (navigationState)
            {
                case NavigationState.Rotating:
                    RotateTowardsCurrentPoint();
                    break;
                case NavigationState.Moving:
                    MoveTowardsCurrentPoint();
                    break;
                    
                // case NavigationState.Stationary:
                //     StopWheels();
                //     break;
            }
        }
    }
    
    private float _maxRotationSpeed = 20f; // Maximum wheel rotation speed
    private float _rotationSpeed = 4.0f;
    private float _rotationThreshold = 0.5f;
    private float _initialRotationAngle = 0f;

    // private void RotateTowardsCurrentPoint()
    //  {
    //      Vector3 targetPoint = _pathPoints.First();
    //      Vector3 directionToTarget = targetPoint - frameRb.position;
    //      directionToTarget.y = 0; // Ignore height difference
    //
    //      if (directionToTarget.magnitude < 0.1f)
    //      {
    //          navigationState = NavigationState.Moving;
    //          return; // Already at the point
    //      }
    //
    //      // Calculate the desired rotation
    //      Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
    //      
    //      // Calculate the angle between the current and target rotation
    //      float angleToTarget = Quaternion.Angle(frameRb.rotation, targetRotation);
    //      
    //      // **Check if we need to rotate**
    //      if (angleToTarget < _rotationThreshold)
    //      {
    //          // Stop rotation when facing the target
    //          _leftMotor.targetVelocity = 0;
    //          _rightMotor.targetVelocity = 0;
    //          leftHinge.motor = _leftMotor;
    //          rightHinge.motor = _rightMotor;
    //          frameRb.angularVelocity = Vector3.zero;
    //          _currentVelocity = 0f;
    //          // frameRb.transform.rotation = targetRotation;
    //          
    //          navigationState = NavigationState.Moving;  // Start moving
    //          return;
    //      }
    //
    //      // Determine turn direction (left or right)
    //      Vector3 forward = transform.forward;
    //      forward.y = 0;
    //      forward.Normalize();
    //      float signedAngle = Vector3.SignedAngle(forward, directionToTarget.normalized, Vector3.up);
    //
    //      // Adjust rotation speed dynamically based on angle
    //      // float velocity = Mathf.Clamp(_rotationSpeed * (angleToTarget / 90f), 10f, _maxRotationSpeed);
    //      _currentVelocity = Mathf.MoveTowards(_currentVelocity, _maxRotationSpeed, _rotationSpeed * Time.deltaTime);
    //
    //      Debug.Log($"Signed angle: {signedAngle}");
    //      
    //      if (signedAngle > 0)
    //      {
    //          // Rotate Right
    //          _leftMotor.targetVelocity = _currentVelocity;
    //          _rightMotor.targetVelocity = -_currentVelocity;
    //      }
    //      else
    //      {
    //          // Rotate Left
    //          _rightMotor.targetVelocity = _currentVelocity;
    //          _leftMotor.targetVelocity = -_currentVelocity;
    //      }
    //
    //      // Apply motor force
    //      _leftMotor.force = 1000f;
    //      _rightMotor.force = 1000f;
    //      leftHinge.motor = _leftMotor;
    //      rightHinge.motor = _rightMotor;
    //  }
    
    private void RotateTowardsCurrentPoint()
{
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
    
    Debug.Log($"Angle: {angleToTarget:F2}, Progress: {rotationProgress:F2}, Speed Mult: {speedMultiplier:F2}");
    
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
     
    private float _moveSpeed = 40f; // Maximum move speed
    private float _stopDistanceThreshold = 0.08f; // Distance to stop at
    
    private void MoveTowardsCurrentPoint()
    {
        Vector3 targetPoint = _pathPoints.First();
        Vector3 directionToTarget = targetPoint - frameRb.position;
        directionToTarget.y = 0; // Ignore height difference

        float distanceToTarget = directionToTarget.magnitude;
        Debug.Log($"Distance to target: {distanceToTarget}");

        // **Stop if we reached the target**
        if (distanceToTarget < _stopDistanceThreshold)
        {
            Debug.Log("Reached target point. Stopping.");
        
            _leftMotor.targetVelocity = 0;
            _rightMotor.targetVelocity = 0;
            leftHinge.motor = _leftMotor;
            rightHinge.motor = _rightMotor;
            _currentVelocity = 0f;
            _brakingDistance = null;
            
            // Debug.Log($"Frame position before: {frameRb.position}, target point: {targetPoint}");
            //
            // frameRb.transform.position = new Vector3(targetPoint.x, frameRb.position.y, targetPoint.z);
            
            // Remove the first point from the list
            _pathPoints.RemoveAt(0);
            
            // Stop moving or switch to next point
            navigationState = _pathPoints.Count > 0
                ? NavigationState.Rotating
                : NavigationState.Stationary;
            return;
        }

        // **Acceleration / Deceleration Logic**
        _brakingDistance ??= distanceToTarget / 5;
        
        if (distanceToTarget > _brakingDistance)
        {
            // Accelerate
            _currentVelocity = Mathf.MoveTowards(_currentVelocity, Mathf.Min((float)(_moveSpeed * _brakingDistance * 5f), 200f), 100 * Time.deltaTime);
        }
        else
        {
            // Decelerate
            _currentVelocity = Mathf.MoveTowards(_currentVelocity, _moveSpeed / 2f, 100 * Time.deltaTime);
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
    
    public void SetDestination(Vector3 newDestination)
    {
        // Extract path points from NavMeshPath
        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(newDestination, path);

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
            navigationState = NavigationState.Rotating;

            // Debug visualization of path
            Debug.Log($"Path calculated with {_pathPoints.Count} points (first point skipped)");
        }
        else
        {
            // Handle case where destination is very close and only has one point
            Debug.Log("Destination is very close - no path points to follow");
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

        Debug.Log("Path: ");
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

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision detected with: " + other.gameObject.name);
    }
}

// Coroutine to rotate the wheelchair towards the target position
// private IEnumerator RotateToFace(Vector3 targetPosition)
// {
//     // Get the direction to the target
//     Vector3 targetDirection = targetPosition - transform.position;
//
//     // Calculate the desired rotation
//     Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
//
//     // Calculate the angle between the current and target rotation
//     float angularDistance = Quaternion.Angle(transform.rotation, targetRotation);
//
//     // Calculate the rotation speed to complete the rotation in maxRotationTime seconds
//     float rotationSpeed = angularDistance / maxRotationTime;
//
//     // Rotate towards the target position at the calculated speed
//     float timeElapsed = 0f;
//     while (timeElapsed < maxRotationTime)
//     {
//         transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
//         timeElapsed += Time.deltaTime;
//         yield return null;
//     }
//
//     // Ensure the final rotation is exact (optional)
//     transform.rotation = targetRotation;
//     
//     // Set the flag to indicate that the agent is moving
//     _isMoving = true;
//     
//     // Set the agent's destination (this starts moving the agent immediately)
//     agent.SetDestination(targetPosition);
// }

// Coroutine to make the waypoint move up and down over time
// private IEnumerator MoveWaypointUpDown(GameObject waypoint)
// {
//     float amplitude = 0.1f; // The height of the up and down motion
//     float speed = 1.0f; // The speed of the motion
//     Vector3 initialPosition = waypoint.transform.position;
//
//     while (waypoint != null) // Ensure the waypoint still exists
//     {
//         float newY = initialPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
//         waypoint.transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
//         yield return null;
//     }
// }