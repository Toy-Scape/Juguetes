using UI_System.Menus;
using UnityEngine;

namespace Assets.Scripts.PlayerController
{
    public abstract class PlayerBaseState
    {
        protected global::PlayerController _ctx;
        protected PlayerStateFactory _factory;
        protected PlayerBaseState _currentSubState;
        public PlayerBaseState CurrentSubState => _currentSubState;
        protected PlayerBaseState _currentSuperState;

        public PlayerBaseState(global::PlayerController currentContext, PlayerStateFactory playerStateFactory)
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
            if (GamePauseHandler.IsPaused) return;
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
            ExitState();

            if (_currentSuperState != null)
            {
                _currentSuperState.SetSubState(newState);
            }
            else if (_ctx.CurrentState == this)
            {
                _ctx.CurrentState = newState;
                newState.EnterState(); 
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
