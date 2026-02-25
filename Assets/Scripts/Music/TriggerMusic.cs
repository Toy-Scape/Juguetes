using UnityEngine;

public class TriggerMusic : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<MusicManager>().ChangeToMusicB();
        }
    }
}