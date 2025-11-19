using InteractionSystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Necesario para los callbacks
using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text TMPPlayerState;
    [SerializeField] private PlayerInteractor playerInteractor;

    private CharacterController controller;

    private PlayerMovementHandler movementHandler;
    private PlayerJumpHandler jumpHandler;
    private PlayerCrouchHandler crouchHandler;
    private PlayerStateHandler stateHandler;
    private PlayerPhysicsHandler physicsHandler;
    private PlayerLedgeGrabHandler ledgeGrabHandler;

    private PlayerContext playerContext = new PlayerContext();

    private float ledgeGrabCooldownTimer = 0f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
 
        controller = GetComponent<CharacterController>();

        var crouchConfig = new CrouchConfig(config.standingHeight, config.crouchingHeight);
        var movementConfig = new MovementConfig(config.walkSpeed, config.sprintSpeed, config.crouchSpeed,
                                                config.swimSpeed, config.diveSpeed);
        var jumpConfig = new JumpConfig(config.jumpHeight, config.gravity, config.swimSpeed, config.diveSpeed);
        var stateConfig = new StateConfig();
        var physicsConfig = new PhysicsConfig(config.gravity);

        crouchHandler = new PlayerCrouchHandler(crouchConfig);
        movementHandler = new PlayerMovementHandler(movementConfig, cameraTransform);
        jumpHandler = new PlayerJumpHandler(jumpConfig);
        stateHandler = new PlayerStateHandler(stateConfig); 
        physicsHandler = new PlayerPhysicsHandler(physicsConfig);
        ledgeGrabHandler = new PlayerLedgeGrabHandler(config, transform);
    }

    void Update()
    {
        if (ledgeGrabCooldownTimer > 0f)
            ledgeGrabCooldownTimer -= Time.deltaTime;

        if (CurrentState == PlayerState.LedgeGrabbing)
        {
            playerContext.Velocity = Vector3.zero;

            if (playerContext.IsJumping && !ledgeGrabHandler.IsClimbing)
            {
                ledgeGrabHandler.StartClimb();
            }

            if (ledgeGrabHandler.UpdateClimb())
            {
                CurrentState = PlayerState.Walking;
            }
            else if (playerContext.IsCrouching)
            {
                CurrentState = PlayerState.Falling;
                ledgeGrabCooldownTimer = config.ledgeGrabCooldown;
            }
        }
        else if (ledgeGrabCooldownTimer <= 0f && ledgeGrabHandler.CheckForLedge(playerContext.Velocity, controller.isGrounded))
        {
            CurrentState = PlayerState.LedgeGrabbing;
            playerContext.Velocity = Vector3.zero;
            ledgeGrabHandler.SnapToLedge();
        }
        else
        {
            playerContext.IsGrounded = controller.isGrounded;

            CurrentState = stateHandler.EvaluateState(playerContext);

            movementHandler.HandleMovement(CurrentState, playerContext.MoveInput, transform);
            controller.height = crouchHandler.GetTargetHeight(CurrentState, controller.height);
            playerContext.Velocity = jumpHandler.HandleJump(CurrentState, playerContext.IsJumping, playerContext.Velocity, playerContext.IsInWater, playerContext.IsGrounded);
            playerContext.Velocity = physicsHandler.ApplyGravity(CurrentState, playerContext.Velocity, playerContext.IsGrounded);

            controller.Move(movementHandler.GetFinalMove(playerContext.Velocity) * Time.deltaTime);

            //if (playerContext.IsInteracting && playerInteractor != null)
            //{
            //    playerInteractor.OnInteract();
            //    playerContext.IsInteracting = false; // consumir input
            //}

            if (playerContext.IsAttacking)
            {
                // TODO:: Lógica del gancho/lanzar objetos, etc.
                this.playerContext.IsAttacking = false;
            }
        }
        
        if (TMPPlayerState != null)
            TMPPlayerState.text = "Current State: " + CurrentState.ToString();

    }
    void LateUpdate()
    {
        playerContext.IsInteracting = false;
        playerContext.IsAttacking = false;
        playerContext.NextLimb = false;
        playerContext.PreviousLimb = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
            playerContext.IsInWater = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
            playerContext.IsInWater = false;
    }

    void OnMove(InputValue value)
    {
        playerContext.MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        playerContext.LookInput = value.Get<Vector2>() * config.lookSensitivity;
    }

    void OnSprint(InputValue value)
    {
        playerContext.IsSprinting = value.isPressed;
    }

    void OnJump(InputValue value)
    {
        playerContext.IsJumping = value.isPressed;
    }

    void OnCrouch(InputValue value)
    {
        playerContext.IsCrouching = value.isPressed; 
    }

    void OnCrouchToggle()
    {
        playerContext.IsCrouching = !playerContext.IsCrouching; 
    }



    void OnInteract()
    {
        playerContext.IsInteracting = true;
    }

    void OnAttack()
    {
        playerContext.IsAttacking = true;
    }

    void OnGrab(InputValue value)
    {
        playerContext.IsGrabbing = value.isPressed;
    }

    void OnNext()
    {
        playerContext.NextLimb = true;
    }

    void OnPrevious()
    {
        playerContext.PreviousLimb = true;
    }

}
