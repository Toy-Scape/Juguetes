using UnityEngine;

public class PlayerCrouchHandler
{
    private PlayerController player;
    private CharacterController controller;

    public PlayerCrouchHandler(PlayerController player, CharacterController controller)
    {
        Debug.Log("PlayerCrouchHandler initialized.");
        this.player = player;
        this.controller = controller;
    }

    public void HandleCrouch(PlayerState state)
    {
        float targetHeight = state == PlayerState.Crouching ? player.crouchingHeight : player.standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);
    }
}
