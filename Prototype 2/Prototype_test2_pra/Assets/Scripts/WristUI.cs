using UnityEngine;
using UnityEngine.InputSystem;

public class WristUI : MonoBehaviour
{
    [Header("拖入 LeftHandMenu/Menu 动作")]
    public InputActionReference menuAction;   // 在 Inspector 里拖入

    private Canvas _wristUICanvas;

    private void Awake()
    {
        _wristUICanvas = GetComponent<Canvas>();
        if (_wristUICanvas) _wristUICanvas.enabled = false; // 初始隐藏，可按需
    }

    private void OnEnable()
    {
        if (menuAction != null)
        {
            menuAction.action.Enable();
            menuAction.action.performed += ToggleMenu;
        }
    }

    private void OnDisable()
    {
        if (menuAction != null)
        {
            menuAction.action.performed -= ToggleMenu;
            menuAction.action.Disable();
        }
    }

    private void ToggleMenu(InputAction.CallbackContext ctx)
    {
        if (_wristUICanvas) _wristUICanvas.enabled = !_wristUICanvas.enabled;
    }
}
