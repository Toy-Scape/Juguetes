using UnityEngine;

public class PlayerMovementHandler
{
    private readonly MovementConfig config;
    private readonly Transform cameraTransform;

    private Vector3 moveDirection;
    private float currentSpeed;
    private float lastGroundSpeed;

    public PlayerMovementHandler (MovementConfig config, Transform cameraTransform)
    {
        this.config = config;
        this.cameraTransform = cameraTransform;
        lastGroundSpeed = config.walkSpeed;
    }

    public void HandleMovement (PlayerState state, Vector2 inputDir, Transform playerTransform, PlayerContext playerContext)
    {
        // Dirección relativa a la cámara
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        moveDirection = (forward * inputDir.y + right * inputDir.x).normalized;

        // Velocidad según estado
        switch (state)
        {
            case PlayerState.Walking:
                currentSpeed = config.walkSpeed;
                break;
            case PlayerState.Sprinting:
                currentSpeed = config.sprintSpeed;
                break;
            case PlayerState.Crouching:
                currentSpeed = config.crouchSpeed;
                break;
            case PlayerState.Swimming:
                currentSpeed = config.swimSpeed;
                break;
            case PlayerState.Diving:
                currentSpeed = config.diveSpeed;
                break;
            case PlayerState.WallWalking:
                currentSpeed = config.wallWalkSpeed; // nuevo valor en MovementConfig
                break;
            case PlayerState.Jumping:
            case PlayerState.Falling:
                currentSpeed = lastGroundSpeed;
                break;
            default:
                currentSpeed = config.walkSpeed;
                break;
        }

        if (state == PlayerState.Walking || state == PlayerState.Sprinting || state == PlayerState.Crouching)
            lastGroundSpeed = currentSpeed;

        // Ajuste de dirección si está en pared
        if (state == PlayerState.WallWalking && playerContext.IsOnWall)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, playerContext.WallNormal).normalized;
        }

        // Rotación del jugador
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation;

            if (state == PlayerState.WallWalking && playerContext.IsOnWall)
            {
                // Orienta al jugador con la normal de la pared como "up"
                targetRotation = Quaternion.LookRotation(moveDirection, -playerContext.WallNormal);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(moveDirection);
            }

            playerTransform.rotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                config.rotationSpeed * Time.deltaTime
            );
        }
    }

    public Vector3 GetFinalMove (Vector3 velocity)
    {
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        return finalMove;
    }
}
