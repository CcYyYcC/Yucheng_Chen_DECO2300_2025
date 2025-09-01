using UnityEngine;
using Oculus.Interaction;

public class CustomTransformer : MonoBehaviour, ITransformer
{
    private IGrabbable _grabbable;
    private Pose _startGrabPose;
    private Vector3 _startObjectPos;
    private Vector3 _grabOffset;
    
    // Variables for rainbow color cycling functionality
    private Renderer _renderer;
    private float _startHue;

    public void Initialize(IGrabbable grabbable)
    {
        _grabbable = grabbable;
        // Get renderer component and create unique material instance
        _renderer = GetComponent<Renderer>();
        _renderer.material = new Material(_renderer.material);
    }

    public void BeginTransform()
    {
        var grabPoint = _grabbable.GrabPoints[0];
        _startGrabPose = grabPoint;
        _startObjectPos = transform.position;
        _grabOffset = _startObjectPos - _startGrabPose.position;
        
        // Store starting color hue for rainbow cycling
        Color rgb = _renderer.material.color;
        Color.RGBToHSV(rgb, out float h, out _, out _);
        _startHue = h;
    }

    public void UpdateTransform()
    {
        var grabPoint = _grabbable.GrabPoints[0];
        Vector3 targetPos = grabPoint.position + _grabOffset;
        transform.position = targetPos;
        
        // Rainbow color cycling based on Y movement
        Vector3 delta = grabPoint.position - _startGrabPose.position;
        float hueDelta = delta.y * 2.0f; // Only Y movement affects color
        float hue = Mathf.Repeat(_startHue + hueDelta, 1f);
        _renderer.material.color = Color.HSVToRGB(hue, 1f, 1f);
    }

    public void EndTransform()
    {
        // Called when grab ends
    }
}