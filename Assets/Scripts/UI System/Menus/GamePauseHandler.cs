using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI_System.Menus
{
    public class GamePauseHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _menuSceneName = "SC_Menus";

        // This method is called by PlayerInput via "Send Messages" 
        // Ensure "Behavior" in PlayerInput is set to "Send Messages" or use Unity Events and link to TogglePause
        public void OnPause(InputValue value)
        {
            if (value.isPressed)
            {
                TogglePause();
            }
        }

        // Public method to be called via Unity Events or other scripts
        public void TogglePause()
        {
            // If the menu is already loaded, do nothing (pause logic is usually handled by the menu itself to resume)
            if (SceneManager.sceneCount > 1) return;

            Debug.Log("Game Paused");
            Time.timeScale = 0f;

            // Load the Menu Scene Additively
            SceneManager.LoadSceneAsync(_menuSceneName, LoadSceneMode.Additive);

            // Note: PlayerInput usually handles switching action maps if configured, 
            // otherwise you might need to manually disable gameplay inputs here if strictly needed.
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.name == _menuSceneName)
            {
                Debug.Log("Game Resumed");
                Time.timeScale = 1f;
            }
        }
    }
}
