using Core;
using UnityEngine;

namespace Domain
{
    /// <summary>
    /// State that searches for the target around the last known position for a limited time.
    /// Generates a series of points on the NavMesh within a radius and visits them sequentially.
    /// If target is found (detected by NpcBrain) this state is interrupted externally.
    /// </summary>
    public class SearchingState : IAgentState
    {
        private INavigationAgent _agent;
        private Vector3 _origin;
        private float _radius;
        private float _searchTime;
        private float _speed; // New speed parameter

        private float _timer;
        private Vector3[] _searchPoints;
        private int _currentIndex;
        private bool _waiting;
        private float _waitTimer;

        // Configuration
        private const int DefaultSearchPoints = 6;
        private const float ReachThreshold = 1.0f; // Larger threshold for scaled NPCs

        // Status
        public bool IsFinished { get; private set; }

        private bool _hasArrivedAtSearchArea; // New flag for approach phase
        private float _approachSpeed;

        public SearchingState(INavigationAgent agent, Vector3 origin, float radius, float searchTime, float speed, float approachSpeed)
        {
            _agent = agent;
            Setup(origin, radius, searchTime, speed, approachSpeed);
        }

        public void Setup(Vector3 origin, float radius, float searchTime, float speed, float approachSpeed)
        {
            _origin = origin;
            _radius = Mathf.Max(0.5f, radius);
            _searchTime = Mathf.Max(0f, searchTime);
            _speed = speed;
            _approachSpeed = approachSpeed;
            IsFinished = false;
        }

        public void Enter()
        {
            // Apply speed (Approach speed first)
            if (_agent != null) _agent.Speed = _approachSpeed;

            _timer = 0f;
            _currentIndex = 0;
            _waiting = false;
            _waitTimer = 0f;
            IsFinished = false;
            _hasArrivedAtSearchArea = false; // Reset approach flag

            // Phase 1: Go to origin first
            SetAgentDestination(_origin);

            // Clear previous points
            _searchPoints = null;
        }

        public void Update()
        {
            if (IsFinished) return;

            // Phase 1: Approach Logic
            if (!_hasArrivedAtSearchArea)
            {
                if (_agent != null && !_agent.PathPending && _agent.RemainingDistance < ReachThreshold)
                {
                    // Arrived at origin, start Phase 2
                    StartActualSearch();
                }
                return;
            }

            // Phase 2: Search Logic
            _timer += Time.deltaTime;

            // If time exceeded, stop searching
            if (_searchTime > 0f && _timer >= _searchTime)
            {
                IsFinished = true;
                return;
            }

            if (_searchPoints == null || _searchPoints.Length == 0)
            {
                IsFinished = true;
                return;
            }

            if (_waiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    _waiting = false;
                    MoveToNextSearchPoint();
                }
                return;
            }

            // Check if arrived at search point
            if (_agent != null && !_agent.PathPending && _agent.RemainingDistance < ReachThreshold)
            {
                // Arrived at point
                // Simulate checking / wait
                _waiting = true;
                _waitTimer = Random.Range(0.5f, 1.5f);
            }
        }

        private void StartActualSearch()
        {
            _hasArrivedAtSearchArea = true;
            _timer = 0f;

            // Switch to Search Speed (investigation speed)
            if (_agent != null) _agent.Speed = _speed;

            // Generate search points on NavMesh around origin
            _searchPoints = GenerateSearchPoints(DefaultSearchPoints);

            // Start moving to first point if available
            if (_searchPoints != null && _searchPoints.Length > 0)
            {
                SetAgentDestination(_searchPoints[_currentIndex]);
            }
            else
            {
                // Nowhere to go, finish immediately
                IsFinished = true;
            }
        }

        public void Exit()
        {
            _waiting = false;
            // Optionally stop agent?
            // if (_agent != null) _agent.SetDestination(_agent.transform.position);
        }

        // Utility: Generate points around origin snapped to NavMesh
        // Logic updated to ensure full circular coverage (sector-based)
        private Vector3[] GenerateSearchPoints(int count)
        {
            var list = new System.Collections.Generic.List<Vector3>(count);

            float angleStep = 360f / count;
            float currentAngle = Random.Range(0f, 360f); // Start at random angle

            for (int i = 0; i < count; i++)
            {
                // Calculate angle for this sector (with slight random variation)
                float sectorAngle = currentAngle + (i * angleStep);
                float randomOffset = Random.Range(-angleStep * 0.3f, angleStep * 0.3f);
                float finalRad = (sectorAngle + randomOffset) * Mathf.Deg2Rad;

                // Push distance outwards to cover "circumference" as requested
                // Range between 50% and 100% of radius
                float dist = Random.Range(0.5f, 1f) * _radius;

                Vector3 offset = new Vector3(Mathf.Cos(finalRad), 0f, Mathf.Sin(finalRad)) * dist;
                Vector3 p = _origin + offset;

                // snap to navmesh
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(p, out hit, 3.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    list.Add(hit.position);
                }
            }

            // If we found few points, ensure origin is included
            if (list.Count == 0)
            {
                list.Add(_origin);
            }
            // Shuffle reasonably or keep order? Circle order feels like a "sweep". 
            // Let's keep the circle order so it sweeps the area.

            return list.ToArray();
        }

        private void MoveToNextSearchPoint()
        {
            if (_searchPoints == null || _searchPoints.Length == 0) return;

            _currentIndex++;
            if (_currentIndex >= _searchPoints.Length)
            {
                // Visited all points? Restart or loop?
                // Let's loop until time runs out
                _currentIndex = 0;
            }
            SetAgentDestination(_searchPoints[_currentIndex]);
        }

        private void SetAgentDestination(Vector3 pos)
        {
            if (_agent != null)
                _agent.SetDestination(pos);
        }

        // Expose remaining time (optional) so NpcBrain can check when to fallback
        public float Elapsed => _timer;
        public float Duration => _searchTime;

        // Expose generated search points and metadata for debugging / visualization
        public Vector3[] SearchPoints => _searchPoints;
        public int CurrentIndex => _currentIndex;
        public Vector3 Origin => _origin;
    }
}
