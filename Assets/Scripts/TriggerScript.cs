using System;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    [SerializeField]
    private Transform displayPosition;
    
    private DisplayOrigin _displayOrigin;
    
    [SerializeField]
    private bool registerCollision = false;
    
    private void Start()
    {
        _displayOrigin = displayPosition.GetComponent<DisplayOrigin>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Frame") && registerCollision)
        {
            _displayOrigin.CurrentOriginType = OriginType.Wheelchair;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Frame") && registerCollision)
        {
            _displayOrigin.CurrentOriginType = OriginType.Camera;
        }
    }
}
