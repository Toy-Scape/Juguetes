// Infrastructure/GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure
{
    public class GameManager : MonoBehaviour
    {
        public void PlayerCaught()
        {
            Debug.Log("¡Has perdido!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}