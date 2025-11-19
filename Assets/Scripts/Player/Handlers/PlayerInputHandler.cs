using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private InputSystem_Actions controls;

    [Header("Movement")]
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool IsSprinting;

    [Header("Actions")]
    public bool IsJumping;
    public bool IsCrouching;
    public bool IsGrabbing;
    public bool IsInteracting;
    public bool IsAttacking;

    [Header("Limbs")]
    public bool NextLimb;
    public bool PreviousLimb;

    [Header("Settings")]
    public float lookSensitivity = 1f;

    void Awake ()
    {
        controls = new InputSystem_Actions();

        // Movimiento
        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        // Mirar
        controls.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>() * lookSensitivity;
        controls.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        // Sprint (hold)
        controls.Player.Sprint.started += ctx => IsSprinting = true;
        controls.Player.Sprint.canceled += ctx => IsSprinting = false;

        // Saltar (hold)
        controls.Player.Jump.started += ctx => IsJumping = true;
        controls.Player.Jump.canceled += ctx => IsJumping = false;

        controls.Player.Crouch.started += ctx =>
        {
            if (ctx.control.path.Contains("ctrl"))
                IsCrouching = true;
            else
                IsCrouching = !IsCrouching;
        };

        controls.Player.Crouch.canceled += ctx =>
        {
            if (ctx.control.path.Contains("ctrl"))
                IsCrouching = false;
        };

        // Interactuar (una sola pulsación)
        controls.Player.Interact.performed += ctx => IsInteracting = true;

        // Atacar (una sola pulsación)
        controls.Player.Attack.performed += ctx => IsAttacking = true;

        // Agarrar (hold)
        controls.Player.Grab.started += ctx => IsGrabbing = true;
        controls.Player.Grab.canceled += ctx => IsGrabbing = false;

        // Cambiar limb (eventos de un solo frame)
        controls.Player.Next.performed += ctx => NextLimb = true;
        controls.Player.Previous.performed += ctx => PreviousLimb = true;
    }

    void OnEnable () => controls.Player.Enable();
    void OnDisable () => controls.Player.Disable();
    void LateUpdate ()
    {
        IsInteracting = false;
        IsAttacking = false;
        NextLimb = false;
        PreviousLimb = false;
    }

    void OnDestroy ()
    {
        controls.Dispose();
    }
}
