using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    public Vector3 destination { get; private set; }
    private LineRenderer _lineRenderer;
    private NavMeshPath _path;
    private bool _hasDestination = false;
    private readonly List<GameObject> _cornerMarkers = new List<GameObject>(); // Stores current markers

    private void Start()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = Color.green;
        _lineRenderer.endColor = Color.green;
        _path = new NavMeshPath();
    }

    private void Update()
    {
        if (_hasDestination)
        {
            DrawPath();
        }
    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        _hasDestination = true;
    }

    private void DrawPath()
    {
        if (agent.CalculatePath(destination, _path) && _path.corners.Length > 1)
        {
            _lineRenderer.positionCount = _path.corners.Length;
            _lineRenderer.SetPositions(_path.corners);

            Debug.Log("Path: ");
            ClearCornerMarkers(); // Remove old markers

            for (int i = 1; i < _path.corners.Length; i++)
            {
                Debug.Log($"Point {i}: {_path.corners[i]}");

                // Only spawn a marker when the _path changes direction
                if (i == 0 || i == _path.corners.Length - 1 || IsDirectionChange(i))
                {
                    SpawnMarker(_path.corners[i]);
                }
            }
        }
    }

    private bool IsDirectionChange(int index)
    {
        if (index <= 0 || index >= _path.corners.Length - 1)
            return false;

        Vector3 previous = _path.corners[index - 1];
        Vector3 current = _path.corners[index];
        Vector3 next = _path.corners[index + 1];

        Vector3 dir1 = (current - previous).normalized;
        Vector3 dir2 = (next - current).normalized;

        // Check if there is a significant direction change
        return Vector3.Dot(dir1, dir2) < 0.98f; // Adjust threshold if needed
    }

    private void SpawnMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * 0.2f; // Small sphere
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
