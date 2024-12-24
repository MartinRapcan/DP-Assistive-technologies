using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    // [SerializeField]
    Vector3 destination;
    NavMeshAgent agent;
    private Vector3 target = new Vector3(3, 0, 2);

    void Start()
    {
        // Cache agent component and destination
        agent = GetComponent<NavMeshAgent>();
        destination = agent.destination;
    }
    
    void Update()
    {
        // If the target has moved, update the destination
        if (Vector3.Distance(destination, target) > 1.0f)
        {
            destination = target;
            agent.destination = new Vector3(destination.x, destination.y, destination.z);
        }
    }
}
