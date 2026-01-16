using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsCrouching", true);

            float oldHeight = _ctx.Config.StandingHeight;
            float newHeight = _ctx.Config.CrouchingHeight;

            float delta = (oldHeight - newHeight) / 2f;

            _ctx.CharacterController.height = newHeight;
            _ctx.CharacterController.center -= new Vector3(0, delta, 0);

            _ctx.CharacterController.Move(Vector3.zero);
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

            float oldHeight = _ctx.Config.CrouchingHeight;
            float newHeight = _ctx.Config.StandingHeight;

            float delta = (newHeight - oldHeight) / 2f;

            _ctx.CharacterController.height = newHeight;
            _ctx.CharacterController.center += new Vector3(0, delta, 0);

            _ctx.CharacterController.Move(Vector3.zero);
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
