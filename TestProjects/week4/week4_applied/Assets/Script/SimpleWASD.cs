using UnityEngine;

public class SimpleWASD : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        Vector3 movement = Vector3.zero;
        
        // W and S for forward/backward movement
        if (Input.GetKey(KeyCode.W)) movement.z += 1f;
        if (Input.GetKey(KeyCode.S)) movement.z -= 1f;
        
        // A and D for rotation (turning left and right)
        if (Input.GetKey(KeyCode.A)) transform.Rotate(0, -90f * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.D)) transform.Rotate(0, 90f * Time.deltaTime, 0);
        
        // For efficiency, we check to make sure that we need to move before calling Translate
        if (movement != Vector3.zero)
        {
            transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.Self);
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw a debug ray pointing forward from the player
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}