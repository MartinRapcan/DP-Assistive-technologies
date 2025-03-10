using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private float maxRotationTime = 2f; // Maximum rotation time (in seconds)
    [SerializeField] private Transform casterLeftMesh;
    [SerializeField] private Transform casterRightMesh;
    [SerializeField] private Transform rightWheelTransform;
    [SerializeField] private Transform leftWheelTransform;
    
    private GameObject _currentWaypoint = null; // Reference to the current waypoint
    private bool _isMoving = false; // Flag to check if the agent is moving
    
    private void Update()
    {
        // Perform a raycast from the camera to the mouse position
        if (Input.GetMouseButtonDown(0)) // Detect left mouse button click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.CompareTag("MainCamera")) // Skip the camera collider
                {
                    if (hit.collider.CompareTag("Floor"))
                    {
                        Vector3 hitPoint = hit.point;

                        // Remove any existing waypoint
                        if (_currentWaypoint != null)
                        {
                            Destroy(_currentWaypoint);
                        }

                        // Instantiate a new waypoint at the hit point, offset by 0.5 on the Y-axis
                        Vector3 waypointPosition = hitPoint + new Vector3(0, 0.5f, 0);
                        _currentWaypoint = Instantiate(waypointPrefab, waypointPosition, Quaternion.identity);

                        // hitpoint is the position where the raycast hit the floor
                        Debug.Log("Hit point: " + hitPoint);
                    
                        // // Start a coroutine to make the waypoint move up and down
                        StartCoroutine(MoveWaypointUpDown(_currentWaypoint));

                        _isMoving = true;
                        
                        agent.SetDestination(hitPoint);
                        
                        // // Start rotation to face the target immediately
                        // StartCoroutine(RotateToFace(hitPoint));
                    }
                    break; // Exit loop after first valid hit
                }
            }
        }

        // Check if the agent is close to its destination and remove the waypoint
        if (_currentWaypoint && agent.remainingDistance <= agent.stoppingDistance && _isMoving)
        {
            Destroy(_currentWaypoint);
            _isMoving = false;
        }
    }

    // Coroutine to rotate the wheelchair towards the target position
    private IEnumerator RotateToFace(Vector3 targetPosition)
    {
        // Get the direction to the target
        Vector3 targetDirection = targetPosition - transform.position;

        // Calculate the desired rotation
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        // Calculate the angle between the current and target rotation
        float angularDistance = Quaternion.Angle(transform.rotation, targetRotation);

        // Calculate the rotation speed to complete the rotation in maxRotationTime seconds
        float rotationSpeed = angularDistance / maxRotationTime;

        // Rotate towards the target position at the calculated speed
        float timeElapsed = 0f;
        while (timeElapsed < maxRotationTime)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is exact (optional)
        transform.rotation = targetRotation;
        
        // Set the flag to indicate that the agent is moving
        _isMoving = true;
        
        // Set the agent's destination (this starts moving the agent immediately)
        agent.SetDestination(targetPosition);
    }

    // Coroutine to make the waypoint move up and down over time
    private IEnumerator MoveWaypointUpDown(GameObject waypoint)
    {
        float amplitude = 0.1f; // The height of the up and down motion
        float speed = 1.0f; // The speed of the motion
        Vector3 initialPosition = waypoint.transform.position;

        while (waypoint != null) // Ensure the waypoint still exists
        {
            float newY = initialPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
            waypoint.transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision detected with: " + other.gameObject.name);
    }
}
