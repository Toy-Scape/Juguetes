using UnityEngine;
using UnityEngine.InputSystem;

public class InputPromptIconProvider : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private Sprite iconKeyboard;
    [SerializeField] private Sprite iconXbox;
    [SerializeField] private Sprite iconPlayStation;
    [SerializeField] private Sprite iconSwitch;

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public Sprite GetCurrentInteractIcon()
    {
        if (playerInput == null)
            return iconKeyboard;

        string scheme = playerInput.currentControlScheme?.ToLower() ?? "";

        if (scheme.Contains("gamepad"))
        {
            var device = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
            if (device != null)
            {
                string name = device.name.ToLower();
                string layout = device.layout.ToLower();

                if (name.Contains("ps") || layout.Contains("dualshock"))
                    return iconPlayStation;
                if (name.Contains("switch"))
                    return iconSwitch;
            }

            return iconXbox;
        }

        return iconKeyboard;
    }
}
