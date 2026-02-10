using UnityEngine;

namespace CheckpointSystem
{
    [RequireComponent(typeof(Collider))]
    public class DeadZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            HandleTrigger(other);
        }

        // Un item que esté tocando el checkpoint no se va a resetear al entrar en el deadzone.
        //  Si este objeto que está tocando el deadzone se cae de alguna forma del checkpoint,
        //  no va a accionar el deadzone porque ya habia entrado. 
        // Añado el OnTriggerStay para corregirlo.
        private void OnTriggerStay(Collider other)
        {
            HandleTrigger(other);
        }

        private void HandleTrigger(Collider other)
        {
            // Player
            if (other.TryGetComponent<PlayerController>(out _) ||
                other.GetComponentInParent<PlayerController>() != null)
            {
                CheckpointManager.Instance.RespawnPlayer();
                return;
            }

            // Resettable
            var resettable = other.GetComponentInParent<IResettable>();
            if (resettable != null)
            {
                // Si es un item y sigue tocando el checkpoint - NO resetear
                if (resettable is ResettableItem item && item.IsTouchingCheckpoint)
                    return;

                // Si NO está tocando el checkpoint - resetear
                resettable.ResetState();
            }
        }
    }
}
