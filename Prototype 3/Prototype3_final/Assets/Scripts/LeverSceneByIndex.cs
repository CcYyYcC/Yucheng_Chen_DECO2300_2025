using UnityEngine;
using UnityEngine.SceneManagement;

public class LeverSceneByIndex : MonoBehaviour
{
    [Header("Lever")]
    public HingeJoint hinge;                      // 拖你的 HingeJoint（留空会自动 GetComponent）

    [Header("Trigger Angles (deg)")]
    public float minTriggerAngle = -70f;          // 贴近最小限位时触发
    public float maxTriggerAngle =  70f;          // 贴近最大限位时触发
    public float hysteresis      =   3f;          // 回差：离开阈值多少度后才允许再次触发
    public float holdSeconds     = 0.25f;         // 到达阈值需保持多久才触发

    [Header("Scene Indices (from Build Settings)")]
    [Tooltip("拉到最小角(-70°附近)要加载的 BuildIndex")]
    public int indexAtMin = 0;
    [Tooltip("拉到最大角(+70°附近)要加载的 BuildIndex")]
    public int indexAtMax = 1;
    public bool allowReloadSameScene = false;     // 如果为 false，当前场景与目标相同则不重复加载

    float enterMinTime = -1f, enterMaxTime = -1f;
    bool latchedMin = false, latchedMax = false;

    void Reset() { hinge = GetComponent<HingeJoint>(); }

    void Update()
    {
        if (!hinge) return;

        float a = hinge.angle; // 以“初始姿态”为0°

        // ——最小角方向触发——
        if (a <= minTriggerAngle)
        {
            if (!latchedMin)
            {
                if (enterMinTime < 0f) enterMinTime = Time.unscaledTime;
                if (Time.unscaledTime - enterMinTime >= holdSeconds)
                {
                    latchedMin = true; latchedMax = false;
                    LoadByIndex(indexAtMin);
                }
            }
        }
        else if (a > minTriggerAngle + hysteresis)
        {
            enterMinTime = -1f; latchedMin = false;
        }

        // ——最大角方向触发——
        if (a >= maxTriggerAngle)
        {
            if (!latchedMax)
            {
                if (enterMaxTime < 0f) enterMaxTime = Time.unscaledTime;
                if (Time.unscaledTime - enterMaxTime >= holdSeconds)
                {
                    latchedMax = true; latchedMin = false;
                    LoadByIndex(indexAtMax);
                }
            }
        }
        else if (a < maxTriggerAngle - hysteresis)
        {
            enterMaxTime = -1f; latchedMax = false;
        }
    }

    void LoadByIndex(int buildIndex)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0) return;

        buildIndex = Mathf.Clamp(buildIndex, 0, count - 1);
        if (!allowReloadSameScene &&
            SceneManager.GetActiveScene().buildIndex == buildIndex) return;

        SceneManager.LoadScene(buildIndex);
    }

    // 需要运行时修改可调用这两个方法（比如UI按钮/事件）
    public void SetIndexAtMin(int idx) => indexAtMin = idx;
    public void SetIndexAtMax(int idx) => indexAtMax = idx;
}
