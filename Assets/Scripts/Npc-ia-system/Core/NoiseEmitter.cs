using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Collider))] // Usually needs a collider to hit things
    public class NoiseEmitter : MonoBehaviour
    {
        [Tooltip("Base noise range.")]
        public float noiseRange = 10f;

        [Tooltip("Minimum relative velocity to trigger noise.")]
        public float minVelocity = 2.0f;

        [Tooltip("Cooldown to prevent spamming noise every frame.")]
        public float cooldown = 0.5f;

        private float _lastNoiseTime;

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time < _lastNoiseTime + cooldown) return;

            // Check impact strength
            if (collision.relativeVelocity.magnitude >= minVelocity)
            {
                // Emit noise
                // Optionally scale range by velocity?
                float visualRange = noiseRange * (collision.relativeVelocity.magnitude / minVelocity);
                visualRange = Mathf.Clamp(visualRange, noiseRange, noiseRange * 2f);

                NoiseManager.MakeNoise(transform.position, visualRange);
                _lastNoiseTime = Time.time;
            }
        }

        // Allow manual trigger (e.g. from Animation Event)
        public void EmitManualNoise(float range)
        {
            NoiseManager.MakeNoise(transform.position, range);
        }
    }
}
