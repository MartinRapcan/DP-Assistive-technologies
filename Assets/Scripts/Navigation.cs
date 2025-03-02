using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private GameObject waypointPrefab;
    private GameObject _currentWaypoint = null; // Reference to the current waypoint

    [SerializeField] private float maxRotationTime = 2f; // Maximum rotation time (in seconds)

    void Update()
    {
        // Perform a raycast from the camera to the mouse position
        if (Input.GetMouseButtonDown(0)) // Detect left mouse button click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                // Check if the object hit has the tag "Floor"
                if (hitInfo.collider.CompareTag("Floor"))
                {
                    Vector3 hitPoint = hitInfo.point;

                    // Remove any existing waypoint
                    if (_currentWaypoint != null)
                    {
                        Destroy(_currentWaypoint);
                    }

                    // Instantiate a new waypoint at the hit point, offset by 0.5 on the Y-axis
                    Vector3 waypointPosition = hitPoint + new Vector3(0, 0.5f, 0);
                    _currentWaypoint = Instantiate(waypointPrefab, waypointPosition, Quaternion.identity);

                    // Start a coroutine to make the waypoint move up and down
                    StartCoroutine(MoveWaypointUpDown(_currentWaypoint));

                    // Start rotation to face the target immediately
                    StartCoroutine(RotateToFace(hitPoint));
                }
            }
        }

        // Check if the agent is close to its destination and remove the waypoint
        if (_currentWaypoint != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Destroy(_currentWaypoint);
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
