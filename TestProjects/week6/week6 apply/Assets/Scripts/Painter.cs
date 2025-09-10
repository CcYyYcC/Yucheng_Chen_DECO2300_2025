using UnityEngine;
using UnityEngine.XR;

public class Painter : MonoBehaviour
{
    [Header("画笔设置")]
    public Transform rayOrigin;                 // 笔尖（没指定就用自身）
    public float contactDistance = 0.02f;       // 接触判定距离
    public LayerMask paintableLayers;           // 画布层
    public XRNode hand = XRNode.RightHand;      // 手柄

    [Header("绘制参数")]
    public Color32 penColor = new Color32(255, 0, 0, 255);

    private static Board board;                 // 画布引用
    private RaycastHit hitInfo;

    void Start()
    {
        if (!rayOrigin) rayOrigin = transform;
        if (!board) board = FindObjectOfType<Board>();
        if (!board)
        {
            Debug.LogWarning("Painter: 未找到 Board 组件，无法绘制。");
        }
    }

    void Update()
    {
        // 1. 读取扳机输入
        bool triggerPressed = false;
        var device = InputDevices.GetDeviceAtXRNode(hand);
        if (device.isValid && device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed))
        {
            triggerPressed = pressed;
        }

        // 2. 射线检测是否接触画布
        bool hit = Physics.Raycast(
            new Ray(rayOrigin.position, rayOrigin.forward),
            out hitInfo,
            contactDistance,
            paintableLayers,
            QueryTriggerInteraction.Collide
        );

        // 3. 同时满足：接触 && 扳机
        if (board && hit && triggerPressed)
        {
            // 设置颜色（可选：如果需要动态换色）
            board.SetPainterColor(penColor);

            // 更新笔尖UV坐标
            Vector2 uv = hitInfo.textureCoord;
            board.SetPainterPositon(uv.x, uv.y);

            // 开始绘制
            board.IsDrawing = true;
        }
        else
        {
            // 停止绘制
            if (board) board.IsDrawing = false;
        }

        // 4. 调试射线
        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * contactDistance, (hit && triggerPressed) ? Color.green : Color.gray);
    }
}
