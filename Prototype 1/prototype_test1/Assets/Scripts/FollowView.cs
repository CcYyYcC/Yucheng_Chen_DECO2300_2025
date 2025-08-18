using UnityEngine;

public class FollowView : MonoBehaviour
{
    [Header("视线来源（通常是玩家相机或角色本体）")]
    public Transform viewTarget;

    [Header("相对视线的位置")]
    public float distance = 2.0f;
    public float heightOffset = 0.0f;
    public bool followPosition = true;
    public bool yawOnly = true;

    [Header("平滑")]
    public float positionLerp = 15f;
    public float rotationLerp = 15f;

    void LateUpdate()
    {
        if (!viewTarget) return;

        // 取视线旋转（可选只取水平）
        Quaternion viewRot = viewTarget.rotation;
        if (yawOnly) viewRot = Quaternion.Euler(0f, viewTarget.eulerAngles.y, 0f);

        // 前后反：在 Y 轴上加 180°
        viewRot *= Quaternion.Euler(0f, 180f, 0f);

        // 修正后的前向
        Vector3 fwd = viewRot * Vector3.forward;

        // 位置/朝向
        Vector3 basePos  = followPosition ? viewTarget.position : transform.position;
        Vector3 targetPos = basePos + fwd * distance + Vector3.up * heightOffset;
        Quaternion targetRot = Quaternion.LookRotation(fwd, Vector3.up);

        // 指数平滑
        float pt = 1f - Mathf.Exp(-positionLerp * Time.deltaTime);
        float rt = 1f - Mathf.Exp(-rotationLerp * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, targetPos, pt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rt);
    }
}
