// Domain/ChaseState.cs

using Core;
using UnityEngine;

namespace Domain
{
    public class ChaseState : IAgentState
    {
        private INavigationAgent _agent;
        private Transform _target;
        private NpcBrain _npc;
        private float _speed;

        public ChaseState(NpcBrain npc, INavigationAgent agent, Transform target, float speed)
        {
            // npc se guarda por si en el futuro se necesita acceder a datos del brain
            _npc = npc;
            _agent = agent;
            _target = target;
            _speed = speed;
        }

        public void Enter() { if (_agent != null) _agent.Speed = _speed; }
        public void Exit() { }

        public void Update()
        {
            if (_target != null && _agent != null)
                _agent.SetDestination(_target.position);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}