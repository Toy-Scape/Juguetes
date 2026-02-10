using UnityEngine;

namespace CheckpointSystem
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Feedback")]
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private AudioSource activationAudio;
        [SerializeField] private Renderer visualRenderer;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;

        [Header("Settings")]
        [SerializeField] private float deadZoneOffset = 10f;

        public float DeadZoneOffset => deadZoneOffset;

        private bool isActivated = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isActivated) return;

            if (other.CompareTag("Player"))
            {
                ActivateCheckpoint();
            }
        }
        private void OnTriggerStay(Collider other)
        {
            var item = other.GetComponent<ResettableItem>();
            if (item != null)
            {
                item.IsTouchingCheckpoint = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var item = other.GetComponent<ResettableItem>();
            if (item != null)
            {
                item.IsTouchingCheckpoint = false;
            }
        }

        private void ActivateCheckpoint()
        {
            isActivated = true;

            // Visual/Audio Feedback
            if (activationParticles != null) activationParticles.Play();
            if (activationAudio != null) activationAudio.Play();
            if (visualRenderer != null && activeMaterial != null)
            {
                visualRenderer.material = activeMaterial;
            }

            // Notify Manager
            CheckpointManager.Instance.SetCheckpoint(this);

            Debug.Log($"Checkpoint {gameObject.name} activated!");
        }

        public void Deactivate()
        {
            isActivated = false;
            if (visualRenderer != null && inactiveMaterial != null)
            {
                visualRenderer.material = inactiveMaterial;
            }
        }

        // Helper to visualize in editor
        private void OnDrawGizmos()
        {
            Gizmos.color = isActivated ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
