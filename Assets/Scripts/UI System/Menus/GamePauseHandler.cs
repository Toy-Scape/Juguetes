using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI_System.Menus
{
    public class GamePauseHandler : MonoBehaviour
    {
        [SerializeField] private string _menuSceneName = "_Menu";
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerSnapshot _gameplaySnapshot;
        [SerializeField] private AudioMixerSnapshot _pausedSnapshot;
        [SerializeField] private float _snapshotTransitionTime = 0.2f;

        public static bool IsPaused { get; private set; }

        private bool _isLoading;
        private float _lastToggleTime;
        private const float ToggleCooldown = 0.2f;

        private InputAction _pauseAction;

        private void Awake()
        {
            IsPaused = false;
            _gameplaySnapshot?.TransitionTo(0f);
        }

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
            IsPaused = false;
        }

        private void SubscribeToInput()
        {
            if (_pauseAction != null) return;

            var playerInput = FindFirstObjectByType<PlayerInput>();
            if (playerInput == null || playerInput.actions == null) return;

            _pauseAction = playerInput.actions.FindAction("Pause");
            if (_pauseAction != null)
                _pauseAction.canceled += OnPausePerformed;
        }

        private void UnsubscribeFromInput()
        {
            if (_pauseAction != null)
            {
                _pauseAction.canceled -= OnPausePerformed;
                _pauseAction = null;
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        public void TogglePause()
        {
            if (_isLoading || Time.unscaledTime < _lastToggleTime + ToggleCooldown) return;
            _lastToggleTime = Time.unscaledTime;

            Scene menuScene = SceneManager.GetSceneByName(_menuSceneName);
            if (menuScene.isLoaded)
            {
                MenuManager manager = null;
                foreach (var root in menuScene.GetRootGameObjects())
                {
                    manager = root.GetComponentInChildren<MenuManager>();
                    if (manager != null) break;
                }

                if (manager != null)
                    manager.HandleBackInput();
                else
                    SceneManager.UnloadSceneAsync(_menuSceneName);
            }
            else
            {
                IsPaused = true;
                Time.timeScale = 0f;
                _isLoading = true;
                SceneManager.LoadSceneAsync(_menuSceneName, LoadSceneMode.Additive);
                _pausedSnapshot?.TransitionTo(_snapshotTransitionTime);

                InputMapManager.Instance?.HandleOpenUI();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != _menuSceneName) return;

            _isLoading = false;
            IsPaused = true;

            foreach (var root in scene.GetRootGameObjects())
            {
                var manager = root.GetComponentInChildren<MenuManager>();
                if (manager != null)
                {
                    manager.OpenAsPauseMenu();
                    break;
                }
            }
        }

        private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name != _menuSceneName) return;

        var menuMusic = FindFirstObjectByType<MenuMusicFader>();
        if (menuMusic != null)
            menuMusic.FadeOutAndStop();

        Time.timeScale = 1f;
        _gameplaySnapshot?.TransitionTo(_snapshotTransitionTime);
        InputMapManager.Instance?.HandleCloseUI();
        StartCoroutine(DelayUnpause());
    }


        private IEnumerator DelayUnpause()
        {
            IsPaused = true;
            yield return null;
            IsPaused = false;
        }
    }
}
