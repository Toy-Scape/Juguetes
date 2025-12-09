using UnityEngine;

namespace CheckpointSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRespawn : MonoBehaviour
    {
        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            // Disable CharacterController to allow manual position update
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.position = position;
            transform.rotation = rotation;

            // Re-enable CharacterController
            if (characterController != null)
            {
                characterController.enabled = true;
            }

            Debug.Log("Player Respawned at: " + position);
        }

        // Test method to kill player
        [ContextMenu("Kill Player")]
        public void KillPlayer()
        {
            CheckpointManager.Instance .RespawnPlayer();
        }
    }
}
