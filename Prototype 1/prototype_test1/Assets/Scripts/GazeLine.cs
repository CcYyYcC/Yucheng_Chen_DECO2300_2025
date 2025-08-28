using UnityEngine;

/// <summary>
/// Goal:
/// Draw a thin line starting from the camera and going forward.
/// - If the forward ray hits the "board plane", the line ends exactly at the hit point.
/// - If there is no hit, the line shows a fixed length (maxDistance).
///
/// Typical use:
/// - Attach this script to your Main Camera (or any Camera).
/// - Assign "board" to the Transform of your drawing board (a Transform or RectTransform).
/// - Press Play: you'll see a cyan line from the camera toward the board.
/// </summary>
public class GazeLine : MonoBehaviour
{
    // The board Transform is used only to define a plane:
    //   - plane point   = board.position
    //   - plane normal  = board.forward (the board's facing direction)
    // You can drag a normal GameObject or a UI RectTransform (World Space) here.
    [Header("Board (defines the plane: position + forward)")]
    public Transform board;

    // Basic styling/behaviour knobs for the line.
    [Header("Params")]
    public float maxDistance = 10f;   // How far the line goes if there is no plane hit.
    public float startOffset = 0.05f; // Start a bit in front of the camera (avoid clipping into the camera/face).
    public float width = 0.01f;       // Line thickness in world units (tune to your scene scale).
    public Color color = Color.cyan;  // Line color.

    // Private references we set up at runtime.
    LineRenderer lr;  // The component that actually draws the line in 3D space.
    Camera cam;       // The camera we use for position/direction and for the ray.

    void Awake()
    {
        // 1) Find which Camera to use.
        // If this script is on a Camera object, GetComponent<Camera>() succeeds.
        // If not, we fall back to Camera.main (the first camera tagged "MainCamera").
        cam = GetComponent<Camera>();
        if (!cam) cam = Camera.main;

        // 2) Make sure we have a LineRenderer.
        // If there's none on this GameObject, we add one.
        lr = GetComponent<LineRenderer>();
        if (!lr) lr = gameObject.AddComponent<LineRenderer>();

        // 3) Minimal LineRenderer setup.
        // - We draw a straight line with exactly 2 points: [start, end].
        // - alignment = View keeps the line’s width facing the screen nicely for simple cursor lines.
        // - widthMultiplier sets a base width (we also set start/end width each frame to keep it in sync).
        lr.positionCount = 2;
        lr.alignment = LineAlignment.View;
        lr.widthMultiplier = width;

        // 4) Give the line a simple unlit material so it shows a flat color (not affected by lights).
        // Shader.Find("Unlit/Color") is ideal. If not found, fall back to "Sprites/Default".
        var shader = Shader.Find("Unlit/Color");
        if (lr.material == null)
        {
            // Create a temporary material instance for this line.
            lr.material = new Material(shader ? shader : Shader.Find("Sprites/Default"));
        }
        lr.material.color = color;
    }

    void Update()
    {
        // If we don't have a board or a camera, we cannot calculate the plane/hit.
        if (!board || !cam) { lr.enabled = false; return; }

        // --- 1) Compute the line start and direction ---
        // Start point: the camera's position + a small offset forward so the line does not overlap the camera.
        Vector3 origin = cam.transform.position + cam.transform.forward * startOffset;

        // Direction: straight ahead from the camera (its forward vector).
        Vector3 dir = cam.transform.forward;

        // --- 2) Define the "board plane" we want to hit ---
        // A plane is defined by a point and a normal.
        // - Use board.position as the point on the plane.
        // - Use board.forward as the normal so the plane's facing matches the board's front.
        Plane plane = new Plane(board.forward, board.position);

        // Default end point: no hit → draw a fixed length line in the forward direction.
        Vector3 end = origin + dir * maxDistance;

        // --- 3) Raycast from origin toward dir against the plane ---
        // Plane.Raycast(ray, out enter) returns true if the ray intersects the plane.
        // 'enter' is the distance along the ray at which the hit occurs (>= 0).
        if (plane.Raycast(new Ray(origin, dir), out float enter)
            && enter > 0f                      // Must be in front of the origin (not behind the camera)
            && enter <= maxDistance)           // Only accept hits within our max line length
        {
            // If we hit, set the end point at the exact intersection point.
            // ray.GetPoint(enter) = origin + dir * enter
            end = origin + dir * enter;
        }

        // --- 4) Feed positions and styling to the LineRenderer ---
        lr.enabled = true;                 // Make sure the line is visible this frame.
        lr.startWidth = lr.endWidth = width; // Keep both ends the same thickness (consistent with widthMultiplier).
        lr.SetPosition(0, origin);         // Start point
        lr.SetPosition(1, end);            // End point (hit or maxDistance)
    }

    /// <summary>
    /// Optional helper you can call at runtime to restyle the line.
    /// For example: SetStyle(0.02f, Color.green);
    /// </summary>
    public void SetStyle(float newWidth, Color newColor)
    {
        width = newWidth;
        color = newColor;

        if (lr)
        {
            // Update both the width multiplier and the material color so the change is visible immediately.
            lr.widthMultiplier = width;
            lr.material.color = color;
        }
    }
}
