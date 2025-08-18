using UnityEngine;

public class Zoom : MonoBehaviour
{
    [Header("要缩放的根(默认=本物体，如你的 World Space Canvas)")]
    public Transform targetRoot;

    [Header("用于计算射线的相机(默认=Canvas.worldCamera 或 MainCamera)")]
    public Camera cam;

    [Header("缩放参数")]
    [Tooltip("每个滚轮单位对应的缩放比例(0.1≈10%)")]
    public float zoomStep = 0.15f;          // 单步幅度（滚轮一格）
    [Tooltip("相对初始大小的最小/最大倍数")]
    public float minScaleMultiplier = 0.1f;  // 最小=初始的 0.1 倍
    public float maxScaleMultiplier = 10f;   // 最大=初始的 10 倍
    public bool zoomToMouse = true;          // 围绕鼠标缩放
    [Tooltip("如方向相反可勾选：上滚缩小，下滚放大")]
    public bool invertScroll = false;

    // 记录初始等比缩放
    private float _baseScale = 1f;

    void Awake()
    {
        if (!targetRoot) targetRoot = transform;

        if (!cam)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera)
                cam = canvas.worldCamera;
            else
                cam = Camera.main;
        }

        // 取初始等比缩放（World Space Canvas 常见为 0.01）
        _baseScale = targetRoot.localScale.x;
        if (_baseScale <= 0f) _baseScale = 0.01f;
    }

    void Update()
    {
        // 读取滚轮（优先新API，兼容旧轴）
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Approximately(scroll, 0f))
            scroll = Input.GetAxis("Mouse ScrollWheel") * 120f; // 某些设备返回旧轴

        if (invertScroll) scroll = -scroll;
        if (Mathf.Abs(scroll) < 0.001f) return;  // 无滚动就不处理

        // 当前大小（相对初始的倍数）
        float currentMul = targetRoot.localScale.x / _baseScale;

        // 幂函数实现平滑缩放：scroll>0 放大，scroll<0 缩小
        float factor = Mathf.Pow(1f + Mathf.Max(0.0001f, zoomStep), scroll);
        float newMul = Mathf.Clamp(currentMul * factor, minScaleMultiplier, maxScaleMultiplier);
        if (Mathf.Approximately(newMul, currentMul)) return;

        float newScale = _baseScale * newMul;

        // 围绕鼠标位置缩放：保持鼠标下的点不漂移
        Vector3 pivotWorld = GetMouseWorldOnPlane();
        Vector3 localBefore = targetRoot.InverseTransformPoint(pivotWorld);

        targetRoot.localScale = Vector3.one * newScale;

        if (zoomToMouse)
        {
            Vector3 worldAfter = targetRoot.TransformPoint(localBefore);
            targetRoot.position += (pivotWorld - worldAfter);
        }
    }

    Vector3 GetMouseWorldOnPlane()
    {
        if (!cam) cam = Camera.main;
        Plane plane = new Plane(-cam.transform.forward, targetRoot.position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : targetRoot.position;
    }
}
