using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script switches scenes when the lever (HingeJoint) is pulled
/// to either its minimum or maximum rotation limit.
///
/// ▶ How it works:
/// - When the lever is rotated close to the minimum angle (e.g. -70°), 
///   it loads one scene.
/// - When rotated close to the maximum angle (e.g. +70°), 
///   it loads another scene.
/// - The lever must stay in that position for a short time (holdSeconds)
///   before switching scenes.
/// - A small "hysteresis" range prevents the trigger from firing repeatedly
///   when the lever wobbles around the limit.
///
/// ▶ Usage:
/// 1. Add this script to the lever GameObject that has a HingeJoint.
/// 2. Assign the HingeJoint in the inspector (or it will auto-detect one).
/// 3. Set scene indices (indexAtMin / indexAtMax) from Build Settings.
/// 4. Optionally adjust angles, delay, and sensitivity.
/// </summary>
public class LeverSceneByIndex : MonoBehaviour
{
    [Header("Lever")]
    /// <summary>
    /// The lever’s HingeJoint.  
    /// If not assigned, it will automatically use GetComponent<HingeJoint>().
    /// </summary>
    public HingeJoint hinge;

    [Header("Trigger Angles (degrees)")]
    /// <summary>
    /// Scene change triggers when lever angle is less than or equal to this value.  
    /// Example: -70° means when lever is pulled down fully.
    /// </summary>
    public float minTriggerAngle = -70f;

    /// <summary>
    /// Scene change triggers when lever angle is greater than or equal to this value.  
    /// Example: +70° means when lever is pushed up fully.
    /// </summary>
    public float maxTriggerAngle = 70f;

    /// <summary>
    /// Small buffer angle to avoid rapid re-triggering.  
    /// The lever must move this many degrees away before it can trigger again.
    /// </summary>
    public float hysteresis = 3f;

    /// <summary>
    /// How long (in seconds) the lever must stay in trigger position before switching scenes.  
    /// Prevents accidental quick touches from triggering.
    /// </summary>
    public float holdSeconds = 0.25f;

    [Header("Scene Indices (from Build Settings)")]
    [Tooltip("Scene to load when lever is pulled down to the minimum angle (-70°).")]
    public int indexAtMin = 0;

    [Tooltip("Scene to load when lever is pushed up to the maximum angle (+70°).")]
    public int indexAtMax = 1;

    /// <summary>
    /// If false, the script won’t reload the same scene that’s already active.
    /// </summary>
    public bool allowReloadSameScene = false;

    // Internal timers and latch states
    float enterMinTime = -1f;
    float enterMaxTime = -1f;
    bool latchedMin = false;
    bool latchedMax = false;

    void Reset()
    {
        // Auto-assign HingeJoint if this component is attached to the same object
        hinge = GetComponent<HingeJoint>();
    }

    void Update()
    {
        if (!hinge) return;

        // Current lever angle (0° = initial rest position)
        float a = hinge.angle;

        // --- Trigger at MIN angle (pull down) ---
        if (a <= minTriggerAngle)
        {
            if (!latchedMin)
            {
                // Start timing when lever first reaches the threshold
                if (enterMinTime < 0f) enterMinTime = Time.unscaledTime;

                // If held long enough, trigger the scene load
                if (Time.unscaledTime - enterMinTime >= holdSeconds)
                {
                    latchedMin = true;
                    latchedMax = false; // Reset opposite direction
                    LoadByIndex(indexAtMin);
                }
            }
        }
        // Reset when lever leaves the min region
        else if (a > minTriggerAngle + hysteresis)
        {
            enterMinTime = -1f;
            latchedMin = false;
        }

        // --- Trigger at MAX angle (push up) ---
        if (a >= maxTriggerAngle)
        {
            if (!latchedMax)
            {
                if (enterMaxTime < 0f) enterMaxTime = Time.unscaledTime;
                if (Time.unscaledTime - enterMaxTime >= holdSeconds)
                {
                    latchedMax = true;
                    latchedMin = false;
                    LoadByIndex(indexAtMax);
                }
            }
        }
        // Reset when lever leaves the max region
        else if (a < maxTriggerAngle - hysteresis)
        {
            enterMaxTime = -1f;
            latchedMax = false;
        }
    }

    /// <summary>
    /// Loads a scene by its Build Index.
    /// If "allowReloadSameScene" is false, it won’t reload the current scene.
    /// </summary>
    void LoadByIndex(int buildIndex)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0) return;

        // Clamp index to valid range
        buildIndex = Mathf.Clamp(buildIndex, 0, count - 1);

        // Skip if same scene and not allowed to reload
        if (!allowReloadSameScene &&
            SceneManager.GetActiveScene().buildIndex == buildIndex)
            return;

        // Load the target scene
        SceneManager.LoadScene(buildIndex);
    }

    /// <summary>
    /// You can change the scene index for the minimum lever position at runtime.
    /// </summary>
    public void SetIndexAtMin(int idx) => indexAtMin = idx;

    /// <summary>
    /// You can change the scene index for the maximum lever position at runtime.
    /// </summary>
    public void SetIndexAtMax(int idx) => indexAtMax = idx;
}
