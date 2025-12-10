using UnityEngine;

public class PlayerMovementHandler
{
    private readonly MovementConfig config;
    private readonly Transform cameraTransform;

    private Vector3 moveDirection;
    private Vector2 currentInputVector;
    private float currentSpeed;
    private float lastGroundSpeed;
    private float turnSmoothVelocity;

    public PlayerMovementHandler(MovementConfig config, Transform cameraTransform)
    {
        this.config = config;
        this.cameraTransform = cameraTransform;
        lastGroundSpeed = config.walkSpeed;
    }

    public void HandleMovement(PlayerState state, Vector2 inputDir, Transform playerTransform, PlayerContext playerContext)
    {
        // 1. Input Smoothing
        currentInputVector = Vector2.Lerp(currentInputVector, inputDir, Time.deltaTime * config.inputSmoothing);

        // Calculate target speed based on state
        float targetSpeed = 0f;
        // Use raw input magnitude for responsiveness in state checks, but smoothed for movement
        if (inputDir.magnitude > 0.01f)
        {
            switch (state)
            {
                case PlayerState.Walking: targetSpeed = config.walkSpeed; break;
                case PlayerState.Sprinting: targetSpeed = config.sprintSpeed; break;
                case PlayerState.Crouching: targetSpeed = config.crouchSpeed; break;
                case PlayerState.Swimming: targetSpeed = config.swimSpeed; break;
                case PlayerState.Diving: targetSpeed = config.diveSpeed; break;
                case PlayerState.WallWalking: targetSpeed = config.wallWalkSpeed; break;
                case PlayerState.Jumping:
                case PlayerState.Falling:
                    targetSpeed = lastGroundSpeed; // Keep momentum in air
                    break;
                default: targetSpeed = config.walkSpeed; break;
            }
        }

        // 2. Acceleration / Deceleration
        // Determine if we are accelerating or decelerating
        // If target speed is higher than current, we accelerate. If lower (or 0), we decelerate.
        // We also want to decelerate if we are changing direction significantly, but smooth turn handles that directionally.
        // Simple accel/decel based on magnitudes:

        bool isAccelerating = Mathf.Abs(targetSpeed) > currentSpeed;

        // Refined accel determination: 
        // If we have input and targetSpeed > current, accel.
        // If no input, decel.
        // If input matches current speed, maintain.

        float accelRate = config.deceleration;
        if (inputDir.magnitude > 0.01f && targetSpeed >= currentSpeed)
        {
            accelRate = config.acceleration;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelRate * Time.deltaTime);

        if (state == PlayerState.Walking || state == PlayerState.Sprinting || state == PlayerState.Crouching)
            lastGroundSpeed = currentSpeed;


        // Calculate Move Direction (Camera Relative) from SMOOTHED input
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        moveDirection = (forward * currentInputVector.y + right * currentInputVector.x).normalized;

        // Wall Adjustment
        if (state == PlayerState.WallWalking && playerContext.IsOnWall)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, playerContext.WallNormal).normalized;
        }

        // 3. Smooth Rotation
        if (playerContext.IsPushing)
        {
            currentSpeed *= playerContext.PushSpeedMultiplier;
            // Strafe rotation
            if (forward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(forward);
                playerTransform.rotation = Quaternion.RotateTowards(
                    playerTransform.rotation,
                    targetRotation,
                    config.rotationSpeed * playerContext.PushSpeedMultiplier * Time.deltaTime
                );
            }
        }
        else if (moveDirection.magnitude > 0.01f) // Rotate only if we have significant move direction
        {
            if (state == PlayerState.WallWalking && playerContext.IsOnWall)
            {
                // Wall rotation logic specific
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, -playerContext.WallNormal);
                playerTransform.rotation = Quaternion.RotateTowards(
                    playerTransform.rotation,
                    targetRotation,
                    config.rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                // SmoothDampAngle for standard movement to give it that "weight" and smooth turn
                // We use Atan2 to get the target angle from the move direction
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(playerTransform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, config.turnSmoothTime);
                playerTransform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }
    }

    public Vector3 GetFinalMove(Vector3 velocity)
    {
        // Apply currentSpeed to the direction
        // Note: moveDirection is normalized. 
        // If magnitude of input is 0, moveDirection might be zero? 
        // No, (0,0).normalized is (0,0,0).

        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        return finalMove;
    }
}
