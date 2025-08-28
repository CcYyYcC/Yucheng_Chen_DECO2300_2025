using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsibility:
/// - Wires up the PAINT UI panel: color buttons, Clear, Eraser toggle, Pen button, Close panel,
///   Reset (forwarded), and the brush-size Slider.
/// - Shows a visual cursor model for the pen or eraser (purely visual; does not affect drawing).
/// - For Reset: **does not** move the board here; it forwards to an external ResetManager,
///   so there is only one place that owns "reset" logic.
///
/// Notes:
/// - This script depends on a separate `Paint` component (the actual drawing logic).
/// - `PenCursorOnBoard` / `EraserCursorOnBoard` are optional visual helpers.
/// - It keep an internal `assumedEraser` flag because `Paint` doesn’t expose `IsEraser`.
/// </summary>
public class PaintPanel : MonoBehaviour
{
    [Header("Target drawing logic")]
    public Paint paint; // The Paint component that handles drawing on a RawImage

    [Header("Color buttons (the Image.color will be used as brushColor)")]
    public Button[] colorButtons;

    [Header("Clear / Eraser(toggle) / Pen / Close / Reset(forward)")]
    public Button clearButton;        // Clears the canvas (to backgroundColor)
    public Button eraserToggleButton; // Switch eraser <-> brush on the Paint
    public Button penButton;          // Force back to brush mode (if currently erasing)
    public Button closePaintPanel;    // Hide this UI panel GameObject
    public Button resetCanvasButton;  // Forward a Reset call to ResetManager

    [Header("Brush size Slider (recommended 1..128)")]
    public Slider sizeSlider;

    [Header("External Reset Manager (single source of truth for reset)")]
    public ResetManager resetManager; // Owns the actual Transform reset logic

    // ---- Visual cursors (purely cosmetic; do not change draw logic) ----
    [Header("Visual-only: show/hide models for pen & eraser")]
    public PenCursorOnBoard penCursor;        // Instance that moves a pen model under mouse
    public EraserCursorOnBoard eraserCursor;  // Instance that moves an eraser model under mouse

    // Internal UI state: what we *assume* the current tool is (since Paint has no IsEraser getter).
    private bool assumedEraser = false;

    void Awake()
    {
        // Ensure we have a Paint target. If not found, disable this component to avoid null refs.
        if (!paint) paint = FindObjectOfType<Paint>();
        if (!paint) { Debug.LogError("[PaintPanel] Paint component not found."); enabled = false; return; }

        // ---------------------- Color buttons wiring ----------------------
        // Each color button uses its Image.color as the new brushColor for Paint.
        // Important: color selection does NOT show/hide any cursor visuals.
        if (colorButtons != null)
        {
            foreach (var btn in colorButtons)
            {
                if (!btn) continue;
                var img = btn.GetComponent<Image>();
                var c = img ? img.color : Color.black;

                // Closure-safe capture: we copy 'c' to a local variable per iteration above.
                btn.onClick.AddListener(() => paint.SetColor(c));
            }
        }

        // -------------------------- Clear button --------------------------
        // Clears the paint texture to backgroundColor.
        // Also hide any active visual cursor to avoid confusing states.
        if (clearButton)
        {
            clearButton.onClick.AddListener(() =>
            {
                paint.ClearTexture();
                if (penCursor)    penCursor.Show(false);
                if (eraserCursor) eraserCursor.Show(false);
            });
        }

        // -------------------- Eraser <-> Brush toggle ---------------------
        // Calls Paint.ToggleEraser() and flips our local assumed flag.
        // Visual policy: when switching to eraser, show eraser cursor and hide pen cursor.
        if (eraserToggleButton)
        {
            eraserToggleButton.onClick.AddListener(() =>
            {
                paint.ToggleEraser();
                assumedEraser = !assumedEraser;

                if (eraserCursor) eraserCursor.Show(true);
                if (penCursor)    penCursor.Show(false);
            });
        }

        // -------------------------- Pen button ----------------------------
        // If we think we are in eraser mode, toggle again to return to brush.
        // Visual policy: when switching to pen, show pen cursor and hide eraser cursor.
        if (penButton)
        {
            penButton.onClick.AddListener(() =>
            {
                if (assumedEraser)
                {
                    paint.ToggleEraser(); // back to brush
                    assumedEraser = false;
                }

                if (penCursor)    penCursor.Show(true);
                if (eraserCursor) eraserCursor.Show(false);
            });
        }

        // ------------------------- Close panel ----------------------------
        // Only hides THIS UI panel (SetActive(false)).
        // Also hides visual cursors (so nothing hovers when panel is closed).
        if (closePaintPanel)
        {
            closePaintPanel.onClick.AddListener(() =>
            {
                if (penCursor)    penCursor.Show(false);
                if (eraserCursor) eraserCursor.Show(false);
                gameObject.SetActive(false);
            });
        }

        // ----------------------- Brush size Slider ------------------------
        // Single source for brush/eraser thickness: Paint.SetBrushSize controls the radius.
        // We guard min/max and use whole numbers for stable pixel sizes.
        if (sizeSlider)
        {
            if (sizeSlider.minValue <= 0f) sizeSlider.minValue = 1f;
            if (sizeSlider.maxValue < sizeSlider.minValue) sizeSlider.maxValue = 128f;
            sizeSlider.wholeNumbers = true;

            // Initialize the slider GUI to match Paint’s current radius.
            sizeSlider.value = Mathf.Clamp(paint.brushRadius, sizeSlider.minValue, sizeSlider.maxValue);

            // Hook change: pushing size back into Paint (which affects both brush & eraser).
            sizeSlider.onValueChanged.AddListener(v => paint.SetBrushSize(v));
        }

        // ----------------------------- Reset ------------------------------
        // Do NOT move/rotate/scale anything here. We forward to ResetManager
        // so there is a single authority for "reset" logic (prevents conflicts).
        // Also hide cursors so the board returns clean.
        if (resetCanvasButton)
        {
            resetCanvasButton.onClick.AddListener(() =>
            {
                if (penCursor)    penCursor.Show(false);
                if (eraserCursor) eraserCursor.Show(false);
                resetManager?.ResetNow(); // Safe call (?.) if ResetManager not assigned
            });
        }
    }

    void OnEnable()
    {
        // When the panel re-opens, update the slider display to the current brushRadius
        // without firing the onValueChanged callback (so we don’t re-apply it).
        if (sizeSlider) sizeSlider.SetValueWithoutNotify(paint.brushRadius);
    }

    void OnDisable()
    {
        // If the panel is hidden/disabled, also hide the visual cursors to avoid floating models.
        if (penCursor)    penCursor.Show(false);
        if (eraserCursor) eraserCursor.Show(false);
    }

    void OnDestroy()
    {
        // Same as OnDisable: ensure visuals are hidden if this component is destroyed.
        if (penCursor)    penCursor.Show(false);
        if (eraserCursor) eraserCursor.Show(false);
    }
}
