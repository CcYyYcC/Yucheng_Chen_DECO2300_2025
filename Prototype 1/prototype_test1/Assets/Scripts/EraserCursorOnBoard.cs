using UnityEngine;
using UnityEngine.UI;

/// 让橡皮模型贴着“画板(RawImage)”平面，橡皮尖对齐鼠标命中点（仅展示，不影响绘制/点击）
public class EraserCursorOnBoard : MonoBehaviour
{
    [Header("引用（务必赋值）")]
    public Transform eraserModel;   // 橡皮模型（建议无Collider，Layer=Ignore Raycast）
    public Transform eraserTip;     // 橡皮尖子物体（局部位置放在接触点）
    public RawImage board;          // 画板 RawImage（World Space Canvas）
    public Camera uiCam;            // 画板使用的摄像机（不填会自动找）

    [Header("外观与跟随")]
    public float surfaceGap = 0.002f;        // 橡皮尖离画板表面留的微小间隙(米)，避免贴穿/Z-fighting
    public Vector2 planarOffset = Vector2.zero; // 沿画板平面的小偏移 (x=board.right, y=board.up)
    public Vector3 modelEulerOffset = new Vector3(25, 0, 0); // 额外姿态（微微倾斜）
    public float smoothTime = 0.03f;         // 位置平滑(秒)，设0为立即跟随
    public bool hideWhenOffBoard = true;     // 鼠标不在画板范围时是否隐藏橡皮

    [Header("调试")]
    public bool debugLog = false;

    private bool _visible;
    private Vector3 _vel;

    void Awake()
    {
        // 自动补引用
        if (!board)
        {
            var p = FindObjectOfType<Paint>();
            if (p) board = p.rawImage;
        }
        if (!uiCam && board && board.canvas && board.canvas.renderMode == RenderMode.WorldSpace)
            uiCam = board.canvas.worldCamera ? board.canvas.worldCamera : Camera.main;

        Show(false); // 初始隐藏
    }

    void Update()
    {
        if (!_visible) return;

        if (!eraserModel || !eraserTip || !board)
        {
            if (debugLog) Debug.LogWarning("[EraserCursor] 引用未设置：eraserModel/eraserTip/board。");
            return;
        }

        var rt = board.rectTransform;
        var cam = uiCam ? uiCam : Camera.main;
        if (!cam)
        {
            if (debugLog) Debug.LogWarning("[EraserCursor] 未找到摄像机，请给 uiCam 赋值。");
            return;
        }

        // ① 鼠标射线与画板“平面”求交
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(rt.forward, rt.position);
        if (!plane.Raycast(ray, out float enter))
        {
            if (hideWhenOffBoard) eraserModel.gameObject.SetActive(false);
            return;
        }
        Vector3 hitWorld = ray.origin + ray.direction * enter;

        // ② 转到画板局部坐标，判断是否在 Rect 内
        Vector3 hitLocal = rt.InverseTransformPoint(hitWorld);
        Vector2 half = rt.rect.size * 0.5f;
        bool inside =
            hitLocal.x >= -half.x && hitLocal.x <= half.x &&
            hitLocal.y >= -half.y && hitLocal.y <= half.y;

        if (!inside)
        {
            if (hideWhenOffBoard) { eraserModel.gameObject.SetActive(false); }
            return;
        }

        // ③ 目标旋转：朝向画板法线 + 自定义角
        Quaternion targetRot = Quaternion.LookRotation(rt.forward, rt.up) * Quaternion.Euler(modelEulerOffset);

        // ④ 目标“橡皮尖世界坐标”
        Vector3 tipWorldTarget =
            hitWorld +
            rt.right * planarOffset.x +
            rt.up    * planarOffset.y +
            rt.forward * surfaceGap;

        // ⑤ 用“橡皮尖→枢轴”的世界向量回推模型位置
        Quaternion oldRot = eraserModel.rotation;
        eraserModel.rotation = targetRot;

        Vector3 pivotToTipWorld = eraserModel.TransformVector(eraserTip.localPosition);
        Vector3 modelPosTarget = tipWorldTarget - pivotToTipWorld;

        // ⑥ 设置位置与旋转
        if (smoothTime > 0f)
            eraserModel.position = Vector3.SmoothDamp(eraserModel.position, modelPosTarget, ref _vel, smoothTime);
        else
            eraserModel.position = modelPosTarget;

        eraserModel.rotation = targetRot;
        if (!eraserModel.gameObject.activeSelf) eraserModel.gameObject.SetActive(true);
    }

    /// 供按钮调用：显示/隐藏
    public void Show(bool on)
    {
        _visible = on;
        if (eraserModel) eraserModel.gameObject.SetActive(on);
    }

    public void Toggle() => Show(!_visible);
}
