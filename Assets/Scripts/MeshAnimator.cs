using System;
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
    
    [Range(0.1f, 10f)][SerializeField] private float rotationSmoothness = 2f;
    private float _currentAngle;
    
    private void Start()
    {
        _currentAngle = transform.eulerAngles.y;
    }
    
    private void Update()
    { 
        RotateCaster();
    }

    private void FixedUpdate()
    {
        RotateCasterHandle();
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

    // private void RotateCasterHandle()
    // {
    //     // Get the rotation of the wheelchair
    //     float targetAngle = frameRb.rotation.eulerAngles.y;
    //     
    //     // Normalize the angle to always be positive (0 to 360)
    //     if (targetAngle < 0) targetAngle += 360f;
    //     if (_currentAngle < 0) _currentAngle += 360f;
    //     
    //     Debug.Log($"Target angle: {targetAngle}");
    //     Debug.Log($"Current angle: {_currentAngle}");
    //
    //     // Smooth the angle transition to avoid sudden jumps
    //     _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, Time.fixedDeltaTime * rotationSmoothness);
    //     
    //     Debug.Log($"Current angle after lerp: {_currentAngle}");
    //     Debug.Log($"Quaternion Euler: {Quaternion.Euler(0, _currentAngle, 0)}");
    //
    //     // Apply the smoothed rotation to the caster handles
    //     // casterHandleLeftMesh.rotation = Quaternion.Euler(0, _currentAngle + 180f, 0);
    //     casterHandleRightMesh.rotation = Quaternion.Euler(0, -_currentAngle, 0);
    // }
    
    private void RotateCasterHandle()
    {
        // Get the rotation of the wheelchair
        float targetAngle = frameRb.rotation.eulerAngles.y;

        // Smooth the angle transition
        _currentAngle = Mathf.LerpAngle(_currentAngle, targetAngle, Time.deltaTime * rotationSmoothness);

        // Apply the smoothed rotation to the caster handles (no negation)
        casterHandleRightMesh.rotation = Quaternion.Euler(0, _currentAngle, 0);
        casterHandleLeftMesh.rotation = Quaternion.Euler(0, _currentAngle + 180f, 0);
        
        // Get the current local rotation
        Quaternion currentRotation = casterHandleRightMesh.localRotation;

        // Create a new rotation with Y inverted
        casterHandleRightMesh.localRotation = new Quaternion(
            currentRotation.x,    // Keep X as is
            -currentRotation.y,   // Invert Y
            currentRotation.z,    // Keep Z as is
            currentRotation.w     // Keep W as is
        );
        casterHandleLeftMesh.localRotation = new Quaternion(
            currentRotation.x, // Keep X as is
            -currentRotation.y, // Invert Y
            currentRotation.z, // Keep Z as is
            currentRotation.w // Keep W as is
        );
    }
}