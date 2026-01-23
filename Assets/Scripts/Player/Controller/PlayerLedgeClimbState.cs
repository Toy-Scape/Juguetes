using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerLedgeClimbState : PlayerBaseState
    {

        public PlayerLedgeClimbState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsClimbing", true);
            _ctx.CharacterController.enabled = false;
            _ctx.Animator.applyRootMotion = false; 
        }

        public override void UpdateState()
        {
           
        }

        public void FinishClimb()
        {
            Vector3 targetXZ = _ctx.LedgePosition + (-_ctx.LedgeNormal * _ctx.Config.LedgeForwardOffset);
            Vector3 finalPos = targetXZ;

            if (Physics.Raycast(targetXZ + Vector3.up * 1.0f, Vector3.down, out RaycastHit hit, 2.0f))
            {
                finalPos = hit.point; 
            }
            else
            {
                finalPos += Vector3.up * 0.1f; 
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
            _ctx.Animator.applyRootMotion = false; 
            
            if (_ctx.CharacterController.enabled)
            {
                _ctx.CharacterController.Move(Vector3.down * 0.1f);
            }
        }

        public override void CheckSwitchStates() { } 

        public override void InitializeSubState() { }
    }
}
