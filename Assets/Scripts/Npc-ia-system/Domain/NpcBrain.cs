using Core;
using SO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Domain
{
    public class NpcBrain : MonoBehaviour
    {
        [Header("Routes")]
        public PatrolRouteSO[] patrolRoutes;
        public bool randomRouteOnStart = true;

        [Header("Movement Settings")]
        public float patrolSpeed = 3.5f;
        public float chaseSpeed = 5.0f;
        public float approachSpeed = 4.0f;
        public float searchSpeed = 2.0f;

        [Header("Detection Settings")]
        public VisionSensor vision;
        public Core.HearingSensor hearing; // New Hearing Sensor
        public float detectionTime = 2f;
        [Tooltip("Time to search for target after losing it before returning to patrol.")]
        public float searchDuration = 5f;
        public float searchRadius = 5f;

        [Header("Events")]
        public UnityEvent<string> onStateChanged = new UnityEvent<string>();
        public UnityEvent<Transform> onTargetDetected = new UnityEvent<Transform>();
        public UnityEvent onTargetLost = new UnityEvent();
        public UnityEvent<Vector3> onNoiseHeard = new UnityEvent<Vector3>();
        public UnityEvent onStartChasing = new UnityEvent();
        public UnityEvent onStartSearching = new UnityEvent();
        public UnityEvent onReturnToPatrol = new UnityEvent();
        public UnityEvent onReachPatrolPoint = new UnityEvent();
        public UnityEvent<Transform> onTargetCaught = new UnityEvent<Transform>();

        [Header("Catch Settings")]
        [Tooltip("Distance at which the NPC 'catches' the target during chase.")]
        public float catchDistance = 1.5f;

        private ITargetDetector _targetDetector;
        private INavigationAgent _agent;
        private IAgentState _currentState;

        // Reusable states to avoid 'new' allocations in Update
        private PatrolState _patrolState;
        private ChaseState _chaseState;
        private SearchingState _searchState;

        private PatrolRouteSO _currentRoute;
        private float _detectionTimer;
        private Transform _currentTargetTransform; // Keep track of the transform for chase logic
        private IVisibleTarget _currentIVisibleTarget;

        void Start()
        {
            InitializeDependencies();
            InitializeStates();
        }

        private void InitializeDependencies()
        {
            // Bind detector
            if (vision != null)
                _targetDetector = vision;
            else
                _targetDetector = GetComponent<ITargetDetector>();

            // Bind Hearing
            if (hearing == null) hearing = GetComponent<Core.HearingSensor>();
            if (hearing != null)
            {
                hearing.OnNoiseDetected += OnNoiseHeard;
            }

            // Get navigation adapter
            _agent = GetComponent<INavigationAgent>();
            if (_agent == null)
            {
                // FALLBACK: If no adapter is found, we do NOT force AddComponent explicitly violating architecture layers here.
                // Instead we log an error or assume the user has set it up correctly as per documentation.
                // Ideally, a bootstrap script or Inspector setup ensures the Adapter is present.
                Debug.LogError($"{name}: No INavigationAgent found. Please add a NavMeshAgentAdapter.");
            }
        }

        private void OnNoiseHeard(Vector3 noisePos)
        {
            // Priority: Logic
            // 1. If Chasing -> Ignore noise (Visual contact is higher priority)
            // 2. If Patrol -> Switch to Search at noise pos
            // 3. If Search -> Update search to new noise pos (distraction)

            if (_currentState is ChaseState) return;

            Debug.Log($"{name}: Heard noise at {noisePos}. Investigating.");

            // Fire event
            onNoiseHeard?.Invoke(noisePos);

            // Switch to Searching
            if (_searchState == null)
                _searchState = StateFactory.CreateSearching(_agent, noisePos, searchRadius, searchDuration, searchSpeed, approachSpeed);
            else
                _searchState.Setup(noisePos, searchRadius, searchDuration, searchSpeed, approachSpeed);

            onStartSearching?.Invoke();
            ChangeState(_searchState);
        }

        private void InitializeStates()
        {
            if (patrolRoutes != null && patrolRoutes.Length > 0)
            {
                _currentRoute = randomRouteOnStart
                    ? patrolRoutes[Random.Range(0, patrolRoutes.Length)]
                    : patrolRoutes[0];
            }

            // Create initial states
            _patrolState = StateFactory.CreatePatrol(this, _agent, _currentRoute, patrolSpeed);
            // Chase and Search states might be re-created or reset. For now we can keep references or create on demand.
            // Optimized approach: Create once and reset/configure when entering.
            _chaseState = StateFactory.CreateChase(this, _agent, null, chaseSpeed);
            // Searching state needs origin, so we might create it on demand or have a setter.

            // Start patrolling
            ChangeState(_patrolState);
        }

        void Update()
        {
            if (_currentState != null)
                _currentState.Update();

            HandleDetection();
            CheckForCatch();
        }

        private void CheckForCatch()
        {
            // Only check when chasing and have a valid target
            if (!(_currentState is ChaseState)) return;
            if (_currentTargetTransform == null) return;

            float distance = Vector3.Distance(transform.position, _currentTargetTransform.position);
            if (distance <= catchDistance)
            {
                onTargetCaught?.Invoke(_currentTargetTransform);

                // Optionally: Stop chasing after catching (return to patrol)
                // You can remove this if you want to keep chasing until the target escapes
                // onReturnToPatrol?.Invoke();
                // ChangeState(_patrolState);
            }
        }

        private void HandleDetection()
        {
            if (_targetDetector == null) return;

            // Use the specific interface method if multiple exist, otherwise standard
            // We cast to VisionSensor to use the specific optimized method if available, 
            // or modify ITargetDetector to include GetVisibleTargetsInterface. 
            // For now, let's stick to the interface usage.

            // If VisionSensor is known, use the optimized path
            if (vision != null)
            {
                var visibleTargets = vision.GetVisibleTargetsInterface();
                ProcessTargets(visibleTargets);
            }
            else
            {
                // Fallback to transform based list
                var transforms = _targetDetector.GetVisibleTargets(transform);
                if (transforms.Count > 0)
                {
                    // Convert to temporary list of simple targets if needed, or just pick closest transform
                    // Logic simplification: Just pick closest transform
                    Transform closest = GetClosestTransform(transforms);
                    UpdateTargetLogic(closest, null);
                }
                else
                {
                    UpdateTargetLogic(null, null);
                }
            }
        }

        private void ProcessTargets(System.Collections.Generic.List<IVisibleTarget> targets)
        {
            if (targets != null && targets.Count > 0)
            {
                IVisibleTarget closest = null;
                float bestDist = float.MaxValue;
                Vector3 myPos = transform.position;

                foreach (var t in targets)
                {
                    float d = Vector3.SqrMagnitude(myPos - t.TargetPosition);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        closest = t;
                    }
                }
                UpdateTargetLogic(closest?.Transform, closest);
            }
            else
            {
                UpdateTargetLogic(null, null);
            }
        }

        private Transform GetClosestTransform(System.Collections.Generic.List<Transform> targets)
        {
            Transform closest = null;
            float bestDist = float.MaxValue;
            Vector3 myPos = transform.position;
            foreach (var t in targets)
            {
                float d = Vector3.SqrMagnitude(myPos - t.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    closest = t;
                }
            }
            return closest;
        }

        private void UpdateTargetLogic(Transform targetTrans, IVisibleTarget targetInterface)
        {
            if (targetTrans != null)
            {
                // Target found
                if (_currentTargetTransform == null)
                {
                    onTargetDetected?.Invoke(targetTrans);
                    _detectionTimer = 0f;
                }

                _currentTargetTransform = targetTrans;
                _currentIVisibleTarget = targetInterface;
                _detectionTimer += Time.deltaTime;

                if (_detectionTimer >= detectionTime && !(_currentState is ChaseState))
                {
                    // Switch to Chase
                    // Optimize: reuse state instance
                    if (_chaseState == null)
                        _chaseState = StateFactory.CreateChase(this, _agent, _currentTargetTransform, chaseSpeed);
                    else
                        _chaseState.SetTarget(_currentTargetTransform);

                    onStartChasing?.Invoke();
                    ChangeState(_chaseState);
                    _detectionTimer = 0f;
                }
            }
            else
            {
                // Target lost or not found
                if (_currentTargetTransform != null)
                {
                    onTargetLost?.Invoke();
                    UnityEngine.Vector3 lastKnownPos = _currentTargetTransform.position;
                    if (_currentIVisibleTarget != null && _currentIVisibleTarget.IsValid)
                        lastKnownPos = _currentIVisibleTarget.TargetPosition;

                    // Switch to Searching if we were chasing
                    if (_currentState is ChaseState)
                    {
                        if (_searchState == null)
                            _searchState = StateFactory.CreateSearching(_agent, lastKnownPos, searchRadius, searchDuration, searchSpeed, approachSpeed);
                        else
                            _searchState.Setup(lastKnownPos, searchRadius, searchDuration, searchSpeed, approachSpeed);

                        onStartSearching?.Invoke();
                        ChangeState(_searchState);
                    }
                }

                _currentTargetTransform = null;
                _currentIVisibleTarget = null;
                _detectionTimer = 0f;

                // Check if Searching is done
                if (_currentState is SearchingState searchState)
                {
                    if (searchState.IsFinished)
                    {
                        onReturnToPatrol?.Invoke();
                        ChangeState(_patrolState);
                    }
                }
            }
        }

        public void ChangeState(IAgentState newState)
        {
            if (newState == null) return;
            // Prevent re-entry of same state instance if logical
            if (_currentState == newState) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
            onStateChanged?.Invoke(newState.GetType().Name);
        }

        public void ChangeRoute(PatrolRouteSO newRoute)
        {
            if (newRoute == null) return;
            _currentRoute = newRoute;
            _patrolState = StateFactory.CreatePatrol(this, _agent, _currentRoute, patrolSpeed);
            if (_currentState is PatrolState)
                ChangeState(_patrolState);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (vision != null)
                vision.DrawGizmosSelected();

            // Draw Search Radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange
            if (_currentState is SearchingState searchState)
            {
                // Draw at investigation origin
                Gizmos.DrawWireSphere(searchState.Origin, searchRadius);
            }
            else
            {
                // Draw around NPC to show range reference
                Gizmos.DrawWireSphere(transform.position, searchRadius);
            }

            // Draw Patrol Route
            if (patrolRoutes != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var route in patrolRoutes)
                {
                    if (route == null) continue;
                    var points = route.GetPathPoints();
                    if (points == null || points.Count == 0) continue;

                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector3 p1 = points[i].GetPosition();
                        Vector3 p2 = points[(i + 1) % points.Count].GetPosition();
                        Gizmos.DrawLine(p1, p2);
                        Gizmos.DrawSphere(p1, 0.2f);
                    }
                }
            }
        }
#endif
    }
}
