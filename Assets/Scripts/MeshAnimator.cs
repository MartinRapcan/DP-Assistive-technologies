using UnityEngine;

public class MeshAnimator : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    [SerializeField] private Rigidbody casterLeftRb;
    [SerializeField] private Rigidbody casterRightRb;

    [SerializeField] private Transform casterLeftMesh;
    [SerializeField] private Transform casterRightMesh;

    private Vector3 _offset;

    private void Start()
    {
        _offset = transform.localPosition - target.localPosition;
    }

    private void Update()
    {
            RotateCaster();
            
            Vector3 rotatedOffset = target.localRotation * _offset;
            transform.localPosition = target.localPosition + rotatedOffset;

            transform.rotation = target.rotation;
    }
    
    private void RotateCaster()
    {
        casterLeftMesh.Rotate(-Vector3.right, casterLeftRb.angularVelocity.magnitude);
        casterRightMesh.Rotate(Vector3.right, casterRightRb.angularVelocity.magnitude);
    }
}