using UnityEngine;

public class PlayerCrouchHandler
{
    private readonly CrouchConfig config;

    public PlayerCrouchHandler(CrouchConfig config)
    {
        this.config = config;
    }

    public float GetTargetHeight(PlayerState state, float currentHeight)
    {
        float targetHeight = state == PlayerState.Crouching ? config.crouchingHeight : config.standingHeight;
        return Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * config.transitionSpeed);
    }
}
