using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsJumping", true);
            _ctx.Context.IsJumping = false; // Consume input
            
            // Apply Jump Force
            // v = sqrt(h * -2 * g)
            float jumpVelocity = Mathf.Sqrt(_ctx.Config.JumpHeight * -2f * _ctx.Config.Gravity);
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, jumpVelocity, _ctx.Context.Velocity.z);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.HandleMovement(_ctx.Config.WalkSpeed); // Air control
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsJumping", false);
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.Velocity.y < 0)
            {
                SwitchState(_factory.Fall());
            }
        }

        public override void InitializeSubState() { }
    }
}
