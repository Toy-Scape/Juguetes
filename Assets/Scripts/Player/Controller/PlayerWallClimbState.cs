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
            _ctx.CharacterController.enabled = false;
            
            // Snap rotation
            _targetRot = Quaternion.LookRotation(-_ctx.WallNormal, Vector3.up);
            _ctx.transform.rotation = _targetRot;
        }

        public override void UpdateState()
        {
            Vector2 input = _ctx.Context.MoveInput;
            
            // Movement relative to wall
            Vector3 wallRight = Vector3.Cross(_ctx.WallNormal, Vector3.up).normalized;
            // Depending on wall normal, cross product direction might need flip.
            // If normal is (0,0,-1) [Back], Up is (0,1,0), Cross is (1,0,0) [Right]. Correct.
            
            Vector3 moveDir = (wallRight * input.x + Vector3.up * input.y).normalized;
            
            if (input.magnitude > 0.1f)
            {
                // Simple position move since CC is disabled
                // We project movement on the wall plane to be safe
                Vector3 targetPos = _ctx.transform.position + moveDir * _ctx.Config.WallClimbSpeed * Time.deltaTime;
                _ctx.transform.position = targetPos;
            }

            // Snap stick to wall (Raycast forward)
            if (Physics.Raycast(_ctx.transform.position + Vector3.up, _ctx.transform.forward, out RaycastHit hit, 1.0f))
            {
                 // Keep distance
                 float wallDist = 0.4f; // Adjust as needed
                 Vector3 properPos = hit.point + hit.normal * wallDist;
                 properPos.y = _ctx.transform.position.y; // Keep Y form movement
                 
                 // Apply corrections
                 Vector3 finalPos = _ctx.transform.position;
                 finalPos.x = Mathf.Lerp(finalPos.x, properPos.x, Time.deltaTime * 10f);
                 finalPos.z = Mathf.Lerp(finalPos.z, properPos.z, Time.deltaTime * 10f);
                 _ctx.transform.position = finalPos;
                 
                 // Recalculate normal if wall curves (or corner - though user said ignore corners)
                 // _ctx.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            }

            CheckSwitchStates();
        }

        public override void FixedUpdateState() { }

        public override void ExitState()
        {
            _ctx.Animator.SetBool("IsWallClimbing", false);
            _ctx.CharacterController.enabled = true;
        }

        public override void CheckSwitchStates()
        {
            if (_ctx.Context.IsCrouching) // Drop
            {
                SwitchState(_factory.Air());
            }
            else if (_ctx.CheckForLedge()) // Top reached
            {
                SwitchState(_factory.LedgeGrab());
            }
            else if (!_ctx.CheckForWall()) // Ran out of wall
            {
                // Double check it's not just the raycast missing due to movement
                // If really no wall, fall
                SwitchState(_factory.Air());
            }
            else if (IsGroundBelow()) // Hit floor
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
