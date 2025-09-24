using UnityEngine;

public class WhiteboardClearer : MonoBehaviour
{
    [Header("The parent object that holds all stroke objects (e.g. LineRenderers)")]
    public Transform strokesRoot;

    // This function will show up in UnityEvent dropdown (PhysicsButton.OnPressed)
    public void ClearWhiteBoard()
    {
        if (strokesRoot == null)
        {
            Debug.LogWarning("[WhiteboardClearer] StrokesRoot not assigned!");
            return;
        }

        // Destroy all child GameObjects under strokesRoot
        for (int i = strokesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(strokesRoot.GetChild(i).gameObject);
        }

        // Optionally also reset LineRenderers and TrailRenderers inside strokesRoot
        foreach (var lr in strokesRoot.GetComponentsInChildren<LineRenderer>(true))
        {
            lr.positionCount = 0;
        }
        foreach (var tr in strokesRoot.GetComponentsInChildren<TrailRenderer>(true))
        {
            tr.Clear();
        }

        Debug.Log("[WhiteboardClearer] All strokes cleared.");
    }
}
