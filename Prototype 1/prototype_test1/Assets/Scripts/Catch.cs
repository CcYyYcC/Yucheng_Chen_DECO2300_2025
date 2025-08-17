using UnityEngine;

public class Catch : MonoBehaviour
{
    [Header("要平移的根对象(默认=本物体)")]
    public Transform targetRoot;

    [Header("用于计算鼠标射线的相机(默认=Canvas.worldCamera 或 MainCamera)")]
    public Camera cam;

    [Header("按哪个键拖动")]
    public KeyCode panButton = KeyCode.Mouse2; // 中键

    private bool _panning;
    private Vector3 _grabOffset; // 目标位置 - 鼠标平面命中点

    void Reset()
    {
        targetRoot = targetRoot ? targetRoot : transform;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera)
            cam = canvas.worldCamera;
        else
            cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(panButton))
        {
            Vector3 hit = GetMouseWorldOnPlane();
            _grabOffset = targetRoot.position - hit;
            _panning = true;
        }
        else if (Input.GetKeyUp(panButton))
        {
            _panning = false;
        }

        if (_panning)
        {
            Vector3 hit = GetMouseWorldOnPlane();
            targetRoot.position = hit + _grabOffset;
        }
    }

    Vector3 GetMouseWorldOnPlane()
    {
        if (cam == null) cam = Camera.main;
        Plane plane = new Plane(-cam.transform.forward, targetRoot.position);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        return targetRoot.position;
    }
}
