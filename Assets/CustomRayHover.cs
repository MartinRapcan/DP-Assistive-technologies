// using System;
// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.EventSystems;
//
// /// <summary>
// /// A simplified version of XRInteractorReticleVisual that simply logs the hit position
// /// when the XR Ray Interactor hovers over a Canvas in world space.
// /// </summary>
// [AddComponentMenu("XR/Visual/Custom Ray Hover")]
// [DisallowMultipleComponent]
// [RequireComponent(typeof(XRRayInteractor))]
// public class CustomRayHover : MonoBehaviour
// {
//     [SerializeField]
//     private LayerMask m_RaycastMask = -1;
//     /// <summary>
//     /// Layer mask for ray cast.
//     /// </summary>
//     public LayerMask raycastMask
//     {
//         get => m_RaycastMask;
//         set => m_RaycastMask = value;
//     }
//
//     [SerializeField]
//     private bool m_LogLocalCanvasPosition = true;
//     /// <summary>
//     /// Whether to log the local position relative to the Canvas
//     /// instead of the world position.
//     /// </summary>
//     public bool logLocalCanvasPosition
//     {
//         get => m_LogLocalCanvasPosition;
//         set => m_LogLocalCanvasPosition = value;
//     }
//
//     [SerializeField]
//     private float m_MaxRaycastDistance = 10f;
//     /// <summary>
//     /// The max distance to Raycast from this Interactor.
//     /// </summary>
//     public float maxRaycastDistance
//     {
//         get => m_MaxRaycastDistance;
//         set => m_MaxRaycastDistance = value;
//     }
//
//     private XRRayInteractor m_RayInteractor;
//     private Vector3 m_LastHitPosition;
//     private Vector3 m_LastLocalPosition;
//     private Canvas m_LastHitCanvas;
//     private bool m_HadHitLastFrame;
//
//     /// <summary>
//     /// See <see cref="MonoBehaviour"/>.
//     /// </summary>
//     protected void Awake()
//     {
//         TryGetComponent(out m_RayInteractor);
//     }
//
//     /// <summary>
//     /// See <see cref="MonoBehaviour"/>.
//     /// </summary>
//     protected void Update()
//     {
//         if (m_RayInteractor == null || m_RayInteractor.isActiveAndEnabled == false)
//             return;
//
//         // Skip if blocked by interaction within group
//         if (m_RayInteractor.disableVisualsWhenBlockedInGroup && m_RayInteractor.IsBlockedByInteractionWithinGroup())
//             return;
//
//         CheckForCanvasHit();
//     }
//
//     private void CheckForCanvasHit()
//     {
//         bool hasHit = false;
//         Vector3 hitPosition = Vector3.zero;
//         Canvas hitCanvas = null;
//         Vector3 localPosition = Vector3.zero;
//
//         if (m_RayInteractor.TryGetCurrentRaycast(
//             out RaycastHit? raycastHit,
//             out int raycastEndpointIndex,
//             out UnityEngine.EventSystems.RaycastResult? uiRaycastHit,
//             out int uiRaycastEndpointIndex,
//             out bool isUIHitClosest))
//         {
//             if (isUIHitClosest && uiRaycastHit.HasValue)
//             {
//                 // Hit a UI element
//                 var hit = uiRaycastHit.Value;
//                 hitPosition = hit.worldPosition;
//                 hasHit = true;
//
//                 // Try to get the Canvas component from the gameObject or its parents
//                 Transform canvasTransform = hit.gameObject.transform;
//                 while (canvasTransform != null)
//                 {
//                     if (canvasTransform.TryGetComponent(out Canvas canvas))
//                     {
//                         hitCanvas = canvas;
//                         
//                         // Calculate local position relative to canvas
//                         localPosition = hitCanvas.transform.InverseTransformPoint(hitPosition);
//                         break;
//                     }
//                     canvasTransform = canvasTransform.parent;
//                 }
//             }
//             else if (raycastHit.HasValue)
//             {
//                 // Check if we hit a Canvas in the 3D world
//                 var hit = raycastHit.Value;
//                 hitPosition = hit.point;
//                 
//                 // Try to get Canvas from the hit object
//                 Canvas canvas = null;
//                 if (hit.transform.TryGetComponent(out canvas))
//                 {
//                     hitCanvas = canvas;
//                     hasHit = true;
//                 }
//                 else
//                 {
//                     Canvas parentCanvas = hit.transform.GetComponentInParent<Canvas>();
//                     if (parentCanvas != null)
//                     {
//                         hitCanvas = parentCanvas;
//                         hasHit = true;
//                     }
//                 }
//                 
//                 // Calculate local position relative to canvas if we found one
//                 if (hitCanvas != null)
//                 {
//                     localPosition = hitCanvas.transform.InverseTransformPoint(hitPosition);
//                 }
//             }
//         }
//
//         // Log positions only when there's a change or we just hit/left the canvas
//         if (hasHit && 
//             (hitCanvas != m_LastHitCanvas || 
//              !m_HadHitLastFrame || 
//              Vector3.Distance(hitPosition, m_LastHitPosition) > 0.001f))
//         {
//             if (m_LogLocalCanvasPosition && hitCanvas != null)
//             {
//                 Debug.Log($"Canvas Hit (Local): {localPosition} on Canvas: {hitCanvas.name}");
//             }
//             else
//             {
//                 Debug.Log($"Canvas Hit (World): {hitPosition}");
//             }
//
//             m_LastHitPosition = hitPosition;
//             m_LastLocalPosition = localPosition;
//             m_LastHitCanvas = hitCanvas;
//         }
//         else if (!hasHit && m_HadHitLastFrame)
//         {
//             Debug.Log("Ray no longer hitting Canvas");
//         }
//
//         m_HadHitLastFrame = hasHit;
//     }
// }

