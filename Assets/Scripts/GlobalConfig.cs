using UnityEngine;

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
    
    private void Awake()
    {
        if (navigationType != NavigationType.Auto) return;
        
        var interfaceTransform = transform.Find("Interface");
        if (interfaceTransform != null)
        {
            interfaceTransform.gameObject.SetActive(false);
        }
    }
}
