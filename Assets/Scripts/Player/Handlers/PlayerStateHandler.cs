using UnityEngine;

public class PlayerStateHandler
{
    private readonly StateConfig config;

    public PlayerStateHandler (StateConfig config)
    {
        this.config = config;
    }

    public PlayerState EvaluateState (PlayerContext playerContext)
    {
        if (playerContext.IsInWater)
            return playerContext.IsCrouching ? PlayerState.Diving : PlayerState.Swimming;

        if (!playerContext.IsGrounded && !playerContext.IsCrouching)
        {
            if (playerContext.Velocity.y > 0) return PlayerState.Jumping;
            else if (playerContext.Velocity.y < 0) return PlayerState.Falling;
        }

        if (playerContext.IsCrouching) return PlayerState.Crouching;
        if (playerContext.IsSprinting) return PlayerState.Sprinting;
        if (playerContext.MoveInput.magnitude > config.minMoveThreshold) return PlayerState.Walking;

        return PlayerState.Idle;
    }
}
