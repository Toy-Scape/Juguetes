using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerLedgeClimbState : PlayerBaseState
    {

        public PlayerLedgeClimbState(AntiGravityPlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsClimbing", true);
            _ctx.CharacterController.enabled = false;
            _ctx.Animator.applyRootMotion = false; // Prevent animation from moving the transform
        }

        public override void UpdateState()
        {
           
        }

        public void FinishClimb()
        {
            // Calculate target position (forward from ledge)
            Vector3 targetXZ = _ctx.LedgePosition + (-_ctx.LedgeNormal * _ctx.Config.LedgeForwardOffset);
            Vector3 finalPos = targetXZ;

            // Raycast to find exact floor height to prevent bouncing/falling
            if (Physics.Raycast(targetXZ + Vector3.up * 1.0f, Vector3.down, out RaycastHit hit, 2.0f))
            {
                finalPos = hit.point; // Snap exactly to floor
            }
            else
            {
                finalPos += Vector3.up * 0.1f; // Fallback
            }
            finalPos.y++;
            _ctx.transform.position = finalPos;
            
            SwitchState(_factory.Grounded());
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsClimbing", false);
            _ctx.CharacterController.enabled = true;
            _ctx.Animator.applyRootMotion = false; // Ensure code continues to drive movement
            
            if (_ctx.CharacterController.enabled)
            {
                // Force a significant move down to ensure isGrounded updates immediately
                _ctx.CharacterController.Move(Vector3.down * 0.1f);
            }
        }

        public override void CheckSwitchStates() { } // No exit until climb finishes

        public override void InitializeSubState() { }
    }
}
