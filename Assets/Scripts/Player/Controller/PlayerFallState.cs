using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerFallState : PlayerBaseState
    {
        public PlayerFallState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        private float _airSpeed;

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsFalling", true);
            
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
