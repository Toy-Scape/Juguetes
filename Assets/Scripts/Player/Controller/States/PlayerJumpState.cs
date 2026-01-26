using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerJumpState : PlayerBaseState
    {
        public PlayerJumpState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        private float _airSpeed;

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsJumping", true);
            _ctx.Context.IsJumping = false;
            
            float jumpVelocity = Mathf.Sqrt(_ctx.Config.JumpHeight * -2f * _ctx.Config.Gravity);
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, jumpVelocity, _ctx.Context.Velocity.z);

            float currentLimit = new Vector3(_ctx.Context.Velocity.x, 0, _ctx.Context.Velocity.z).magnitude;
            _airSpeed = Mathf.Max(currentLimit, _ctx.Config.WalkSpeed);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.HandleMovement(_airSpeed);
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
