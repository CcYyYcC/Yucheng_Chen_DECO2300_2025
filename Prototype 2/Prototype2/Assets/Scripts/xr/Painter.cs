using UnityEngine;
using UnityEngine.XR;

public class Painter : MonoBehaviour
{
    public Color32 penColor = new Color32(255, 0, 0, 255);
    public Transform rayOrigin;
    public XRNode hand = XRNode.RightHand;  // 默认右手

    private static Board board;
    private RaycastHit hitInfo;
    private bool isDrawing;

    void Start()
    {
        if (!rayOrigin) rayOrigin = transform;
        if (!board) board = FindObjectOfType<Board>();

        Debug.Log($"[Painter] Start -> rayOrigin={(rayOrigin ? rayOrigin.name : "null")}, hand={hand}, board={(board ? board.name : "null")}, penColor={penColor}");
    }

    void Update()
    {
        // 1. 从手柄读取扳机输入
        var device = InputDevices.GetDeviceAtXRNode(hand);
        bool triggerPressed = false;
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
                triggerPressed = pressed;

            if (device.TryGetFeatureValue(CommonUsages.trigger, out float axis))
                Debug.Log($"[Painter] Trigger axis={axis:F2}, pressed={triggerPressed}");
        }
        else
        {
            Debug.LogWarning($"[Painter] XR device not valid: {hand}");
        }

        // 2. 发射短射线（笔尖前方）
        Ray r = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(r, out hitInfo, 0.02f)) // 2cm 以内
        {
            Debug.Log($"[Painter] RayHit -> col={hitInfo.collider.name}, tag={hitInfo.collider.tag}, dist={hitInfo.distance:F3}, uv=({hitInfo.textureCoord.x:F3},{hitInfo.textureCoord.y:F3}), trigger={triggerPressed}");

            if (hitInfo.collider.CompareTag("Board") && triggerPressed)
            {
                board.SetPainterPositon(hitInfo.textureCoord.x, hitInfo.textureCoord.y);
                board.SetPainterColor(penColor);
                board.IsDrawing = true;

                if (!isDrawing)
                {
                    Debug.Log("[Painter] 开始绘制");
                    isDrawing = true;
                }
                return;
            }
        }
        else
        {
            Debug.Log($"[Painter] RayHit=false, trigger={triggerPressed}");
        }

        // 没有满足条件 → 停止绘制
        if (isDrawing)
        {
            Debug.Log("[Painter] 停止绘制");
            board.IsDrawing = false;
            isDrawing = false;
        }
    }
}
