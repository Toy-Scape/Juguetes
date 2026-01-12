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

        private bool _isLoading = false;
        private float _lastToggleTime = 0f;
        private const float ToggleCooldown = 0.2f;

        // Public method to be called via Unity Events or other scripts
        public void TogglePause()
        {
            if (_isLoading || Time.unscaledTime < _lastToggleTime + ToggleCooldown) return;
            _lastToggleTime = Time.unscaledTime;

            // Check if the menu scene is already loaded
            Scene menuScene = SceneManager.GetSceneByName(_menuSceneName);

            if (menuScene.isLoaded)
            {
                // If loaded, try to handle "Back" input via MenuManager
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.HandleBackInput();
                }
                else
                {
                    // Fallback
                    _isLoading = true;
                    SceneManager.UnloadSceneAsync(_menuSceneName);
                }
            }
            else
            {
                Debug.Log("Game Paused");
                Time.timeScale = 0f;

                // Load the Menu Scene Additively
                _isLoading = true;
                SceneManager.LoadSceneAsync(_menuSceneName, LoadSceneMode.Additive);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == _menuSceneName)
            {
                _isLoading = false;
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.name == _menuSceneName)
            {
                Debug.Log("Game Resumed");
                Time.timeScale = 1f;

                // Restore Player controls and Lock Cursor
                if (InputMapManager.Instance != null)
                {
                    InputMapManager.Instance.SwitchToActionMap(ActionMaps.Player);
                }
            }
        }
    }
}
