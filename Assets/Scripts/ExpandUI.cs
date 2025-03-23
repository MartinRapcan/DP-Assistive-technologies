using System.Collections.Generic;
using UnityEngine;

public enum ExpandedButtonType
{
    Up,
    Down,
    Left,
    Right,
    None
}

public enum FunctionState
{
    Active,
    Idle,
}

public class ExpandUI : MonoBehaviour
{
    [SerializeField] private GameObject upButtonExpand;
    [SerializeField] private GameObject downButtonExpand;
    [SerializeField] private GameObject leftButtonExpand;
    [SerializeField] private GameObject rightButtonExpand;
    public Movement movement;
    
    public ExpandedButtonType expandedButtonType { get; set; } = ExpandedButtonType.None;
    
    // Static variable that can be accessed from anywhere
    public FunctionState currentFunctionState = FunctionState.Idle;
    
    private void Start()
    {
        upButtonExpand.SetActive(false);
        downButtonExpand.SetActive(false);
        leftButtonExpand.SetActive(false);
        rightButtonExpand.SetActive(false);
    }
    
    private void SetActiveButtons()
    {
        upButtonExpand.SetActive(true && expandedButtonType == ExpandedButtonType.Up);
        downButtonExpand.SetActive(true && expandedButtonType == ExpandedButtonType.Down);
        leftButtonExpand.SetActive(true && expandedButtonType == ExpandedButtonType.Left);
        rightButtonExpand.SetActive(true && expandedButtonType == ExpandedButtonType.Right);
    }
    
    private void Update()
    {
        SetActiveButtons();
    }
}
