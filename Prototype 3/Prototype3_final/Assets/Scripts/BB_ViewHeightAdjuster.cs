using UnityEngine;

/// <summary>
/// Simple height adjustment script for Meta/Oculus rigs.
///
/// ▶ How it works:
/// Hold the selected button (default = right thumbstick),
/// then move your controller up or down to raise or lower your view height.
/// When you release the button, the new height stays fixed.
///
/// This script only changes the Y position of the OVRCameraRig’s TrackingSpace,
/// so your whole view moves up or down together.
/// </summary>
public class BB_ViewHeightAdjuster : MonoBehaviour
{
    [Header("OVR / Building Blocks")]
    /// <summary>
    /// The OVRCameraRig in your scene.  
    /// If not assigned, the script will try to find one automatically.
    /// </summary>
    public OVRCameraRig cameraRig;

    /// <summary>
    /// The controller used to measure up/down movement (for example, RightHandAnchor).
    /// </summary>
    public Transform controller;

    [Header("Input")]
    /// <summary>
    /// The button you need to hold while changing height.  
    /// Default: Right thumbstick press.
    /// </summary>
    public OVRInput.RawButton holdButton = OVRInput.RawButton.RThumbstick;

    [Header("Tuning")]
    /// <summary>
    /// How sensitive the height change is.  
    /// Bigger value = faster height change.
    /// </summary>
    public float sensitivity = 1.0f;

    /// <summary>
    /// The lowest allowed height (in meters).
    /// </summary>
    public float minHeight = 0.3f;

    /// <summary>
    /// The highest allowed height (in meters).
    /// </summary>
    public float maxHeight = 2.2f;

    // Internal values
    Transform _trackingSpace;   // Reference to TrackingSpace inside the camera rig
    float _baseOffsetY;         // The height offset when adjustment starts
    float _startCtrlY;          // Controller's starting Y position
    bool _adjusting;            // Whether the player is currently adjusting height

    void Awake()
    {
        // Automatically find the OVRCameraRig if not set manually
        if (!cameraRig) cameraRig = FindObjectOfType<OVRCameraRig>();

        // Get the TrackingSpace transform from the rig
        _trackingSpace = cameraRig ? cameraRig.trackingSpace : null;
    }

    void Update()
    {
        if (!_trackingSpace || !controller) return;

        // When the button is first pressed, start height adjustment
        if (OVRInput.GetDown(holdButton))
        {
            _adjusting   = true;
            _baseOffsetY = _trackingSpace.localPosition.y; // Current height
            _startCtrlY  = controller.position.y;           // Controller's start Y
        }

        // While holding the button, move view up/down with controller movement
        if (_adjusting && OVRInput.Get(holdButton))
        {
            // Calculate the Y difference from where the controller started
            float dy = (controller.position.y - _startCtrlY) * sensitivity;

            // Add the change to the base height, but keep it in allowed range
            float targetY = Mathf.Clamp(_baseOffsetY + dy, minHeight, maxHeight);

            // Update the tracking space height
            var lp = _trackingSpace.localPosition;
            lp.y = targetY;
            _trackingSpace.localPosition = lp;
        }

        // When the button is released, stop adjusting (keep current height)
        if (_adjusting && OVRInput.GetUp(holdButton))
        {
            _adjusting = false;
        }
    }
}
