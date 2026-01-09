// Infrastructure/NpcController.cs

using UnityEngine;

namespace Infrastructure
{
    public class NpcController : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
                FindFirstObjectByType<GameManager>().PlayerCaught();
        }
    }
}