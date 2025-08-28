using UnityEngine;

/// <summary>
/// Purpose:
/// - A tiny *relay* component for UI buttons or other scripts.
/// - It forwards a reset request to a central `ResetManager`.
///
/// - Keeps UI (buttons) simple and decoupled from scene logic.
/// - All actual reset logic stays in one place (`ResetManager`), avoiding conflicts.
/// </summary>
public class PanelResetProxy : MonoBehaviour
{
    // Drag ResetManager here in the Inspector.
    // If it's not assigned, the call will be safely ignored (no error).
    public ResetManager resetManager;

    // Call this from a UI Button (OnClick) or other scripts.
    // The null-conditional operator (?.) avoids errors if resetManager is missing.
    public void ResetFromPanel() => resetManager?.ResetNow();
}
