using Core;
using UnityEngine;

namespace Domain
{
    /// <summary>
    /// Example component to make the Player (or any object) detectable by the NPC VisionSensor using the new interface system.
    /// allows specifying a precise target center (e.g. Spine/Chest) for better visibility checks.
    /// </summary>
    public class PlayerTarget : MonoBehaviour, IVisibleTarget
    {
        [Tooltip("Transform to target for visibility (e.g. Chest/Head). If null, uses this transform.")]
        public Transform targetCenter;

        [Tooltip("Is this target currently valid? (e.g. not dead, not invisible)")]
        public bool isValid = true;

        public Vector3 TargetPosition
        {
            get
            {
                if (targetCenter != null) return targetCenter.position;
                // Default to a slight offset up if no center defined, to avoid feet-targeting issues if user forgets
                return transform.position + Vector3.up * 1.5f;
            }
        }

        public bool IsValid => isValid && gameObject.activeInHierarchy;

        public Transform Transform => transform;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(TargetPosition, 0.2f);
        }
#endif
    }
}
