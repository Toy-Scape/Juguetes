using UnityEngine;
using UnityEngine.InputSystem;

public class RadialMenuController : MonoBehaviour
{
    [SerializeField] private RadialMenu radialMenu;

    void OnOpenRadialMenu (InputValue value)
    {
        Debug.Log("a");
        if (value.isPressed)
        {
            radialMenu.Show();
        }
        else
        {
            radialMenu.ConfirmSelection();
        }
    }

    void OnNavigate (InputValue value)
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
