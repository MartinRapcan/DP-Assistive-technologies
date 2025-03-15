using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    private void Update()
    {
        transform.position = target.position; 
        Debug.Log($"Position: {transform.position}, Target: {target.position}");
    }
}
