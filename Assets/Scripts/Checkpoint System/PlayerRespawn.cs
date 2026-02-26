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
            
            // Force Cinematic system to stop and release cameras
            var cinematicPlayer = FindFirstObjectByType<CinematicSystem.Application.CinematicPlayer>();
            if (cinematicPlayer != null && cinematicPlayer.IsPlaying)
            {
                cinematicPlayer.Stop();
            }

            // Force Camera Manager to reset to normal gameplay cameras
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.ResetCamerasToGameplay();
            }

            // Force all CinemachineTriggers to reset their active status. This fixes cameras 
            // staying at priority 1000 if the player is killed inside a trigger and respawns outside.
            CinemachineTrigger.ResetAllTriggers();

            Debug.Log("Player Respawned at: " + position);
        }

        // Test method to kill player
        [ContextMenu("Kill Player")]
        public void KillPlayer()
        {
            CheckpointManager.Instance.RespawnPlayer();
        }
    }
}
