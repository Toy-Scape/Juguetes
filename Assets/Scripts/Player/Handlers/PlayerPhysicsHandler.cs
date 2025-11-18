using UnityEngine;

public class PlayerPhysicsHandler
{
    private readonly PhysicsConfig config;

    public PlayerPhysicsHandler(PhysicsConfig config)
    {
        this.config = config;
    }

    public Vector3 ApplyGravity(PlayerState state, Vector3 velocity, bool isGrounded)
    {
        if (state == PlayerState.Swimming || state == PlayerState.Diving)
            return new Vector3(velocity.x, 0, velocity.z);

        if (isGrounded && velocity.y < 0)
            return new Vector3(velocity.x, -2f, velocity.z);

        return velocity + Vector3.up * config.gravity * Time.deltaTime;
    }
}
