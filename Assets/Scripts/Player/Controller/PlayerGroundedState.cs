using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerGroundedState : PlayerBaseState
    {
        public PlayerGroundedState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Context.IsGrounded = true;
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, -2f, _ctx.Context.Velocity.z); 
            _ctx.Animator.SetBool("IsGrounded", true);
            _ctx.Animator.SetBool("IsFalling", false);
            
            InitializeSubState(); 
        }

        public override void UpdateState()
        {
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, -5f, _ctx.Context.Velocity.z);
            
            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsGrounded", false);
        }

        private float _ungroundedTimer = 0f;

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsJumping && !_ctx.Context.IsCrouching && !_ctx.Context.IsGrabbing)
            {
                SwitchState(_factory.Air());
            }
            else if (!_ctx.CharacterController.isGrounded)
            {
                _ungroundedTimer += Time.deltaTime;
                if (_ungroundedTimer > 0.1f) 
                {
                    SwitchState(_factory.Air());
                }
            }
            else
            {
                _ungroundedTimer = 0f; 
            }
        }

        public override void InitializeSubState()
        {
            if (_ctx.Context.IsCrouching)
            {
                SetSubState(_factory.Crouch());
            }
            else if (_ctx.Context.MoveInput != Vector2.zero)
            {
                if (_ctx.Context.IsSprinting)
                {
                    SetSubState(_factory.Run());
                }
                else
                {
                    SetSubState(_factory.Walk());
                }
            }
            else
            {
                SetSubState(_factory.Idle());
            }
        }
    }
}
