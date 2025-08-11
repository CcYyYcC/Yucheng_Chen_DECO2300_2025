using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI onScreenText;
    public DebugRotator targetRotator;


    void Start()
    {
        // Find the Text element by name
        onScreenText = GameObject.Find("OnScreenText").GetComponent<TextMeshProUGUI>();
        
        // Find the DebugRotator script by name
        targetRotator = GameObject.Find("DebugCube").GetComponent<DebugRotator>();
    }
    void Update()
    {
        // Update the UI text with the current rotation value
        onScreenText.text = "Rotation: " + targetRotator.currentRotation.ToString("F1");
    }
}