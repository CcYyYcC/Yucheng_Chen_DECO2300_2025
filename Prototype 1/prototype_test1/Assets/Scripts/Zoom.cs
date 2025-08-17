using UnityEngine;

public class Zoom : MonoBehaviour
{
    [Header("要缩放的根(默认=本物体，如你的 World Space Canvas)")]
    public Transform targetRoot;

    [Header("用于计算射线的相机(默认=Canvas.worldCamera 或 MainCamera)")]
    public Camera cam;

    [Header("缩放参数")]
    [Tooltip("每个“虚拟步进”的缩放比例(0.1=10%)")]
    public float zoomStep = 0.15f;         // 单步幅度
    [Tooltip("长按W/S时每秒步进次数")]
    public float stepsPerSecond = 8f;      // 速度
    [Tooltip("相对初始大小的最小/最大倍数")]
    public float minScaleMultiplier = 0.1f; // 最小=初始的 0.1 倍
    public float maxScaleMultiplier = 10f;  // 最大=初始的 10 倍
    public bool zoomToMouse = true;        // 围绕鼠标缩放

    [Header("键位")]
    public KeyCode zoomInKey = KeyCode.W;
    public KeyCode zoomOutKey = KeyCode.S;

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

        // 取初始等比缩放（假设 XYZ 一致，World Space Canvas 通常是 0.01）
        _baseScale = targetRoot.localScale.x;
        if (_baseScale <= 0f) _baseScale = 0.01f;
    }

    void Update()
    {
        float dir = 0f;
        if (Input.GetKey(zoomInKey))  dir += 1f;  // W 放大
        if (Input.GetKey(zoomOutKey)) dir -= 1f;  // S 缩小
        if (Mathf.Approximately(dir, 0f)) return;

        // 当前大小（相对初始的倍数）
        float currentMul = targetRoot.localScale.x / _baseScale;

        // 幂函数实现与帧率无关的平滑缩放
        float factor = Mathf.Pow(1f + Mathf.Max(0.0001f, zoomStep), dir * stepsPerSecond * Time.deltaTime);
        float newMul = Mathf.Clamp(currentMul * factor, minScaleMultiplier, maxScaleMultiplier);
        if (Mathf.Approximately(newMul, currentMul)) return;

        float oldScale = targetRoot.localScale.x;
        float newScale = _baseScale * newMul;

        // 围绕鼠标位置缩放：保持鼠标下点不漂移
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
