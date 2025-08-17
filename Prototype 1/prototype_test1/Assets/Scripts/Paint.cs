using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class Paint : MonoBehaviour
{
    [Header("画板")]
    public RawImage rawImage;

    [Header("所画的图片（运行时创建）")]
    public Texture2D texture;

    [Header("背景色 / 画笔色（记住用户色用于切回）")]
    public Color backgroundColor = Color.white;
    public Color brushColor = Color.black;   // 用户选择的画笔颜色
    [Header("当前绘制颜色（内部使用）")]
    public Color paintColor = Color.black;   // 实际落到像素上的颜色

    [Header("画笔半径（像素）")]
    public int brushRadius = 12;             // 调大=加粗

    [Header("橡皮开关")]
    public bool eraserMode = false;

    [Header("渲染倍率（提高贴图分辨率）")]
    public int renderScale = 3;              // 建议 2~4

    private Vector2? lastMousePos = null;    // 上一次像素位置
    private bool isDrawing = false;
    private Camera uiCam;                    // World Space Canvas 的相机

    private IEnumerator Start()
    {
        if (rawImage == null) rawImage = GetComponent<RawImage>();

        // World Space 模式下使用 Canvas 的 World Camera（没有就退回 MainCamera）
        var canvas = rawImage.canvas;
        uiCam = (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            ? (canvas.worldCamera != null ? canvas.worldCamera : Camera.main)
            : null; // Overlay/ScreenSpace-Camera 可为 null

        // 等一帧，确保 RectTransform 尺寸稳定（避免 0 宽高）
        yield return null;

        // 按 Rect 大小 * 渲染倍率 创建更高分辨率贴图
        RectTransform rt = rawImage.rectTransform;
        int width  = Mathf.Max(1, Mathf.RoundToInt(rt.rect.width  * Mathf.Max(1, renderScale)));
        int height = Mathf.Max(1, Mathf.RoundToInt(rt.rect.height * Mathf.Max(1, renderScale)));

        // 安全夹取，避免超出显卡上限
        int max = SystemInfo.maxTextureSize;
        width  = Mathf.Clamp(width,  4, max);
        height = Mathf.Clamp(height, 4, max);

        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode   = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear; // 略柔一点

        rawImage.texture = texture;

        // 初始化颜色（进入时是画笔模式）
        eraserMode = false;
        paintColor = brushColor;

        ClearTexture();
    }

    void Update()
    {
        // Q 键：切换橡皮 ↔ 画笔
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleEraser();
        }

        if (Input.GetMouseButtonDown(0)) { lastMousePos = null; isDrawing = true; }
        if (Input.GetMouseButtonUp(0))   { isDrawing = false;  lastMousePos = null; }

        if (!Input.GetMouseButton(0)) return;

        // 仅在鼠标位于画板范围内时绘制（World Space 要传 uiCam）
        if (!RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, Input.mousePosition, uiCam))
        {
            lastMousePos = null;
            return;
        }

        Vector2 pixelUV = GetPixelUV();
        if (isDrawing && lastMousePos.HasValue && lastMousePos.Value != pixelUV)
        {
            DrawLine(lastMousePos.Value, pixelUV);
        }
        else if (isDrawing)
        {
            DrawCircle(pixelUV);
            texture.Apply(false);
        }
        lastMousePos = pixelUV;
    }

    // 屏幕坐标 -> 贴图像素坐标
    Vector2 GetPixelUV()
    {
        Vector2 mousePos = Input.mousePosition;
        RectTransform rt = rawImage.rectTransform;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePos, uiCam, out localMousePos);

        // 局部(中心0,0) -> 左下角(0,0)
        Rect rect = rt.rect;
        localMousePos += new Vector2(rect.width * 0.5f, rect.height * 0.5f);

        // 本地 -> 贴图像素（贴图尺寸已经乘了 renderScale，这里直接按比例映射即可）
        return new Vector2(
            localMousePos.x * texture.width  / rect.width,
            localMousePos.y * texture.height / rect.height
        );
    }

    // 分段插值 + 圆刷（步数更密：0.25f）
    void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(1, brushRadius * 0.25f)));
        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(start, end, i / (float)steps);
            DrawCircle(p);
        }
        texture.Apply(false);
    }

    // 圆形笔刷（简单直观）
    void DrawCircle(Vector2 center)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int r  = Mathf.Max(1, brushRadius);
        int r2 = r * r;

        int xmin = Mathf.Max(cx - r, 0);
        int xmax = Mathf.Min(cx + r, texture.width  - 1);
        int ymin = Mathf.Max(cy - r, 0);
        int ymax = Mathf.Min(cy + r, texture.height - 1);

        for (int y = ymin; y <= ymax; y++)
        {
            int dy = y - cy; int dy2 = dy * dy;
            for (int x = xmin; x <= xmax; x++)
            {
                int dx = x - cx;
                if (dx * dx + dy2 <= r2) texture.SetPixel(x, y, paintColor);
            }
        }
    }

    // —— 提供给 UI 的接口（不丢）——
    public void SetColor(Color c)
    {
        brushColor = c;                  // 记住用户色
        if (!eraserMode) paintColor = brushColor; // 非橡皮时立即生效
    }

    public void SetBrushSize(float size) // 配合 Slider
    {
        brushRadius = Mathf.Max(1, Mathf.RoundToInt(size));
    }

    public void ClearTexture()
    {
        if (texture == null) return;
        Color32[] buf = new Color32[texture.width * texture.height];
        for (int i = 0; i < buf.Length; i++) buf[i] = backgroundColor;
        texture.SetPixels32(buf);
        texture.Apply(false);
    }

    // 橡皮：开/关
    public void ToggleEraser() => SetEraser(!eraserMode);

    public void SetEraser(bool on)
    {
        eraserMode = on;
        paintColor = eraserMode ? backgroundColor : brushColor;
    }
}
