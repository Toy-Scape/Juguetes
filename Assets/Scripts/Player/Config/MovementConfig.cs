using UnityEngine;

public class MovementConfig
{
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float swimSpeed;
    public float diveSpeed;
    public float rotationSpeed;
    public float wallWalkSpeed;
    public float acceleration;
    public float deceleration;
    public float inputSmoothing;
    public float turnSmoothTime;

    public MovementConfig(float walkSpeed, float sprintSpeed, float crouchSpeed,
                          float swimSpeed, float diveSpeed, float wallWalkSpeed,
                          float rotationSpeed, float acceleration, float deceleration,
                          float inputSmoothing, float turnSmoothTime)
    {
        this.walkSpeed = walkSpeed;
        this.sprintSpeed = sprintSpeed;
        this.crouchSpeed = crouchSpeed;
        this.swimSpeed = swimSpeed;
        this.diveSpeed = diveSpeed;
        this.wallWalkSpeed = wallWalkSpeed;
        this.rotationSpeed = rotationSpeed;
        this.acceleration = acceleration;
        this.deceleration = deceleration;
        this.inputSmoothing = inputSmoothing;
        this.turnSmoothTime = turnSmoothTime;
    }
}