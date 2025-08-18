using UnityEngine;

public class Catch : MonoBehaviour
{
    [Header("要平移的根对象(默认=本物体)")]
    public Transform targetRoot;

    [Header("用于计算射线的相机(默认=Canvas.worldCamera 或 MainCamera)")]
    public Camera cam;

    [Header("按哪个键拖动")]
    public KeyCode panButton = KeyCode.Mouse2; // 中键

    private bool _panning;
    private Vector3 _grabOffset; // 目标位置 - 鼠标命中点
    private Plane _dragPlane;    // ✅ 拖拽时锁定的平面（不随相机转）

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
    }

    void Update()
    {
        if (Input.GetKeyDown(panButton))
        {
            // ✅ 在按下那一刻锁定平面：用画板自身的朝向 & 当前位置
            _dragPlane = new Plane(targetRoot.forward, targetRoot.position);

            // 计算按下时的命中点，并记录抓取偏移
            if (RaycastOnDragPlane(out var hit))
            {
                _grabOffset = targetRoot.position - hit;
                _panning = true;
            }
        }
        else if (Input.GetKeyUp(panButton))
        {
            _panning = false;
        }

        if (_panning)
        {
            if (RaycastOnDragPlane(out var hit))
            {
                // 沿锁定平面平移，不会再“远近变化”
                targetRoot.position = hit + _grabOffset;
            }
        }
    }

    bool RaycastOnDragPlane(out Vector3 hitPoint)
    {
        if (!cam) cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (_dragPlane.Raycast(ray, out float enter))
        {
            hitPoint = ray.GetPoint(enter);
            return true;
        }
        hitPoint = targetRoot.position;
        return false;
    }
}
