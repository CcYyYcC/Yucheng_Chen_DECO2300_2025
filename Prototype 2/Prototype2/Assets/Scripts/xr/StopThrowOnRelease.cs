using UnityEngine;

public class StopThrowOnRelease : MonoBehaviour
{
    public bool enableGravityOnRelease = true;
    public void StopNow()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = enableGravityOnRelease;
            rb.Sleep();
        }
    }
}
