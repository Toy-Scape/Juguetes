using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerFallState : PlayerBaseState
    {
        public PlayerFallState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsFalling", true);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.HandleMovement(_ctx.Config.WalkSpeed); // Air control
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsFalling", false);
        }

        public override void CheckSwitchStates ()
        {
            if (_ctx.Context.Velocity.y == Mathf.Epsilon && !_ctx.Context.IsGrounded)
            {
                SwitchState(_factory.LedgeGrab());
            }
        }


        public override void InitializeSubState() { }
    }
}
