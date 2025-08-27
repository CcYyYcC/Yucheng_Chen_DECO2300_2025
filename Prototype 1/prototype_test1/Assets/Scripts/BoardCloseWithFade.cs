using UnityEngine;
using System.Collections;

/// BoardCloseWithFade.cs
/// ------------------------------------------------------------
/// Purpose: Fade out the entire board/root and then disable it.
/// How to use:
/// 1) Attach this script to your board root GameObject (the parent that includes the UI/canvas).
/// 2) If the root has no CanvasGroup, one will be added automatically.
/// 3) Hook your "I know" button OnClick -> BoardCloseWithFade.Close().
/// Note: CanvasGroup fades UI elements. Non-UI/3D children won’t visually fade,
///       but they will be hidden when the root is deactivated at the end.
public class BoardCloseWithFade : MonoBehaviour
{
    [Tooltip("CanvasGroup used to control UI alpha/interaction.")]
    public CanvasGroup canvasGroup;

    [Tooltip("Fade-out duration in seconds (unscaled time).")]
    public float fadeDuration = 0.25f;

    void Awake()
    {
        // Ensure there is a CanvasGroup on the root (create if missing).
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Initial visible state.
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Called by the "I know" button. Starts the fade-out, then disables the root.
    /// </summary>
    public void Close()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(FadeOutThenDisable());
    }

    private IEnumerator FadeOutThenDisable()
    {
        float d = Mathf.Max(0.01f, fadeDuration);
        float startAlpha = canvasGroup ? canvasGroup.alpha : 1f;

        // Stop UI interaction during fade.
        if (canvasGroup) canvasGroup.interactable = false;

        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime; // unaffected by Time.timeScale
            if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / d);
            yield return null;
        }

        // Ensure fully transparent and non-blocking, then disable the whole board.
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }
}
