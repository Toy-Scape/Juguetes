using UnityEngine;

public class PlayerStateHandler
{
    private readonly StateConfig config;
    private readonly PlayerInputHandler input;

    public PlayerStateHandler(StateConfig config, PlayerInputHandler input)
    {
        this.config = config;
        this.input = input;
    }

    public PlayerState EvaluateState(Vector3 velocity, bool isInWater, bool isGrounded)
    {
        if (isInWater)
            return input.IsCrouching ? PlayerState.Diving : PlayerState.Swimming;

        if (!isGrounded && !input.IsCrouching)
        {
            if (velocity.y > 0) return PlayerState.Jumping;
            else if (velocity.y < 0) return PlayerState.Falling;
        }

        if (input.IsCrouching) return PlayerState.Crouching;
        if (input.IsSprinting) return PlayerState.Sprinting;
        if (input.MoveInput.magnitude > config.minMoveThreshold) return PlayerState.Walking;

        return PlayerState.Idle;
    }
}