using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;

/// <summary>
/// A simplified version of XRInteractorReticleVisual that simply logs the hit position
/// when the XR Ray Interactor hovers over a Canvas in world space.
/// </summary>
[AddComponentMenu("XR/Visual/Custom Ray Hover")]
[DisallowMultipleComponent]
[RequireComponent(typeof(XRRayInteractor))]
public class CustomRayHover : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_RaycastMask = -1;
    /// <summary>
    /// Layer mask for ray cast.
    /// </summary>
    public LayerMask raycastMask
    {
        get => m_RaycastMask;
        set => m_RaycastMask = value;
    }

    [SerializeField]
    private bool m_LogLocalCanvasPosition = true;
    /// <summary>
    /// Whether to log the local position relative to the Canvas
    /// instead of the world position.
    /// </summary>
    public bool logLocalCanvasPosition
    {
        get => m_LogLocalCanvasPosition;
        set => m_LogLocalCanvasPosition = value;
    }

    [SerializeField]
    private float m_MaxRaycastDistance = 10f;
    /// <summary>
    /// The max distance to Raycast from this Interactor.
    /// </summary>
    public float maxRaycastDistance
    {
        get => m_MaxRaycastDistance;
        set => m_MaxRaycastDistance = value;
    }

    private XRRayInteractor m_RayInteractor;
    private Vector3 m_LastHitPosition;
    private Vector3 m_LastLocalPosition;
    private Canvas m_LastHitCanvas;
    private bool m_HadHitLastFrame;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void Awake()
    {
        TryGetComponent(out m_RayInteractor);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void Update()
    {
        if (m_RayInteractor == null || m_RayInteractor.isActiveAndEnabled == false)
            return;

        // Skip if blocked by interaction within group
        if (m_RayInteractor.disableVisualsWhenBlockedInGroup && m_RayInteractor.IsBlockedByInteractionWithinGroup())
            return;

        CheckForCanvasHit();
    }

    private void CheckForCanvasHit()
    {
        bool hasHit = false;
        Vector3 hitPosition = Vector3.zero;
        Canvas hitCanvas = null;
        Vector3 localPosition = Vector3.zero;
        bool isMinimapHit = false;

        if (m_RayInteractor.TryGetCurrentRaycast(
            out RaycastHit? raycastHit,
            out int raycastEndpointIndex,
            out UnityEngine.EventSystems.RaycastResult? uiRaycastHit,
            out int uiRaycastEndpointIndex,
            out bool isUIHitClosest))
        {
            if (isUIHitClosest && uiRaycastHit.HasValue)
            {
                // Hit a UI element
                var hit = uiRaycastHit.Value;
                hitPosition = hit.worldPosition;
                
                // Check if the hit object or any of its parents has the "MinimapMain" tag
                Transform hitTransform = hit.gameObject.transform;
                while (hitTransform != null)
                {
                    if (hitTransform.CompareTag("MinimapMain"))
                    {
                        isMinimapHit = true;
                        break;
                    }
                    hitTransform = hitTransform.parent;
                }
                
                // Only process hits on objects with "MinimapMain" tag
                if (isMinimapHit)
                {
                    hasHit = true;

                    // Try to get the Canvas component from the gameObject or its parents
                    Transform canvasTransform = hit.gameObject.transform;
                    while (canvasTransform != null)
                    {
                        if (canvasTransform.TryGetComponent(out Canvas canvas))
                        {
                            hitCanvas = canvas;
                            
                            // Calculate local position relative to canvas
                            localPosition = hitCanvas.transform.InverseTransformPoint(hitPosition);
                            break;
                        }
                        canvasTransform = canvasTransform.parent;
                    }
                }
            }
            else if (raycastHit.HasValue)
            {
                // Check if we hit a Canvas in the 3D world
                var hit = raycastHit.Value;
                hitPosition = hit.point;
                
                // Check if the hit object or any of its parents has the "MinimapMain" tag
                Transform hitTransform = hit.transform;
                while (hitTransform != null)
                {
                    if (hitTransform.CompareTag("MinimapMain"))
                    {
                        isMinimapHit = true;
                        break;
                    }
                    hitTransform = hitTransform.parent;
                }
                
                // Only process hits on objects with "MinimapMain" tag
                if (isMinimapHit)
                {
                    // Try to get Canvas from the hit object
                    Canvas canvas = null;
                    if (hit.transform.TryGetComponent(out canvas))
                    {
                        hitCanvas = canvas;
                        hasHit = true;
                    }
                    else
                    {
                        Canvas parentCanvas = hit.transform.GetComponentInParent<Canvas>();
                        if (parentCanvas != null)
                        {
                            hitCanvas = parentCanvas;
                            hasHit = true;
                        }
                    }
                    
                    // Calculate local position relative to canvas if we found one
                    if (hitCanvas != null)
                    {
                        localPosition = hitCanvas.transform.InverseTransformPoint(hitPosition);
                    }
                }
            }
        }

        // Log positions only when there's a change or we just hit/left the minimap
        if (hasHit && 
            (hitCanvas != m_LastHitCanvas || 
             !m_HadHitLastFrame || 
             Vector3.Distance(hitPosition, m_LastHitPosition) > 0.001f))
        {
            if (m_LogLocalCanvasPosition && hitCanvas != null)
            {
                Debug.Log($"MinimapMain Hit (Local): {localPosition} on Canvas: {hitCanvas.name}");
            }
            else
            {
                Debug.Log($"MinimapMain Hit (World): {hitPosition}");
            }

            m_LastHitPosition = hitPosition;
            m_LastLocalPosition = localPosition;
            m_LastHitCanvas = hitCanvas;
        }
        else if (!hasHit && m_HadHitLastFrame)
        {
            Debug.Log("Ray no longer hitting MinimapMain");
        }

        m_HadHitLastFrame = hasHit;
    }
}