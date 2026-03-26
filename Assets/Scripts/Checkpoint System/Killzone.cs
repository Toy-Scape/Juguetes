using UnityEngine;
using CheckpointSystem;

public class KillZoneCustomRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        var respawn = player.GetComponent<PlayerRespawn>();
        if (respawn != null)
        {
            respawn.Respawn(respawnPoint.position, respawnPoint.rotation);
        }
    }
}