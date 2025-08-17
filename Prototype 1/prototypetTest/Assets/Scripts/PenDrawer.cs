using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PenDrawer : MonoBehaviour
{
    [Header("Refs")]
    public Transform tip;                      // 笔尖Transform
    public LayerMask canvasLayer;              // 画布层（只勾 Canvas）
    public Renderer colorIndicatorRenderer;    // 可选：显示颜色的小球

    [Header("Move (Shift + Mouse)")]
    public bool requireShiftToMove = true;     // Shift 模拟抓取
    public float movePlaneHeight = 1.0f;       // 未接触画布时的水平移动高度
    public float moveLerp = 15f;               // MovePosition的趋近速度

    [Header("Brush")]
    public Color brushColor = Color.black;
    public int brushRadius = 10;

    [Header("Contact Settings")]
    public float contactRayBack = 0.005f;      // 从接触点往外退一点再回射（避免在体内）
    public float maxContactDistance = 0.01f;   // 认定为“贴住”的最大允许间隙
    public float slideDamping = 0.95f;         // 接触时沿法线的速度衰减（防抖）

    [Header("Debug")]
    public bool debugDraw = false;

    Rigidbody _rb;
    Camera _cam;
    bool _isGrabbing;
    bool _isDrawing;                           // 左键正在画
    Vector2? _lastUV = null;
    Vector3 _targetPos;
    Quaternion _targetRot;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = false;
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        _cam = Camera.main;
        _targetPos = transform.position;
        _targetRot = transform.rotation;
    }

    void Update()
    {
        // —— 抓取（Shift + 鼠标）：决定物理目标位姿
        bool wantMove = requireShiftToMove ? (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) : true;
        if (wantMove)
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            Vector3 dest;
            // 如果当前贴着画布，用“当前接触平面”来解算目标点；否则用水平面
            if (TryGetCurrentContact(out var contact))
            {
                Plane plane = new Plane(contact.normal, contact.point);
                if (plane.Raycast(ray, out float enter))
                    dest = ray.GetPoint(enter);
                else
                    dest = transform.position;
            }
            else
            {
                Plane plane = new Plane(Vector3.up, new Vector3(0f, movePlaneHeight, 0f));
                if (plane.Raycast(ray, out float enter))
                    dest = ray.GetPoint(enter);
                else
                    dest = transform.position;
            }

            _targetPos = dest;
            // 朝向保持当前，也可以让笔朝向移动方向（可选）
        }

        // 左键状态
        if (Input.GetMouseButtonDown(0)) { _isDrawing = true;  _lastUV = null; }
        if (Input.GetMouseButtonUp(0))   { _isDrawing = false; _lastUV = null; }
    }

    void FixedUpdate()
    {
        // —— 用 MovePosition/MoveRotation 推动笔（尊重物理碰撞，不会穿透）
        Vector3 newPos = Vector3.Lerp(_rb.position, _targetPos, 1f - Mathf.Exp(-moveLerp * Time.fixedDeltaTime));
        _rb.MovePosition(newPos);
        _rb.MoveRotation(_targetRot);

        // 接触时把速度沿接触法线方向衰减，贴住更稳
        if (TryGetCurrentContact(out var c))
        {
            Vector3 v = _rb.velocity;
            Vector3 vn = Vector3.Project(v, c.normal);
            _rb.velocity = v - vn * (1f - slideDamping);
        }

        // —— 画：左键+接触画布，取接触点UV并落笔
        if (_isDrawing && TryGetHitOnCanvas(out var hit))
        {
            var painter = hit.collider.GetComponent<CanvasPainter>();
            if (painter != null)
            {
                Vector2 uv = hit.textureCoord;
                // 插值补点避免断裂
                const int STEPS = 6;
                if (_lastUV.HasValue)
                {
                    for (int i = 1; i <= STEPS; i++)
                    {
                        Vector2 u = Vector2.Lerp(_lastUV.Value, uv, i / (float)STEPS);
                        painter.PaintAtUV(u, brushColor, brushRadius);
                    }
                }
                else
                {
                    painter.PaintAtUV(uv, brushColor, brushRadius);
                }
                _lastUV = uv;

                if (debugDraw) Debug.DrawRay(hit.point, hit.normal * 0.03f, Color.green, Time.fixedDeltaTime);
            }
        }
    }

    // 读取“当前最可信的接触”：从 Tip 的球碰撞体获取接触点与法线
    bool TryGetCurrentContact(out (Vector3 point, Vector3 normal) contact)
    {
        contact = default;

        // 用一个很小的 OverlapSphere 认为“接触/贴近”
        Collider[] cols = Physics.OverlapSphere(tip.position, maxContactDistance, canvasLayer, QueryTriggerInteraction.Ignore);
        if (cols.Length == 0) return false;

        // 取最近的那个
        Collider col = cols[0];
        float best = float.MaxValue;
        for (int i = 0; i < cols.Length; i++)
        {
            float d = Vector3.SqrMagnitude(ClosestPointOn(col, tip.position) - tip.position);
            if (d < best) { best = d; col = cols[i]; }
        }

        // 通过一条“从接触点外微退再回射”的射线取得精确命中 & 法线（MeshCollider 才能给 UV）
        Vector3 cp = ClosestPointOn(col, tip.position);
        Vector3 dir = (tip.position - cp).sqrMagnitude > 1e-8f ? (tip.position - cp).normalized : transform.forward;
        Vector3 origin = tip.position + dir * contactRayBack;
        if (col.Raycast(new Ray(origin, -dir), out RaycastHit hit, contactRayBack + 0.05f))
        {
            contact = (hit.point, hit.normal);
            return true;
        }
        return false;
    }

    // 将“接触 + 左键”转换为真正的命中信息（含 UV）
    bool TryGetHitOnCanvas(out RaycastHit hit)
    {
        hit = default;
        if (!TryGetCurrentContact(out var c)) return false;

        // 再做一次极短射线，保证一定拿到 RaycastHit（含 textureCoord）
        Vector3 dir = c.normal; // 从表面里往外退，再沿 -normal 回射
        Vector3 origin = c.point + dir * contactRayBack;
        return Physics.Raycast(origin, -dir, out hit, contactRayBack + 0.05f, canvasLayer, QueryTriggerInteraction.Ignore);
    }

    // MeshCollider 的 ClosestPoint 只有 convex 才可用；为兼容 Box/Plane/Quad：
    Vector3 ClosestPointOn(Collider col, Vector3 pos)
    {
        // 对 Box/Sphere/Capsule 有效；非凸 MeshCollider 退化为 AABB 最近点（近似就够）
        return col.ClosestPoint(pos);
    }

    // 颜色接口（给色环/按钮）
    public void SetBrushColor(Color c)
    {
        brushColor = c;
        if (colorIndicatorRenderer != null)
        {
            var mat = colorIndicatorRenderer.material;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", brushColor);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", brushColor);
        }
    }
}
