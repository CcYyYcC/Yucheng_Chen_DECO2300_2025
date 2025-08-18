using UnityEngine;

public class PlayerGrabber : MonoBehaviour
{
    public float grabDistance = 3f;
    private GameObject grabbedObject = null;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grabbedObject == null)
            {
                GrabObject();
            }
            else
            {
                ThrowObject();
            }
        }
    }

    void GrabObject()
{
    Ray ray = new Ray(transform.position, transform.forward);
    RaycastHit hit;
    
    if (Physics.Raycast(ray, out hit, grabDistance))
    {
        Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
        if (grabbable != null && !grabbable.isGrabbed)
        {
            // Grab the object we're looking at
            grabbedObject = grabbable.gameObject;
            grabbable.isGrabbed = true;
            
            // Make object follow player
            grabbedObject.transform.SetParent(transform);
            grabbedObject.transform.localPosition = new Vector3(0, 1, 1);
            
            Debug.Log("Grabbed: " + grabbedObject.name + " using raycast");
        }
    }
}
void ThrowObject()
{
    if (grabbedObject != null)
    {
        // Release object
        grabbedObject.transform.SetParent(null);
        
        // Get the Grabbable component and mark as not grabbed
        Grabbable grabbable = grabbedObject.GetComponent<Grabbable>();
        if (grabbable != null)
        {
            grabbable.isGrabbed = false;
        }
        
        // Apply throwing force
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * 10f, ForceMode.Impulse);
        }
        
        Debug.Log("Threw: " + grabbedObject.name);
        grabbedObject = null;
    }
}
}