using System;
using UnityEngine;
using UnityEngine.AI;

public enum MinimapType
{
    Corner,
    Preview
}

public enum NavigationType
{
    Auto,
    Manual
}

public enum NavigationState
{
    Rotating,
    Moving,
    Stationary
}

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right,
    Stop,
    None,
    ForwardRight,
    ForwardLeft,
    BackwardLeft,
    BackwardRight
}

public enum InterfaceType
{
    DoubleCamera,
    SingleCamera,
    TopCamera,
    None
}

public class GlobalConfig : MonoBehaviour
{
    [Header("Configuration")]
    public InterfaceType interfaceType = InterfaceType.None;
    public NavigationType navigationType = NavigationType.Manual;
    
    [Header("Conditional Rendering")]
    [SerializeField] private Rigidbody frameRb;
    [SerializeField] private GameObject minimap;
    [SerializeField] private GameObject navMeshAgent;
    
    private void Awake()
    {
        if (navigationType != NavigationType.Auto)
        {
            minimap.SetActive(false);
            var navigationScript = GetComponent<Navigation>();
            
            navigationScript.enabled = false;
            navMeshAgent.SetActive(false);
            return;
        }
        
        var interfaceTransform = transform.Find("Interface");
        if (interfaceTransform != null)
        {
            interfaceTransform.gameObject.SetActive(false);
        }
    }
}
