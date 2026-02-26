using Core;
using UnityEngine;
using UnityEngine.Events;

namespace Domain.StaticNpc
{
    public class StaticNpcBrain : MonoBehaviour
    {
        [Header("Components")]
        [Tooltip("The vision sensor used to detect targets. If null, will try to GetComponent<ITargetDetector>()")]
        public VisionSensor vision;
        [Tooltip("The IK Controller for head movement. If null, tries to GetComponent()")]
        public StaticNpcScannerIK scannerIK;

        [Header("Detection Settings")]
        public float detectionTime = 2f;

        [Header("Events")]
        public UnityEvent<Transform> onDetecting = new UnityEvent<Transform>(); // Fired continuously while recognizing
        public UnityEvent<Transform> onTargetDetected = new UnityEvent<Transform>(); // Fired when fully spotted
        public UnityEvent onTargetLost = new UnityEvent();

        private ITargetDetector _targetDetector;
        private Transform _currentTarget;
        private float _detectionTimer;
        private bool _isFullyDetected;

        private void Start()
        {
            if (vision != null)
                _targetDetector = vision;
            else
                _targetDetector = GetComponent<ITargetDetector>();

            if (scannerIK == null)
                scannerIK = GetComponent<StaticNpcScannerIK>();

            if (_targetDetector == null)
            {
                Debug.LogError($"{gameObject.name}: StaticNpcBrain requires an ITargetDetector (like VisionSensor).");
            }
        }

        private void Update()
        {
            HandleDetection();
        }

        private void HandleDetection()
        {
            if (_targetDetector == null) return;

            Transform closestTarget = null;

            // Optional optimization: Use the specific VisionSensor interface if available
            if (vision != null)
            {
                var visibleTargets = vision.GetVisibleTargetsInterface();
                closestTarget = GetClosestTarget(visibleTargets);
            }
            else
            {
                var transforms = _targetDetector.GetVisibleTargets(transform);
                closestTarget = GetClosestTransform(transforms);
            }

            if (closestTarget != null)
            {
                // We see someone
                if (_currentTarget != closestTarget)
                {
                    // Newly spotted
                    Debug.Log($"[StaticNpcBrain] Newly spotted target: {closestTarget.name}");
                    _currentTarget = closestTarget;
                    _detectionTimer = 0f;
                    _isFullyDetected = false;
                    
                    if (scannerIK != null)
                    {
                        scannerIK.SetTarget(_currentTarget, detectionTime);
                    }
                }

                _detectionTimer += Time.deltaTime;

                if (!_isFullyDetected)
                {
                    // Debug.Log($"[StaticNpcBrain] Detecting... {_detectionTimer}/{detectionTime}");
                    onDetecting?.Invoke(_currentTarget);

                    if (_detectionTimer >= detectionTime)
                    {
                        Debug.Log($"[StaticNpcBrain] Fully detected target: {_currentTarget.name}");
                        _isFullyDetected = true;
                        onTargetDetected?.Invoke(_currentTarget);
                    }
                }
            }
            else
            {
                // Nobody in sight
                if (_currentTarget != null)
                {
                    Debug.Log($"[StaticNpcBrain] Target lost: {_currentTarget.name}");
                    _currentTarget = null;
                    _detectionTimer = 0f;
                    _isFullyDetected = false;
                    
                    if (scannerIK != null)
                    {
                        scannerIK.ClearTarget();
                    }

                    onTargetLost?.Invoke();
                }
            }
        }

        private Transform GetClosestTarget(System.Collections.Generic.List<IVisibleTarget> targets)
        {
            if (targets == null || targets.Count == 0) return null;

            Transform closest = null;
            float bestDist = float.MaxValue;
            Vector3 myPos = transform.position;

            foreach (var t in targets)
            {
                float d = Vector3.SqrMagnitude(myPos - t.TargetPosition);
                if (d < bestDist)
                {
                    bestDist = d;
                    closest = t.Transform;
                }
            }
            return closest;
        }

        private Transform GetClosestTransform(System.Collections.Generic.List<Transform> targets)
        {
            if (targets == null || targets.Count == 0) return null;

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
    }
}
