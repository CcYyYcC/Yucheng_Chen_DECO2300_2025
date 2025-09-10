using System.Linq;
using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    public Vector2Int textureSize = new Vector2Int(1024, 1024);

    [Header("初始底色（如果不用底图）")]
    public Color clearColor = Color.white;

    [Header("可选：作为底图的贴图（不填则用 clearColor 生成纯色底图）")]
    public Texture2D initialBackground;

    [HideInInspector] public Texture2D texture;      // 实时绘制用
    [HideInInspector] public Texture2D baseTexture;  // 干净底图快照

    void Awake()
    {
        // 1) 创建绘制贴图
        texture = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        // 2) 创建底图快照（baseTexture）
        baseTexture = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGBA32, false);
        baseTexture.wrapMode = TextureWrapMode.Clamp;
        baseTexture.filterMode = FilterMode.Bilinear;

        if (initialBackground != null)
        {
            // 把底图缩放/拷贝到 baseTexture（简单版：直接采样）
            var bg = initialBackground.GetPixels(0, 0,
                Mathf.Min(initialBackground.width,  textureSize.x),
                Mathf.Min(initialBackground.height, textureSize.y));
            // 先全部清空
            var fill = Enumerable.Repeat(clearColor, textureSize.x * textureSize.y).ToArray();
            baseTexture.SetPixels(fill);
            // 覆盖左下角区域（够用；需要完整缩放可以自己做 resample）
            baseTexture.SetPixels(0, 0, Mathf.Min(initialBackground.width, textureSize.x),
                                    Mathf.Min(initialBackground.height, textureSize.y), bg);
        }
        else
        {
            var fill = Enumerable.Repeat(clearColor, textureSize.x * textureSize.y).ToArray();
            baseTexture.SetPixels(fill);
        }
        baseTexture.Apply(false);

        // 3) 初始化绘制贴图 = 底图内容
        texture.SetPixels(baseTexture.GetPixels());
        texture.Apply(false);

        // 4) 赋到材质
        var r = GetComponent<Renderer>();
        if (r && r.material) r.material.mainTexture = texture;
    }

    public void ClearToBase()
    {
        texture.SetPixels(baseTexture.GetPixels());
        texture.Apply(false);
    }
}
