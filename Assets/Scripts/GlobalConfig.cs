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
    public InterfaceType interfaceType = InterfaceType.None;
    public NavigationType navigationType = NavigationType.Manual;
    
    [SerializeField] private Rigidbody frameRb;
    [SerializeField] private GameObject minimap;
    
    private void Awake()
    {
        if (navigationType != NavigationType.Auto)
        {
            minimap.SetActive(false);
            var navigationScript = GetComponent<Navigation>();
            var navMeshAgent = GetComponent<NavMeshAgent>();
            
            navigationScript.enabled = false;
            navMeshAgent.enabled = false;
            return;
        }
        
        var interfaceTransform = transform.Find("Interface");
        if (interfaceTransform != null)
        {
            interfaceTransform.gameObject.SetActive(false);
        }
    }
}
