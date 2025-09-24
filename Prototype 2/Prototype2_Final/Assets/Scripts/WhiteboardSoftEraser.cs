using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Whiteboard soft eraser tool:
/// - Casts a ray from the eraser tip to detect the whiteboard surface.
/// - Uses a precomputed Gaussian mask to "softly" erase drawn pixels with smooth edges,
///   instead of sharp square blocks.
/// - Can either restore to the board's base texture / clear color, or fade pixels to transparency.
/// - Interpolates between consecutive touch points to avoid gaps when moving quickly.
/// 
/// Requirements:
/// - Whiteboard GameObject must be tagged "Whiteboard" and have a collider.
/// - Whiteboard script must expose: texture (Texture2D), baseTexture (Texture2D snapshot), textureSize (Vector2Int).
/// - Whiteboard texture format should support alpha (RGBA32) if using transparent erase.
/// </summary>
public class WhiteboardSoftEraser : MonoBehaviour
{
    [Header("Tip (ray origin/length)")]
    [SerializeField] private Transform _tip;               // Transform of the eraser tip (raycast origin)

    [Header("Radius (pixels) and softness")]
    [Tooltip("Radius in pixels. Final diameter = 2*radius + 1")]
    [SerializeField] private int _radius = 20;             // Eraser radius in pixels

    [Tooltip("Gaussian softness factor (0.1 = harder edge, 1.0 = very soft)")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _softness = 0.7f;       // Controls Gaussian sigma

    [Header("Erase target")]
    [Tooltip("Color to erase into (if the board is a flat background color, assign that)")]
    [SerializeField] private Color _eraseToColor = Color.white;

    [Tooltip("If true, will gradually reduce Alpha instead of restoring color. Requires transparent whiteboard material & RGBA32 texture.")]
    [SerializeField] private bool _eraseToTransparent = false;

    [Header("Ray direction (default uses object's local Y axis)")]
    [SerializeField] private bool _useTipAxis = false;     // If true, use _tipAxis instead of transform.up
    [SerializeField] private Vector3 _tipAxis = Vector3.up;// Local axis used when _useTipAxis is true

    private float _tipHeight = 0.02f;                      // Ray length (based on tip scale)
    private Whiteboard _whiteboard;                        // Reference to whiteboard component
    private RaycastHit _hit;                               // Last raycast hit info

    // Stroke state
    private bool _touchedLastFrame = false;                // Whether eraser was touching board in previous frame
    private Vector2Int _lastPx;                            // Previous pixel coordinates
    private Quaternion _lastRot;                           // Previous rotation (stabilizes hand feel)

    // Precomputed Gaussian mask
    private float[] _mask;                                 // Flattened array of weights, size = diameter * diameter
    private int _diameter;                                 // Stamp diameter
    private float _sigma;                                  // Gaussian sigma, proportional to radius * softness

    void Start()
    {
        // Set ray length from tip scale if available
        if (_tip != null)
            _tipHeight = Mathf.Max(0.005f, _tip.localScale.y);

        BuildMask();
    }

    void OnValidate()
    {
        // Ensure valid inspector values
        if (_radius < 1) _radius = 1;
        if (_softness < 0.1f) _softness = 0.1f;
        BuildMask();
    }

    void Update()
    {
        EraseUpdate();
    }

    /// <summary>
    /// Builds the Gaussian weight mask based on radius and softness.
    /// Values near the center = 1, tapering to 0 at edges.
    /// </summary>
    private void BuildMask()
    {
        _diameter = _radius * 2 + 1;
        _mask = new float[_diameter * _diameter];

        // Gaussian: w = exp(-d^2 / (2*sigma^2))
        _sigma = Mathf.Max(0.001f, _radius * _softness);

        int i = 0;
        for (int y = -_radius; y <= _radius; y++)
        {
            for (int x = -_radius; x <= _radius; x++, i++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > _radius)
                {
                    _mask[i] = 0f; // Outside circle
                }
                else
                {
                    float w = Mathf.Exp(-(dist * dist) / (2f * _sigma * _sigma));
                    _mask[i] = Mathf.Clamp01(w);
                }
            }
        }
    }

