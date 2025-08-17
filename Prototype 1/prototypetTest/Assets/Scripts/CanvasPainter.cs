using UnityEngine;

public class CanvasPainter : MonoBehaviour
{
    [Header("Paint Texture")]
    public int texSize = 1024;
    public Color clearColor = Color.white;

    Texture2D _tex;
    Renderer _r;

    void Awake()
    {
        _r = GetComponent<Renderer>();

        _tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false, false);
        _tex.wrapMode = TextureWrapMode.Clamp;

        var cols = new Color[texSize * texSize];
        for (int i = 0; i < cols.Length; i++) cols[i] = clearColor;
        _tex.SetPixels(cols);
        _tex.Apply();

        var mat = _r.material;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _tex); // URP
        else mat.mainTexture = _tex;                                       // Built-in
    }

    // 在UV处画一个圆点（半径=像素）
    public void PaintAtUV(Vector2 uv, Color color, int radius)
    {
        int cx = Mathf.RoundToInt(uv.x * (texSize - 1));
        int cy = Mathf.RoundToInt((1f - uv.y) * (texSize - 1)); // 翻V轴

        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            int yy = cy + y; if (yy < 0 || yy >= texSize) continue;
            for (int x = -radius; x <= radius; x++)
            {
                int xx = cx + x; if (xx < 0 || xx >= texSize) continue;
                if (x * x + y * y <= r2) _tex.SetPixel(xx, yy, color);
            }
        }
        _tex.Apply(false);
    }
}
