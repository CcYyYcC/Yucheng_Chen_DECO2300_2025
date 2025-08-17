using UnityEngine;
using UnityEngine.UI;

public class PaintPanel : MonoBehaviour
{
    [Header("目标绘制脚本")]
    public Paint paint;

    [Header("颜色按钮（Image 的颜色即为画笔色）")]
    public Button[] colorButtons;

    [Header("清空 / 橡皮 / 关闭")]
    public Button clearButton;
    public Button eraserToggleButton;
    public Button closePaintPanel;

    [Header("笔刷粗细滑条（建议最小1，最大128）")]
    public Slider sizeSlider;

    void Awake()
    {
        if (paint == null) paint = FindObjectOfType<Paint>();
        if (paint == null) { Debug.LogError("[PaintPanel] 未找到 Paint 组件。"); enabled = false; return; }

        // 颜色按钮：闭包安全
        if (colorButtons != null)
        {
            foreach (var btn in colorButtons)
            {
                if (btn == null) continue;
                var img = btn.GetComponent<Image>();
                var color = (img != null) ? img.color : Color.black;
                btn.onClick.AddListener(() => paint.SetColor(color));
            }
        }

        // 清空
        if (clearButton != null)
            clearButton.onClick.AddListener(() => paint.ClearTexture());

        // 橡皮开关
        if (eraserToggleButton != null)
            eraserToggleButton.onClick.AddListener(() => paint.ToggleEraser());

        // 关闭
        if (closePaintPanel != null)
            closePaintPanel.onClick.AddListener(() => gameObject.SetActive(false));

        // 粗细滑条
        if (sizeSlider != null)
        {
            if (sizeSlider.minValue <= 0f) sizeSlider.minValue = 1f;
            if (sizeSlider.maxValue < sizeSlider.minValue) sizeSlider.maxValue = 128f;

            sizeSlider.wholeNumbers = true;
            sizeSlider.value = Mathf.Clamp(paint.brushRadius, (int)sizeSlider.minValue, (int)sizeSlider.maxValue);

            sizeSlider.onValueChanged.AddListener(v => paint.SetBrushSize(v));
        }
    }

    void OnEnable()
    {
        // 打开面板时同步一次当前笔刷大小（运行中可能外部改过）
        if (sizeSlider != null)
        {
            // 若你的 Unity 版本没有这个 API，可改用：sizeSlider.value = paint.brushRadius;
            sizeSlider.SetValueWithoutNotify(paint.brushRadius);
        }
    }
}
