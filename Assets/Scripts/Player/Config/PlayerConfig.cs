using UnityEngine;

[System.Serializable]
public class PlayerConfig
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    public float WalkSpeed => walkSpeed;

    [SerializeField] private float sprintSpeed = 8f;
    public float SprintSpeed => sprintSpeed;

    [SerializeField] private float crouchSpeed = 2f;
    public float CrouchSpeed => crouchSpeed;

    [SerializeField] private float swimSpeed = 3f;
    public float SwimSpeed => swimSpeed;

    [SerializeField] private float diveSpeed = 2f;
    public float DiveSpeed => diveSpeed;

    [SerializeField] private float wallWalkSpeed = 3f;
    public float WallWalkSpeed => wallWalkSpeed;

    [SerializeField] private float rotationSpeed = 720f;
    public float RotationSpeed => rotationSpeed;

    [SerializeField] private float lookSensitivity = 1f;
    public float LookSensitivity => lookSensitivity;

    [Header("Smoothing & Feel")]
    [SerializeField] private float acceleration = 10f;
    public float Acceleration => acceleration;

    [SerializeField] private float deceleration = 10f;
    public float Deceleration => deceleration;

    [SerializeField] private float inputSmoothing = 10f;
    public float InputSmoothing => inputSmoothing;

    [SerializeField] private float turnSmoothTime = 0.1f;
    public float TurnSmoothTime => turnSmoothTime;

    [Header("Vibration Feedback")]
    [SerializeField] private Vector3 jumpVibration = new Vector3(0.02f, 0.05f, 0.1f); // Subtle
    public Vector3 JumpVibration => jumpVibration;

    [SerializeField] private Vector3 landVibration = new Vector3(0.15f, 0.05f, 0.15f); // Noticeable but not harsh
    public Vector3 LandVibration => landVibration;

    [SerializeField] private Vector3 interactVibration = new Vector3(0.05f, 0.05f, 0.1f); // Soft click
    public Vector3 InteractVibration => interactVibration;

    [SerializeField] private Vector3 dragVibration = new Vector3(0.1f, 0.0f, 0.0f); // Rumble, no time needed
    public Vector3 DragVibration => dragVibration;

    [SerializeField] private float minFallForceForFeedback = 8.0f; // Threshold for heavy landing
    public float MinFallForceForFeedback => minFallForceForFeedback;


    [Header("Jump & Physics")]
    [SerializeField] private float jumpHeight = 2f;
    public float JumpHeight => jumpHeight;

    [SerializeField] private float gravity = -9.81f;
    public float Gravity => gravity;

    [SerializeField] private float coyoteTime = 0.15f;
    public float CoyoteTime => coyoteTime;

    [SerializeField] private float jumpBufferTime = 0.1f;
    public float JumpBufferTime => jumpBufferTime;

    [SerializeField] private float standingHeight = 2f;
    public float StandingHeight => standingHeight;

    [SerializeField] private float crouchingHeight = 1f;
    public float CrouchingHeight => crouchingHeight;


    [Header("LedgeGrab")]
    [SerializeField] private float ledgeDetectionDistance = 0.5f;
    public float LedgeDetectionDistance => ledgeDetectionDistance;

    [SerializeField] private float ledgeGrabHeight = 1f;
    public float LedgeGrabHeight => ledgeGrabHeight;

    [SerializeField] private float ledgeClimbUpSpeed = 5f;
    public float LedgeClimbUpSpeed => ledgeClimbUpSpeed;

    [SerializeField] private float ledgeSnapSpeed = 10f;
    public float LedgeSnapSpeed => ledgeSnapSpeed;

    [SerializeField] private float ledgeSnapOffsetY = 0.5f;
    public float LedgeSnapOffsetY => ledgeSnapOffsetY;

    [SerializeField] private float ledgeForwardOffset = 0.5f;
    public float LedgeForwardOffset => ledgeForwardOffset;

    [SerializeField] private float ledgeGrabCooldown = 0.5f;
    public float LedgeGrabCooldown => ledgeGrabCooldown;
}
