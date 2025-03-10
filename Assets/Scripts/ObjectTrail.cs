using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTrail : MonoBehaviour
{
    private readonly List<Vector3> _points = new List<Vector3>();
    
    [SerializeField] private InteractionsCounter interactionsCounter; // Reference to InteractionsCounter script
    [SerializeField] private Transform targetTransform;       // The transform to track
    // [SerializeField] private float updateInterval = 1.0f;     // Time in seconds between points
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.2f, 0); // Offset for the line

    private const float PipeRadius = 0.01f; // Controls the thickness of your pipe
    private float _timeSinceLastPoint = 0f;
    
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer component is missing!");
            enabled = false;
            return;
        }
        
        // Configure LineRenderer for 3D pipe appearance
        lineRenderer.startWidth = PipeRadius * 2;
        lineRenderer.endWidth = PipeRadius * 2;
        lineRenderer.numCornerVertices = 8; // Controls how rounded the corners are
        lineRenderer.numCapVertices = 8;    // Controls how rounded the ends are
        lineRenderer.alignment = LineAlignment.View; // Makes the line face the camera
    }
    
    private void Update()
    {
        // Make sure we have a target transform
        if (targetTransform == null)
        {
            Debug.LogWarning("Target Transform is not assigned to ObjectTrail script!");
            return;
        }
        
        // Update timer
        _timeSinceLastPoint += Time.deltaTime;
        
        // Add a new point every updateInterval seconds
        if (!interactionsCounter.hasStarted || interactionsCounter.hasEnded) return;
        
        AddPoint(targetTransform.position + offset);
        _timeSinceLastPoint = 0f;
    }
    
    private void AddPoint(Vector3 position)
    {
        // Add the point to our list
        _points.Add(position);
        
        // Update the line renderer
        lineRenderer.positionCount = _points.Count;
        for (var i = 0; i < _points.Count; i++)
        {
            lineRenderer.SetPosition(i, _points[i]);
        }
    }
    
    // Update all positions in LateUpdate to ensure newest position is used
    private void LateUpdate()
    {
        if (_points.Count > 0 && targetTransform && interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
        {
            // Always update the latest point to follow the transform with the offset
            _points[_points.Count - 1] = targetTransform.position + offset;
            lineRenderer.SetPosition(_points.Count - 1, targetTransform.position + offset);
        }
    }
}