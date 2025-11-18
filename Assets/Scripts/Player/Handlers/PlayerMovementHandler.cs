using UnityEngine;

public class PlayerMovementHandler
{
    private PlayerController player;
    private CharacterController controller;
    private Transform cameraTransform;

    private Vector3 moveDirection;
    private float currentSpeed;

    public PlayerMovementHandler(PlayerController player, CharacterController controller, Transform cameraTransform)
    {
        Debug.Log("PlayerMovementHandler initialized.");
        this.player = player;
        this.controller = controller;
        this.cameraTransform = cameraTransform;
    }

    public void HandleMovement(PlayerState state, Vector2 inputDir, bool isSprinting)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        moveDirection = (forward * inputDir.y + right * inputDir.x).normalized;

        currentSpeed = state switch
        {
            PlayerState.Walking => player.walkSpeed,
            PlayerState.Sprinting => player.sprintSpeed,
            PlayerState.Crouching => player.crouchSpeed,
            PlayerState.Swimming => player.swimSpeed,
            PlayerState.Diving => player.diveSpeed,
            _ => player.walkSpeed
        };

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, targetRotation, 720f * Time.deltaTime);
        }
    }

    public void ApplyGravity(PlayerState state)
    {
        if (state == PlayerState.Swimming || state == PlayerState.Diving)
        {
            player.Velocity = new Vector3(player.Velocity.x, 0, player.Velocity.z);
        }
        else
        {
            if (controller.isGrounded && player.Velocity.y < 0)
                player.Velocity = new Vector3(player.Velocity.x, -2f, player.Velocity.z);
            else
                player.Velocity += Vector3.up * player.gravity * Time.deltaTime;
        }
    }

    public Vector3 GetFinalMove(Vector3 velocity)
    {
        Vector3 finalMove = moveDirection * currentSpeed;
        finalMove.y = velocity.y;
        return finalMove;
    }
}
