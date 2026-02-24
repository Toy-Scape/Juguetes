using UnityEngine;

namespace Domain.StaticNpc
{
    [RequireComponent(typeof(Animator))]
    public class StaticNpcScannerIK : MonoBehaviour
    {
        [Header("IK Weights")]
        [Range(0, 1)] public float weight = 1.0f;
        [Range(0, 1)] public float bodyWeight = 0.3f;
        [Range(0, 1)] public float headWeight = 1.0f;
        [Range(0, 1)] public float eyesWeight = 1.0f;
        [Range(0, 1)] public float clampWeight = 0.5f;

        [Header("Sweep Settings")]
        public bool isSweeping = true;
        public float scanAngle = 60f; // degrees left and right
        public float scanSpeed = 1.5f;
        [Tooltip("The distance of the imaginary point the NPC looks at while sweeping.")]
        public float sweepFocusDistance = 5f;

        [Header("Targeting")]
        [Tooltip("The actual target to look at when not sweeping. Usually the player.")]
        public Transform _target;
        public float transitionSpeed = 5f;
        
        [Tooltip("Offset applied to the target's position (e.g. to avoid looking at the feet)")]
        public Vector3 targetOffset = new Vector3(0, 1.5f, 0);

        private Animator _animator;
        private Vector3 _currentLookPosition;
        private float _timeCounter;
        
        // Detection smoothing variables
        private float _detectionProgress;
        private float _detectionDuration;
        private Vector3 _startLookPosition;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            // Initialize look position in front
            _currentLookPosition = GetBaseLookPosition() + transform.forward * sweepFocusDistance;
        }

        private void Update()
        {
            if (isSweeping)
            {
                _timeCounter += Time.deltaTime * scanSpeed;
            }
        }

        public void SetTarget(Transform targetTarget, float detectionTime)
        {
            _target = targetTarget;
            isSweeping = false;
            _detectionDuration = detectionTime;
            _detectionProgress = 0f;
            _startLookPosition = _currentLookPosition;
        }

        public void ClearTarget()
        {
            _target = null;
            isSweeping = true;
            _detectionProgress = 0f;
        }

        private Vector3 GetBaseLookPosition()
        {
            // Usually we originate the look calculation from the head level
            Transform head = _animator.GetBoneTransform(HumanBodyBones.Head);
            if (head != null) return head.position;
            return transform.position + Vector3.up * 1.5f; 
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_animator == null) return;

            Vector3 desiredLookPosition;

            if (!isSweeping && _target != null)
            {
                // Look at the target, interpolating based on detection time
                Vector3 finalTargetPosition = _target.position + targetOffset; 
                
                if (_detectionDuration > 0)
                {
                    _detectionProgress += Time.deltaTime;
                    float t = Mathf.Clamp01(_detectionProgress / _detectionDuration);
                    // Add smooth step for a more natural turn
                    t = Mathf.SmoothStep(0f, 1f, t);
                    desiredLookPosition = Vector3.Lerp(_startLookPosition, finalTargetPosition, t);
                }
                else
                {
                    desiredLookPosition = finalTargetPosition;
                }
            }
            else
            {
                // Sweeping mode (calculate lateral sweeping position)
                Vector3 basePos = GetBaseLookPosition();
                float currentAngle = Mathf.Sin(_timeCounter) * scanAngle;
                Vector3 direction = Quaternion.AngleAxis(currentAngle, transform.up) * transform.forward;
                desiredLookPosition = basePos + direction * sweepFocusDistance;
            }

            // In Sweep mode apply transition speed to keep it smooth, 
            // but in detection mode the Lerp time dictates the transition.
            if (isSweeping)
            {
                _currentLookPosition = Vector3.Lerp(_currentLookPosition, desiredLookPosition, Time.deltaTime * transitionSpeed);
            }
            else
            {
                _currentLookPosition = desiredLookPosition;
            }

            _animator.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
            _animator.SetLookAtPosition(_currentLookPosition);
        }
    }
}
