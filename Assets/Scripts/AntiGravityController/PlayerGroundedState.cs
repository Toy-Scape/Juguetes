using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerGroundedState : PlayerBaseState
    {
        public PlayerGroundedState(AntiGravityPlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Context.IsGrounded = true;
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, -2f, _ctx.Context.Velocity.z); // Stick to ground
            _ctx.Animator.SetBool("IsGrounded", true);
            _ctx.Animator.SetBool("IsFalling", false); // Ensure falling is off
            _ctx.Animator.SetBool("IsJumping", false); // Ensure jumping is off
            
            InitializeSubState(); // <--- CRITICAL FIX
        }

        public override void UpdateState()
        {
            // Force stick to ground every frame
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
                // Add tolerance (Coyote Time for falling) to prevent flickering on uneven ground
                _ungroundedTimer += Time.deltaTime;
                if (_ungroundedTimer > 0.1f) // 0.1s tolerance
                {
                    SwitchState(_factory.Air());
                }
            }
            else
            {
                _ungroundedTimer = 0f; // Reset timer if grounded
                
                // Crouch logic is handled by sub-states (Idle/Walk/Run -> Crouch)
                // But if we want to support direct transition from Grounded super-state logic:
                // For now, let's leave it to sub-states to handle crouch switching.
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
