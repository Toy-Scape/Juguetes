using InteractionSystem.Interfaces;
using UnityEngine;

namespace CheckpointSystem
{
    [RequireComponent(typeof(Collider))]
    public class DeadZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // 1. Check for Player
            if (other.GetComponent<PlayerController>() != null || other.GetComponentInParent<PlayerController>() != null)
            {
                CheckpointManager.Instance.RespawnPlayer();
                return;
            }

            // 2. Check for Resettable Objects
            var resettable = other.GetComponentInParent<IResettable>();
            if (resettable != null)
            {
                resettable.ResetState();
            }
        }
    }
}
