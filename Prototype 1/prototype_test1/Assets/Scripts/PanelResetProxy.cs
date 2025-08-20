using UnityEngine;

public class PanelResetProxy : MonoBehaviour
{
    public ResetManager resetManager; // 拖场景中的 _ResetManager 进来
    public void ResetFromPanel() => resetManager?.ResetNow();
}
