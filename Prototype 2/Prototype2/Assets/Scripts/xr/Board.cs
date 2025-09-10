using System.Linq;
using UnityEngine;

/// <summary>
/// 画板
/// </summary>
public class Board : MonoBehaviour
{
    [Range(0, 1)]
    public float lerp = 0.05f;
    public Texture2D initailizeTexture;
    private Texture2D currentTexture;
    private Vector2 paintPos;

    private bool isDrawing = false;
    private int lastPaintX;
    private int lastPaintY;
    private int painterTipsWidth = 30;
    private int painterTipsHeight = 15;
    private int textureWidth;
    private int textureHeight;

    private Color32[] painterColor;
    private Color32[] currentColor;
    private Color32[] originColor;

    private void Start()
    {
        //获取原始图片的大小 
        Texture2D originTexture = GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
        textureWidth = originTexture.width;
        textureHeight = originTexture.height;

        Debug.Log($"[Board] Start() -> originTexture={originTexture.name}, size={textureWidth}x{textureHeight}");

        //设置当前图片
        currentTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false, true);
        currentTexture.SetPixels32(originTexture.GetPixels32());
        currentTexture.Apply();

        //赋值给黑板
        GetComponent<MeshRenderer>().material.mainTexture = currentTexture;

        //初始化画笔的颜色
        painterColor = Enumerable.Repeat<Color32>(new Color32(255, 0, 0, 255), painterTipsWidth * painterTipsHeight).ToArray<Color32>();

        Debug.Log($"[Board] 初始化完成 -> painterTips={painterTipsWidth}x{painterTipsHeight}, 默认颜色={painterColor[0]}");
    }

    private void LateUpdate()
    {
        int texPosX = (int)(paintPos.x * (float)textureWidth - (float)(painterTipsWidth / 2));
        int texPosY = (int)(paintPos.y * (float)textureHeight - (float)(painterTipsHeight / 2));

        if (isDrawing)
        {
            Debug.Log($"[Board] 绘制中 -> UV=({paintPos.x:F3},{paintPos.y:F3}), tex=({texPosX},{texPosY}), color={painterColor[0]}");

            currentTexture.SetPixels32(texPosX, texPosY, painterTipsWidth, painterTipsHeight, painterColor);

            if (lastPaintX != 0 && lastPaintY != 0)
            {
                int lerpCount = (int)(1 / lerp);
                Debug.Log($"[Board] 插值绘制 -> from=({lastPaintX},{lastPaintY}) to=({texPosX},{texPosY}), steps={lerpCount}");

                for (int i = 0; i <= lerpCount; i++)
                {
                    int x = (int)Mathf.Lerp((float)lastPaintX, (float)texPosX, lerp);
                    int y = (int)Mathf.Lerp((float)lastPaintY, (float)texPosY, lerp);

                    Debug.Log($"[Board] 插值点 -> step={i}, pos=({x},{y})");

                    currentTexture.SetPixels32(x, y, painterTipsWidth, painterTipsHeight, painterColor);
                }
            }
            currentTexture.Apply();
            lastPaintX = texPosX;
            lastPaintY = texPosY;
        }
        else
        {
            if (lastPaintX != 0 || lastPaintY != 0)
            {
                Debug.Log("[Board] 停止绘制，重置 lastPaintX/Y");
            }
            lastPaintX = lastPaintY = 0;
        }
    }

    public void SetPainterPositon(float x, float y)
    {
        paintPos.Set(x, y);
        Debug.Log($"[Board] SetPainterPositon -> ({x:F3},{y:F3})");
    }

    public bool IsDrawing
    {
        get { return isDrawing; }
        set
        {
            if (isDrawing != value)
            {
                Debug.Log($"[Board] IsDrawing 改变 -> {value}");
            }
            isDrawing = value;
        }
    }

    public void SetPainterColor(Color32 color)
    {
        if (!painterColor[0].IsEqual(color))
        {
            for (int i = 0; i < painterColor.Length; i++)
            {
                painterColor[i] = color;
            }
            Debug.Log($"[Board] SetPainterColor -> RGBA({color.r},{color.g},{color.b},{color.a})");
        }
    }
}

public static class MethodExtention
{
    public static bool IsEqual(this Color32 origin, Color32 compare)
    {
        return origin.r == compare.r && origin.g == compare.g &&
               origin.b == compare.b && origin.a == compare.a;
    }
}
