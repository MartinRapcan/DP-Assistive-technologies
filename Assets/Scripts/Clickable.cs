using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clickable : MonoBehaviour
{
    [SerializeField] private float alphaThreshold = 0.1f;
    
    // Start is called before the first frame update
    private void Start()
    {       
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
