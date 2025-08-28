using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))] // Ensure a RawImage exists on this GameObject
public class Paint : MonoBehaviour
{
    // This script turns a RawImage into a drawable canvas.
    // At runtime it creates a Texture2D, assigns it to RawImage.texture,
    // and then “paints” pixels on that texture while you hold the left mouse button.
    // The “eraser” is just drawing with backgroundColor instead of brushColor.

    [Header("Canvas (which RawImage to paint on)")]
    public RawImage rawImage;

    [Header("Runtime paint texture (created and assigned at Start)")]
    public Texture2D texture;

    [Header("Background / Brush colors (store user brush color for toggling)")]
    public Color backgroundColor = Color.white; // Used by ClearTexture() and the eraser
    public Color brushColor = Color.black;      // User-selected brush color

    [Header("Current drawing color (internal)")]
    public Color paintColor = Color.black;      // The color actually written to pixels

    [Header("Brush radius in pixels (larger = thicker)")]
    public int brushRadius = 12;

    [Header("Eraser toggle")]
    public bool eraserMode = false;             // true = eraser (draw backgroundColor)

    [Header("Render scale (texture resolution = Rect size * renderScale)")]
    public int renderScale = 3;                 // 2–4 recommended (higher = sharper but heavier)

    // —— Runtime state —— //
    private Vector2? lastMousePos = null;       // Last painted pixel position (for lines)
    private bool isDrawing = false;             // Are we currently holding LMB to draw?
    private Camera uiCam;                       // Camera used for World Space UI coordinate conversion

    // Use a coroutine Start so we can wait one frame for RectTransform sizing to settle.
    private IEnumerator Start()
    {
        // 1) Get RawImage if not assigned.
        if (rawImage == null) rawImage = GetComponent<RawImage>();

        // 2) Choose the camera for ScreenPoint → UI conversions:
        //    - World Space Canvas: use canvas.worldCamera (fallback to Camera.main)
        //    - Overlay / Screen Space Camera: can pass null
        var canvas = rawImage.canvas;
        uiCam = (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            ? (canvas.worldCamera != null ? canvas.worldCamera : Camera.main)
            : null;

        // 3) Wait one frame so RectTransform has final width/height (avoids 0-size).
        yield return null;

        // 4) Create the paint texture from RawImage rect * renderScale.
        RectTransform rt = rawImage.rectTransform;
        int width  = Mathf.Max(1, Mathf.RoundToInt(rt.rect.width  * Mathf.Max(1, renderScale)));
        int height = Mathf.Max(1, Mathf.RoundToInt(rt.rect.height * Mathf.Max(1, renderScale)));

        // 5) Clamp to GPU maximum texture size (safety).
        int max = SystemInfo.maxTextureSize;
        width  = Mathf.Clamp(width,  4, max);
        height = Mathf.Clamp(height, 4, max);

        // 6) Create and configure the Texture2D.
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode   = TextureWrapMode.Clamp;    // Avoid tiling at UV edges
        texture.filterMode = FilterMode.Trilinear;     // Slight smoothing when scaled

        // 7) Assign our canvas texture to the RawImage.
        rawImage.texture = texture;

        // 8) Start in brush mode.
        eraserMode = false;
        paintColor = brushColor;

        // 9) Clear the canvas to backgroundColor.
        ClearTexture();
    }

    void Update()
    {
        // Quick toggle: Q switches eraser ↔ brush.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleEraser();
        }

        // LMB press/release to start/stop drawing.
        if (Input.GetMouseButtonDown(0)) { lastMousePos = null; isDrawing = true; }
        if (Input.GetMouseButtonUp(0))   { isDrawing = false;  lastMousePos = null; }

        // Not holding LMB → do nothing.
        if (!Input.GetMouseButton(0)) return;

        // Only draw when the mouse is inside the RawImage rect.
        // (For World Space UI, pass uiCam; for Overlay you can pass null.)
        if (!RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, Input.mousePosition, uiCam))
        {
            lastMousePos = null; // Break the line continuity when leaving the rect
            return;
        }

