using UnityEngine;

public class ChildCollider : MonoBehaviour
{
    [SerializeField] private InteractionsCounter interactionsCounter; // Reference to InteractionsCounter script
    [SerializeField] private Transform followTransform;
    
    private bool _isProcessingTrigger;
    
    private void Update()
    {
        transform.position = followTransform.position;
        transform.rotation = followTransform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If we are already processing a trigger, skip the logic to avoid stack overflow
        if (_isProcessingTrigger) return;
        
        // Mark as processing trigger
        _isProcessingTrigger = true;
        
        try
        {
            if (other.isTrigger)
            {
                if (other.CompareTag("StartTrigger") && !interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
                {
                    Debug.Log("Start trigger");
                    interactionsCounter.hasStarted = true;  // Set hasStarted via property
                }
                else if (other.CompareTag("EndTrigger") && interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
                {
                    Debug.Log("End trigger");
                    interactionsCounter.hasEnded = true;  // Set hasEnded via property
                }
            }
            else
            {
                if (interactionsCounter.hasStarted && !interactionsCounter.hasEnded)
                {
                    // Increment number of collisions if the process has started and not ended
                    interactionsCounter.IncrementNumberOfCollisions();
                }
            }
        }
        finally
        {
            // Ensure flag is reset so it can process future triggers
            _isProcessingTrigger = false;
        }
    }
}