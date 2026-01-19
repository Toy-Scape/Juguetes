using UnityEngine;
using UnityEngine.AI;
using Core;

namespace Infrastructure
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentAdapter : MonoBehaviour, INavigationAgent
    {
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public bool PathPending => _agent.pathPending;

        public float RemainingDistance => _agent.remainingDistance;

        public Vector3 Velocity => _agent.velocity;

        public float Speed
        {
            get => _agent.speed;
            set => _agent.speed = value;
        }

        public void SetDestination(Vector3 target)
        {
            _agent.SetDestination(target);
        }
    }
}
