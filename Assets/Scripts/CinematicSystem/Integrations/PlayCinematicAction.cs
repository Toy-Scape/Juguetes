using CinematicSystem.Application;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Integrations
{
    [CreateAssetMenu(fileName = "PlayCinematicAction", menuName = "Dialogue System/Actions/Play Cinematic")]
    public class PlayCinematicAction : ActionBase
    {
        [Tooltip("The cinematic asset to play.")]
        [SerializeField] private CinematicAsset cinematic;

        public override void Execute(DialogueContext context)
        {
            if (cinematic == null)
            {
                Debug.LogWarning("[PlayCinematicAction] No cinematic assigned.");
                return;
            }

            // Find the player in the scene. 
            // In a more complex setup, we might resolve this via a ServiceLocator or Singleton.
            var player = FindFirstObjectByType<CinematicPlayer>();

            if (player != null)
            {
                player.Play(cinematic);
            }
            else
            {
                Debug.LogError("[PlayCinematicAction] No CinematicPlayer found in the scene.");
            }
        }
    }
}
