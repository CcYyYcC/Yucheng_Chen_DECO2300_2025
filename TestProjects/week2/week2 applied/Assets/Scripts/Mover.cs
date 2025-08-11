using UnityEngine;

public class Mover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 45f;
    public Vector3 moveDirection = Vector3.forward;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
