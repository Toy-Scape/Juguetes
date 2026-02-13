using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.PlayerController;
using CinematicSystem.Application;
using CinematicSystem.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlayerConfig config;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Unity.Cinemachine.CinemachineCamera gameplayCamera;
    [SerializeField] private Unity.Cinemachine.CinemachineCamera crouchingCamera;
    [SerializeField] private TMP_Text TMPPlayerState;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GrabInteractor grabInteractor;

    [SerializeField] private CinematicAsset cinematic;



    public CharacterController CharacterController { get; private set; }
    public Animator Animator => playerAnimator;
    public Transform CameraTransform => cameraTransform;
    public PlayerConfig Config => config;
    public PlayerMovementAudioSystem AudioSystem { get; private set; }


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

    private PlayerInput _playerInput;

    void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
        AudioSystem = GetComponent<PlayerMovementAudioSystem>();

        _states = new PlayerStateFactory(this);

        // Try to get PlayerInput component
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null) _playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerAnimator == null)
            playerAnimator = GetComponent<Animator>();

        if (playerAnimator != null)
            playerAnimator.applyRootMotion = false;

        if (grabInteractor == null)
            grabInteractor = GetComponent<GrabInteractor>();
    }

    void Start()
    {
        _currentState = _states.Grounded();
        _currentState.EnterState();
    }

    void Update()
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

    private void ApplyMovement()
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

    public void SetGrabState(bool isGrabbing, float resistance, Transform target = null)
    {
        Context.IsGrabbing = isGrabbing;
        Context.GrabTarget = target;
        _pushSpeedMultiplier = isGrabbing ? (1f / Mathf.Max(resistance, 1f)) : 1f;
    }

    public void SetPickState(bool isPicking)
    {
        Context.IsPicking = isPicking;
        Animator.SetBool("IsPicking", isPicking);
    }

    public void HandleMovement(float targetSpeed)
    {
        if (!CharacterController.enabled) return;

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

                grabInteractor.ValidateRotation(rotationAmount, transform.position, out float allowedAngle, out Vector3 effectivePivot, true);
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

    public void ClimbEnd()
    {
        FinishLedgeClimb();
    }

    #region Input Handling
    public void OnMove(InputValue value) => Context.MoveInput = value.Get<Vector2>();

    public void OnLook(InputValue value)
    {
        float sensitivityMultiplier = 1f;
        string scheme = "None";

        if (_playerInput != null)
        {
            scheme = _playerInput.currentControlScheme;
            if (scheme == "Keyboard&Mouse")
                sensitivityMultiplier = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            else if (scheme == "Gamepad" || scheme == "Joystick")
                sensitivityMultiplier = PlayerPrefs.GetFloat("GamepadSensitivity", 1f);
        }

        Vector2 rawInput = value.Get<Vector2>();
        Context.LookInput = rawInput * config.LookSensitivity * sensitivityMultiplier;
    }
    public void OnSprint(InputValue value) => Context.IsSprinting = value.isPressed;
    public void OnJump(InputValue value) {
        Context.IsJumping = value.isPressed && !Context.IsCrouching && (CurrentState is PlayerLedgeGrabState || CanStand()); 
    }
    public void OnCrouch(InputValue value) => Context.IsCrouching = value.isPressed;
    public void OnSprintToggle(InputValue value) => Context.IsSprinting = !Context.IsSprinting;
    public void OnCrouchToggle(InputValue value) => Context.IsCrouching = !Context.IsCrouching;


    public void OnGrab(InputValue value)
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
        else if (Context.IsGrounded && grabInteractor.TryGrab())
        {
            SetGrabState(true, grabInteractor.CurrentResistance, grabInteractor.GrabbedObjectTransform);
        }
    }
    #endregion

    #region Ledge Detection
    public Vector3 LedgePosition { get; private set; }
    public Vector3 LedgeNormal { get; private set; }
    private float _ledgeGrabCooldownTimer = 0f;

    public void SetLedgeGrabCooldown(float duration)
    {
        _ledgeGrabCooldownTimer = duration;
    }

    

    
                private Unity.Cinemachine.CinemachineOrbitalFollow _orbitalFollow;
    private float _originalTopHeight;
    private float _originalMiddleHeight;
    private float _originalBottomHeight;
    private bool _orbitsCached = false;

    public void SetCameraHeightOffset(float yOffset)
    {
        if (_orbitalFollow == null)
        {
            if (gameplayCamera != null)
            {
                _orbitalFollow = gameplayCamera.GetComponent<Unity.Cinemachine.CinemachineOrbitalFollow>();
                if (_orbitalFollow == null) Debug.LogWarning("Gameplay Camera assigned but no CinemachineOrbitalFollow found!");
            }
            
            if (_orbitalFollow == null)
            {
                var vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
                if (vcam != null) _orbitalFollow = vcam.GetComponent<Unity.Cinemachine.CinemachineOrbitalFollow>();
            }
        }
        
        if (_orbitalFollow == null) return;
        
        // Cache original heights
        if (!_orbitsCached)
        {
            try 
            {
                _originalTopHeight = _orbitalFollow.Orbits.Top.Height;
                _originalMiddleHeight = _orbitalFollow.Orbits.Center.Height;
                _originalBottomHeight = _orbitalFollow.Orbits.Bottom.Height;
                _orbitsCached = true;
                Debug.Log($"Cached camera orbits: Top={_originalTopHeight}, Mid={_originalMiddleHeight}, Bot={_originalBottomHeight}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SetCameraHeightOffset Error Caching: {e.Message}");
                return;
            }
        }

        // Modify struct and assign back
        try
        {
            var orbits = _orbitalFollow.Orbits;
            orbits.Top.Height = _originalTopHeight + yOffset;
            orbits.Center.Height = _originalMiddleHeight + yOffset;
            orbits.Bottom.Height = _originalBottomHeight + yOffset;
            _orbitalFollow.Orbits = orbits;
            
             Debug.Log($"Adjusted camera orbits by {yOffset}. New Heights: Top={orbits.Top.Height}, Mid={orbits.Center.Height}, Bot={orbits.Bottom.Height}");
        }
        catch (System.Exception e)
        {
             Debug.LogError($"SetCameraHeightOffset Error Modifying: {e.Message}");
        }
    }



    public bool CanStand()
    {
        float radius = CharacterController.radius;
        float currentHeight = config.CrouchingHeight;
        float targetHeight = config.StandingHeight;
        float heightDifference = targetHeight - currentHeight;
        
        // Start from top of crouched capsule
        Vector3 bottom = transform.position + CharacterController.center;
        Vector3 start = bottom + Vector3.up * (currentHeight / 2f);
        
        Debug.Log($"CanStand: pos={transform.position}, center={CharacterController.center}, start={start}, radius={radius}, checkDist={heightDifference}");
        Debug.DrawRay(start, Vector3.up * heightDifference, Color.red, 1f);
        
        // Check with SphereCast
        bool isBlocked = Physics.SphereCast(start, radius * 0.8f, Vector3.up, out RaycastHit hit, heightDifference, ~0, QueryTriggerInteraction.Ignore);
        
        if (isBlocked)
        {
            Debug.Log($"CanStand: BLOCKED by {hit.collider.name} at distance {hit.distance}");
            return false;
        }
        
        // Also try OverlapCapsule as backup
        Vector3 point1 = transform.position + Vector3.up * (currentHeight / 2f);
        Vector3 point2 = transform.position + Vector3.up * (targetHeight / 2f);
        Collider[] overlaps = Physics.OverlapCapsule(point1, point2, radius * 0.8f, ~0, QueryTriggerInteraction.Ignore);
        
        foreach (var overlap in overlaps)
        {
            if (overlap != CharacterController)
            {
                Debug.Log($"CanStand: BLOCKED by overlap with {overlap.name}");
                return false;
            }
        }
        
        Debug.Log("CanStand: CLEAR");
        return true;
    }

    public bool CheckForLedge(bool ignoreVerticalVelocity = false)
    {
        if (Context.IsPicking || Context.IsGrabbing) return false;

        if (_ledgeGrabCooldownTimer > 0)
        {
            _ledgeGrabCooldownTimer -= Time.deltaTime;
            return false;
        }

        if (!ignoreVerticalVelocity)
        {
            if (Context.IsGrounded || (Context.Velocity.y >= 0)) return false;
            if (Context.Velocity.y > -0.3f) return false;
        }

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

    private bool IsValidLedgeEdge(Vector3 hitPoint, Vector3 hitNormal, out Vector3 topPoint)
    {
        topPoint = Vector3.zero;
        Vector3 checkAbove = hitPoint + Vector3.up * 0.3f - hitNormal * 0.1f;
        if (Physics.Raycast(checkAbove, Vector3.up, 0.5f)) return false;

        Vector3 checkTop = hitPoint + Vector3.up * 1.0f - hitNormal * 0.2f;
        if (Physics.Raycast(checkTop, Vector3.down, out RaycastHit topHit, 1.5f))
        {
            if (Vector3.Dot(topHit.normal, Vector3.up) > 0.7f)
            {
                topPoint = topHit.point;
                return true;
            }
        }
        return false;
    }


    private Coroutine _freezeGrabbablesCoroutine;
    public void FreezeNearbyGrabbables(float duration)
    {
        if (_freezeGrabbablesCoroutine != null)
            StopCoroutine(_freezeGrabbablesCoroutine);

        _freezeGrabbablesCoroutine = StartCoroutine(FreezeGrabbablesCoroutine(duration));
    }
    private IEnumerator FreezeGrabbablesCoroutine(float duration)
    {
        float radius = 2f; // Radio de detecci�n
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        List<(Grabbable, bool)> frozenObjects = new List<(Grabbable, bool)>();

        foreach (var hit in hits)
        {
            var grabbable = hit.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                var rb = grabbable.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    frozenObjects.Add((grabbable, rb.isKinematic));

                    rb.isKinematic = true;
                }
            }
        }

        yield return new WaitForSeconds(duration);

        // Restaurar constraints
        foreach (var (grabbable, originalKinematic) in frozenObjects)
        {
            var rb = grabbable.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = originalKinematic;
            }
        }

        _freezeGrabbablesCoroutine = null;
    }
    #endregion

    #region Wall Climb Detection
    public Vector3 WallPosition { get; private set; }
    public Vector3 WallNormal { get; private set; }

    public bool CheckForWall()
    {
        if (Context.IsGrounded) return false;

        Vector3 origin = transform.position + Vector3.up * 1f; // Check from center/chest height

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, Config.WallClimbDetectionDistance))
        {
            var climbable = hit.collider.GetComponent<ClimbableWall>();
            if (climbable != null && climbable.CanBeClimbed())
            {
                WallPosition = hit.point;
                WallNormal = hit.normal;
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Animation Events
    public void FinishLedgeClimb()
    {
        if (_currentState is PlayerLedgeClimbState climbState)
        {
            climbState.FinishClimb();
        }
    }

    public void OnStandingAnimationEnter()
    {
        CharacterController.enabled = false;
        var CinematicPlayer = FindFirstObjectByType<CinematicPlayer>();
        if (CinematicPlayer != null)
        {
            CinematicPlayer.Play(cinematic);
        }
    }

    public void OnStandingAnimationFinished()
    {
        CharacterController.enabled = true;
        var CinematicPlayer = FindFirstObjectByType<CinematicPlayer>();
        if (CinematicPlayer != null)
        {
            CinematicPlayer.Stop();
        }
    }


    #endregion

    public void OnNextDialogue()
    {
        // Uno de los problemas de usar el input system con SendMessage/BroadcastMessage es que todos los eventos lanzados en el PlayerInput se envían a todos los componentes del objeto/hijos que lo contiene.
        // Esto significa que si en el PlayerInput hay un evento llamado "NextDialogue" y el objeto contenedor no tiene ningun script con ese método, se lanzará un error cada vez que se pulse el botón asignado a ese evento, incluso si el método existe en otro objeto o componente que sí lo necesita.
        // Lo dejo como una advertencia :)
        Debug.LogWarning("Hola :)");
    }
}
