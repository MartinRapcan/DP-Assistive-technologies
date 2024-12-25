using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private NavMeshAgent agent;
    
    
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
                    Debug.Log($"Ray hit Floor at {hitPoint}");
                    agent.SetDestination(hitPoint); // Move the agent to the hit point
                }
            }
        }
    }
}
