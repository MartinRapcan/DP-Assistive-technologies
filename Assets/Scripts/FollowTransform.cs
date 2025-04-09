using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    private Vector3 _offset;

    private void Start()
    {
        _offset = transform.localPosition - target.localPosition;
    }
    
    private void Update()
    {
        Vector3 rotatedOffset = target.localRotation * _offset;
        transform.localPosition = target.localPosition + rotatedOffset;

        transform.rotation = target.rotation;
    }
}
