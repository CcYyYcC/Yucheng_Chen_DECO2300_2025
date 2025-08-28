using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Show an eraser model on a World-Space RawImage board.
/// The eraser tip sticks to the mouse hit point on the board surface (visual only; no drawing).
/// </summary>
public class EraserCursorOnBoard : MonoBehaviour
{
    [Header("Refs (assign in Inspector)")]
    public Transform eraserModel;   // Eraser model (no Collider; put on Ignore Raycast layer)
    public Transform eraserTip;     // Child located at the eraser tip (local position at contact point)
    public RawImage board;          // Drawing board (RawImage on World Space Canvas)
    public Camera uiCam;            // Camera used for UI ray (auto-find if empty)

    [Header("Look & Follow")]
    public float surfaceGap = 0.002f;        // Small gap above the board to avoid z-fighting
    public Vector2 planarOffset = Vector2.zero; // Along-board offset (x=board.right, y=board.up)
    public Vector3 modelEulerOffset = new Vector3(25, 0, 0); // Extra tilt for nicer look
    public float smoothTime = 0.03f;         // Follow smoothing (0 = snap)
    public bool hideWhenOffBoard = true;     // Hide when mouse is outside board rect

    [Header("Debug")]
    public bool debugLog = false;

    private bool _visible;
    private Vector3 _vel;

    void Awake()
    {
        // Auto-find board from a Paint component if not assigned.
        if (!board)
        {
            var p = FindObjectOfType<Paint>();
            if (p) board = p.rawImage;
        }

        // Auto-pick camera: Canvas.worldCamera (World Space) else Camera.main.
        if (!uiCam && board && board.canvas && board.canvas.renderMode == RenderMode.WorldSpace)
            uiCam = board.canvas.worldCamera ? board.canvas.worldCamera : Camera.main;

        Show(false); // Start hidden
    }

    void Update()
    {
        if (!_visible) return;

        if (!eraserModel || !eraserTip || !board)
        {
            if (debugLog) Debug.LogWarning("[EraserCursor] Missing refs: eraserModel/eraserTip/board.");
            return;
        }

        var rt = board.rectTransform;
        var cam = uiCam ? uiCam : Camera.main;
        if (!cam)
        {
            if (debugLog) Debug.LogWarning("[EraserCursor] No camera. Assign uiCam.");
            return;
        }

        // 1) Raycast from camera through mouse to the board plane.
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(rt.forward, rt.position);
        if (!plane.Raycast(ray, out float enter))
        {
            if (hideWhenOffBoard) eraserModel.gameObject.SetActive(false);
            return;
        }
        Vector3 hitWorld = ray.origin + ray.direction * enter;

        // 2) Convert to board local and check inside rect.
        Vector3 hitLocal = rt.InverseTransformPoint(hitWorld);
        Vector2 half = rt.rect.size * 0.5f;
        bool inside =
            hitLocal.x >= -half.x && hitLocal.x <= half.x &&
            hitLocal.y >= -half.y && hitLocal.y <= half.y;

        if (!inside)
        {
            if (hideWhenOffBoard) { eraserModel.gameObject.SetActive(false); }
            return;
        }

        // 3) Desired rotation: face board normal + optional tilt.
        Quaternion targetRot = Quaternion.LookRotation(rt.forward, rt.up) * Quaternion.Euler(modelEulerOffset);

        // 4) Desired tip position in world.
        Vector3 tipWorldTarget =
            hitWorld +
            rt.right * planarOffset.x +
            rt.up    * planarOffset.y +
            rt.forward * surfaceGap;

        // 5) Convert tip (local) to world vector and back out model position.
        Quaternion oldRot = eraserModel.rotation; // (not used further; kept to mirror original code)
        eraserModel.rotation = targetRot;

        Vector3 pivotToTipWorld = eraserModel.TransformVector(eraserTip.localPosition);
        Vector3 modelPosTarget = tipWorldTarget - pivotToTipWorld;

        // 6) Move & rotate model.
        if (smoothTime > 0f)
            eraserModel.position = Vector3.SmoothDamp(eraserModel.position, modelPosTarget, ref _vel, smoothTime);
        else
            eraserModel.position = modelPosTarget;

        eraserModel.rotation = targetRot;

        // Ensure visible when inside.
        if (!eraserModel.gameObject.activeSelf) eraserModel.gameObject.SetActive(true);
    }

    /// <summary>Show or hide the visual eraser.</summary>
    public void Show(bool on)
    {
        _visible = on;
        if (eraserModel) eraserModel.gameObject.SetActive(on);
    }

    public void Toggle() => Show(!_visible);
}
