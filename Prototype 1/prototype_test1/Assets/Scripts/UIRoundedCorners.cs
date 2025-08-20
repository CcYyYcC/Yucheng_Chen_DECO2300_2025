using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

/// 可挂载在 Image / RawImage 上的圆角效果（World Space 友好）
[ExecuteAlways, DisallowMultipleComponent]
[AddComponentMenu("UI/Effects/URoundedCorners (World-Safe)")]
public class UIRoundedCorners : BaseMeshEffect
{
    public enum RadiusUnit { LocalUnits, WorldUnits }

    [Header("Corner Radius")]
    public RadiusUnit radiusUnit = RadiusUnit.WorldUnits;
    [Min(0f)] public float cornerRadius = 0.03f;   // World: 0.02~0.06m 常用；Local: 像素/本地单位
    [Range(2, 32)] public int cornerSegments = 8;
    public bool leftTop = true, rightTop = true, leftBottom = true, rightBottom = true;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive()) return;

        var rt = graphic.rectTransform;
        Rect rect = rt.rect;
        float w = rect.width, h = rect.height;
        if (w <= 0f || h <= 0f) return;

        // 获取世界缩放，非等比缩放时做圆角补偿（确保“世界里”看起来是圆角）
        var ls = rt.lossyScale;
        float sx = Mathf.Abs(ls.x);
        float sy = Mathf.Abs(ls.y);
        sx = Mathf.Max(sx, 1e-6f);
        sy = Mathf.Max(sy, 1e-6f);

        // 计算本地空间的 x/y 半径：World 模式下 rLocalX = rWorld/sx, rLocalY = rWorld/sy
        float rLocalX, rLocalY;
        if (radiusUnit == RadiusUnit.WorldUnits)
        {
            rLocalX = cornerRadius / sx;
            rLocalY = cornerRadius / sy;
        }
        else
        {
            rLocalX = rLocalY = cornerRadius;
        }

        // 不能超过各方向的一半
        rLocalX = Mathf.Clamp(rLocalX, 0f, w * 0.5f);
        rLocalY = Mathf.Clamp(rLocalY, 0f, h * 0.5f);
        if (rLocalX <= 1e-4f || rLocalY <= 1e-4f) return;

        // 颜色 & UV（Image 用 Sprite 外UV；RawImage 用 uvRect；其他 0..1）
        Color32 col = graphic.color;
        Vector4 outerUV;
        if (graphic is Image img && img.sprite)
            outerUV = DataUtility.GetOuterUV(img.sprite);
        else if (graphic is RawImage raw)
        {
            var uv = raw.uvRect;
            outerUV = new Vector4(uv.xMin, uv.yMin, uv.xMax, uv.yMax);
        }
        else outerUV = new Vector4(0, 0, 1, 1);

        float left = rect.xMin, right = rect.xMax, bottom = rect.yMin, top = rect.yMax;
        // UV 半径按本地尺寸比例换算（X/Y 分别计算，避免非等比拉花）
        float uvRX = rLocalX / w * (outerUV.z - outerUV.x);
        float uvRY = rLocalY / h * (outerUV.w - outerUV.y);

        vh.Clear();

        // 基础 16 顶点：注意用 rLocalX / rLocalY 分别推进内列/内行
        // 0..3 左列
        vh.AddVert(new Vector3(left, top),           col, new Vector2(outerUV.x,                outerUV.w));           //0
        vh.AddVert(new Vector3(left, top - rLocalY), col, new Vector2(outerUV.x,                outerUV.w - uvRY));    //1
        vh.AddVert(new Vector3(left, bottom + rLocalY), col, new Vector2(outerUV.x,             outerUV.y + uvRY));    //2
        vh.AddVert(new Vector3(left, bottom),        col, new Vector2(outerUV.x,                outerUV.y));           //3

        // 4..7 左内列
        vh.AddVert(new Vector3(left + rLocalX, top), col, new Vector2(outerUV.x + uvRX,         outerUV.w));           //4
        vh.AddVert(new Vector3(left + rLocalX, top - rLocalY), col, new Vector2(outerUV.x + uvRX, outerUV.w - uvRY));  //5
        vh.AddVert(new Vector3(left + rLocalX, bottom + rLocalY), col, new Vector2(outerUV.x + uvRX, outerUV.y + uvRY));//6
        vh.AddVert(new Vector3(left + rLocalX, bottom), col, new Vector2(outerUV.x + uvRX,       outerUV.y));           //7

        // 8..11 右内列
        vh.AddVert(new Vector3(right - rLocalX, top), col, new Vector2(outerUV.z - uvRX,         outerUV.w));           //8
        vh.AddVert(new Vector3(right - rLocalX, top - rLocalY), col, new Vector2(outerUV.z - uvRX, outerUV.w - uvRY));  //9
        vh.AddVert(new Vector3(right - rLocalX, bottom + rLocalY), col, new Vector2(outerUV.z - uvRX, outerUV.y + uvRY));//10
        vh.AddVert(new Vector3(right - rLocalX, bottom), col, new Vector2(outerUV.z - uvRX,      outerUV.y));           //11

        // 12..15 右列
        vh.AddVert(new Vector3(right, top),          col, new Vector2(outerUV.z,                outerUV.w));           //12
        vh.AddVert(new Vector3(right, top - rLocalY),col, new Vector2(outerUV.z,                outerUV.w - uvRY));    //13
        vh.AddVert(new Vector3(right, bottom + rLocalY), col, new Vector2(outerUV.z,            outerUV.y + uvRY));    //14
        vh.AddVert(new Vector3(right, bottom),       col, new Vector2(outerUV.z,                outerUV.y));           //15

        // 三个矩形
        vh.AddTriangle(2, 5, 1);  vh.AddTriangle(2, 6, 5);
        vh.AddTriangle(7, 8, 4);  vh.AddTriangle(7, 11, 8);
        vh.AddTriangle(10, 13, 9);vh.AddTriangle(10, 14, 13);

        int seg = Mathf.Clamp(cornerSegments, 2, 32);
        // 四角扇形（注意：位置偏移用 rLocalX/rLocalY，UV 偏移用 uvRX/uvRY）
        AddCorner(vh, new Vector2(right - rLocalX, top - rLocalY),   9,  0f,   rightTop,   rLocalX, rLocalY,
                  new Vector2(outerUV.z - uvRX,  outerUV.w - uvRY),  uvRX, uvRY, col, seg);
        AddCorner(vh, new Vector2(left  + rLocalX, top - rLocalY),   5,  90f,  leftTop,    rLocalX, rLocalY,
                  new Vector2(outerUV.x + uvRX,  outerUV.w - uvRY),  uvRX, uvRY, col, seg);
        AddCorner(vh, new Vector2(left  + rLocalX, bottom + rLocalY),6,  180f, leftBottom, rLocalX, rLocalY,
                  new Vector2(outerUV.x + uvRX,  outerUV.y + uvRY),  uvRX, uvRY, col, seg);
        AddCorner(vh, new Vector2(right - rLocalX, bottom + rLocalY),10, 270f, rightBottom,rLocalX, rLocalY,
                  new Vector2(outerUV.z - uvRX,  outerUV.y + uvRY),  uvRX, uvRY, col, seg);
    }

    void AddCorner(VertexHelper vh, Vector2 center, int baseIndex, float startDeg, bool enable,
                   float rx, float ry, Vector2 uvCenter, float uvRX, float uvRY, Color32 col, int seg)
    {
        if (!enable || rx <= 0f || ry <= 0f) return;

        int start = vh.currentVertCount;
        float step = 90f / seg;
        float a = startDeg;

        for (int j = 0; j <= seg; j++)
        {
            float rad = a * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            var pos = new Vector3(center.x + cos * rx, center.y + sin * ry, 0f);          // 椭圆补偿（世界里是圆）
            var uv0 = new Vector2(uvCenter.x + cos * uvRX, uvCenter.y + sin * uvRY);
            vh.AddVert(pos, col, uv0);
            a -= step;
        }
        for (int j = 0; j < seg; j++)
            vh.AddTriangle(baseIndex, start + j + 1, start + j);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (graphic) { graphic.SetVerticesDirty(); graphic.SetMaterialDirty(); }
    }
#endif
}
