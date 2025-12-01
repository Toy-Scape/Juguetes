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


    [Header("Jump & Physics")]
    [SerializeField] private float jumpHeight = 2f;
    public float JumpHeight => jumpHeight;

    [SerializeField] private float gravity = -9.81f;
    public float Gravity => gravity;

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

    [SerializeField] private float ledgeSnapOffsetY = 0.5f;
    public float LedgeSnapOffsetY => ledgeSnapOffsetY;

    [SerializeField] private float ledgeForwardOffset = 0.5f;
    public float LedgeForwardOffset => ledgeForwardOffset;

    [SerializeField] private float ledgeGrabCooldown = 0.5f;
    public float LedgeGrabCooldown => ledgeGrabCooldown;
}
