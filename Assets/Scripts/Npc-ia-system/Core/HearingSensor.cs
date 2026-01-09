using System;
using UnityEngine;

namespace Core
{
    public class HearingSensor : MonoBehaviour
    {
        [Header("Hearing Settings")]
        [Tooltip("Multiplies the incoming noise range. >1 means sensitive hearing, <1 means hard of hearing.")]
        public float hearingSensitivity = 1.0f;

        [Tooltip("Minimum volume/range required to trigger a reaction.")]
        public float hearingThreshold = 2.0f;

        // Internal event for NpcBrain to subscribe to (not exposed in Inspector)
        public event Action<Vector3> OnNoiseDetected;

        private void OnEnable()
        {
            NoiseManager.OnNoiseEmitted += HandleNoise;
        }

        private void OnDisable()
        {
            NoiseManager.OnNoiseEmitted -= HandleNoise;
        }

        private void HandleNoise(Vector3 pos, float range)
        {
            // Calculate effective range based on sensitivity
            float effectiveRange = range * hearingSensitivity;

            float dist = Vector3.Distance(transform.position, pos);

            if (dist <= effectiveRange && effectiveRange >= hearingThreshold)
            {
                OnNoiseDetected?.Invoke(pos);
            }
        }
    }
}