    /// <summary>
    /// Performs raycast to find whiteboard and applies erase stamp(s).
    /// </summary>
    private void EraseUpdate()
    {
        // Ray origin = tip, direction = tip axis or transform.up
        Vector3 origin = _tip.position;
        Vector3 dir = _useTipAxis ? (_tip.TransformDirection(_tipAxis)) : transform.up;

        if (Physics.Raycast(origin, dir, out _hit, _tipHeight))
        {
            if (_hit.transform.CompareTag("Whiteboard"))
            {
                // Cache whiteboard reference
                if (_whiteboard == null)
                    _whiteboard = _hit.transform.GetComponent<Whiteboard>();
                if (_whiteboard == null || _whiteboard.texture == null) return;

                // Convert UV (0..1) to pixel coordinates
                Vector2 uv = _hit.textureCoord;
                Vector2Int px = new Vector2Int(
                    Mathf.FloorToInt(uv.x * _whiteboard.textureSize.x),
                    Mathf.FloorToInt(uv.y * _whiteboard.textureSize.y)
                );

                // If already touching last frame, interpolate between positions
                if (_touchedLastFrame)
                {
                    // Step count proportional to distance / (radius*0.5)
                    float dist = Vector2Int.Distance(_lastPx, px);
                    int steps = Mathf.Max(1, Mathf.CeilToInt(dist / Mathf.Max(1f, _radius * 0.5f)));

                    for (int s = 1; s <= steps; s++)
                    {
                        float t = s / (float)steps;
                        int ix = Mathf.RoundToInt(Mathf.Lerp(_lastPx.x, px.x, t));
                        int iy = Mathf.RoundToInt(Mathf.Lerp(_lastPx.y, px.y, t));
                        Stamp(ix, iy);
                    }

                    // Keep previous rotation to stabilize
                    transform.rotation = _lastRot;
                }
                else
                {
                    // First touch: single stamp
                    Stamp(px.x, px.y);
                }

                _lastPx = px;
                _lastRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        // Reset state if not hitting a whiteboard
        _whiteboard = null;
        _touchedLastFrame = false;
    }

    /// <summary>
    /// Applies a Gaussian-weighted "erase" stamp centered at (cx, cy).
    /// Restores pixels to baseTexture or clearColor, or reduces alpha if transparent mode enabled.
    /// </summary>
    private void Stamp(int cx, int cy)
    {
        if (_whiteboard == null || _whiteboard.texture == null) return;

        int texW = _whiteboard.texture.width;
        int texH = _whiteboard.texture.height;

        // Stamp bounds in texture space
        int left   = cx - _radius;
        int right  = cx + _radius;
        int bottom = cy - _radius;
        int top    = cy + _radius;

        // Clamp to texture bounds
        int x0 = Mathf.Clamp(left,   0, texW - 1);
        int y0 = Mathf.Clamp(bottom, 0, texH - 1);
        int x1 = Mathf.Clamp(right,  0, texW - 1);
        int y1 = Mathf.Clamp(top,    0, texH - 1);

        int rw = x1 - x0 + 1;
        int rh = y1 - y0 + 1;
        if (rw <= 0 || rh <= 0) return;

        // Get pixel block from current drawing layer
        Color[] curBlock = _whiteboard.texture.GetPixels(x0, y0, rw, rh);

        // Get corresponding block from base texture (snapshot) or fallback color
        Color[] baseBlock;
        if (_whiteboard.baseTexture != null)
            baseBlock = _whiteboard.baseTexture.GetPixels(x0, y0, rw, rh);
        else
        {
            baseBlock = new Color[rw * rh];
            for (int i = 0; i < baseBlock.Length; i++) baseBlock[i] = _eraseToColor;
        }

        // Blend each pixel with Gaussian weights
        for (int iy = 0; iy < rh; iy++)
        {
            for (int ix = 0; ix < rw; ix++)
            {
                int tx = x0 + ix;
                int ty = y0 + iy;

                int mx = tx - left;   // local mask x (0..diameter-1)
                int my = ty - bottom; // local mask y
                int mi = my * _diameter + mx; // mask index

                float w = _mask[mi];
                if (w <= 0f) continue;

                int bi = iy * rw + ix; // block index

                Color cur   = curBlock[bi];
                Color baseC = baseBlock[bi];

                if (_eraseToTransparent)
                {
                    // Transparent erase: fade alpha toward 0, and nudge RGB toward base to reduce gray edges
                    Color target = new Color(baseC.r, baseC.g, baseC.b, 0f);
                    curBlock[bi] = Color.Lerp(cur, target, w);
                }
                else
                {
                    // Restore to base texture (true erasure)
                    curBlock[bi] = Color.Lerp(cur, baseC, w);
                }
            }
        }

        // Write back to texture
        _whiteboard.texture.SetPixels(x0, y0, rw, rh, curBlock);
        _whiteboard.texture.Apply(false);
    }
}
