using UnityEngine;

public class PlayerJumpHandler
{
    private PlayerController player;
    private CharacterController controller;

    public PlayerJumpHandler(PlayerController player, CharacterController controller)
    {
        Debug.Log("PlayerJumpHandler initialized.");
        this.player = player;
        this.controller = controller;
    }

    public void HandleJump(PlayerState state, bool isJumping)
    {
        if (isJumping && controller.isGrounded && !player.IsInWater)
        {
            player.Velocity = new Vector3(player.Velocity.x,
                Mathf.Sqrt(player.jumpHeight * -2f * player.gravity),
                player.Velocity.z);
        }
        else if (isJumping && player.IsInWater)
        {
            if (state == PlayerState.Swimming)
                player.Velocity = new Vector3(player.Velocity.x, player.swimSpeed, player.Velocity.z);
            else if (state == PlayerState.Diving)
                player.Velocity = new Vector3(player.Velocity.x, -player.diveSpeed, player.Velocity.z);
        }
    }
}
