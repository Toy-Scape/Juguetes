using CinematicSystem.Application;
using CinematicSystem.Core;
using UnityEngine;

public class IngameCinematicTrigger : MonoBehaviour
{
    [SerializeField] private CinematicAsset cinematic;

    private void OnTriggerExit(Collider other)
    {
        var player = FindFirstObjectByType<CinematicPlayer>();

        if (player != null)
        {
            player.Play(cinematic);
            this.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[PlayCinematicAction] No CinematicPlayer found in the scene.");
        }
        
    }
}