        // Convert mouse to texture pixel coordinates.
        Vector2 pixelUV = GetPixelUV();

        // If we have a previous point and it changed → draw a line of circles between them.
        if (isDrawing && lastMousePos.HasValue && lastMousePos.Value != pixelUV)
        {
            DrawLine(lastMousePos.Value, pixelUV);
        }
        // First click or no movement → draw a single dot (circle).
        else if (isDrawing)
        {
            DrawCircle(pixelUV);
            texture.Apply(false); // Push changes to GPU (false = faster, no mipmap rebuild)
        }

        // Remember this position for the next frame (line continuity).
        lastMousePos = pixelUV;
    }

    // ===================== Screen point → Texture pixel =====================
    Vector2 GetPixelUV()
    {
        // 1) Get screen-space mouse position (pixels; bottom-left is (0,0)).
        Vector2 mousePos = Input.mousePosition;
        RectTransform rt = rawImage.rectTransform;

        // 2) Convert to the RawImage's local space (centered at (0,0)).
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, mousePos, uiCam, out localMousePos);

        // 3) Shift to a bottom-left origin (0,0) for easier math.
        Rect rect = rt.rect;
        localMousePos += new Vector2(rect.width * 0.5f, rect.height * 0.5f);

        // 4) Map local coordinates proportionally to texture pixel coordinates.
        //    Note: texture size = rect size * renderScale, so proportional mapping is enough.
        return new Vector2(
            localMousePos.x * texture.width  / rect.width,
            localMousePos.y * texture.height / rect.height
        );
    }

    // ======================== Draw a line (lerp + circle brush) ========================
    void DrawLine(Vector2 start, Vector2 end)
    {
        // Choose how many steps based on distance and brush size.
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / Mathf.Max(1, brushRadius * 0.25f)));

        // Lerp along the segment and stamp a circle at each step.
        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(start, end, i / (float)steps);
            DrawCircle(p);
        }

        // Apply once after the loop (more efficient).
        texture.Apply(false);
    }

    // ======================== Draw one circular brush dab =========================
    void DrawCircle(Vector2 center)
    {
        // Round center to integer pixels; compute radius and radius^2.
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int r  = Mathf.Max(1, brushRadius);
        int r2 = r * r;

        // Compute a bounding box inside the texture to avoid out-of-range writes.
        int xmin = Mathf.Max(cx - r, 0);
        int xmax = Mathf.Min(cx + r, texture.width  - 1);
        int ymin = Mathf.Max(cy - r, 0);
        int ymax = Mathf.Min(cy + r, texture.height - 1);

        // Circle equation: dx*dx + dy*dy <= r*r → fill with paintColor.
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

    // ============================= UI-facing API ================================
    public void SetColor(Color c)
    {
        // Update the user-facing brush color.
        brushColor = c;

        // If not in eraser mode, immediately use it as the active paintColor.
        if (!eraserMode) paintColor = brushColor;
    }

    public void SetBrushSize(float size) // e.g., hooked to a Slider
    {
        // Clamp to integer pixels ≥ 1.
        brushRadius = Mathf.Max(1, Mathf.RoundToInt(size));
    }

    public void ClearTexture()
    {
        // Fill the entire texture with backgroundColor in one go (fast path).
        if (texture == null) return;
        Color32[] buf = new Color32[texture.width * texture.height];
        for (int i = 0; i < buf.Length; i++) buf[i] = backgroundColor;
        texture.SetPixels32(buf);
        texture.Apply(false);
    }

    // Toggle eraser ↔ brush
    public void ToggleEraser() => SetEraser(!eraserMode);

    public void SetEraser(bool on)
    {
        eraserMode = on;
        // Eraser paints backgroundColor; brush paints brushColor.
        paintColor = eraserMode ? backgroundColor : brushColor;
    }
}
