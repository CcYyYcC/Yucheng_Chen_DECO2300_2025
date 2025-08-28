using UnityEngine;

/// <summary>
/// GazeRelativeBoardReset (Beginner-friendly comments, no logic changes)
/// --------------------------------------------------------------------
/// Purpose:
/// - At startup, record the board's pose *relative to the player's camera*.
/// - When resetting, place the board back to that same relative pose,
///   but relative to the camera's *current* pose (so it always appears
///   in front of where the player is looking now).
/// </summary>
public class GazeRelativeBoardReset : MonoBehaviour
{
    [Header("Player camera (leave empty to use Camera.main)")]
    public Transform cameraT;

    [Header("Board root (the object to place/rotate/scale)")]
    public Transform board;

    [Header("Hotkey (you can also call ResetNow() from other scripts)")]
    public bool listenKey = true;
    public KeyCode resetKey = KeyCode.R;

    [Header("If board is inactive, reactivate before resetting")]
    public bool reactivateOnReset = true;

    // ---- Data captured at startup, in the camera's local space ----
    // We store the board's *relative* transform w.r.t. the camera:
    //   - initPos_inCam : board position in camera local space
    //   - initRot_inCam : rotation offset from camera to board
    //   - initLocalScale: board's local scale (kept as-is)
    Vector3    initPos_inCam;   // x = right, y = up, z = forward (camera-local)
    Quaternion initRot_inCam;   // rotation from camera to board
    Vector3    initLocalScale;  // board's localScale (unchanged on reset)

    bool inited;                // did we capture the initial state?
    bool pendingReset;          // defer reset to LateUpdate (after others moved)

    void Awake()
    {
        // Auto-pick the camera if none assigned
        if (!cameraT && Camera.main) cameraT = Camera.main.transform;

        // Capture the initial relative transform right away
        CaptureInitial();
    }

    /// <summary>
    /// Capture the *current* board pose as the new initial:
    /// - Convert board's world position into camera-local (InverseTransformPoint)
    /// - Store the rotation offset from camera to board (inv(camRot) * boardRot)
    /// - Remember board.localScale
    /// </summary>
    [ContextMenu("Capture Initial (Record Now)")]
    public void CaptureInitial()
    {
        if (!cameraT || !board) return;

        initPos_inCam  = cameraT.InverseTransformPoint(board.position);
        initRot_inCam  = Quaternion.Inverse(cameraT.rotation) * board.rotation;
        initLocalScale = board.localScale;

        inited = true;
    }

    void Update()
    {
        // Optionally listen for a hotkey; just set a flag here.
        // We actually perform the reset in LateUpdate to ensure
        // the camera/player has finished any own movement this frame.
        if (!listenKey || !inited) return;
        if (Input.GetKeyDown(resetKey)) pendingReset = true;
    }

    void LateUpdate()
    {
        // Perform the reset after all Update() calls (and after the camera
        // potentially moved this frame), so placement uses the latest camera pose.
        if (!pendingReset) return;
        pendingReset = false;
        ResetNow();
    }

    /// <summary>
    /// Reset the board to the *initial relative pose*, but relative to the
    /// camera's *current* transform:
    ///   worldPos = cam.TransformPoint(initPos_inCam)
    ///   worldRot = cam.rotation * initRot_inCam
    ///   localScale = initLocalScale
    /// Optionally re-activate the board if it was hidden.
    /// </summary>
    [ContextMenu("Reset Now")]
    public void ResetNow()
    {
        if (!inited || !cameraT || !board) return;

        if (reactivateOnReset && !board.gameObject.activeSelf)
            board.gameObject.SetActive(true);

        board.position   = cameraT.TransformPoint(initPos_inCam);
        board.rotation   = cameraT.rotation * initRot_inCam;
        board.localScale = initLocalScale;
    }
}
