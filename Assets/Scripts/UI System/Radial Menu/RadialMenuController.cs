using UnityEngine;
using UnityEngine.InputSystem;

public class RadialMenuController : MonoBehaviour
{
    [SerializeField] private RadialMenu radialMenu;
    private CameraManager cameraManager;

    private void Start ()
    {
        cameraManager = FindFirstObjectByType<CameraManager>();
    }

    void OnOpenRadialMenu (InputValue value)
    {
        if (value.isPressed)
        {
            radialMenu.Show();
            cameraManager.LockCameraMovement();
            cameraManager.UnlockCursor();
        }
        else
        {
            radialMenu.ConfirmSelection();
            radialMenu.Hide();
            cameraManager.UnlockCameraMovement();
            cameraManager.LockCursor();
        }

    }

    void OnNavigateRadialMenu (InputValue value)
    {
        if (!radialMenu.isActiveAndEnabled) return;
        Vector2 input = value.Get<Vector2>();
        radialMenu.SelectWithJoystick(input);
    }

    void OnPoint (InputValue value)
    {
        if (!radialMenu.isActiveAndEnabled) return;

        Vector2 mousePos = value.Get<Vector2>();
        Vector2 center = radialMenu.transform.position;
        radialMenu.SelectWithMouse(mousePos, center);
    }


    void OnRadialConfirm ()
    {
        if (!radialMenu.isActiveAndEnabled) return;
        radialMenu.ConfirmSelection();
    }

    void OnRadialCancel ()
    {
        if (!radialMenu.isActiveAndEnabled) return;
        radialMenu.Hide();
    }
}
