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
        [Tooltip("Euler angles to correct the forward direction if the assigned bone rotates weirdly (e.g. bones pointing up instead of forward).")]
        public Vector3 eyeRotationOffset = Vector3.zero;

        [Header("Layer Masks")]
        [Tooltip("Optimization: Only check these layers for targets. Set to 'Everything' if unsure, or specific layers for performance.")]
        public LayerMask detectionMask = -1; // Default to Everything
        public LayerMask obstacleMask;  // Lo que bloquea la visión

        [Header("Visualization")]
        [Tooltip("A Spot Light used to visualize the vision cone. Its range, angle, position, and rotation will trace the vision properties.")]
        public Light visionLight;

        // Cache to avoid allocations
        private Collider[] _overlapBuffer = new Collider[64];
        private List<Transform> _visibleTransformsCache = new List<Transform>();

        // New interface-based cache
        private List<IVisibleTarget> _visibleTargetsCache = new List<IVisibleTarget>();

        private void LateUpdate()
        {
            if (visionLight != null)
            {
                // Sync Light properties to vision settings
                visionLight.type = LightType.Spot;
                visionLight.spotAngle = viewAngle;
                visionLight.range = viewRadius;
                
                // Sync Transform
                visionLight.transform.position = GetEyePosition();
                
                Vector3 forward = GetEyeForward();
                Vector3 up = GetEyeUp();
                
                if (forward != Vector3.zero)
                {
                    visionLight.transform.rotation = Quaternion.LookRotation(forward, up);
                }
            }
        }

        public List<IVisibleTarget> GetVisibleTargetsInterface()
        {
            _visibleTargetsCache.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, viewRadius, _overlapBuffer, detectionMask);
            Vector3 eyePos = GetEyePosition();
            Vector3 forward = GetEyeForward();

            // Debug.Log($"[VisionSensor] OverlapSphere found {count} potential colliders.");

            for (int i = 0; i < count; i++)
            {
                var col = _overlapBuffer[i];
                // Use GetComponentInParent to handle cases where collider is on a child object
                var target = col.GetComponentInParent<IVisibleTarget>();
                if (target != null)
                {
                    if (!target.IsValid) continue;

                    if (!_visibleTargetsCache.Contains(target))
                    {
                        bool isVisible = CheckVisibility(eyePos, forward, target);
                        // Debug.Log($"[VisionSensor] Evaluating {col.name}: Visble? {isVisible}");
                        if (isVisible)
                        {
                            _visibleTargetsCache.Add(target);
                        }
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
                return CheckVisibility(GetEyePosition(), GetEyeForward(), iTarget);
            }

            // Fallback for non-IVisibleTarget objects
            return CheckVisibilityFallback(targetTrans);
        }

        private bool CheckVisibility(Vector3 eyePos, Vector3 forward, IVisibleTarget target)
        {
            Vector3 targetPos = target.TargetPosition;
            Vector3 dirToTarget = (targetPos - eyePos);
            float distance = dirToTarget.magnitude;

            if (distance > viewRadius) 
            {
                // Debug.Log($"[VisionSensor] Out of radius: {distance} > {viewRadius}");
                return false;
            }

            // Angle check
            float angle = Vector3.Angle(forward, dirToTarget);
            if (angle > viewAngle / 2f) 
            {
                // Debug.Log($"[VisionSensor] Out of angle: {angle} > {viewAngle / 2f}");
                return false;
            }

            // Occlusion check: RaycastAll to find first valid hit on the obstacle mask
            // We use RaycastAll and ignore triggers manually, or rely on Physics settings.
            RaycastHit[] hits = Physics.RaycastAll(eyePos, dirToTarget.normalized, distance, obstacleMask, QueryTriggerInteraction.Ignore);
            
            // Sort hits by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                // Verify if the hit object is part of the target
                if (hit.collider.transform == target.Transform || hit.collider.transform.IsChildOf(target.Transform))
                {
                    return true; // We hit the target, so it is visible
                }
                
                // If we hit something else that is in the obstacleMask and not a trigger, it's occluded
                if (!hit.collider.isTrigger) 
                {
                    // Debug.Log($"[VisionSensor] Occluded by {hit.collider.name}");
                    return false;
                }
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
            Vector3 forward = GetEyeForward();
            if (Vector3.Angle(forward, dirToTarget) > viewAngle / 2f) return false;

            if (Physics.Raycast(eyePos, dirToTarget.normalized, dist, obstacleMask, QueryTriggerInteraction.Ignore)) return false;

            return true;
        }

        private Vector3 GetEyePosition()
        {
            if (eyeTransform != null) return eyeTransform.position;
            return transform.TransformPoint(eyeOffset);
        }

        private Vector3 GetEyeForward()
        {
            if (eyeTransform != null)
            {
                return eyeTransform.rotation * Quaternion.Euler(eyeRotationOffset) * Vector3.forward;
            }
            return transform.rotation * Quaternion.Euler(eyeRotationOffset) * Vector3.forward;
        }

        private Vector3 GetEyeUp()
        {
            if (eyeTransform != null)
            {
                return eyeTransform.rotation * Quaternion.Euler(eyeRotationOffset) * Vector3.up;
            }
            return transform.rotation * Quaternion.Euler(eyeRotationOffset) * Vector3.up;
        }

#if UNITY_EDITOR
        public void DrawGizmosSelected()
        {
            Vector3 eyePos = GetEyePosition();
            Vector3 forward = GetEyeForward();
            Vector3 up = GetEyeUp();

            // Draw View Arc from the eye position, oriented with the eye's up vector
            UnityEditor.Handles.color = new Color(1, 1, 0, 0.2f);
            Vector3 initialPos = Quaternion.AngleAxis(-viewAngle / 2f, up) * forward;
            UnityEditor.Handles.DrawSolidArc(eyePos, up, initialPos, viewAngle, viewRadius);

            // Outline of the arc for better readability
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireArc(eyePos, up, initialPos, viewAngle, viewRadius);
            UnityEditor.Handles.DrawLine(eyePos, eyePos + initialPos * viewRadius);
            UnityEditor.Handles.DrawLine(eyePos, eyePos + Quaternion.AngleAxis(viewAngle / 2f, up) * forward * viewRadius);

            // Draw Eye Pos
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(eyePos, 0.1f);
        }

        private void OnDrawGizmosSelected() { DrawGizmosSelected(); }
#endif
    }
}
