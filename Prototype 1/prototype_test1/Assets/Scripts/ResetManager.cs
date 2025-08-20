using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 独立复位管理器：放在“常驻、不被隐藏”的物体上（例如场景根的 _ResetManager）
/// 目标：
/// 1) 无论面板/UI被隐藏，或整个画板父集被 SetActive(false)，按 R/按钮都能让它们“复活并回到初始”
/// 2) 恢复内容包括：父集的 local 位置/旋转/缩放、每个子物体的激活状态、(可选) Paint 的状态与画布内容
/// 使用：
/// - groupRoot 指向“画板 + UI 的共同父级”
/// - paintTargets 填入所有需要被重置的 Paint 组件（可留空）
/// </summary>
public class ResetManager : MonoBehaviour
{
    [Header("要复位的父级（画板+UI 的共同根节点）")]
    public GameObject groupRoot;                      // 即便 inactive，引用仍有效

    [Header("需要同步复位的 Paint 组件（可选，多个）")]
    public Paint[] paintTargets;                      // 留空则不重置 Paint 状态

    [Header("按键设置")]
    public bool enableHotkey = true;
    public KeyCode resetKey = KeyCode.R;

    [Header("复位行为")]
    public bool reactivateOnReset = true;             // 复位前若父集隐藏 -> 先激活
    public bool restoreChildrenActiveStates = true;   // 恢复每个子物体的初始显示/隐藏状态
    public bool resetPaintState = true;               // 恢复 Paint 的尺寸/颜色/橡皮
    public bool clearPaintTextureOnReset = true;      // 复位时清空画布

    [Header("Awake 时自动记录“初始状态”")]
    public bool captureOnAwake = true;

    // —— 初始 Transform（父集）——
    Vector3 initLocalPos;
    Quaternion initLocalRot;
    Vector3 initLocalScale;

    // —— 初始“子物体激活状态”快照 ——（包含所有后代；捕获时包含 inactive 物体）
    List<Transform> initNodes = new List<Transform>();
    List<bool> initActive = new List<bool>();

    // —— 初始 Paint 状态 ——（与 paintTargets 一一对应）
    int[] initBrushRadius;
    Color[] initBrushColor;
    bool[] initEraser;

    bool initialized;

    void Awake()
    {
        if (!groupRoot)
        {
            Debug.LogError("[ResetManager] 请在 Inspector 指定 groupRoot（画板父集）。");
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
    /// 记录“当前状态”为“初始状态”（父集local姿态、全树激活状态、Paint状态）
    /// 可在运行中调用（例如允许用户“重设默认摆放/配置”）
    /// </summary>
    [ContextMenu("Capture Initial From Current")]
    public void CaptureInitial()
    {
        // 1) 父集 Transform（local）
        var t = groupRoot.transform;
        initLocalPos   = t.localPosition;
        initLocalRot   = t.localRotation;
        initLocalScale = t.localScale;

        // 2) 记录全树激活状态（包含隐藏物体）
        initNodes.Clear(); initActive.Clear();
        if (restoreChildrenActiveStates)
        {
            // true 参数：包含 inactive 的对象
            var all = groupRoot.GetComponentsInChildren<Transform>(true);
            foreach (var node in all)
            {
                initNodes.Add(node);
                initActive.Add(node.gameObject.activeSelf);
            }
        }

        // 3) 记录 Paint 初始状态
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
                initBrushColor[i]  = p.brushColor; // 你在 Paint 里把 brushColor 定义为“用户当前的画笔色”
                initEraser[i]      = p.eraserMode;
            }
        }

        initialized = true;
    }

    /// <summary>
    /// 执行复位：必要时先激活父集 -> 恢复父集local姿态 -> 恢复子物体激活状态 -> (可选)重置Paint并清空画布
    /// </summary>
    [ContextMenu("Reset Now")]
    public void ResetNow()
    {
        if (!groupRoot) return;

        if (!initialized)
        {
            Debug.LogWarning("[ResetManager] 未初始化，已自动用当前状态作为初始。");
            CaptureInitial();
        }

        // 0) 若父集被隐藏 -> 先激活（这样后面设置子物体 active 才会生效）
        if (reactivateOnReset && !groupRoot.activeSelf)
            groupRoot.SetActive(true);

        // 1) 恢复父集 local 位姿
        var t = groupRoot.transform;
        t.localPosition = initLocalPos;
        t.localRotation = initLocalRot;
        t.localScale    = initLocalScale;

        // 2) 恢复整棵树每个节点的激活状态（把 UI 面板也重新显示出来）
        if (restoreChildrenActiveStates && initNodes.Count == initActive.Count)
        {
            for (int i = 0; i < initNodes.Count; i++)
            {
                var node = initNodes[i];
                if (!node) continue; // 有的节点可能在运行中被销毁了
                node.gameObject.SetActive(initActive[i]);
            }
        }

        // 3) (可选) 恢复 Paint 状态 + 清空画布
        if (paintTargets != null && paintTargets.Length > 0 && resetPaintState)
        {
            for (int i = 0; i < paintTargets.Length; i++)
            {
                var p = paintTargets[i];
                if (!p) continue;

                // 恢复笔刷、颜色、橡皮
                if (initBrushRadius != null && i < initBrushRadius.Length)
                    p.SetBrushSize(initBrushRadius[i]);

                if (initBrushColor != null && i < initBrushColor.Length)
                    p.SetColor(initBrushColor[i]);

                if (initEraser != null && i < initEraser.Length)
                    p.SetEraser(initEraser[i]);
                else
                    p.SetEraser(false); // 若没记录，默认回到画笔

                // 清空画布
                if (clearPaintTextureOnReset)
                    p.ClearTexture();
            }
        }
    }
}
