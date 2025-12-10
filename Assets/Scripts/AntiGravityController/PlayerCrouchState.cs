using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(AntiGravityPlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsCrouching", true);
            _ctx.CharacterController.height = _ctx.Config.CrouchingHeight;
            _ctx.CharacterController.center = new Vector3(0, _ctx.Config.CrouchingHeight / 2, 0);
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.HandleMovement(_ctx.Config.CrouchSpeed);
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsCrouching", false);
            _ctx.CharacterController.height = _ctx.Config.StandingHeight;
            _ctx.CharacterController.center = new Vector3(0, _ctx.Config.StandingHeight / 2, 0);
        }

        public override void CheckSwitchStates()
        {
            if (!_ctx.Context.IsCrouching)
            {
                // Check overhead obstruction before standing up? (Optional for now)
                SwitchState(_factory.Idle());
            }
        }

        public override void InitializeSubState() { }
    }
}
