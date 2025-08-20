using UnityEngine;

/// 开场记录：画板相对于“玩家相机”的位姿；
/// 重置：依据“当前相机姿态”恢复到当初的相对位姿（与当前视线一致）。
public class GazeRelativeBoardReset : MonoBehaviour
{
    [Header("玩家相机（不填则用 Camera.main）")]
    public Transform cameraT;

    [Header("画板根节点（要被放置/旋转/缩放的对象）")]
    public Transform board;

    [Header("热键（可由外部调用 ResetNow 替代）")]
    public bool listenKey = true;
    public KeyCode resetKey = KeyCode.R;

    [Header("复位前若画板被隐藏则先激活")]
    public bool reactivateOnReset = true;

    // —— 开场记录的“相机坐标系下”的相对位姿 —— //
    Vector3    initPos_inCam;   // 画板在相机局部坐标中的位置 (x=右, y=上, z=前)
    Quaternion initRot_inCam;   // 相机到画板的相对旋转
    Vector3    initLocalScale;  // 画板自身缩放（保持不变）

    bool inited;
    bool pendingReset;          // 用于把按键重置延后到 LateUpdate（确保玩家/相机已先复位）

    void Awake()
    {
        if (!cameraT && Camera.main) cameraT = Camera.main.transform;
        CaptureInitial();
    }

    /// 记录“当前”为初始：将画板的世界位姿投到相机坐标系下
    [ContextMenu("Capture Initial (Record Now)")]
    public void CaptureInitial()
    {
        if (!cameraT || !board) return;

        initPos_inCam  = cameraT.InverseTransformPoint(board.position);
        initRot_inCam  = Quaternion.Inverse(cameraT.rotation) * board.rotation;
        initLocalScale = board.localScale;

        inited = true;
    }

    void Update()
    {
        if (!listenKey || !inited) return;
        if (Input.GetKeyDown(resetKey)) pendingReset = true;
    }

    void LateUpdate()
    {
        if (!pendingReset) return;
        pendingReset = false;
        ResetNow();  // 放到 LateUpdate，保证相机/玩家先完成自身的 Reset
    }

    /// 外部可直接调用：依据“当前相机姿态”恢复到开场相对位姿
    [ContextMenu("Reset Now")]
    public void ResetNow()
    {
        if (!inited || !cameraT || !board) return;

        if (reactivateOnReset && !board.gameObject.activeSelf)
            board.gameObject.SetActive(true);

        board.position   = cameraT.TransformPoint(initPos_inCam);
        board.rotation   = cameraT.rotation * initRot_inCam;
        board.localScale = initLocalScale;
    }
}
