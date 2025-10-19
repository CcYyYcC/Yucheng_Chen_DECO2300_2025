using UnityEngine;

/// 按住指定按钮，上下抬手 ⇒ 仅调整 OVRCameraRig.TrackingSpace 的本地Y偏移；
/// 松手后保持在新高度，不再变化。
public class BB_ViewHeightAdjuster : MonoBehaviour
{
    [Header("OVR / Building Blocks")]
    public OVRCameraRig cameraRig;                 // 场景里的 OVRCameraRig（[BuildingBlock] Camera Rig）
    public Transform controller;                    // 用来测量位移的手柄(如 RightHandAnchor)

    [Header("Input")]
    public OVRInput.RawButton holdButton = OVRInput.RawButton.RThumbstick; // 右摇杆按压

    [Header("Tuning")]
    public float sensitivity = 1.0f;               // 手柄Y位移 -> 高度变化比例
    public float minHeight = 0.3f;                 // 允许的最低视角高度(米)
    public float maxHeight = 2.2f;                 // 允许的最高视角高度(米)

    Transform _trackingSpace;
    float _baseOffsetY;                             // 开始时的偏移
    float _startCtrlY;                              // 开始时手柄世界Y
    bool  _adjusting;

    void Awake()
    {
        if (!cameraRig) cameraRig = FindObjectOfType<OVRCameraRig>();
        _trackingSpace = cameraRig ? cameraRig.trackingSpace : null;
    }

    void Update()
    {
        if (!_trackingSpace || !controller) return;

        // 开始调整
        if (OVRInput.GetDown(holdButton))
        {
            _adjusting    = true;
            _baseOffsetY  = _trackingSpace.localPosition.y;  // 记录当前高度偏移
            _startCtrlY   = controller.position.y;            // 记录手柄起始高度（世界Y）
        }

        // 调整中
        if (_adjusting && OVRInput.Get(holdButton))
        {
            float dy = (controller.position.y - _startCtrlY) * sensitivity;
            float targetY = Mathf.Clamp(_baseOffsetY + dy, minHeight, maxHeight);

            var lp = _trackingSpace.localPosition;
            lp.y = targetY;                     // 只改 TrackingSpace 的本地Y
            _trackingSpace.localPosition = lp;  // 视角实时改变
        }

        // 结束调整——保持当前高度，不再改变
        if (_adjusting && OVRInput.GetUp(holdButton))
        {
            _adjusting = false;                 // 什么都不做，偏移值就固定住了
        }
    }
}
