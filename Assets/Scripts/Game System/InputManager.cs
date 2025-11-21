using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TMP_Text currentInputMap;

    private string currentMap;
    private InputAction openRadialAction;
    private bool justSwitched; // evita rebotes

    private void Awake ()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        SwitchToPlayer();

        openRadialAction = playerInput.actions["OpenRadialMenu"];
        openRadialAction.performed += OnOpenRadialPerformed;
        openRadialAction.canceled += OnOpenRadialCanceled;

    }
    private void Update ()
    {
        currentInputMap.text = $"Current Action Map: {currentMap}";

        if (currentMap == "Player")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    private void OnDestroy ()
    {
        openRadialAction.performed -= OnOpenRadialPerformed;
        openRadialAction.canceled -= OnOpenRadialCanceled;
    }

    void OnOpenRadial(InputValue value)
    {
        if (justSwitched)
            return;
        justSwitched = true;
        Invoke(nameof(ResetSwitchFlag), 0.1f);
        if (value.isPressed && currentMap == "Player")
        {
            SwitchToRadialMenu();
        }
        else if (!value.isPressed && currentMap == "RadialMenu")
        {
            SwitchToPlayer();
        }
    }
    private void OnOpenRadialPerformed (InputAction.CallbackContext ctx)
    {
        if (currentMap == "Player")
        {
            SwitchToRadialMenu();
        }
    }

    private void OnOpenRadialCanceled (InputAction.CallbackContext ctx)
    {
        if (currentMap == "RadialMenu")
        {
            SwitchToPlayer();
        }
    }

    private void ResetSwitchFlag ()
    {
        justSwitched = false;
    }

    private void SwitchToPlayer ()
    {
        playerInput.SwitchCurrentActionMap("Player");
        currentMap = "Player";
        RebindOpenRadial();
    }

    private void SwitchToRadialMenu ()
    {
        playerInput.SwitchCurrentActionMap("RadialMenu");
        currentMap = "RadialMenu";
        RebindOpenRadial();
    }

    private void RebindOpenRadial ()
    {
        if (openRadialAction != null)
        {
            openRadialAction.performed -= OnOpenRadialPerformed;
            openRadialAction.canceled -= OnOpenRadialCanceled;
        }

        openRadialAction = playerInput.currentActionMap.FindAction("OpenRadialMenu");
        openRadialAction.performed += OnOpenRadialPerformed;
        openRadialAction.canceled += OnOpenRadialCanceled;
    }

    public void SwitchToUI ()
    {
        playerInput.SwitchCurrentActionMap("UI");
        currentMap = "UI";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public string GetCurrentMap ()
    {
        return currentMap;
    }
}
