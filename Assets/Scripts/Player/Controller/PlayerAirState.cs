using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerAirState : PlayerBaseState
    {
        public PlayerAirState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Context.IsGrounded = false;
            InitializeSubState(); 
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            
            float gravity = _ctx.Config.Gravity;
            
            float previousYVelocity = _ctx.Context.Velocity.y;
            float newYVelocity = previousYVelocity + gravity * Time.deltaTime;
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f; 
            
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, nextYVelocity, _ctx.Context.Velocity.z);
        }

        public override void FixedUpdateState() { }

        public override void ExitState() { }

        public override void CheckSwitchStates()
        {
            if (_ctx.CharacterController.isGrounded && _ctx.Context.Velocity.y <= 0.1f)
            {
                SwitchState(_factory.Grounded());
            }
            else if (_ctx.CheckForLedge())
            {
                SwitchState(_factory.LedgeGrab());
            }
            else if (_ctx.CheckForWall() && _ctx.Context.MoveInput.y > 0.1f)
            {
                SwitchState(_factory.WallClimb());
            }
        }

        public override void InitializeSubState()
        {
            if (_ctx.Context.IsJumping) 
            {
                SetSubState(_factory.Jump());
            }
            else
            {
                SetSubState(_factory.Fall());
            }
        }
    }
}
