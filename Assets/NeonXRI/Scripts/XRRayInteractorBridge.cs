using PupilLabs;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRRayInteractorBridge : MonoBehaviour
{
    [SerializeField]
    private XRRayInteractor target;
    [SerializeField]
    private Transform reference;
    [SerializeField]
    private bool registerOnEnable = true;
    
    public void OnGazeDataReady(GazeDataProvider gazeDataProvider)
    {
        if (reference == null)
        {
            reference = Camera.main.transform;
        }

        if (target == null)
        {    
            target = reference.GetComponentInChildren<XRRayInteractor>();
        }
        
        target.transform.position = reference.TransformPoint(gazeDataProvider.GazeRay.origin);
        target.transform.forward = reference.TransformDirection(gazeDataProvider.GazeRay.direction);
    }
    
    protected virtual void OnEnable()
    {
        if (registerOnEnable)
        {
            ServiceLocator.Instance.GazeDataProvider.gazeDataReady.AddListener(OnGazeDataReady);
        }
    }

    protected virtual void OnDisable()
    {
        if (registerOnEnable)
        {
            ServiceLocator.Instance.GazeDataProvider.gazeDataReady.RemoveListener(OnGazeDataReady);
        }
    }
}