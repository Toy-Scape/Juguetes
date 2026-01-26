using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerLedgeGrabState : PlayerBaseState
    {
        private Vector3 _targetPos;
        private Quaternion _targetRot;

        private bool _isClimbing = false;

        public PlayerLedgeGrabState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _isClimbing = false;
            _ctx.Animator.SetBool("IsHanging", true);
            _ctx.Animator.SetBool("IsFalling", false); 
            _ctx.Animator.SetBool("IsJumping", false); 
            _ctx.Animator.SetBool("IsGrounded", false);
            
            _ctx.Context.Velocity = Vector3.zero;
            _ctx.Context.IsGrabbingLedge = true;
            _ctx.CharacterController.enabled = false;

            _targetPos = new Vector3(_ctx.LedgePosition.x, _ctx.LedgePosition.y - _ctx.Config.LedgeSnapOffsetY, _ctx.LedgePosition.z);
            _targetRot = Quaternion.LookRotation(-_ctx.LedgeNormal, Vector3.up);
        }

        public override void UpdateState()
        {
            // Snap to ledge
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, _targetRot, _ctx.Config.LedgeSnapSpeed * Time.deltaTime);
            _ctx.transform.position = Vector3.Lerp(_ctx.transform.position, _targetPos, _ctx.Config.LedgeSnapSpeed * Time.deltaTime);

            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsHanging", false);
            _ctx.Context.IsGrabbingLedge = false;
            
            if (!_isClimbing)
            {
                _ctx.CharacterController.enabled = true;

                if (_ctx.CharacterController.enabled)
                {
                    _ctx.CharacterController.Move(Vector3.down * 0.01f);
                }
            }
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsCrouching)
            {
                _ctx.FreezeNearbyGrabbables(0.4f);

                _ctx.SetLedgeGrabCooldown(_ctx.Config.LedgeGrabCooldown);
                SwitchState(_factory.Air());
            }
            else if (_ctx.Context.IsJumping)
            {
                float dist = Vector3.Distance(_ctx.transform.position, _targetPos);
                if (dist < 0.1f)
                {
                    _isClimbing = true;
                    SwitchState(_factory.LedgeClimb());
                }
            }
        }

        public override void InitializeSubState() { }
    }
}
