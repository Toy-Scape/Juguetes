using UnityEngine;

namespace CheckpointSystem
{
    [RequireComponent(typeof(Collider))]
    public class DeadZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // TryGetComponent es más eficiente que GetComponent.
            if (other.TryGetComponent<PlayerController>(out _) ||
                other.GetComponentInParent<PlayerController>() != null)
            {
                CheckpointManager.Instance.RespawnPlayer();
                return;
            }

            var resettable = other.GetComponentInParent<IResettable>();
            if (resettable != null)
            {
                if (resettable is ResettableItem item && item.IsTouchingCheckpoint) return;
                resettable.ResetState();
            }
        }
    }
}
