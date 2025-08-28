using UnityEngine;
using UnityEngine.UI;

/// <summary>

/// Purpose:
/// Show a pen model on a World-Space RawImage board. The *pen tip* sticks
/// to the mouse hit point on the board surface (visual only; does not affect drawing/clicks).
///
/// How it works (per frame when visible):
/// 1) Raycast from the UI camera through the mouse to a plane defined by the board.
/// 2) If the ray hits that plane and the hit lies inside the board's Rect, proceed.
/// 3) Compute a desired rotation facing the board normal (plus an optional tilt).
/// 4) Compute the desired *tip* world position (hit + small gap + optional planar offset).
/// 5) Convert the tip's local offset to world space and back out the pen model pivot position,
///    so the tip lands exactly on the surface.
/// 6) Move (optionally smoothed) and rotate the model; hide if the mouse leaves the board.
/// </summary>
public class PenCursorOnBoard : MonoBehaviour
{
    [Header("Refs (assign in Inspector)")]
    public Transform penModel;      // The whole pen model (no Collider; Layer = Ignore Raycast recommended)
    public Transform penTip;        // A child located at the pen tip (local position at contact point)
    public RawImage board;          // The drawing board (RawImage on a World Space Canvas)
    public Camera uiCam;            // Camera used for UI ray; auto-found if empty

    [Header("Look & Follow")]
    public float surfaceGap = 0.002f;           // Tiny gap above board to avoid z-fighting/penetration
    public Vector2 planarOffset = Vector2.zero; // Along-board offset (x=board.right, y=board.up)
    public Vector3 modelEulerOffset = new Vector3(25, 0, 0); // Extra tilt so the pen looks natural
    public float smoothTime = 0.03f;            // Follow smoothing (0 = snap immediately)
    public bool hideWhenOffBoard = true;        // Hide when mouse is outside the board rect

    [Header("Debug")]
    public bool debugLog = false;

    private bool _visible;   // Whether the visual is currently active
    private Vector3 _vel;    // Velocity for SmoothDamp

    void Awake()
    {
        // Auto-find the board from a Paint component if not assigned.
        if (!board)
        {
            var p = FindObjectOfType<Paint>();
            if (p) board = p.rawImage;
        }

        // Auto-pick the camera: Canvas.worldCamera (World Space) else Camera.main.
        if (!uiCam && board && board.canvas && board.canvas.renderMode == RenderMode.WorldSpace)
            uiCam = board.canvas.worldCamera ? board.canvas.worldCamera : Camera.main;

        Show(false); // Start hidden
    }

    void Update()
    {
        // Do nothing if not supposed to be visible.
        if (!_visible) return;

        // Basic reference checks.
        if (!penModel || !penTip || !board)
        {
            if (debugLog) Debug.LogWarning("[PenCursor] Missing refs: penModel/penTip/board.");
            return;
        }

        var rt = board.rectTransform;
        var cam = uiCam ? uiCam : Camera.main;
        if (!cam)
        {
            if (debugLog) Debug.LogWarning("[PenCursor] No camera. Assign uiCam.");
            return;
        }

        // 1) Raycast from camera through mouse to the board plane.
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(rt.forward, rt.position); // Plane uses board forward + position
        if (!plane.Raycast(ray, out float enter))
        {
            if (hideWhenOffBoard) penModel.gameObject.SetActive(false);
            return;
        }
        Vector3 hitWorld = ray.origin + ray.direction * enter;

        // 2) Convert hit to board local space and check if it's inside the Rect.
        Vector3 hitLocal = rt.InverseTransformPoint(hitWorld);
        Vector2 half = rt.rect.size * 0.5f;
        bool inside =
            hitLocal.x >= -half.x && hitLocal.x <= half.x &&
            hitLocal.y >= -half.y && hitLocal.y <= half.y;

        if (!inside)
        {
            if (hideWhenOffBoard) { penModel.gameObject.SetActive(false); }
            return;
        }

        // 3) Desired rotation: face the board normal (rt.forward) with rt.up as up, plus optional tilt.
        Quaternion targetRot = Quaternion.LookRotation(rt.forward, rt.up) * Quaternion.Euler(modelEulerOffset);

        // 4) Desired pen tip world position: hit point + planar offset + small surface gap.
        Vector3 tipWorldTarget =
            hitWorld +
            rt.right * planarOffset.x +
            rt.up    * planarOffset.y +
            rt.forward * surfaceGap;

        // 5) Place the model so its *tip* lands on tipWorldTarget.
        //    Temporarily set rotation so TransformVector uses the target orientation.
        Quaternion oldRot = penModel.rotation; // (kept for parity; not used further)
        penModel.rotation = targetRot;

        // Convert the tip's local position to a world-space vector from the model pivot.
        Vector3 pivotToTipWorld = penModel.TransformVector(penTip.localPosition);

        // Compute where the model pivot must be so that (pivot + pivotToTipWorld) == tipWorldTarget.
        Vector3 modelPosTarget = tipWorldTarget - pivotToTipWorld;

        // 6) Move and rotate the model (optionally smoothed). Ensure active when inside.
        if (smoothTime > 0f)
            penModel.position = Vector3.SmoothDamp(penModel.position, modelPosTarget, ref _vel, smoothTime);
        else
            penModel.position = modelPosTarget;

        penModel.rotation = targetRot;

        if (!penModel.gameObject.activeSelf) penModel.gameObject.SetActive(true);
    }

    /// <summary>
    /// Show or hide the visual pen.
    /// Call this from your Pen button (true) and hide it on other actions (false).
    /// </summary>
    public void Show(bool on)
    {
        _visible = on;
        if (penModel) penModel.gameObject.SetActive(on);
    }

    /// <summary>
    /// Toggle visibility (handy for quick testing).
    /// </summary>
    public void Toggle() => Show(!_visible);
}
