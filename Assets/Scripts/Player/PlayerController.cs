using InteractionSystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text TMPPlayerState;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private GrabInteractor grabInteractor;
    [SerializeField] private GamepadVibration gamepadVibration;

    [SerializeField] private LimbManager limbManager;

    private CharacterController controller;

    private PlayerMovementHandler movementHandler;
    private PlayerJumpHandler jumpHandler;
    private PlayerCrouchHandler crouchHandler;
    private PlayerStateHandler stateHandler;
    private PlayerPhysicsHandler physicsHandler;
    private PlayerLedgeGrabHandler ledgeGrabHandler;
    private WallDetectionHandler wallHandler;

    private PlayerContext playerContext = new();

    private float ledgeGrabCooldownTimer = 0f;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (gamepadVibration == null) gamepadVibration = GetComponent<GamepadVibration>();
        if (gamepadVibration == null) gamepadVibration = GetComponentInChildren<GamepadVibration>();

        var crouchConfig = new CrouchConfig(config.StandingHeight, config.CrouchingHeight);
        var movementConfig = new MovementConfig(config.WalkSpeed, config.SprintSpeed, config.CrouchSpeed,
                                                config.SwimSpeed, config.DiveSpeed, config.WallWalkSpeed,
                                                config.RotationSpeed, config.Acceleration, config.Deceleration,
                                                config.InputSmoothing, config.TurnSmoothTime);
        var jumpConfig = new JumpConfig(config.JumpHeight, config.Gravity, config.SwimSpeed, config.DiveSpeed, config.CoyoteTime, config.JumpBufferTime);
        var stateConfig = new StateConfig();
        var physicsConfig = new PhysicsConfig(config.Gravity);

        crouchHandler = new PlayerCrouchHandler(crouchConfig);
        movementHandler = new PlayerMovementHandler(movementConfig, cameraTransform);
        jumpHandler = new PlayerJumpHandler(jumpConfig);
        stateHandler = new PlayerStateHandler(stateConfig);
        physicsHandler = new PlayerPhysicsHandler(physicsConfig);
        ledgeGrabHandler = new PlayerLedgeGrabHandler(config, transform);
        wallHandler = new WallDetectionHandler(transform); // NUEVO
    }

    void Update()
    {
        // Detección de pared
        playerContext.CanWalkOnWalls = limbManager.GetContext().CanClimbWalls;
        if (playerContext.CanWalkOnWalls)
        {
            playerContext.IsOnWall = wallHandler.CheckForWall(playerContext, 1f);
        }

        // Check for ledge grab BEFORE applying gravity/physics
        playerContext.IsGrabbingLedge = TryLedgeGrab();

        if (!playerContext.IsGrabbingLedge)
        {
            playerContext.IsGrounded = controller.isGrounded;

            CurrentState = stateHandler.EvaluateState(playerContext);

            jumpHandler.UpdateTimers(Time.deltaTime, playerContext.IsGrounded, playerContext.IsJumping);

            movementHandler.HandleMovement(CurrentState, playerContext.MoveInput, transform, playerContext);
            controller.height = crouchHandler.GetTargetHeight(CurrentState, controller.height);

            // Logic for Jump Vibration & Execution
            bool wasGrounded = playerContext.IsGrounded;
            Vector3 currentVelocity = playerContext.Velocity;
            bool jumped = jumpHandler.HandleJump(CurrentState, ref currentVelocity, playerContext.IsInWater, playerContext.IsGrounded, playerContext.IsPushing);
            playerContext.Velocity = currentVelocity;

            if (jumped && gamepadVibration != null)
                gamepadVibration.Vibrate(config.JumpVibration.x, config.JumpVibration.y, config.JumpVibration.z);

            playerContext.Velocity = physicsHandler.ApplyGravity(CurrentState, playerContext.Velocity, playerContext.IsGrounded);

            controller.Move(movementHandler.GetFinalMove(playerContext.Velocity) * Time.deltaTime);

            // Check landing
            if (!wasGrounded && controller.isGrounded && gamepadVibration != null)
                gamepadVibration.Vibrate(config.LandVibration.x, config.LandVibration.y, config.LandVibration.z);

            // Continuous Drag Vibration
            if (playerContext.IsPushing && playerContext.Velocity.magnitude > 0.1f && gamepadVibration != null)
            {
                gamepadVibration.SetContinuousVibration(config.DragVibration.x, config.DragVibration.y);
            }
            else if (gamepadVibration != null)
            {
                gamepadVibration.StopContinuousVibration();
            }
        }

        if (TMPPlayerState != null)
            TMPPlayerState.text = "Current State: " + CurrentState.ToString();
    }

    private bool TryLedgeGrab()
    {
        if (ledgeGrabCooldownTimer > 0f)
            ledgeGrabCooldownTimer -= Time.deltaTime;

        if (CurrentState == PlayerState.LedgeGrabbing)
        {
            playerContext.Velocity = Vector3.zero;

            if (playerContext.IsJumping && !ledgeGrabHandler.IsClimbing && !ledgeGrabHandler.IsSnapping)
            {
                ledgeGrabHandler.StartClimb();
            }

            if (ledgeGrabHandler.Update())
            {
                CurrentState = PlayerState.Walking;
            }
            else if (playerContext.IsCrouching)
            {
                CurrentState = PlayerState.Falling;
                ledgeGrabCooldownTimer = config.LedgeGrabCooldown;
            }
        }
        else if (ledgeGrabCooldownTimer <= 0f && ledgeGrabHandler.CheckForLedge(playerContext.Velocity, controller.isGrounded))
        {
            CurrentState = PlayerState.LedgeGrabbing;
            playerContext.Velocity = Vector3.zero;
            ledgeGrabHandler.StartSnap();
        }
        return CurrentState == PlayerState.LedgeGrabbing;
    }

    void LateUpdate()
    {
        playerContext.IsInteracting = false;
        playerContext.NextLimb = false;
        playerContext.PreviousLimb = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
            playerContext.IsInWater = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
            playerContext.IsInWater = false;
    }

    #region Inputs
    void OnMove(InputValue value) => playerContext.MoveInput = value.Get<Vector2>();

    void OnLook(InputValue value) => playerContext.LookInput = value.Get<Vector2>() * config.LookSensitivity;

    void OnSprint(InputValue value)
    {
        bool pressed = value.isPressed;
        if (Keyboard.current != null)
            playerContext.IsSprinting = pressed;
        else if (Gamepad.current != null && pressed)
            playerContext.IsSprinting = !playerContext.IsSprinting;
    }

    void OnJump(InputValue value) => playerContext.IsJumping = value.isPressed;

    void OnCrouch(InputValue value) => playerContext.IsCrouching = value.isPressed;

    void OnCrouchToggle() => playerContext.IsCrouching = !playerContext.IsCrouching;

    void OnInteract() => playerContext.IsInteracting = true;

    void OnGrab(InputValue value) => playerContext.IsGrabbing = value.isPressed;

    void OnNext() => playerContext.NextLimb = true;

    void OnPrevious() => playerContext.PreviousLimb = true;

    public void OnAim(InputValue value)
    {
        bool pressed = value.isPressed;

        if (Mouse.current != null && pressed)
        {
            limbManager.GetContext().IsAiming = !limbManager.GetContext().IsAiming;
            Debug.Log($"Aim (mouse toggle): {limbManager.GetContext().IsAiming}");
        }
        else if (Gamepad.current != null)
        {
            limbManager.GetContext().IsAiming = pressed;
            Debug.Log($"Aim (gamepad hold): {limbManager.GetContext().IsAiming}");
        }

        if (limbManager.GetContext().IsAiming)
            limbManager.UseSecondary();
    }

    public void OnShoot(InputValue value)
    {
        if (value.isPressed)
            limbManager.UseActive();
    }
    #endregion

    public void SetGrabState(bool isGrabbing, float resistance)
    {
        playerContext.IsPushing = isGrabbing;
        playerContext.PushSpeedMultiplier = 1f / Mathf.Max(resistance, 1f);
    }
}
