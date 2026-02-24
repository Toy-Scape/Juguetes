using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerLedgeClimbState : PlayerBaseState
    {

        public PlayerLedgeClimbState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetTrigger("Climb");

            if (_ctx.AudioSystem != null)
            {
                Debug.Log("PLAYING CLIMB SOUND");
                _ctx.AudioSystem.PlayClimbMovement();
            }
            else
            {
                Debug.LogError("AudioSystem NULL");
            }

            _ctx.CharacterController.enabled = false;
            _ctx.Animator.applyRootMotion = false;

            _ctx.Context.IsGrabbingLedge = false;
            _ctx.Context.IsWallClimbing = true;
        }

        public void FinishClimb()
        {
            var cc = _ctx.CharacterController;

            float radius = cc.radius;
            float height = cc.height;
            float skin = cc.skinWidth;

            float forwardOffset = _ctx.Config.LedgeForwardOffset + radius + 0.02f;
            float upOffset = height * 0.5f + 1f;

            Vector3 basePos = _ctx.LedgePosition + (-_ctx.LedgeNormal * forwardOffset);

            Vector3 rayStart = basePos + Vector3.up * upOffset;
            Vector3 finalPos = basePos;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, upOffset + 1f))
            {
                finalPos = hit.point;
                finalPos += Vector3.up * (skin + 1.02f);
                finalPos += -_ctx.LedgeNormal * 0.02f;
            }
            else
            {
                finalPos += Vector3.up * 0.1f;
            }

            _ctx.transform.position = finalPos;

            _ctx.FreezeNearbyGrabbables(0.4f);
            _ctx.Context.IsWallClimbing = false;
            SwitchState(_factory.Grounded());
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.CharacterController.enabled = true;
            _ctx.Animator.applyRootMotion = false;
            _ctx.FreezeNearbyGrabbables(0.4f);

            if (_ctx.CharacterController.enabled)
            {
                _ctx.CharacterController.Move(Vector3.down * 0.1f);
            }
        }

        public override void CheckSwitchStates() { }

        public override void InitializeSubState() { }

        public override void UpdateState() { }
    }
}
