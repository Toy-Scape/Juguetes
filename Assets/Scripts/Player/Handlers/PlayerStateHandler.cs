using UnityEngine;

public class PlayerStateHandler
{
    private PlayerController player;
    private CharacterController controller;
    private PlayerInputHandler input;

    public PlayerStateHandler(PlayerController player, CharacterController controller, PlayerInputHandler input)
    {
        Debug.Log("PlayerStateHandler initialized.");

        this.player = player;
        this.controller = controller;
        this.input = input;
    }

    public PlayerState EvaluateState(Vector3 velocity)
    {
        if (player.IsInWater)
            return input.IsCrouching ? PlayerState.Diving : PlayerState.Swimming;

        if (!controller.isGrounded && !input.IsCrouching)
        {
            if (velocity.y > 0) return PlayerState.Jumping;
            else if (velocity.y < 0) return PlayerState.Falling;
        }

        if (input.IsCrouching) return PlayerState.Crouching;
        if (input.IsSprinting) return PlayerState.Sprinting;
        if (input.MoveInput.magnitude > 0.1f) return PlayerState.Walking;

        return PlayerState.Idle;
    }

}
