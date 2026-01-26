using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerWalkState : PlayerBaseState
    {
        public PlayerWalkState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.HandleMovement(_ctx.Config.WalkSpeed);
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsCrouching)
            {
                SwitchState(_factory.Crouch());
            }
            else if (_ctx.Context.MoveInput == Vector2.zero)
            {
                SwitchState(_factory.Idle());
            }
            else if (_ctx.Context.IsSprinting)
            {
                SwitchState(_factory.Run());
            }
        }

        public override void InitializeSubState() { }
    }
}
