using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTrail : MonoBehaviour
{
    private readonly List<Vector3> _points = new List<Vector3>();
    
    [SerializeField] private InteractionTracker interactionsTracker; // Reference to InteractionsCounter script
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineRadius = 0.01f;
    
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
        lineRenderer.startWidth = lineRadius;
        lineRenderer.endWidth = lineRadius;
        lineRenderer.numCornerVertices = 8; // Controls how rounded the corners are
        lineRenderer.numCapVertices = 8;    // Controls how rounded the ends are
        lineRenderer.alignment = LineAlignment.View; // Makes the line face the camera
    }
    
    private void Update()
    {
        // Add a new point if tracking is active
        if (!interactionsTracker.hasStarted || interactionsTracker.hasEnded) return;
        
        // Use this object's world position
        AddPoint(transform.position);
    }
    
    private void AddPoint(Vector3 position)
    {
        // Add the point to our list
        _points.Add(position);
        
        // Update the line renderer
        lineRenderer.positionCount = _points.Count;
        
        // Debug.Log($"Adding point {position} to line renderer. Total points: {_points.Count}");
        
        for (var i = 0; i < _points.Count; i++)
        {
            lineRenderer.SetPosition(i, _points[i]);
        }
    }
    
    // Update all positions in LateUpdate to ensure newest position is used
    private void LateUpdate()
    {
        if (_points.Count > 0 && interactionsTracker.hasStarted && !interactionsTracker.hasEnded)
        {
            // Always update the latest point to follow this transform
            _points[_points.Count - 1] = transform.position;
            lineRenderer.SetPosition(_points.Count - 1, transform.position);
        }
    }
}