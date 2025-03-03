using UnityEngine;

/// <summary>
/// Animates wheelchair meshes to express the physical movement of the rig.
/// </summary>
public class VRWC_MeshAnimator : MonoBehaviour
{
    public Rigidbody casterLeftRB;
    public Rigidbody casterRightRB;

    public Transform casterLeftMesh;
    public Transform casterRightMesh;


    void Update()
    {
        RotateCaster();
    }
    
    void RotateCaster()
    {
        casterLeftMesh.Rotate(-Vector3.right, casterLeftRB.angularVelocity.magnitude);
        casterRightMesh.Rotate(Vector3.right, casterRightRB.angularVelocity.magnitude);
    }
}
