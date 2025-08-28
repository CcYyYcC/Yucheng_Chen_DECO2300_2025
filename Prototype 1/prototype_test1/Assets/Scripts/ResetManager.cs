using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Independent Reset Manager
/// Put this script on a permanent Object
///
/// Goal:
/// 1) Even if the panel/UI or the whole drawing board group is hidden (SetActive(false)),
///    pressing R or clicking the reset button can bring everything back to life and restore the initial state.
/// 2) The reset includes: 
///    - Parent transform (position/rotation/scale)
///    - Active/inactive states of child objects
///    - (Optional) Paint state and canvas content
///
/// Usage:
/// - Assign "groupRoot" to the parent GameObject that contains both the board and the UI.
/// - Fill "paintTargets" with all Paint components you want to reset (optional).
/// </summary>
public class ResetManager : MonoBehaviour
{
    [Header("Parent object to reset (board + UI root)")]
    public GameObject groupRoot; // Reference to the group root (still valid even if inactive)

    [Header("Paint components to reset (optional)")]
    public Paint[] paintTargets; // Leave empty if you don't want to reset Paint state

    [Header("Hotkey Settings")]
    public bool enableHotkey = true;
    public KeyCode resetKey = KeyCode.R;

    [Header("Reset Behavior")]
    public bool reactivateOnReset = true;           // If parent is inactive, reactivate it first
    public bool restoreChildrenActiveStates = true;// Restore initial active/inactive state of children
    public bool resetPaintState = true;            // Restore Paint brush size/color/eraser state
    public bool clearPaintTextureOnReset = true;   // Clear canvas when resetting

    [Header("Capture initial state automatically in Awake()")]
    public bool captureOnAwake = true;

    // --- Initial Transform (for the parent) ---
    Vector3 initLocalPos;
    Quaternion initLocalRot;
    Vector3 initLocalScale;

    // --- Snapshot of initial active states (all children, including inactive) ---
    List<Transform> initNodes = new List<Transform>();
    List<bool> initActive = new List<bool>();

    // --- Initial Paint states ---
    int[] initBrushRadius;
    Color[] initBrushColor;
    bool[] initEraser;

    bool initialized;

    void Awake()
    {
        if (!groupRoot)
        {
            Debug.LogError("[ResetManager] Please assign groupRoot (the board + UI parent).");
            return;
        }
        if (captureOnAwake) CaptureInitial();
    }

    void Update()
    {
        if (!enableHotkey || !initialized) return;
        if (Input.GetKeyDown(resetKey)) ResetNow();
    }

    /// <summary>
    /// Capture the current state as the "initial state".
    /// This includes: parent transform, all children's active states, Paint state.
    /// Can be called at runtime to redefine the default reset position/config.
    /// </summary>
    [ContextMenu("Capture Initial From Current")]
    public void CaptureInitial()
    {
        // 1) Save parent transform (local)
        var t = groupRoot.transform;
        initLocalPos   = t.localPosition;
        initLocalRot   = t.localRotation;
        initLocalScale = t.localScale;

        // 2) Save active states of the entire hierarchy
        initNodes.Clear(); initActive.Clear();
        if (restoreChildrenActiveStates)
        {
            // true = include inactive objects
            var all = groupRoot.GetComponentsInChildren<Transform>(true);
            foreach (var node in all)
            {
                initNodes.Add(node);
                initActive.Add(node.gameObject.activeSelf);
            }
        }

        // 3) Save Paint states
        if (paintTargets != null && paintTargets.Length > 0)
        {
            initBrushRadius = new int[paintTargets.Length];
            initBrushColor  = new Color[paintTargets.Length];
            initEraser      = new bool[paintTargets.Length];

            for (int i = 0; i < paintTargets.Length; i++)
            {
                var p = paintTargets[i];
                if (!p) continue;
                initBrushRadius[i] = p.brushRadius;
                initBrushColor[i]  = p.brushColor;   // user’s current brush color
                initEraser[i]      = p.eraserMode;
            }
        }

        initialized = true;
    }

    /// <summary>
    /// Perform reset:
    /// - Reactivate parent if hidden
    /// - Restore parent transform
    /// - Restore children's active states
    /// - (Optional) Reset Paint state and clear canvas
    /// </summary>
    [ContextMenu("Reset Now")]
    public void ResetNow()
    {
        if (!groupRoot) return;

        if (!initialized)
        {
            Debug.LogWarning("[ResetManager] Not initialized. Capturing current state as initial.");
            CaptureInitial();
        }

        // 0) Reactivate parent if hidden
        if (reactivateOnReset && !groupRoot.activeSelf)
            groupRoot.SetActive(true);

        // 1) Restore parent transform
        var t = groupRoot.transform;
        t.localPosition = initLocalPos;
        t.localRotation = initLocalRot;
        t.localScale    = initLocalScale;

        // 2) Restore child objects' active states
        if (restoreChildrenActiveStates && initNodes.Count == initActive.Count)
        {
            for (int i = 0; i < initNodes.Count; i++)
            {
                var node = initNodes[i];
                if (!node) continue; // skip if destroyed during runtime
                node.gameObject.SetActive(initActive[i]);
            }
        }

        // 3) Restore Paint state and clear canvas
        if (paintTargets != null && paintTargets.Length > 0 && resetPaintState)
        {
            for (int i = 0; i < paintTargets.Length; i++)
            {
                var p = paintTargets[i];
                if (!p) continue;

                // Brush size
                if (initBrushRadius != null && i < initBrushRadius.Length)
                    p.SetBrushSize(initBrushRadius[i]);

                // Brush color
                if (initBrushColor != null && i < initBrushColor.Length)
                    p.SetColor(initBrushColor[i]);

                // Eraser mode
                if (initEraser != null && i < initEraser.Length)
                    p.SetEraser(initEraser[i]);
                else
                    p.SetEraser(false); // default back to pen

                // Clear canvas
                if (clearPaintTextureOnReset)
                    p.ClearTexture();
            }
        }
    }
}
