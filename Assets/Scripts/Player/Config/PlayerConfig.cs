using UnityEngine;

[System.Serializable]
public class PlayerConfig
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    public float swimSpeed = 3f;
    public float diveSpeed = 2f;

    [Header("Jump & Physics")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;

    [Header("LedgeGrab")]
    public float ledgeDetectionDistance = 0.5f;
    public float ledgeGrabHeight = 1f;
    public float ledgeClimbUpSpeed = 5f;
    public float ledgeSnapOffsetY = 0.5f;
    public float ledgeForwardOffset = 0.5f;
    public float ledgeGrabCooldown = 0.5f;
}
