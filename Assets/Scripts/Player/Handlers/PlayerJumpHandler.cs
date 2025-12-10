using UnityEngine;

public class PlayerJumpHandler
{
    private readonly JumpConfig config;

    public PlayerJumpHandler(JumpConfig config)
    {
        this.config = config;
    }

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    public void UpdateTimers(float deltaTime, bool isGrounded, bool jumpPressed)
    {
        if (isGrounded)
        {
            coyoteTimeCounter = config.coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= deltaTime;
        }

        if (jumpPressed)
        {
            jumpBufferCounter = config.jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= deltaTime;
        }
    }

    // Returns true if a jump was performed
    public bool HandleJump(PlayerState state, ref Vector3 velocity, bool isInWater, bool isGrounded, bool isPushing)
    {
        // Standard Jump with Coyote & Buffer
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isInWater && !isPushing)
        {
            velocity.y = Mathf.Sqrt(config.jumpHeight * -2f * config.gravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f; // Consume coyote time to prevent double jump
            return true;
        }
        // Water Jump Logic (Immediate)
        else if (jumpBufferCounter > 0f && isInWater)
        {
            if (state == PlayerState.Swimming)
                velocity.y = config.swimSpeed;
            else if (state == PlayerState.Diving)
                velocity.y = -config.diveSpeed;

            jumpBufferCounter = 0f;
            return true;
        }
        return false;
    }
}
