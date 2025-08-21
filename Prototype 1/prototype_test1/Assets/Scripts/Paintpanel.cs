using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 仅负责“绘图 UI 面板”的交互逻辑：
/// - 颜色选择、清空、橡皮切换、回到画笔、关闭面板；
/// - 滑条统一控制笔/橡皮粗细；
/// - “重置”按钮不再直接改 Transform，而是转发给 ResetManager（避免冲突/失效）
/// </summary>
public class PaintPanel : MonoBehaviour
{
    [Header("目标绘制脚本（画笔逻辑）")]
    public Paint paint;

    [Header("颜色按钮（Image 的颜色即为画笔色）")]
    public Button[] colorButtons;

    [Header("清空 / 橡皮(切换) / 笔 / 关闭 / 复位（转发）")]
    public Button clearButton;
    public Button eraserToggleButton;     // 橡皮 ↔ 画笔 切换
    public Button penButton;              // 一键回到“画笔”
    public Button closePaintPanel;        // 关闭本 UI 面板
    public Button resetCanvasButton;      // 转发给 ResetManager 的“复位”按钮

    [Header("笔刷粗细滑条（建议最小1，最大128）")]
    public Slider sizeSlider;

    [Header("外部复位管理器（独立存在，优先级最高）")]
    public ResetManager resetManager;

    // —— 新增：展示用游标（笔/橡皮），不影响绘制逻辑 —— //
    [Header("展示用：笔/橡皮模型显隐控制")]
    public PenCursorOnBoard penCursor;      // 挂载了笔模型的实例
    public EraserCursorOnBoard eraserCursor;   // 挂载了橡皮模型的实例

    // 仅用于 UI 同步当前“我们认为的工具状态”（如果 Paint 没暴露 IsEraser）
    private bool assumedEraser = false;

    void Awake()
    {
        // 找不到 Paint 就禁用，避免空引用
        if (!paint) paint = FindObjectOfType<Paint>();
        if (!paint) { Debug.LogError("[PaintPanel] 未找到 Paint 组件。"); enabled = false; return; }

        // —— 颜色按钮：点击即把其 Image.color 设置为画笔色 —— //
        if (colorButtons != null)
        {
            foreach (var btn in colorButtons)
            {
                if (!btn) continue;
                var img = btn.GetComponent<Image>();
                var c = img ? img.color : Color.black;
                btn.onClick.AddListener(() => paint.SetColor(c));
                // 颜色按钮不改变游标显隐
            }
        }

        // 清空画布
        if (clearButton)
            clearButton.onClick.AddListener(() =>
            {
                paint.ClearTexture();
                // 触发其他功能时：两种游标均隐藏
                if (penCursor) penCursor.Show(false);
                if (eraserCursor) eraserCursor.Show(false);
            });

        // 橡皮 ↔ 画笔 切换（调用 Paint 的 ToggleEraser）
        if (eraserToggleButton)
            eraserToggleButton.onClick.AddListener(() =>
            {
                paint.ToggleEraser();
                assumedEraser = !assumedEraser;

                // 显示橡皮游标、隐藏笔游标
                if (eraserCursor) eraserCursor.Show(true);
                if (penCursor)    penCursor.Show(false);
            });

        // “笔”按钮：如果当前是橡皮，则再切一次回到画笔
        if (penButton)
            penButton.onClick.AddListener(() =>
            {
                if (assumedEraser)
                {
                    paint.ToggleEraser();
                    assumedEraser = false;
                }
                // 显示笔游标、隐藏橡皮游标
                if (penCursor)    penCursor.Show(true);
                if (eraserCursor) eraserCursor.Show(false);
            });

        // 关闭本 UI 面板（注意：只是隐藏 UI，不影响 ResetManager 工作）
        if (closePaintPanel)
            closePaintPanel.onClick.AddListener(() =>
            {
                // 关闭面板时游标也隐藏
                if (penCursor)    penCursor.Show(false);
                if (eraserCursor) eraserCursor.Show(false);
                gameObject.SetActive(false);
            });

        // 粗细滑条：统一控制笔/橡皮的粗细（Paint 里要确保 SetBrushSize 同步两者）
        if (sizeSlider)
        {
            // 基本边界与整数步进
            if (sizeSlider.minValue <= 0f) sizeSlider.minValue = 1f;
            if (sizeSlider.maxValue < sizeSlider.minValue) sizeSlider.maxValue = 128f;
            sizeSlider.wholeNumbers = true;

            // 初始值与回调
            sizeSlider.value = Mathf.Clamp(paint.brushRadius, sizeSlider.minValue, sizeSlider.maxValue);
            sizeSlider.onValueChanged.AddListener(v => paint.SetBrushSize(v));
        }

        // “复位”按钮转发：调用 ResetManager（不要直接在这里改 Transform，避免与 ResetManager 冲突）
        if (resetCanvasButton)
            resetCanvasButton.onClick.AddListener(() =>
            {
                // 复位时游标隐藏
                if (penCursor)    penCursor.Show(false);
                if (eraserCursor) eraserCursor.Show(false);
                resetManager?.ResetNow();
            });
    }

    void OnEnable()
    {
        // 打开面板时，同步一次滑条显示值（避免外部改过 brushRadius 导致显示不同步）
        if (sizeSlider) sizeSlider.SetValueWithoutNotify(paint.brushRadius);
    }

    void OnDisable() { if (penCursor) penCursor.Show(false); if (eraserCursor) eraserCursor.Show(false); }
    void OnDestroy() { if (penCursor) penCursor.Show(false); if (eraserCursor) eraserCursor.Show(false); }
}
