using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteboardEraser : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _eraserSize = 16;

    // 橡皮颜色：默认白色；如果你的 Whiteboard 有 clearColor 字段，建议把它的值拖进来
    [SerializeField] private Color _eraseColor = Color.white;

    private float _tipHeight;
    private Color[] _colors;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    void Start()
    {
        // 射线长度沿用“笔尖的本地 Y 尺寸”，和你的画笔一致
        _tipHeight = _tip ? _tip.localScale.y : 0.02f;

        // 预生成要填充的一块颜色
        _colors = new Color[_eraserSize * _eraserSize];
        for (int i = 0; i < _colors.Length; i++) _colors[i] = _eraseColor;
    }

    void Update()
    {
        Erase();
    }

    private void Erase()
    {
        // 和画笔同样的射线逻辑：从 _tip 位置沿 transform.up
        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                if (_whiteboard == null) return;

                // 如果你希望自动用 Whiteboard 的背景色，且 Whiteboard 暴露了 public Color clearColor：
                // _eraseColor = _whiteboard.clearColor;  // 可取消注释使用

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                int x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_eraserSize / 2));
                int y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_eraserSize / 2));

                // 建议用 >= 的边界判断，避免越界
                if (y < 0 || y >= _whiteboard.textureSize.y || x < 0 || x >= _whiteboard.textureSize.x)
                {
                    _touchedLastFrame = false;
                    return;
                }

                if (_touchedLastFrame)
                {
                    _whiteboard.texture.SetPixels(x, y, _eraserSize, _eraserSize, _colors);

                    // 插值补点，防止拖动时出现断点
                    for (float f = 0.02f; f <= 1.0f; f += 0.02f)
                    {
                        int lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        int lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _eraserSize, _eraserSize, _colors);
                    }

                    // 锁定旋转，和你的画笔一致，减少画面抖动
                    transform.rotation = _lastTouchRot;

                    _whiteboard.texture.Apply(false);
                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }

    void OnValidate()
    {
        if (_eraserSize < 1) _eraserSize = 1;
        // 同时更新颜色块大小
        if (_colors == null || _colors.Length != _eraserSize * _eraserSize)
        {
            _colors = new Color[_eraserSize * _eraserSize];
            for (int i = 0; i < _colors.Length; i++) _colors[i] = _eraseColor;
        }
    }
}
