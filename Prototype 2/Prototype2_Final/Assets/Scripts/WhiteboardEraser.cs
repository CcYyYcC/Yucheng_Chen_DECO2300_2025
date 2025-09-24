using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Whiteboard eraser tool:
/// Casts a short ray from the eraser tip, detects a "Whiteboard"-tagged surface,
/// converts UV hit coordinates to texture space, and paints a square of _eraseColor
/// to "erase" previously drawn pixels. It also interpolates between frames to avoid gaps
/// when moving quickly.
/// 
/// Requirements:
/// - The whiteboard surface must have a Collider and be tagged "Whiteboard".
/// - The Whiteboard component must expose `texture` (Texture2D) and `textureSize` (Vector2Int).
/// - The eraser tip transform (`_tip`) should point its local up direction toward the board.
/// </summary>
public class WhiteboardEraser : MonoBehaviour
{
    [SerializeField] private Transform _tip;          // Transform representing the eraser tip (ray origin)
    [SerializeField] private int _eraserSize = 16;    // Square size (in pixels) of the eraser stamp

    // Eraser color: default white. If your Whiteboard exposes a `clearColor`,
    // consider assigning it here for a perfect "reset" color match.
    [SerializeField] private Color _eraseColor = Color.white;

    private float _tipHeight;                         // Ray length derived from the tip's local Y scale
    private Color[] _colors;                          // Prebuilt color buffer used by SetPixels

    private RaycastHit _touch;                        // Stores raycast hit information
    private Whiteboard _whiteboard;                   // Cached reference to the current whiteboard hit
    private Vector2 _touchPos, _lastTouchPos;         // Current/previous draw positions in texture space (pixels)
    private bool _touchedLastFrame;                   // Whether we were erasing in the previous frame
    private Quaternion _lastTouchRot;                 // Cached rotation to reduce jitter while erasing

    void Start()
    {
        // Use the tip's local Y scale as the ray length (keeps behavior consistent with the pen logic)
        _tipHeight = _tip ? _tip.localScale.y : 0.02f;

        // Allocate and prefill the stamp colors once for performance
        _colors = new Color[_eraserSize * _eraserSize];
        for (int i = 0; i < _colors.Length; i++) _colors[i] = _eraseColor;
    }

    void Update()
    {
        // Perform erasing logic each frame
        Erase();
    }

    /// <summary>
    /// Raycasts from the tip along transform.up, checks for a Whiteboard, converts hit UV to pixel coords,
    /// paints a square of _eraseColor, and interpolates between last and current positions to fill gaps.
    /// </summary>
    private void Erase()
    {
        // Same ray logic as the pen: cast from _tip along the object's up direction
        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                // Lazy cache the Whiteboard component on first contact
                if (_whiteboard == null)
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                if (_whiteboard == null) return;

                // Optional: match eraser color to the board's background if available
                // _eraseColor = _whiteboard.clearColor;  // Uncomment if Whiteboard exposes `clearColor`

                // Convert hit UV (0..1) to texture-space coordinates
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                int x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_eraserSize / 2));
                int y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_eraserSize / 2));

                // Robust bounds check to avoid SetPixels out-of-range errors
                if (y < 0 || y >= _whiteboard.textureSize.y || x < 0 || x >= _whiteboard.textureSize.x)
                {
                    _touchedLastFrame = false;
                    return;
                }

                if (_touchedLastFrame)
                {
                    // Paint the current square stamp
                    _whiteboard.texture.SetPixels(x, y, _eraserSize, _eraserSize, _colors);

                    // Interpolate along the path from the last stamp to the current position
                    // to prevent gaps when the eraser moves quickly between frames
                    for (float f = 0.02f; f <= 1.0f; f += 0.02f)
                    {
                        int lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        int lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _eraserSize, _eraserSize, _colors);
                    }

                    // Lock rotation to the previous frame to reduce visual jitter while in contact
                    transform.rotation = _lastTouchRot;

                    // Apply all pixel changes to the GPU texture (no mipmap rebuild)
                    _whiteboard.texture.Apply(false);
                }

                // Update trailing state for the next frame
                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        // If not hitting a valid whiteboard, reset state
        _whiteboard = null;
        _touchedLastFrame = false;
    }

    void OnValidate()
    {
        // Keep eraser size valid in the inspector
        if (_eraserSize < 1) _eraserSize = 1;

        // Reallocate the color buffer if size changed (keeps SetPixels dimensions consistent)
        if (_colors == null || _colors.Length != _eraserSize * _eraserSize)
        {
            _colors = new Color[_eraserSize * _eraserSize];
            for (int i = 0; i < _colors.Length; i++) _colors[i] = _eraseColor;
        }
    }
}
