using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple physically-driven button that uses a ConfigurableJoint's linear limit
/// to detect "pressed" and "released" states, and fires UnityEvents accordingly.
/// 
/// How it works:
/// - Records the local start position in Start().
/// - Each frame, measures how far the object has moved from the start along the joint's travel,
///   normalized by the joint's linearLimit.limit (range 0..1).
/// - Applies a small dead zone to avoid jitter near 0.
/// - Compares the normalized value against a threshold to decide when to press/release.
/// 
/// Requirements:
/// - Attach this script to a GameObject that has a ConfigurableJoint configured for linear motion
///   along the button's push axis.
/// - Set up the joint's Linear Limit to match your intended travel distance.
/// - Hook up onPressed / onReleased events in the Inspector if needed.
/// </summary>
public class PhysicsButton : MonoBehaviour
{
    [SerializeField] private float threshold = 0.1f;   // Hysteresis threshold around the edges (prevents rapid toggling)
    [SerializeField] private float deadZone = 0.025f;  // Small range near 0 treated as no movement to reduce noise

    private bool _isPressed;                           // Current logical state of the button
    private Vector3 _startPos;                         // Local position when the scene starts (rest position)
    private ConfigurableJoint _joint;                  // The joint that constrains/defines button travel
    
    public UnityEvent onPressed, onReleased;           // Events fired when button transitions to pressed/released

    void Start()
    {
        _startPos = transform.localPosition;           // Cache starting local position as the reference point
        _joint = GetComponent<ConfigurableJoint>();    // Fetch the ConfigurableJoint on the same GameObject
    }

    void Update()
    {
        // Evaluate the normalized travel value and apply threshold logic:
        // If not pressed yet and we are past (1 - threshold) => press.
        if (!_isPressed && GetValue() + threshold >= 1)
            Pressed();

        // If currently pressed and we fall below (0 + threshold) => release.
        if (_isPressed && GetValue() - threshold <= 0)
            Released();
    }

    /// <summary>
    /// Computes how far the button has traveled from its start position,
    /// normalized by the joint's linear limit (0..1). Adds a dead zone and clamps to [-1, 1].
    /// 
    /// Note: Assumes the joint's Linear Limit (Soft Joint Limit) is set to the maximum intended travel.
    /// </summary>
    private float GetValue()
    {
        // Distance moved from start divided by the allowed linear travel from the joint
        var value = Vector3.Distance(_startPos, transform.localPosition) / _joint.linearLimit.limit;

        // Suppress tiny movements near zero to avoid flicker
        if (Math.Abs(value) < deadZone)
            value = 0;

        // Clamp to a safe range; typically value will be [0,1], but clamped to [-1,1] for robustness
        return Mathf.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Handles the transition to the pressed state and invokes the onPressed event.
    /// </summary>
    private void Pressed()
    {
        _isPressed = true;
        onPressed.Invoke();
        Debug.Log("Pressed");
    }

    /// <summary>
    /// Handles the transition to the released state and invokes the onReleased event.
    /// </summary>
    private void Released()
    {
        _isPressed = false;
        onReleased.Invoke();
        Debug.Log("Released");
    }
}
