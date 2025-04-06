using UnityEngine;

public class CollisionTracker : MonoBehaviour
{
    [SerializeField] private InteractionTracker interactionTracker;
    [SerializeField] private Movement movement;
    [SerializeField] private GlobalConfig globalConfig;
    [SerializeField] private BoxCollider boxCollider;
    [SerializeField] private Navigation navigation;
    
    private void Start()
    {
        if(globalConfig.navigationType == NavigationType.Manual)
        {
            boxCollider.center = boxCollider.center + new Vector3(0, 0, 0.1f);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (globalConfig.navigationType == NavigationType.Auto)
        {
            navigation.StopNavigation();
        }
        if (!other.gameObject.CompareTag("Obstacle")) return;
        interactionTracker.IncrementNumberOfCollisions();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartTrigger") && !interactionTracker.hasStarted && !interactionTracker.hasEnded)
        {
            interactionTracker.hasStarted = true;
            if (movement.direction != Direction.None)
            {
                interactionTracker.SetInteractionType(movement.direction.ToString(), movement._speed);
            }
        }
        else if (other.CompareTag("EndTrigger") && interactionTracker.hasStarted && !interactionTracker.hasEnded)
        {
            interactionTracker.hasEnded = true;
            if (movement.direction != Direction.None)
            {
                interactionTracker.SetInteractionType(movement.direction.ToString(), movement._speed);
            }
        }
    }
}