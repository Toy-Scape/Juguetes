using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    private PlayerInputHandler input;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text TMPPlayerState;

    private CharacterController controller;
    private PlayerMovementHandler movementHandler;
    private PlayerJumpHandler jumpHandler;
    private PlayerCrouchHandler crouchHandler;
    private PlayerStateHandler stateHandler;

    [Header("Movement Settings")]
    [SerializeField] public float walkSpeed = 4f;
    [SerializeField] public float sprintSpeed = 8f;
    [SerializeField] public float crouchSpeed = 2f;
    [SerializeField] public float swimSpeed = 3f;
    [SerializeField] public float diveSpeed = 2f;
    [SerializeField] public float jumpHeight = 2f;
    [SerializeField] public float gravity = -9.81f;

    [Header("Crouch Settings")]
    [SerializeField] public float standingHeight = 2f;
    [SerializeField] public float crouchingHeight = 1f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public Vector3 Velocity { get; set; }
    public bool IsInWater { get; set; }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();

        movementHandler = new PlayerMovementHandler(this, controller, cameraTransform);
        jumpHandler = new PlayerJumpHandler(this, controller);
        crouchHandler = new PlayerCrouchHandler(this, controller);
        stateHandler = new PlayerStateHandler(this, controller, input);
    }

    void Update()
    {
        CurrentState = stateHandler.EvaluateState(
            Velocity
        );

        movementHandler.HandleMovement(CurrentState, input.MoveInput, input.IsSprinting);
        jumpHandler.HandleJump(CurrentState, input.IsJumping);
        crouchHandler.HandleCrouch(CurrentState);


        movementHandler.ApplyGravity(CurrentState);

        controller.Move(movementHandler.GetFinalMove(Velocity) * Time.deltaTime);

        if (TMPPlayerState != null)
            TMPPlayerState.text = "Current State: " + CurrentState.ToString();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
            IsInWater = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
            IsInWater = false;
    }
}
