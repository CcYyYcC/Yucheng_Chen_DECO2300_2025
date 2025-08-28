using UnityEngine;

/// <summary>
/// Zoom.cs
/// Attach this script to an object (World Space Canvas).
/// Allows zooming in/out with the mouse scroll wheel, with options for:
/// - Zoom around the mouse cursor
/// - Custom zoom step, min/max scale
/// - Inverted scroll direction
/// </summary>
public class Zoom : MonoBehaviour
{
    [Header("Root object to scale (default = this object, e.g. your World Space Canvas)")]
    public Transform targetRoot;

    [Header("Camera used for ray calculations (default = Canvas.worldCamera or MainCamera)")]
    public Camera cam;

    [Header("Zoom Settings")]
    [Tooltip("Scale factor per scroll step (0.1 ≈ 10%)")]
    public float zoomStep = 0.15f;          // How much to scale for each scroll unit
    [Tooltip("Min/max multiplier relative to initial size")]
    public float minScaleMultiplier = 0.1f;  // Minimum = 0.1 × initial size
    public float maxScaleMultiplier = 10f;   // Maximum = 10 × initial size
    public bool zoomToMouse = true;          // Zoom around the mouse position
    [Tooltip("Invert scroll direction (up = zoom out, down = zoom in)")]
    public bool invertScroll = false;

    // Store the initial uniform scale of the object
    private float _baseScale = 1f;

    void Awake()
    {
        // If no root is assigned, use this object's transform
        if (!targetRoot) targetRoot = transform;

        // If no camera is assigned, try to find one
        if (!cam)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera)
                cam = canvas.worldCamera;
            else
                cam = Camera.main;
        }

        // Store the initial scale (for World Space Canvas, often 0.01)
        _baseScale = targetRoot.localScale.x;
        if (_baseScale <= 0f) _baseScale = 0.01f;
    }

    void Update()
    {
        // Get scroll input (new API first, fallback to legacy axis)
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scroll, 0f))
            scroll = Input.GetAxis("Mouse ScrollWheel") * 120f; // Some devices only support old axis

        if (invertScroll) scroll = -scroll;
        if (Mathf.Abs(scroll) < 0.001f) return;  // Do nothing if no scroll input

        // Current scale (relative to the base scale)
        float currentMul = targetRoot.localScale.x / _baseScale;

        // Exponential scaling: scroll > 0 = zoom in, scroll < 0 = zoom out
        float factor = Mathf.Pow(1f + Mathf.Max(0.0001f, zoomStep), scroll);
        float newMul = Mathf.Clamp(currentMul * factor, minScaleMultiplier, maxScaleMultiplier);
        if (Mathf.Approximately(newMul, currentMul)) return;

        float newScale = _baseScale * newMul;

        // Get world position of the mouse on a plane aligned with the canvas
        Vector3 pivotWorld = GetMouseWorldOnPlane();
        Vector3 localBefore = targetRoot.InverseTransformPoint(pivotWorld);

        // Apply the new scale
        targetRoot.localScale = Vector3.one * newScale;

        // Adjust position so the point under the mouse stays fixed
        if (zoomToMouse)
        {
            Vector3 worldAfter = targetRoot.TransformPoint(localBefore);
            targetRoot.position += (pivotWorld - worldAfter);
        }
    }

    /// <summary>
    /// Finds the world position under the mouse cursor on a plane
    /// that is aligned with the canvas or target object.
    /// </summary>
    Vector3 GetMouseWorldOnPlane()
    {
        if (!cam) cam = Camera.main;
        Plane plane = new Plane(-cam.transform.forward, targetRoot.position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : targetRoot.position;
    }
}
