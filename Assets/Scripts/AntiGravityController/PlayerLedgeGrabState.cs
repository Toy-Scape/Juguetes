using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerLedgeGrabState : PlayerBaseState
    {
        private Vector3 _targetPos;
        private Quaternion _targetRot;

        public PlayerLedgeGrabState(AntiGravityPlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsHanging", true);
            _ctx.Animator.SetBool("IsFalling", false); // Clear conflicting state
            _ctx.Animator.SetBool("IsJumping", false); // Clear conflicting state
            _ctx.Animator.SetBool("IsGrounded", false); // Clear conflicting state
            
            _ctx.Context.Velocity = Vector3.zero;
            _ctx.Context.IsGrabbingLedge = true;
            _ctx.CharacterController.enabled = false;

            _targetPos = new Vector3(_ctx.LedgePosition.x, _ctx.LedgePosition.y - _ctx.Config.LedgeSnapOffsetY, _ctx.LedgePosition.z);
            _targetRot = Quaternion.LookRotation(-_ctx.LedgeNormal, Vector3.up);
        }

        public override void UpdateState()
        {
            // Snap to ledge
            _ctx.transform.position = Vector3.Lerp(_ctx.transform.position, _targetPos, _ctx.Config.LedgeSnapSpeed * Time.deltaTime);
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, _targetRot, _ctx.Config.LedgeSnapSpeed * Time.deltaTime);

            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsHanging", false);
            _ctx.Context.IsGrabbingLedge = false;
            _ctx.CharacterController.enabled = true;

            // Force a small move to update isGrounded immediately after enabling
            if (_ctx.CharacterController.enabled)
            {
                _ctx.CharacterController.Move(Vector3.down * 0.01f);
            }
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsJumping)
            {
                SwitchState(_factory.LedgeClimb());
            }
            else if (_ctx.Context.IsCrouching)
            {
                _ctx.SetLedgeGrabCooldown(_ctx.Config.LedgeGrabCooldown);
                SwitchState(_factory.Air()); // Switch to Air to apply gravity immediately
            }
        }

        public override void InitializeSubState() { }
    }
}
