using UnityEngine;

/// 最简视线：从相机前方发出一条线，与“画板平面”求交；命中→画到交点，未命中→画固定长度
public class GazeLine : MonoBehaviour
{
    [Header("画板（用于定义平面：position + forward）")]
    public Transform board;               // 指到你的画板/画布根节点（RectTransform 也可）

    [Header("参数")]
    public float maxDistance = 10f;       // 没命中时的显示长度
    public float startOffset = 0.05f;     // 起点从相机前方略偏移，避免贴脸
    public float width = 0.01f;           // 线宽（按你的世界单位调整）
    public Color color = Color.cyan;      // 线颜色

    LineRenderer lr;
    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();                 // 建议把脚本挂在相机上
        if (!cam) cam = Camera.main;

        // 创建/获取 LineRenderer（最简配置，Unlit 纯色）
        lr = GetComponent<LineRenderer>();
        if (!lr) lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.alignment = LineAlignment.View;
        lr.widthMultiplier = width;

        var shader = Shader.Find("Unlit/Color");
        if (lr.material == null) lr.material = new Material(shader ? shader : Shader.Find("Sprites/Default"));
        lr.material.color = color;
    }

    void Update()
    {
        if (!board || !cam) { lr.enabled = false; return; }

        // 起点与方向
        Vector3 origin = cam.transform.position + cam.transform.forward * startOffset;
        Vector3 dir = cam.transform.forward;

        // 用画板的位置与朝向定义一个平面（board.forward 为法线，指向画板正面）
        Plane plane = new Plane(board.forward, board.position);

        Vector3 end = origin + dir * maxDistance;   // 默认：没命中，画固定长度
        if (plane.Raycast(new Ray(origin, dir), out float enter) && enter > 0f && enter <= maxDistance)
            end = origin + dir * enter;

        // 画线
        lr.enabled = true;
        lr.startWidth = lr.endWidth = width;        // 防运行时被改
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);
    }

    // 运行时可改样式（可选）
    public void SetStyle(float newWidth, Color newColor)
    {
        width = newWidth;
        color = newColor;
        if (lr)
        {
            lr.widthMultiplier = width;
            lr.material.color = color;
        }
    }
}
