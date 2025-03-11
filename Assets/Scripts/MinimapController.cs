using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [SerializeField] private Canvas minimapCorner;
    [SerializeField] private Canvas minimapPreview;
    
    private MinimapType _minimapType = MinimapType.Corner;
    
    private void Start()
    {
        minimapCorner.enabled = true;
        minimapPreview.enabled = false;
    }
    
    private void Update()
    {
        switch (_minimapType)
        {
            case MinimapType.Corner:
                minimapCorner.enabled = true;
                minimapPreview.enabled = false;
                break;
            case MinimapType.Preview:
                minimapCorner.enabled = false;
                minimapPreview.enabled = true;
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
    
    public void OpenMinimap()
    {
        _minimapType = MinimapType.Preview;
    }
    
    public void CloseMinimap()
    {
        _minimapType = MinimapType.Corner;
    }
}
