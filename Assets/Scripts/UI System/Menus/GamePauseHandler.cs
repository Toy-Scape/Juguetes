using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

namespace UI_System.Menus
{
    public class GamePauseHandler : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _menuSceneName = "SC_Menus";

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerSnapshot _gameplaySnapshot;
        [SerializeField] private AudioMixerSnapshot _pausedSnapshot;
        [SerializeField] private float _snapshotTransitionTime = 0.2f;


        // Static property to check if the game is paused
        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            // Reset pause state when this component (usually in the main scene) loads
            IsPaused = false;

            _gameplaySnapshot?.TransitionTo(0f);
        }

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
                IsPaused = true;
                Time.timeScale = 0f;

                // Load the Menu Scene Additively
                _isLoading = true;
                SceneManager.LoadSceneAsync(_menuSceneName, LoadSceneMode.Additive);

                _pausedSnapshot?.TransitionTo(_snapshotTransitionTime);
            }
        }

        private InputAction _pauseAction;

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;

            SubscribeToInput();
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;

            UnsubscribeFromInput();

            // Ensure IsPaused is reset if this component is disabled
            // This might happen on scene transitions
            IsPaused = false;
        }

        private void SubscribeToInput()
        {
            if (_pauseAction != null) return;

            // Try to find PlayerInput to get the action
            var playerInput = FindFirstObjectByType<PlayerInput>();
            if (playerInput == null || playerInput.actions == null) return;

            // Prefer globally finding the action or fallback
            // Try explicit map first if needed, but generic search is usually okay
            _pauseAction = playerInput.actions.FindAction("Pause"); // safer than indexer
            if (_pauseAction == null) _pauseAction = playerInput.actions["Pause"]; // Fallback
            if (_pauseAction != null)
            {
                _pauseAction.performed += OnPausePerformed;
            }
        }

        private void UnsubscribeFromInput()
        {
            if (_pauseAction != null)
            {
                _pauseAction.performed -= OnPausePerformed;
                _pauseAction = null;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == _menuSceneName)
            {
                _isLoading = false;
                IsPaused = true; // Ensure it is true when menu is loaded
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.name == _menuSceneName)
            {
                Debug.Log("Game Resumed");
                IsPaused = false;
                Time.timeScale = 1f;

                _gameplaySnapshot?.TransitionTo(_snapshotTransitionTime);

                // Restore Player controls and Lock Cursor
                if (InputMapManager.Instance != null)
                {
                    InputMapManager.Instance.SwitchToActionMap(ActionMaps.Player);
                }
            }
        }
    }
}
