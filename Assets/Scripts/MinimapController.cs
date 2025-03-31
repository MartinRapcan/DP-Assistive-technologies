using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinimapController : MonoBehaviour
{
    [Header("Buttons and Cameras")] [SerializeField]
    private Camera mainCamera;

    [SerializeField] private Camera environmentCamera;
    [SerializeField] private Button minimapMain;

    [Header("Hover Settings")] [SerializeField]
    private float hoverTimeToConfirm = 5f;

    [SerializeField] private float hoverRadiusThreshold = 20f;

    [Header("Navigation Script")] [SerializeField]
    private Navigation navigation;

    // Using external enum from another file
    private int minimapTypeValue = 0; // 0 = Corner, 1 = Preview
    private RectTransform mainButtonRect;

    // For continuous pointer tracking
    private Coroutine pointerTrackingCoroutine;

    // Position tracking variables
    private Vector2 lastHoverPosition;
    private Vector2 confirmPosition;
    private Vector2 lastNormalizedPosition;

    // Hover timers
    private float cornerHoverTimer = 0f;
    private float mainHoverTimer = 0f;
    private float closeHoverTimer = 0f;
    private float confirmHoverTimer = 0f;


}