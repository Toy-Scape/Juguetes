using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public class PlayerWallClimbState : PlayerBaseState
    {
        private Quaternion _targetRot;

        public PlayerWallClimbState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory) { }

        public override void EnterState()
        {
            _ctx.Animator.SetBool("IsWallClimbing", true);
            _ctx.Context.Velocity = Vector3.zero;
            _ctx.Context.IsWallClimbing = true;
            _ctx.CharacterController.enabled = false;
            
            // Snap rotation
            _targetRot = Quaternion.LookRotation(-_ctx.WallNormal, Vector3.up);
            _ctx.transform.rotation = _targetRot;
        }

        public override void UpdateState()
        {
            Vector2 input = _ctx.Context.MoveInput;
            
            _ctx.Animator.SetFloat("ClimbHorizontal", input.x);
            _ctx.Animator.SetFloat("ClimbVertical", input.y);
            
            Vector3 wallRight = Vector3.Cross(_ctx.WallNormal, Vector3.up).normalized;
            
            Vector3 moveDir = (wallRight * input.x + Vector3.up * input.y).normalized;
            
            if (input.magnitude > 0.1f)
            {
                Vector3 targetPos = _ctx.transform.position + moveDir * _ctx.Config.WallClimbSpeed * Time.deltaTime;
                _ctx.transform.position = targetPos;
            }

            if (Physics.Raycast(_ctx.transform.position + Vector3.up, _ctx.transform.forward, out RaycastHit hit, 1.0f))
            {
                 float wallDist = 0.4f;
                 Vector3 properPos = hit.point + hit.normal * wallDist;
                 properPos.y = _ctx.transform.position.y; 
                 
                 Vector3 finalPos = _ctx.transform.position;
                 finalPos.x = Mathf.Lerp(finalPos.x, properPos.x, Time.deltaTime * 10f);
                 finalPos.z = Mathf.Lerp(finalPos.z, properPos.z, Time.deltaTime * 10f);
                 _ctx.transform.position = finalPos;

                _ctx.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            }

            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsWallClimbing", false);
            _ctx.CharacterController.enabled = true;
            _ctx.Context.IsWallClimbing = false;
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsCrouching) 
            {
                SwitchState(_factory.Air());
            }
            else if (_ctx.CheckForLedge(true)) 
            {
                SwitchState(_factory.LedgeGrab());
            }
            else if (!_ctx.CheckForWall())
            {
                SwitchState(_factory.Air());
            }
            else if (IsGroundBelow())
            {
                SwitchState(_factory.Grounded());
            }
        }

        private bool IsGroundBelow()
        {
             return Physics.Raycast(_ctx.transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
        }

        public override void InitializeSubState() { }
    }
}
