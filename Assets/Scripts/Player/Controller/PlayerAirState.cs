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
            InitializeSubState(); // <--- CRITICAL FIX
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            
            // Apply Gravity
            float gravity = _ctx.Config.Gravity;
            // Optional: Stronger gravity when falling for better feel
            // if (_ctx.Context.Velocity.y < 0) gravity *= 1.5f;
            
            float previousYVelocity = _ctx.Context.Velocity.y;
            float newYVelocity = previousYVelocity + gravity * Time.deltaTime;
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f; // Verlet integration approximation
            
            _ctx.Context.Velocity = new Vector3(_ctx.Context.Velocity.x, nextYVelocity, _ctx.Context.Velocity.z);
        }

        public override void FixedUpdateState() { }

        public override void ExitState() { }

        public override void CheckSwitchStates()
        {
            // Debug.Log($"Air Check: Grounded={_ctx.CharacterController.isGrounded}, VelY={_ctx.Context.Velocity.y}");

            // Only switch to Grounded if we are falling or stable, NOT if we are moving up (Jumping)
            if (_ctx.CharacterController.isGrounded && _ctx.Context.Velocity.y <= 0.1f)
            {
                SwitchState(_factory.Grounded());
            }
            else if (_ctx.CheckForLedge())
            {
                SwitchState(_factory.LedgeGrab());
            }
            else if (_ctx.CheckForWall())
            {
                SwitchState(_factory.WallClimb());
            }
        }

        public override void InitializeSubState()
        {
            if (_ctx.Context.IsJumping) // If we entered air via Jump button (and are moving up)
            {
                // This logic is a bit tricky because IsJumping might be true from the frame before.
                // Usually Jump state handles the initial force.
                // If we are just falling (walked off ledge), go to Fall.
                SetSubState(_factory.Jump());
            }
            else
            {
                SetSubState(_factory.Fall());
            }
        }
    }
}
