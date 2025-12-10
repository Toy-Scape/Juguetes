using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(AntiGravityPlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            // Do NOT reset velocity to zero here. 
            // Let UpdateState -> HandleMovement(0f) handle the deceleration.
        }

        public override void UpdateState()
        {
            CheckSwitchStates();
            _ctx.HandleMovement(0f); // Apply deceleration
        }

        public override void FixedUpdateState() { }

        public override void ExitState() { }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsCrouching)
            {
                SwitchState(_factory.Crouch());
            }
            else if (_ctx.Context.MoveInput != Vector2.zero)
            {
                // Debug.Log($"Idle -> Walk/Run. Input: {_ctx.Context.MoveInput}");
                if (_ctx.Context.IsSprinting)
                {
                    SwitchState(_factory.Run());
                }
                else
                {
                    SwitchState(_factory.Walk());
                }
            }
            else
            {
                // Debug.Log("Idle: No Input");
            }
        }

        public override void InitializeSubState() { }
    }
}
