using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadialMenuController : MonoBehaviour
{
    [SerializeField] private RadialMenu radialMenu;

    public static event Action OnRadialOpen;
    public static event Action OnRadialClose;

    void OnOpenRadialMenu(InputValue value)
    {
        if (value.isPressed && radialMenu.CanBeOpened())
        {
            radialMenu.Show();
            OnRadialOpen?.Invoke();
        }
        else
        {
            radialMenu.ConfirmSelection();
            radialMenu.Hide();
            OnRadialClose?.Invoke();
        }
    }

    void OnNavigateRadialMenu(InputValue value)
    {
        if (!radialMenu.isActiveAndEnabled) return;
        Vector2 input = value.Get<Vector2>();
        radialMenu.SelectWithJoystick(input);
    }

    void OnPointRadialMenu(InputValue value)
    {
        if (!radialMenu.isActiveAndEnabled) return;
        Vector2 mousePos = value.Get<Vector2>();
        Vector2 center = radialMenu.transform.position;
        radialMenu.SelectWithMouse(mousePos, center);
    }

    void OnRadialConfirm()
    {
        if (!radialMenu.isActiveAndEnabled) return;
        radialMenu.ConfirmSelection();
    }

    void OnRadialCancel()
    {
        if (!radialMenu.isActiveAndEnabled) return;
        radialMenu.Hide();
    }
}
