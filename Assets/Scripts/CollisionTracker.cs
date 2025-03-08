using System;
using UnityEngine;

public class CollisionTracker : MonoBehaviour
{
    [SerializeField] private InteractionsCounter interactionsCounter; // Reference to InteractionsCounter script
    [SerializeField] private Movement movement; // Reference to Movement script

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("Obstacle") || !interactionsCounter.hasStarted ||
            interactionsCounter.hasEnded) return;
        interactionsCounter.IncrementNumberOfCollisions();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartTrigger") && !interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
        {
            Debug.Log("Start trigger");
            interactionsCounter.SetInteractionType(movement.direction);
            interactionsCounter.hasStarted = true;  // Set hasStarted via property
        }
        else if (other.CompareTag("EndTrigger") && interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
        {
            Debug.Log("End trigger");
            interactionsCounter.SetInteractionType(movement.direction);
            interactionsCounter.hasEnded = true;  // Set hasEnded via property
        }
    }
}