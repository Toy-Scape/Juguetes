using System.Collections.Generic;

namespace Assets.Scripts.AntiGravityController
{
    public class PlayerStateFactory
    {
        AntiGravityPlayerController _context;
        Dictionary<string, PlayerBaseState> _states = new Dictionary<string, PlayerBaseState>();

        public PlayerStateFactory(AntiGravityPlayerController currentContext)
        {
            _context = currentContext;
        }

        public PlayerBaseState Idle() { return GetState("Idle", () => new PlayerIdleState(_context, this)); }
        public PlayerBaseState Walk() { return GetState("Walk", () => new PlayerWalkState(_context, this)); }
        public PlayerBaseState Run() { return GetState("Run", () => new PlayerRunState(_context, this)); }
        public PlayerBaseState Jump() { return GetState("Jump", () => new PlayerJumpState(_context, this)); }
        public PlayerBaseState Fall() { return GetState("Fall", () => new PlayerFallState(_context, this)); }
        public PlayerBaseState Grounded() { return GetState("Grounded", () => new PlayerGroundedState(_context, this)); }
        public PlayerBaseState Air() { return GetState("Air", () => new PlayerAirState(_context, this)); }
        public PlayerBaseState Crouch() { return GetState("Crouch", () => new PlayerCrouchState(_context, this)); }
        public PlayerBaseState LedgeGrab() { return GetState("LedgeGrab", () => new PlayerLedgeGrabState(_context, this)); }
        public PlayerBaseState LedgeClimb() { return GetState("LedgeClimb", () => new PlayerLedgeClimbState(_context, this)); }

        private PlayerBaseState GetState(string key, System.Func<PlayerBaseState> createMethod)
        {
            if (!_states.ContainsKey(key))
            {
                _states[key] = createMethod();
            }
            return _states[key];
        }
    }
}
