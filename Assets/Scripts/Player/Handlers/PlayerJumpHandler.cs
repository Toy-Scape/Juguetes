using UnityEngine;

public class PlayerJumpHandler
{
    private readonly JumpConfig config;

    public PlayerJumpHandler (JumpConfig config)
    {
        this.config = config;
    }

    public Vector3 HandleJump (PlayerState state, bool isJumping, Vector3 velocity, bool isInWater, bool isGrounded)
    {
        if (isJumping && isGrounded && !isInWater)
        {
            velocity.y = Mathf.Sqrt(config.jumpHeight * -2f * config.gravity);
        }
        else if (isJumping && isInWater)
        {
            if (state == PlayerState.Swimming)
                velocity.y = config.swimSpeed;
            else if (state == PlayerState.Diving)
                velocity.y = -config.diveSpeed;
        }
        return velocity;
    }
}
