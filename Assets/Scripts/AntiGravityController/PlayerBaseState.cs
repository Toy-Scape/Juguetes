using UnityEngine;

namespace Assets.Scripts.AntiGravityController
{
    public abstract class PlayerBaseState
    {
        protected AntiGravityPlayerController _ctx;
        protected PlayerStateFactory _factory;
        protected PlayerBaseState _currentSubState;
        public PlayerBaseState CurrentSubState => _currentSubState;
        protected PlayerBaseState _currentSuperState;

        public PlayerBaseState(AntiGravityPlayerController currentContext, PlayerStateFactory playerStateFactory)
        {
            _ctx = currentContext;
            _factory = playerStateFactory;
        }

        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
        public abstract void CheckSwitchStates();
        public abstract void InitializeSubState();

        public void UpdateStates()
        {
            UpdateState();
            if (_currentSubState != null)
            {
                _currentSubState.UpdateStates();
            }
        }

        public void FixedUpdateStates()
        {
            FixedUpdateState();
            if (_currentSubState != null)
            {
                _currentSubState.FixedUpdateStates();
            }
        }

        protected void SwitchState(PlayerBaseState newState)
        {
            // Exit current state
            ExitState();

            // Enter new state
            // newState.EnterState(); // Removed: SetSubState calls EnterState, or we call it manually if root

            if (_currentSuperState != null)
            {
                _currentSuperState.SetSubState(newState);
            }
            else if (_ctx.CurrentState == this)
            {
                // If we are the root state, update context
                _ctx.CurrentState = newState;
                newState.EnterState(); // Only call if no super state handles it
            }
        }

        protected void SetSuperState(PlayerBaseState newSuperState)
        {
            _currentSuperState = newSuperState;
        }

        public void SetSubState(PlayerBaseState newSubState)
        {
            _currentSubState = newSubState;
            newSubState.SetSuperState(this);
            newSubState.EnterState();
        }

        public override string ToString()
        {
            return GetType().Name.Replace("Player", "").Replace("State", "");
        }
    }
}
