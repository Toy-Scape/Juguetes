using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent]
    public class VisionSensor : MonoBehaviour, ITargetDetector
    {
        [Header("View Settings")]
        public float viewRadius = 10f;
        [Range(0, 360)] public float viewAngle = 90f;

        [Header("Eye Configuration (Scaled NPC Support)")]
        [Tooltip("Optional: specific transform for the eyes. If null, uses eyeOffset relative to this transform.")]
        public Transform eyeTransform;
        [Tooltip("Offset from the pivot to the eyes if eyeTransform is null.")]
        public Vector3 eyeOffset = new Vector3(0, 1.5f, 0);

        [Header("Layer Masks")]
        [Tooltip("Optimization: Only check these layers for targets. Set to 'Everything' if unsure, or specific layers for performance.")]
        public LayerMask detectionMask = -1; // Default to Everything
        public LayerMask obstacleMask;  // Lo que bloquea la visión

        // Cache to avoid allocations
        private Collider[] _overlapBuffer = new Collider[64];
        private List<Transform> _visibleTransformsCache = new List<Transform>();

        // New interface-based cache
        private List<IVisibleTarget> _visibleTargetsCache = new List<IVisibleTarget>();

        /// <summary>
        /// Returns valid IVisibleTargets currently seen.
        /// </summary>
        public List<IVisibleTarget> GetVisibleTargetsInterface()
        {
            _visibleTargetsCache.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, viewRadius, _overlapBuffer, detectionMask);
            Vector3 eyePos = GetEyePosition();
            Vector3 forward = eyeTransform != null ? eyeTransform.forward : transform.forward;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                // Use GetComponentInParent to handle cases where collider is on a child object
                var target = col.GetComponentInParent<IVisibleTarget>();
                if (target != null)
                {
                    if (!target.IsValid) continue;

                    // Parse unique ID or use reference to avoid duplicate adds if multiple colliders belong to same target
                    if (!_visibleTargetsCache.Contains(target) && CheckVisibility(eyePos, forward, target))
                    {
                        _visibleTargetsCache.Add(target);
                    }
                }
            }
            return _visibleTargetsCache;
        }

        /// <summary>
        /// Legacy compatibility wrapper (if other systems still rely on Transform list).
        /// Prefer GetVisibleTargetsInterface.
        /// </summary>
        public List<Transform> GetVisibleTargets(Transform self)
        {
            _visibleTransformsCache.Clear();
            var targets = GetVisibleTargetsInterface();
            for (int i = 0; i < targets.Count; i++)
            {
                _visibleTransformsCache.Add(targets[i].Transform);
            }
            return _visibleTransformsCache;
        }

        public bool CanSeeTarget(Transform self, Transform targetTrans)
        {
            // Try to upgrade to interface check if possible
            if (targetTrans.TryGetComponent<IVisibleTarget>(out var iTarget))
            {
                if (!iTarget.IsValid) return false;
                return CheckVisibility(GetEyePosition(), eyeTransform != null ? eyeTransform.forward : transform.forward, iTarget);
            }

            // Fallback for non-IVisibleTarget objects
            return CheckVisibilityFallback(targetTrans);
        }

        private bool CheckVisibility(Vector3 eyePos, Vector3 forward, IVisibleTarget target)
        {
            Vector3 targetPos = target.TargetPosition;
            Vector3 dirToTarget = (targetPos - eyePos);
            float distance = dirToTarget.magnitude;

            if (distance > viewRadius) return false;

            // Angle check
            if (Vector3.Angle(forward, dirToTarget) > viewAngle / 2f) return false;

            // Occlusion check
            if (Physics.Raycast(eyePos, dirToTarget.normalized, out RaycastHit hit, distance, obstacleMask))
            {
                // Verify if the hit object is part of the target
                if (hit.collider.transform == target.Transform || hit.collider.transform.IsChildOf(target.Transform))
                {
                    return true; // We hit the target, so it is visible
                }
                return false; // We hit something else, so it is occluded
            }

            return true;
        }

        private bool CheckVisibilityFallback(Transform target)
        {
            if (target == null) return false;
            Vector3 eyePos = GetEyePosition();
            Vector3 targetPos = target.position; // Pivot might be at feet!
            Vector3 dirToTarget = (targetPos - eyePos);
            float dist = dirToTarget.magnitude;

            if (dist > viewRadius) return false;
            Vector3 forward = eyeTransform != null ? eyeTransform.forward : transform.forward;
            if (Vector3.Angle(forward, dirToTarget) > viewAngle / 2f) return false;

            if (Physics.Raycast(eyePos, dirToTarget.normalized, dist, obstacleMask)) return false;

            return true;
        }

        private Vector3 GetEyePosition()
        {
            if (eyeTransform != null) return eyeTransform.position;
            return transform.TransformPoint(eyeOffset);
        }

#if UNITY_EDITOR
        public void DrawGizmosSelected()
        {
            Vector3 eyePos = GetEyePosition();
            Vector3 forward = eyeTransform != null ? eyeTransform.forward : transform.forward;

            // Draw View Arc
            UnityEditor.Handles.color = new Color(1, 1, 0, 0.1f);
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up,
                Quaternion.Euler(0, -viewAngle / 2, 0) * forward,
                viewAngle, viewRadius); // Note based on feet for radius is usually fine, visual ref.

            // Draw Eye Pos
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(eyePos, 0.1f);
        }

        private void OnDrawGizmosSelected() { DrawGizmosSelected(); }
#endif
    }
}
