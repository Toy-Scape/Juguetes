using Core;
using SO;
using UnityEngine;

namespace Domain
{
    public class PatrolState : IAgentState
    {
        private INavigationAgent _agent;
        private PatrolRouteSO _route;
        private float _speed; // Speed param

        private int _currentIndex;
        private float _waitTimer;
        private bool _waiting;
        private NpcBrain _npc;

        private System.Collections.Generic.List<SO.PatrolPointData> _currentPath;

        public PatrolState(NpcBrain npc, INavigationAgent agent, PatrolRouteSO route, float speed)
        {
            // El parámetro npc se conserva para futuras extensiones
            _npc = npc;
            _agent = agent;
            _route = route;
            _currentIndex = 0;
            _waitTimer = 0f;
            _waiting = false;
            _speed = speed;
        }

        public void Enter()
        {
            // Apply speed: route speed if enabled, otherwise global
            if (_agent != null)
            {
                if (_route != null && _route.useRouteSpeed)
                    _agent.Speed = _route.defaultSpeed;
                else
                    _agent.Speed = _speed;
            }

            if (_route == null || _route.patrolPoints == null || _route.patrolPoints.Length == 0)
                return;

            // Generate path (linear or catmull-rom)
            _currentPath = _route.GetPathPoints();

            if (_currentPath == null || _currentPath.Count == 0) return;

            // Find the closest point to resume patrol from
            if (_route.randomPatrol)
            {
                _currentIndex = Random.Range(0, _currentPath.Count);
            }
            else
            {
                _currentIndex = FindClosestPointIndex();
            }

            // Move to closest point (or next if already there)
            SetDestinationToCurrentPoint();
        }

        private int FindClosestPointIndex()
        {
            if (_npc == null || _currentPath == null || _currentPath.Count == 0) return 0;

            Vector3 npcPos = _npc.transform.position;
            float minDist = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < _currentPath.Count; i++)
            {
                float dist = Vector3.Distance(npcPos, _currentPath[i].GetPosition());
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void SetDestinationToCurrentPoint()
        {
            if (_currentPath == null || _currentPath.Count == 0 || _agent == null) return;

            SO.PatrolPointData point = _currentPath[_currentIndex];
            _agent.SetDestination(point.GetPosition());
        }

        public void Update()
        {
            if (_currentPath == null || _currentPath.Count == 0)
                return;

            if (_waiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    _waiting = false;
                    MoveToNextPoint();
                }
                return;
            }

            if (_agent != null && !_agent.PathPending && _agent.RemainingDistance < 0.5f)
            {
                SO.PatrolPointData current = _currentPath[_currentIndex];

                // Espera si corresponde
                if (current.waitTime > 0f)
                {
                    _waiting = true;
                    _waitTimer = current.waitTime * Random.Range(0.8f, 1.2f);
                    current.onReachPoint?.Invoke();
                }

                if (!_waiting)
                    MoveToNextPoint();
            }
        }

        public void Exit()
        {
            _waiting = false;
        }

        private void MoveToNextPoint()
        {
            if (_currentPath == null || _currentPath.Count == 0)
                return;

            if (_route.randomPatrol)
            {
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, _currentPath.Count);
                } while (newIndex == _currentIndex && _currentPath.Count > 1);

                _currentIndex = newIndex;
            }
            else
            {
                _currentIndex = (_currentIndex + 1) % _currentPath.Count;
            }

            SO.PatrolPointData next = _currentPath[_currentIndex];

            // Apply speed: use per-point speed if overrideSpeed is enabled, otherwise global
            if (_agent != null)
            {
                if (next.overrideSpeed)
                {
                    _agent.Speed = next.moveSpeed;
                }
                else
                {
                    _agent.Speed = _speed; // Global patrol speed
                }

                _agent.SetDestination(next.GetPosition());
            }
        }
    }
}
