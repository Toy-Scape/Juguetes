using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public PlayerInputHandler input;
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    public float swimSpeed = 3f;
    public float diveSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;

    private CharacterController controller;
    private Vector3 velocity; 
    private bool isGrounded;
    private bool isInWater = false;


    [Header("Debugging Tools")]
    public TMP_Text TMPPlayerState;

    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (input == null) input = GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        HandleStateTransitions();
        HandleMovement();
        HandleJump();
        //HandleInteraction();
        HandleCrouch();
        ApplyGravity();

        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        controller.Move(finalMove * Time.deltaTime);

        if (TMPPlayerState != null)
            TMPPlayerState.text = "Current State: " + CurrentState.ToString();
    }

    private Vector3 moveDirection;
    private float currentSpeed;

    void HandleStateTransitions()
    {
        if (isInWater)
        {
            CurrentState = input.IsCrouching ? PlayerState.Diving : PlayerState.Swimming;
        }
        else if (!controller.isGrounded && !input.IsCrouching) 
        {
            if (velocity.y > 0)
                CurrentState = PlayerState.Jumping; 
            else if (velocity.y < 0)
                CurrentState = PlayerState.Falling; 
        }
        else
        {
            if (input.IsCrouching)
                CurrentState = PlayerState.Crouching;
            else if (input.IsSprinting)
                CurrentState = PlayerState.Sprinting;
            else if (input.MoveInput.magnitude > 0.1f)
                CurrentState = PlayerState.Walking;
            else
                CurrentState = PlayerState.Idle;
        }
    }


    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        Vector2 inputDir = input.MoveInput;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * inputDir.y + right * inputDir.x).normalized;

        currentSpeed = walkSpeed;
        switch (CurrentState)
        {
            case PlayerState.Walking: currentSpeed = walkSpeed; break;
            case PlayerState.Sprinting: currentSpeed = sprintSpeed; break;
            case PlayerState.Crouching: currentSpeed = crouchSpeed; break;
            case PlayerState.Swimming: currentSpeed = swimSpeed; break;
            case PlayerState.Diving: currentSpeed = diveSpeed; break;
        }

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        if (input.IsJumping && isGrounded && !isInWater)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (input.IsJumping && isInWater)
        {
            if (CurrentState == PlayerState.Swimming)
                velocity.y = swimSpeed;
            else if (CurrentState == PlayerState.Diving)
                velocity.y = -diveSpeed;
        }
    }

    void HandleCrouch()
    {
        if (CurrentState == PlayerState.Crouching)
        {
            controller.height = Mathf.Lerp(controller.height, crouchingHeight, Time.deltaTime * 10f);
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, standingHeight, Time.deltaTime * 10f);
        }
    }

    void ApplyGravity()
    {
        if (CurrentState == PlayerState.Swimming || CurrentState == PlayerState.Diving)
        {
            velocity.y = 0;
        }
        else
        {
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;
            else
                velocity.y += gravity * Time.deltaTime;
        }
    }

    void HandleLedgeGrab()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
            isInWater = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
            isInWater = false;
    }
}
