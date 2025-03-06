using UnityEngine;

public class MeshAnimator : MonoBehaviour
{
    [SerializeField] private Rigidbody wheelLeftRb;
    [SerializeField] private Rigidbody wheelRightRb;   

    [SerializeField] private Transform casterLeftMesh;
    [SerializeField] private Transform casterRightMesh;
    
    private void Update()
    { 
        RotateCaster();
    }
    
    private void RotateCaster()
    {
        // Use the direction of rotation to determine the correct rotation direction
        var leftRotationDirection = Mathf.Sign(Vector3.Dot(wheelLeftRb.angularVelocity, wheelLeftRb.transform.right));
        var rightRotationDirection = Mathf.Sign(Vector3.Dot(wheelRightRb.angularVelocity, wheelRightRb.transform.right));

        // Apply rotation in the correct direction
        casterLeftMesh.Rotate(Vector3.right, leftRotationDirection * wheelLeftRb.angularVelocity.magnitude * 10);
        casterRightMesh.Rotate(Vector3.right, rightRotationDirection * wheelRightRb.angularVelocity.magnitude * 10);
    }
}