using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerLedgeStandingState : PlayerBaseState
    {
        public PlayerLedgeStandingState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetTrigger("Stand");

            _ctx.CharacterController.enabled = false;
            _ctx.Animator.applyRootMotion = true;
            _ctx.enabled = false;
        }

        public override void UpdateState()
        {
            // Fallback: If animation is done and we haven't switched, switch to Grounded
            AnimatorStateInfo stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Standing") && stateInfo.normalizedTime >= 0.9f)
            {
                SwitchState(_factory.Grounded());
            }
        }

        public void FinishStanding()
        {
            // Position should already be set from the climb state
            // Just ensure we're ready to transition to grounded
            _ctx.FreezeNearbyGrabbables(0.4f);
            _ctx.Animator.applyRootMotion = false;
            _ctx.enabled = true;
            SwitchState(_factory.Grounded());
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.CharacterController.enabled = true;
            _ctx.Animator.applyRootMotion = false;
            _ctx.FreezeNearbyGrabbables(0.4f);

            if (_ctx.CharacterController.enabled)
            {
                // Force a small move down to ensure isGrounded updates immediately
                _ctx.CharacterController.Move(Vector3.down * 0.1f);
            }
        }

        public override void CheckSwitchStates() { } // No exit until standing finishes

        public override void InitializeSubState() { }
    }
}
