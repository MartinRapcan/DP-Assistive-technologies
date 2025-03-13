using UnityEngine;

public class MeshAnimator : MonoBehaviour
{
    [SerializeField] private Rigidbody frameRb;
    
    [SerializeField] private Rigidbody wheelLeftRb;
    [SerializeField] private Rigidbody wheelRightRb;   

    [SerializeField] private Transform casterLeftMesh;
    [SerializeField] private Transform casterRightMesh;
    
    [SerializeField] private Transform casterHandleLeftMesh;
    [SerializeField] private Transform casterHandleRightMesh;
    
    [SerializeField] private Rigidbody casterLeftRb;
    [SerializeField] private Rigidbody casterRightRb;
    
    [Range(0.1f, 10f)][SerializeField] private float rotationSmoothness = 2f;
    private float _currentAngle;
    
    [SerializeField] private Movement movement;
    
    private void Start()
    {
        _currentAngle = transform.eulerAngles.y;
    }
    
    private void Update()
    {
        if (movement.direction == Direction.None) return;
        RotateHandle(casterLeftRb, casterHandleLeftMesh, 180f);
        RotateHandle(casterRightRb, casterHandleRightMesh);
        RotateCaster();
    }

    private void RotateHandle(Rigidbody rb, Transform t, float angleOffset = 0)
    {
        // Get the velocity of the ball
        Vector3 velocity = rb.velocity;

        // Normalize the velocity to get just the direction (magnitude of 1)
        Vector3 moveDirection = velocity.normalized;
        
        Debug.DrawRay(rb.position, moveDirection * 10, Color.red);
        
        // Get the angle of the direction
        float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        
        // Define the target rotation
        Quaternion targetRotation = Quaternion.Euler(0, angle + angleOffset, 0);

        // Smoothly interpolate from the current rotation to the target rotation
        t.rotation = Quaternion.Slerp(t.rotation, targetRotation, Time.deltaTime * 3f); // Adjust the speed here
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