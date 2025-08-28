using UnityEngine;

/// <summary>
/// Goal: While holding a key (middle mouse by default), drag an object on a fixed plane,
/// so it moves parallel to its own facing plane without depth popping.
/// 
/// How it works:
/// 1) On key down, we lock a drag plane using the object's current position and forward.
/// 2) We raycast from the camera through the mouse to that plane to get a hit point.
/// 3) We store an offset = (objectPosition - hitPoint) so the object won't jump to the cursor.
/// 4) While dragging, each frame set objectPosition = currentHit + offset.
/// 5) Camera used: Canvas.worldCamera (for World Space UI) or fallback to Camera.main.
/// </summary>
public class Catch : MonoBehaviour
{
    // The transform we actually move. Defaults to this component's transform.
    [Header("要平移的根对象(默认=本物体)")]
    public Transform targetRoot;

    // Camera used to create the mouse ray. Auto-picks Canvas.worldCamera or Camera.main.
    [Header("用于计算射线的相机(默认=Canvas.worldCamera 或 MainCamera)")]
    public Camera cam;

    // Which key/button starts the drag. Default: Mouse Middle Button.
    [Header("按哪个键拖动")]
    public KeyCode panButton = KeyCode.Mouse2; // 中键

    private bool _panning;        // Are we currently dragging?
    private Vector3 _grabOffset;  // objectPosition - hitPoint (keeps initial relative offset)
    private Plane _dragPlane;     // The locked plane used during this drag (doesn't rotate with camera)

    void Awake()
    {
        // If no target assigned, move this object.
        if (!targetRoot) targetRoot = transform;

        // Auto-select a camera:
        // - If under a World Space Canvas with a worldCamera, use that.
        // - Else fallback to Camera.main.
        if (!cam)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera)
                cam = canvas.worldCamera;
            else
                cam = Camera.main;
        }
    }

    void Update()
    {
        // Start dragging on key down.
        if (Input.GetKeyDown(panButton))
        {
            // Lock the plane at the moment we start:
            //   normal = object forward (so movement stays in the object's plane)
            //   point  = object position
            _dragPlane = new Plane(targetRoot.forward, targetRoot.position);

            // Compute initial hit and remember offset so the object won't snap.
            if (RaycastOnDragPlane(out var hit))
            {
                _grabOffset = targetRoot.position - hit;
                _panning = true;
            }
        }
        // Stop dragging on key up.
        else if (Input.GetKeyUp(panButton))
        {
            _panning = false;
        }

        // While dragging, update position along the locked plane.
        if (_panning)
        {
            if (RaycastOnDragPlane(out var hit))
            {
                // Keep depth unchanged; move within the plane.
                targetRoot.position = hit + _grabOffset;
            }
        }
    }

    /// <summary>
    /// Raycast from the camera through the mouse position onto the locked plane.
    /// Returns true with the world hit point if it intersects; otherwise returns current object position.
    /// </summary>
    bool RaycastOnDragPlane(out Vector3 hitPoint)
    {
        if (!cam) cam = Camera.main; // Safety fallback
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // If the ray hits the plane, get the hit position in world space.
        if (_dragPlane.Raycast(ray, out float enter))
        {
            hitPoint = ray.GetPoint(enter);
            return true;
        }

        // Rare case (e.g., parallel ray): keep current position.
        hitPoint = targetRoot.position;
        return false;
    }
}
