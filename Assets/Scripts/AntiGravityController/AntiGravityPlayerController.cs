using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using AntigravityGrab;

namespace Assets.Scripts.AntiGravityController
{
    [RequireComponent(typeof(CharacterController))]
    public class AntiGravityPlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private TMP_Text TMPPlayerState;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private AntigravityGrabber antigravityGrabber;

        // Components
        public CharacterController CharacterController { get; private set; }
        public Animator Animator => playerAnimator;
        public Transform CameraTransform => cameraTransform;
        public PlayerConfig Config => config;

        // State Machine
        private PlayerBaseState _currentState;
        private PlayerStateFactory _states;

        // Context Data
        public PlayerContext Context { get; private set; } = new PlayerContext();

        // Public properties for States to access
        public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }

        void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            _states = new PlayerStateFactory(this);

            if (playerAnimator == null)
                playerAnimator = GetComponent<Animator>();
            
            if (playerAnimator != null)
                playerAnimator.applyRootMotion = false; // Ensure code drives movement
            
            if (antigravityGrabber == null) antigravityGrabber = GetComponent<AntigravityGrabber>();
        }

        void Start()
        {
            _currentState = _states.Grounded(); // Start in Grounded super-state
            _currentState.EnterState();

            Debug.Log($"Config Check - WalkSpeed: {config.WalkSpeed}, Acceleration: {config.Acceleration}, Deceleration: {config.Deceleration}");
        }

        void Update()
        {
            Context.IsGrounded = CharacterController.isGrounded;
            _currentState.UpdateStates();

            if (TMPPlayerState != null)
                TMPPlayerState.text = $"Grounded: {CharacterController.isGrounded} / State: " + _currentState.ToString();
            
            // Debug State Hierarchy
            string stateChain = _currentState.ToString();
            if (_currentState.CurrentSubState != null) stateChain += " > " + _currentState.CurrentSubState.ToString();
            // Debug.Log($"State: {stateChain} | Input: {Context.MoveInput}");
            
            // Update Animator Speed for Blend Trees
            if (Animator != null)
            {
                Vector3 horizontalVelocity = new Vector3(CharacterController.velocity.x, 0, CharacterController.velocity.z);
                float speed = horizontalVelocity.magnitude;
                Animator.SetFloat("Speed", speed);
                
                // Debug Animator
                // Debug.Log($"Anim Speed: {speed:F2} | IsFalling: {Animator.GetBool("IsFalling")} | IsGrounded: {Animator.GetBool("IsGrounded")}");
            }

            ApplyMovement();
        }

        private void ApplyMovement()
        {
            if (CharacterController.enabled)
            {
                Vector3 movement = Context.Velocity * Time.deltaTime;

                if (antigravityGrabber != null && antigravityGrabber.IsGrabbing)
                {
                    if (!antigravityGrabber.CheckMove(movement))
                    {
                        movement = Vector3.zero;
                        Context.Velocity = Vector3.zero;
                    }
                }

                CharacterController.Move(movement);

                if (antigravityGrabber != null && antigravityGrabber.IsGrabbing)
                {
                    antigravityGrabber.UpdateObjectPosition();
                }
                // Debug.Log($"Velocity: {Context.Velocity}, Grounded: {CharacterController.isGrounded}");
            }
        }

        // Grab State
        private float _pushSpeedMultiplier = 1f;
        private float _turnSmoothVelocity; // For smooth rotation

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
            Vector2 inputDir = Context.MoveInput;
            // Debug.Log($"MoveInput: {inputDir}"); // Uncomment to debug input

            if (antigravityGrabber != null && antigravityGrabber.IsGrabbing && antigravityGrabber.GrabbedObjectTransform != null)
            {
                // Grab Movement Logic: Orbit around object
                
                // 1. Rotation (Input X)
                float rotationInput = inputDir.x;
                if (Mathf.Abs(rotationInput) > 0.01f)
                {
                    // Calculate radius (distance from player to object pivot)
                    float radius = Vector3.Distance(transform.position, antigravityGrabber.GrabbedObjectTransform.position);
                    
                    // Calculate linear speed (affected by resistance)
                    float linearSpeed = targetSpeed * _pushSpeedMultiplier;
                    
                    // Calculate angular speed (v = w * r  =>  w = v / r)
                    // We use Mathf.Max(radius, 0.5f) to avoid super fast rotation if very close
                    float angularSpeed = (linearSpeed / Mathf.Max(radius, 0.5f)) * Mathf.Rad2Deg;
                    
                    // Calculate rotation amount (Inverted direction as requested)
                    // Calculate rotation amount (Inverted direction as requested -> Fixed: removed negative sign)
                    float rotationAmount = rotationInput * angularSpeed * Time.deltaTime;

                    // Validate Rotation and get effective pivot (handling collisions)
                    antigravityGrabber.ValidateRotation(rotationAmount, transform.position, out float allowedAngle, out Vector3 effectivePivot);
                    
                    // If effectivePivot is zero (default return when not grabbing), fallback to object position
                    if (effectivePivot == Vector3.zero) effectivePivot = antigravityGrabber.GrabbedObjectTransform.position;

                    // Calculate new position after rotation around effective pivot
                    Vector3 dir = transform.position - effectivePivot;
                    Quaternion rotQ = Quaternion.Euler(0, allowedAngle, 0);
                    Vector3 newDir = rotQ * dir;
                    Vector3 targetPos = effectivePivot + newDir;
                    
                    // Calculate target rotation
                    Quaternion targetRot = transform.rotation * rotQ;

                    // FINAL SAFETY CHECK: Does this configuration cause the box to penetrate?
                    if (antigravityGrabber.IsConfigurationValid(targetPos, targetRot))
                    {
                        // Apply rotation to player
                        transform.rotation = targetRot;
                        
                        // Calculate velocity needed to reach targetPos
                        Vector3 tangentialVelocity = (targetPos - transform.position) / Time.deltaTime;
                        
                        // 2. Forward/Backward (Input Y)
                        float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
                        Vector3 forwardVelocity = transform.forward * inputDir.y * finalTargetSpeed;
                        
                        // PRESERVE Y VELOCITY to maintain grounding/gravity
                        float existingYVelocity = Context.Velocity.y;
                        Context.Velocity = tangentialVelocity + forwardVelocity;
                        Context.Velocity = new Vector3(Context.Velocity.x, existingYVelocity, Context.Velocity.z);
                    }
                    else
                    {
                        // Collision detected! Block rotation.
                        // We still allow forward/backward movement if possible?
                         // For now, if rotation fails, we just don't rotate. 
                         // But we should still allow forward/back.
                        
                        float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
                        Vector3 forwardVelocity = transform.forward * inputDir.y * finalTargetSpeed;
                        // PRESERVE Y VELOCITY
                        Context.Velocity = new Vector3(forwardVelocity.x, Context.Velocity.y, forwardVelocity.z);
                    }
                }
                else
                {
                    // No rotation, just forward/back
                    float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;
                    
                    // Simple acceleration/deceleration
                    float currentSpeed = new Vector3(Context.Velocity.x, 0, Context.Velocity.z).magnitude;
                    float speed = Mathf.MoveTowards(currentSpeed, finalTargetSpeed, (finalTargetSpeed > currentSpeed ? config.Acceleration : config.Deceleration) * Time.deltaTime);
                    
                    Vector3 targetVelocity = transform.forward * inputDir.y * speed;
                    Context.Velocity = new Vector3(targetVelocity.x, Context.Velocity.y, targetVelocity.z);
                }
            }
            else
            {
                // Standard Movement Logic
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                forward.y = 0; right.y = 0;
                forward.Normalize(); right.Normalize();

                Vector3 moveDirection = (forward * inputDir.y + right * inputDir.x).normalized;

                if (moveDirection.magnitude > 0.1f)
                {
                    // Smooth Rotation
                    float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, Config.TurnSmoothTime);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }

                // Apply resistance multiplier (should be 1f if not grabbing)
                float finalTargetSpeed = targetSpeed * _pushSpeedMultiplier;

                // Simple acceleration/deceleration
                float currentSpeed = new Vector3(Context.Velocity.x, 0, Context.Velocity.z).magnitude;
                float speed = Mathf.MoveTowards(currentSpeed, finalTargetSpeed, (finalTargetSpeed > currentSpeed ? config.Acceleration : config.Deceleration) * Time.deltaTime);
                
                if (moveDirection.magnitude > 0.1f)
                {
                     Vector3 targetVelocity = moveDirection * speed;
                     Context.Velocity = new Vector3(targetVelocity.x, Context.Velocity.y, targetVelocity.z);
                }
                else
                {
                    // Deceleration
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
        public void OnMove(InputValue value) 
        {
            Context.MoveInput = value.Get<Vector2>();
            // Debug.Log($"OnMove received: {Context.MoveInput}");
        }
        public void OnLook(InputValue value) => Context.LookInput = value.Get<Vector2>() * config.LookSensitivity;
        public void OnSprint(InputValue value) => Context.IsSprinting = value.isPressed;
        public void OnJump(InputValue value) => Context.IsJumping = value.isPressed;
        public void OnCrouch(InputValue value) => Context.IsCrouching = value.isPressed;
        public void OnGrab(InputValue value)
        {
            Context.IsGrabbing = value.isPressed;
            if (antigravityGrabber != null)
            {
                if (value.isPressed)
                {
                    if (antigravityGrabber.TryGrab())
                    {
                        SetGrabState(true, antigravityGrabber.CurrentResistance);
                    }
                }
                else
                {
                    antigravityGrabber.ReleaseGrab();
                    SetGrabState(false, 1f);
                }
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

        public bool CheckForLedge()
        {
            if (_ledgeGrabCooldownTimer > 0)
            {
                _ledgeGrabCooldownTimer -= Time.deltaTime;
                return false;
            }

            if (Context.IsGrounded || Context.Velocity.y >= 0) return false;
            if (Context.Velocity.y > -0.3f) return false;

            Vector3 origin = transform.position + Vector3.up * config.LedgeGrabHeight;
            
            // Primary raycast
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, config.LedgeDetectionDistance))
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
            return false;
        }

        private bool IsValidLedgeEdge(Vector3 hitPoint, Vector3 hitNormal, out Vector3 topPoint)
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
        public void FinishLedgeClimb()
        {
            if (_currentState is PlayerLedgeClimbState climbState)
            {
                climbState.FinishClimb();
            }
        }
        #endregion
    }
}
