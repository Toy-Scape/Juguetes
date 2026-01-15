using Assets.Scripts.PlayerController;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private TMP_Text TMPPlayerState;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GrabInteractor grabInteractor;

    public CharacterController CharacterController { get; private set; }
    public Animator Animator => playerAnimator;
    public Transform CameraTransform => cameraTransform;
    public PlayerConfig Config => config;

    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    public PlayerContext Context { get; private set; } = new PlayerContext();
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

    private float grabSpeedSmooth = 0f;
    private float carrySpeedSmooth = 0f;
    private float grabBlendSpeed = 10f;
    private float carryBlendSpeed = 10f;

    private float _pushSpeedMultiplier = 1f;
    private float _turnSmoothVelocity;

    void Awake ()
    {
        CharacterController = GetComponent<CharacterController>();
        _states = new PlayerStateFactory(this);

        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        if (playerAnimator != null)
            playerAnimator.applyRootMotion = false;

        if (grabInteractor == null)
            grabInteractor = GetComponent<GrabInteractor>();
    }

    void Start ()
    {
        _currentState = _states.Grounded();
        _currentState.EnterState();
    }

    void Update ()
    {
        Context.IsGrounded = CharacterController.isGrounded;
        _currentState.UpdateStates();

        if (TMPPlayerState != null)
            TMPPlayerState.text = $"Grounded: {CharacterController.isGrounded} / State: " + _currentState.ToString();

        if (Animator != null)
        {
            Vector3 horizontalVelocity = new Vector3(CharacterController.velocity.x, 0, CharacterController.velocity.z);
            Animator.SetFloat("Speed", horizontalVelocity.magnitude);
        }

        ApplyMovement();
    }

    private void ApplyMovement ()
    {
        if (!CharacterController.enabled)
            return;

        Vector3 movement = Context.Velocity * Time.deltaTime;

        if (grabInteractor != null && grabInteractor.IsGrabbing)
        {
            if (!grabInteractor.CheckMove(movement))
            {
                movement = Vector3.zero;
                Context.Velocity = Vector3.zero;
            }
        }

        CharacterController.Move(movement);

        if (grabInteractor != null && grabInteractor.IsGrabbing)
            grabInteractor.UpdateObjectPosition();
    }

    public void SetGrabState (bool isGrabbing, float resistance, Transform target = null)
    {
        Context.IsGrabbing = isGrabbing;
        Context.GrabTarget = target;
        _pushSpeedMultiplier = isGrabbing ? (1f / Mathf.Max(resistance, 1f)) : 1f;
    }

    public void SetPickState (bool isPicking)
    {
        Context.IsPicking = isPicking;
        Animator.SetBool("IsPicking", isPicking);
    }

    public void HandleMovement (float targetSpeed)
    {
        Vector2 inputDir = Context.MoveInput;

        if (grabInteractor != null && grabInteractor.IsGrabbing && grabInteractor.GrabbedObjectTransform != null)
        {
            Animator.SetBool("IsGrabbing", true);

            float targetGrab = Mathf.Clamp(Context.MoveInput.y, -1f, 1f);
            grabSpeedSmooth = Mathf.Lerp(grabSpeedSmooth, targetGrab, Time.deltaTime * grabBlendSpeed);
            Animator.SetFloat("GrabSpeed", grabSpeedSmooth);

            float rotationInput = inputDir.x;

            if (Mathf.Abs(rotationInput) > 0.01f)
            {
                float radius = Vector3.Distance(transform.position, grabInteractor.GrabbedObjectTransform.position);
                float linearSpeed = targetSpeed * _pushSpeedMultiplier;
                float angularSpeed = (linearSpeed / Mathf.Max(radius, 0.5f)) * Mathf.Rad2Deg;
                float rotationAmount = rotationInput * angularSpeed * Time.deltaTime;

                grabInteractor.ValidateRotation(rotationAmount, transform.position, out float allowedAngle, out Vector3 effectivePivot);
                if (effectivePivot == Vector3.zero)
                    effectivePivot = grabInteractor.GrabbedObjectTransform.position;

                Vector3 dir = transform.position - effectivePivot;
                Quaternion rotQ = Quaternion.Euler(0, allowedAngle, 0);
                Vector3 newDir = rotQ * dir;
                Vector3 targetPos = effectivePivot + newDir;
                Quaternion targetRot = transform.rotation * rotQ;

                if (grabInteractor.IsConfigurationValid(targetPos, targetRot))
                {
                    transform.rotation = targetRot;

                    Vector3 tangentialVelocity = (targetPos - transform.position) / Time.deltaTime;
                    float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
                    Vector3 forwardVelocity = transform.forward * inputDir.y * finalTargetSpeed;

                    float existingYVelocity = Context.Velocity.y;
                    Context.Velocity = tangentialVelocity + forwardVelocity;
                    Context.Velocity = new Vector3(Context.Velocity.x, existingYVelocity, Context.Velocity.z);
                }
                else
                {
                    float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
                    Vector3 forwardVelocity = transform.forward * inputDir.y * finalTargetSpeed;
                    Context.Velocity = new Vector3(forwardVelocity.x, Context.Velocity.y, forwardVelocity.z);
                }
            }
            else
            {
                float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
                float currentSpeed = new Vector3(Context.Velocity.x, 0, Context.Velocity.z).magnitude;
                float speed = Mathf.MoveTowards(currentSpeed, finalTargetSpeed, (finalTargetSpeed > currentSpeed ? config.Acceleration : config.Deceleration) * Time.deltaTime);

                Vector3 targetVelocity = transform.forward * inputDir.y * speed;
                Context.Velocity = new Vector3(targetVelocity.x, Context.Velocity.y, targetVelocity.z);
            }
        }
        else
        {
            Animator.SetBool("IsGrabbing", false);
            grabSpeedSmooth = Mathf.Lerp(grabSpeedSmooth, 0f, Time.deltaTime * grabBlendSpeed);
            Animator.SetFloat("GrabSpeed", grabSpeedSmooth);

            if (grabInteractor != null && grabInteractor.IsPicking)
            {
                Animator.SetBool("IsPicking", true);

                float carrySpeed = Mathf.Clamp01(new Vector3(Context.Velocity.x, 0, Context.Velocity.z).magnitude / config.SprintSpeed);
                float targetCarry = carrySpeed;

                carrySpeedSmooth = Mathf.Lerp(carrySpeedSmooth, targetCarry, Time.deltaTime * carryBlendSpeed);
                Animator.SetFloat("CarrySpeed", carrySpeedSmooth);
            }
            else
            {
                Animator.SetBool("IsPicking", false);
                carrySpeedSmooth = Mathf.Lerp(carrySpeedSmooth, 0f, Time.deltaTime * carryBlendSpeed);
                Animator.SetFloat("CarrySpeed", carrySpeedSmooth);
            }

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0; right.y = 0;
            forward.Normalize(); right.Normalize();

            Vector3 moveDirection = (forward * inputDir.y + right * inputDir.x).normalized;

            if (moveDirection.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, Config.TurnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
            float currentSpeed = new Vector3(Context.Velocity.x, 0, Context.Velocity.z).magnitude;
            float speed = Mathf.MoveTowards(currentSpeed, finalTargetSpeed, (finalTargetSpeed > currentSpeed ? config.Acceleration : config.Deceleration) * Time.deltaTime);

            if (moveDirection.magnitude > 0.1f)
            {
                Vector3 targetVelocity = moveDirection * speed;
                Context.Velocity = new Vector3(targetVelocity.x, Context.Velocity.y, targetVelocity.z);
            }
            else
            {
                Vector3 horizontalVelocity = new Vector3(Context.Velocity.x, 0, Context.Velocity.z);
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, config.Deceleration * Time.deltaTime);
                Context.Velocity = new Vector3(horizontalVelocity.x, Context.Velocity.y, horizontalVelocity.z);
            }
        }
    }

    public void ClimbEnd ()
    {
        FinishLedgeClimb();
    }

    #region Input Handling
    public void OnMove (InputValue value) => Context.MoveInput = value.Get<Vector2>();
    public void OnLook (InputValue value) => Context.LookInput = value.Get<Vector2>() * config.LookSensitivity;
    public void OnSprint (InputValue value) => Context.IsSprinting = value.isPressed;
    public void OnJump (InputValue value) => Context.IsJumping = value.isPressed;
    public void OnCrouch (InputValue value) => Context.IsCrouching = value.isPressed;

    public void OnGrab (InputValue value)
    {
        Context.IsGrabbing = value.isPressed;

        if (grabInteractor == null)
            return;

        if (!value.isPressed)
        {
            if (grabInteractor.IsGrabbing)
            {
                grabInteractor.ReleaseGrab();
                SetGrabState(false, 1f);
            }
            return;
        }

        if (grabInteractor.IsPicking)
        {
            grabInteractor.DropPicked();
            SetPickState(grabInteractor.IsPicking);
            return;
        }

        if (grabInteractor.TryPick())
        {
            SetPickState(grabInteractor.IsPicking);
        }
        else if (grabInteractor.TryGrab())
        {
            SetGrabState(true, grabInteractor.CurrentResistance, grabInteractor.GrabbedObjectTransform);
        }
    }
    #endregion

    #region Ledge Detection
    public Vector3 LedgePosition { get; private set; }
    public Vector3 LedgeNormal { get; private set; }
    private float _ledgeGrabCooldownTimer = 0f;

    public void SetLedgeGrabCooldown (float duration)
    {
        _ledgeGrabCooldownTimer = duration;
    }

    public bool CheckForLedge ()
    {
        if (Context.IsPicking) return false;

        if (_ledgeGrabCooldownTimer > 0)
        {
            _ledgeGrabCooldownTimer -= Time.deltaTime;
            return false;
        }

        if (Context.IsGrounded || Context.Velocity.y >= 0) return false;
        if (Context.Velocity.y > -0.3f) return false;

        Vector3 origin = transform.position + Vector3.up * config.LedgeGrabHeight;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, config.LedgeDetectionDistance))
        {
            if (hit.collider.GetComponent<LedgeGrabSurface>() != null)
            {
                float heightDifference = hit.point.y - transform.position.y;
                if (heightDifference < 0.5f || heightDifference > 2.5f) return false;

                if (IsValidLedgeEdge(hit.point, hit.normal, out Vector3 topPoint))
                {
                    LedgePosition = new Vector3(hit.point.x, topPoint.y, hit.point.z);
                    LedgeNormal = hit.normal;
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsValidLedgeEdge (Vector3 hitPoint, Vector3 hitNormal, out Vector3 topPoint)
    {
        topPoint = Vector3.zero;
        Vector3 checkAbove = hitPoint + Vector3.up * 0.3f - hitNormal * 0.1f;
        if (Physics.Raycast(checkAbove, Vector3.up, 0.5f)) return false;

        Vector3 checkTop = hitPoint + Vector3.up * 0.1f - hitNormal * 0.2f;
        if (Physics.Raycast(checkTop, Vector3.down, out RaycastHit topHit, 0.3f))
        {
            if (Vector3.Dot(topHit.normal, Vector3.up) > 0.7f)
            {
                topPoint = topHit.point;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Animation Events
    public void FinishLedgeClimb ()
    {
        if (_currentState is PlayerLedgeClimbState climbState)
        {
            climbState.FinishClimb();
        }
    }
    #endregion
}
