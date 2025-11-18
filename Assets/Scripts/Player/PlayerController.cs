using TMPro;
using UnityEngine;
using InteractionSystem.Core; 

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text TMPPlayerState;
    [SerializeField] private PlayerInteractor playerInteractor;

    private CharacterController controller;
    private PlayerInputHandler input;

    private PlayerMovementHandler movementHandler;
    private PlayerJumpHandler jumpHandler;
    private PlayerCrouchHandler crouchHandler;
    private PlayerStateHandler stateHandler;
    private PlayerPhysicsHandler physicsHandler;
    private PlayerLedgeGrabHandler ledgeGrabHandler;

    private float ledgeGrabCooldownTimer = 0f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
    public Vector3 Velocity { get; private set; }
    public bool IsInWater { get; private set; }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputHandler>();

        var crouchConfig = new CrouchConfig(config.standingHeight, config.crouchingHeight);
        var movementConfig = new MovementConfig(config.walkSpeed, config.sprintSpeed, config.crouchSpeed,
                                                config.swimSpeed, config.diveSpeed);
        var jumpConfig = new JumpConfig(config.jumpHeight, config.gravity, config.swimSpeed, config.diveSpeed);
        var stateConfig = new StateConfig();
        var physicsConfig = new PhysicsConfig(config.gravity);

        crouchHandler = new PlayerCrouchHandler(crouchConfig);
        movementHandler = new PlayerMovementHandler(movementConfig, cameraTransform);
        jumpHandler = new PlayerJumpHandler(jumpConfig);
        stateHandler = new PlayerStateHandler(stateConfig, input);
        physicsHandler = new PlayerPhysicsHandler(physicsConfig);
        ledgeGrabHandler = new PlayerLedgeGrabHandler(config, transform);
    }

    void Update()
    {
        if (ledgeGrabCooldownTimer > 0f)
            ledgeGrabCooldownTimer -= Time.deltaTime;

        if (CurrentState == PlayerState.LedgeGrabbing)
        {
            Velocity = Vector3.zero;

            if (input.IsJumping && !ledgeGrabHandler.IsClimbing)
            {
                ledgeGrabHandler.StartClimb();
            }

            if (ledgeGrabHandler.UpdateClimb())
            {
                CurrentState = PlayerState.Walking;
            }
            else if (input.IsCrouching)
            {
                CurrentState = PlayerState.Falling;
                ledgeGrabCooldownTimer = config.ledgeGrabCooldown;
            }
        }
        else if (ledgeGrabCooldownTimer <= 0f && ledgeGrabHandler.CheckForLedge(Velocity, controller.isGrounded))
        {
            CurrentState = PlayerState.LedgeGrabbing;
            Velocity = Vector3.zero;
            ledgeGrabHandler.SnapToLedge();
        }
        else
        {
            CurrentState = stateHandler.EvaluateState(Velocity, IsInWater, controller.isGrounded);

            movementHandler.HandleMovement(CurrentState, input.MoveInput, transform);

            Velocity = jumpHandler.HandleJump(CurrentState, input.IsJumping, Velocity, IsInWater, controller.isGrounded);

            controller.height = crouchHandler.GetTargetHeight(CurrentState, controller.height);

            Velocity = physicsHandler.ApplyGravity(CurrentState, Velocity, controller.isGrounded);

            controller.Move(movementHandler.GetFinalMove(Velocity) * Time.deltaTime);
        }

        if (input.IsInteracting && playerInteractor != null)
        {
            playerInteractor.OnInteract();
        }

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
