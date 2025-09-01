using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    public GameObject targetObject;
    
    public Material materialA;
    public Material materialB;
    
    private Renderer targetRenderer;
    
    void Start()
    {
        targetRenderer = targetObject.GetComponent<Renderer>();
        targetRenderer.material = materialA;
    }
    
    public void SwapToMaterialB()
    {
        targetRenderer.material = materialB;
        Debug.Log("Swapped to Material B");
    }
    
    public void SwapToMaterialA()
    {
        targetRenderer.material = materialA;
        Debug.Log("Swapped to Material A");
    }
}