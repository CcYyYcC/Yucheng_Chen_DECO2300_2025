using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteboardSoftEraser : MonoBehaviour
{
    [Header("笔尖(用于射线起点/长度)")]
    [SerializeField] private Transform _tip;

    [Header("半径(像素)与柔和度")]
    [Tooltip("笔头半径，最终直径= 2*radius+1")]
    [SerializeField] private int _radius = 20;

    [Tooltip("高斯软边系数(0.1硬一点 ~ 1.0很柔)")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _softness = 0.7f;

    [Header("擦除目标")]
    [Tooltip("擦到此颜色（如果白板是纯色背景，填背景色）")]
    [SerializeField] private Color _eraseToColor = Color.white;

    [Tooltip("勾选则逐步降低 Alpha（需要白板材质走透明通道 + 纹理 RGBA32）")]
    [SerializeField] private bool _eraseToTransparent = false;

    [Header("射线方向(默认用整体物体的本地Y轴)")]
    [SerializeField] private bool _useTipAxis = false;
    [SerializeField] private Vector3 _tipAxis = Vector3.up; // 可改为 Vector3.forward 等

    private float _tipHeight = 0.02f;
    private Whiteboard _whiteboard;
    private RaycastHit _hit;

    // 触笔状态
    private bool _touchedLastFrame = false;
    private Vector2Int _lastPx;
    private Quaternion _lastRot;

    // 预生成权重掩码（高斯）
    private float[] _mask; // 长度 = diameter * diameter
    private int _diameter;
    private float _sigma;

    void Start()
    {
        if (_tip != null)
            _tipHeight = Mathf.Max(0.005f, _tip.localScale.y);

        BuildMask();
    }

    void OnValidate()
    {
        if (_radius < 1) _radius = 1;
        if (_softness < 0.1f) _softness = 0.1f;
        BuildMask();
    }

    void Update()
    {
        EraseUpdate();
    }

    private void BuildMask()
    {
        _diameter = _radius * 2 + 1;
        _mask = new float[_diameter * _diameter];

        // 高斯权重： w = exp(-d^2 / (2*sigma^2))
        // sigma 越大越柔；用 半径 * softness 来直观控制
        _sigma = Mathf.Max(0.001f, _radius * _softness);

        int i = 0;
        for (int y = -_radius; y <= _radius; y++)
        {
            for (int x = -_radius; x <= _radius; x++, i++)
            {
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist > _radius)
                {
                    _mask[i] = 0f;
                }
                else
                {
                    float w = Mathf.Exp(-(dist * dist) / (2f * _sigma * _sigma));
                    _mask[i] = Mathf.Clamp01(w); // 0..1
                }
            }
        }
    }

    private void EraseUpdate()
    {
        // 1) 发射射线找白板
        Vector3 origin = _tip.position;
        Vector3 dir = _useTipAxis ? (_tip.TransformDirection(_tipAxis)) : transform.up;

        if (Physics.Raycast(origin, dir, out _hit, _tipHeight))
        {
            if (_hit.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                    _whiteboard = _hit.transform.GetComponent<Whiteboard>();
                if (_whiteboard == null || _whiteboard.texture == null) return;

                Vector2 uv = _hit.textureCoord;
                Vector2Int px = new Vector2Int(
                    Mathf.FloorToInt(uv.x * _whiteboard.textureSize.x),
                    Mathf.FloorToInt(uv.y * _whiteboard.textureSize.y)
                );

                // 2) 画/擦 stamp + 路径插值
                if (_touchedLastFrame)
                {
                    // 根据像素距离决定插值步数：步长 ~ 半径的一半
                    float dist = Vector2Int.Distance(_lastPx, px);
                    int steps = Mathf.Max(1, Mathf.CeilToInt(dist / Mathf.Max(1f, _radius * 0.5f)));

                    for (int s = 1; s <= steps; s++)
                    {
                        float t = s / (float)steps;
                        int ix = Mathf.RoundToInt(Mathf.Lerp(_lastPx.x, px.x, t));
                        int iy = Mathf.RoundToInt(Mathf.Lerp(_lastPx.y, px.y, t));
                        Stamp(ix, iy);
                    }

                    // 稳定手感：保持上一帧旋转
                    transform.rotation = _lastRot;
                }
                else
                {
                    Stamp(px.x, px.y);
                }

                _lastPx = px;
                _lastRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }

    /// <summary>
    /// 在中心像素 (cx,cy) 处盖一个高斯软边的“擦除”印章
    /// </summary>
    private void Stamp(int cx, int cy)
{
    if (_whiteboard == null || _whiteboard.texture == null) return;

    int texW = _whiteboard.texture.width;
    int texH = _whiteboard.texture.height;

    int left   = cx - _radius;
    int right  = cx + _radius;
    int bottom = cy - _radius;
    int top    = cy + _radius;

    int x0 = Mathf.Clamp(left,   0, texW - 1);
    int y0 = Mathf.Clamp(bottom, 0, texH - 1);
    int x1 = Mathf.Clamp(right,  0, texW - 1);
    int y1 = Mathf.Clamp(top,    0, texH - 1);

    int rw = x1 - x0 + 1;
    int rh = y1 - y0 + 1;
    if (rw <= 0 || rh <= 0) return;

    // 读“当前绘制层”和“底图快照”的相同区域
    Color[] curBlock  = _whiteboard.texture.GetPixels(x0, y0, rw, rh);

    // 如果没有 baseTexture，就用 clearColor 作为底色
    Color[] baseBlock;
    if (_whiteboard.baseTexture != null)
        baseBlock = _whiteboard.baseTexture.GetPixels(x0, y0, rw, rh);
    else
    {
        baseBlock = new Color[rw * rh];
        for (int i = 0; i < baseBlock.Length; i++) baseBlock[i] = _eraseToColor; // 退路
    }

    for (int iy = 0; iy < rh; iy++)
    {
        for (int ix = 0; ix < rw; ix++)
        {
            int tx = x0 + ix;
            int ty = y0 + iy;

            int mx = tx - left;   // 0..diameter-1
            int my = ty - bottom; // 0..diameter-1
            int mi = my * _diameter + mx;

            float w = _mask[mi];  // 软边权重（中心=1，边缘→0）
            if (w <= 0f) continue;

            int bi = iy * rw + ix;

            Color cur  = curBlock[bi];
            Color baseC = baseBlock[bi];

            if (_eraseToTransparent)
            {
                // 透明擦除：把 alpha 朝 0 拉，同时 RGB 向底图靠一点，避免边缘发灰
                Color target = new Color(baseC.r, baseC.g, baseC.b, 0f);
                curBlock[bi] = Color.Lerp(cur, target, w);
            }
            else
            {
                // 真正意义的“擦除”：向底图颜色还原（不是涂白）
                curBlock[bi] = Color.Lerp(cur, baseC, w);
            }
        }
    }

    _whiteboard.texture.SetPixels(x0, y0, rw, rh, curBlock);
    _whiteboard.texture.Apply(false);
}

}
