using UnityEngine;

namespace CheckpointSystem
{
    public class ResettableItem : MonoBehaviour, IResettable
    {
        [SerializeField] private Vector3 resetOffset = new Vector3(0, 1, 0);
        [SerializeField] private Vector3 resetRotation = Vector3.zero;
        public bool IsTouchingCheckpoint { get; set; }

        public void ResetState()
        {
            // If the item is currently picked, do not reset it (it stays with the player)
            var pickable = GetComponent<Pickable>();
            if (pickable != null && pickable.IsPicked) return;

            if (CheckpointManager.Instance != null)
            {
                // Reset position to last checkpoint
                transform.position = CheckpointManager.Instance.GetLastCheckpointPosition() + resetOffset;
                transform.rotation = Quaternion.Euler(resetRotation);

                // Reset velocity if rigidbody exists
                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"ResettableItem {name} reset to checkpoint position.");
            }
        }
    }
}
