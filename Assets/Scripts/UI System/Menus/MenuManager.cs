using System.Collections;
using CheckpointSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI_System.Menus
{
    public enum MenuStartMode
    {
        MainMenu,
        PauseMenu
    }

    public class MenuManager : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (_uiCanvasGroup != null)
                _uiCanvasGroup.DOKill();
        }

        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _optionsPanel;

        [SerializeField] private GameObject _mainMenuFirstSelected;
        [SerializeField] private GameObject _pauseMenuFirstSelected;
        [SerializeField] private GameObject _optionsFirstSelected;

        [SerializeField] private PauseMenuBlur pauseMenuBlur;
        [SerializeField] private GameObject[] LightSources;
        [SerializeField] private CanvasGroup _uiCanvasGroup;
        [SerializeField] private float _fadeOutDuration = 1f;
        [SerializeField] private float _delayBeforeLoad = 2f;
        [SerializeField] private float _crossFadeDuration = 1f;
        // Removed _nextSceneFrame as we now do a simple Fade-To-Black

        [SerializeField] private Transform _cameraTarget;
        [SerializeField] private float _cameraMoveDuration = 2f;
        [SerializeField] private float _targetOrthographicSize = 5f;

        [SerializeField] private Camera _menuCamera;
        [SerializeField] private string _sceneToPreload;
        [SerializeField] private GameObject menuBackground;

        private void Awake()
        {
            ShowMainMenu();
        }

        private void Start()
        {
            // Only preload if we are in the actual Menu scene (Main Menu mode).
            // If we are loaded additively (Pause/Options), the active scene will be the Game Scene.
            if (SceneManager.GetActiveScene().name == gameObject.scene.name)
            {
                PreloadScene();
            }
        }

        private void PreloadScene()
        {
            if (!string.IsNullOrEmpty(_sceneToPreload))
            {
                var transitionManager = CinematicSystem.Transitions.SceneTransitionManager.Instance;
                // If we are in the Main Menu, we likely need to create the manager.
                if (transitionManager == null)
                {
                    GameObject go = new GameObject("SceneTransitionManager");
                    transitionManager = go.AddComponent<CinematicSystem.Transitions.SceneTransitionManager>();
                }
                transitionManager.PreloadScene(_sceneToPreload);
            }
        }

        public void StartIn(MenuStartMode mode)
        {
            if (mode == MenuStartMode.PauseMenu)
                ShowPauseMenu();
            else
                ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            menuBackground.SetActive(true);
            var music = FindFirstObjectByType<MenuMusicFader>();
            if (music != null)
                music.FadeIn();
            if (_menuCamera != null)
                _menuCamera.gameObject.SetActive(true);

            _mainMenuPanel.SetActive(true);
            _pauseMenuPanel.SetActive(false);
            _optionsPanel.SetActive(false);
            SetFirstSelected(_mainMenuFirstSelected);
        }

        public void ShowPauseMenu()
        {
            _mainMenuPanel.SetActive(false);
            _pauseMenuPanel.SetActive(true);
            _optionsPanel.SetActive(false);
            menuBackground.SetActive(false);
            SetFirstSelected(_pauseMenuFirstSelected);
        }

        public void ShowOptions()
        {
            _mainMenuPanel.SetActive(false);
            _pauseMenuPanel.SetActive(false);
            _optionsPanel.SetActive(true);
            SetFirstSelected(_optionsFirstSelected);
        }

        public void HandleBackInput()
        {
            if (_optionsPanel.activeSelf)
            {
                if (GamePauseHandler.IsPaused)
                    ShowPauseMenu();
                else
                    ShowMainMenu();
            }
            else
            {
                CloseMenu();
            }
        }

        public void CloseMenu()
        {
            if (InputMapManager.Instance != null)
                InputMapManager.Instance.HandleCloseUI();

            SceneManager.UnloadSceneAsync(gameObject.scene);
        }


        public void OnPlayClicked()
        {
            if (string.IsNullOrEmpty(_sceneToPreload))
            {
                Debug.LogError("[MenuManager] Scene to preload is empty! Please assign it in the Inspector.");
                return;
            }

            // Stop music logic from develop
            var music = FindFirstObjectByType<MenuMusicFader>();
            if (music != null)
                music.FadeOutAndStop();

            StartCoroutine(TransitionRoutine(_sceneToPreload));

            // Ensure time scale is running for tweens and load
            Time.timeScale = 1f;
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            Debug.Log("[Transition] Step 1: Fading UI out.");
            if (_uiCanvasGroup != null)
            {
                _uiCanvasGroup.interactable = false;
                _uiCanvasGroup.blocksRaycasts = false;
                _uiCanvasGroup.DOFade(0f, _fadeOutDuration).SetEase(Ease.InOutQuad).SetUpdate(true).SetLink(gameObject);
            }

            // Move Camera if target is assigned
            if (_menuCamera != null && _cameraTarget != null)
            {
                Debug.Log("[Transition] Moving Camera & Changing Projection.");
                _menuCamera.transform.DOMove(_cameraTarget.position, _cameraMoveDuration).SetEase(Ease.InOutQuad).SetUpdate(true).SetLink(gameObject);
                _menuCamera.transform.DORotate(_cameraTarget.rotation.eulerAngles, _cameraMoveDuration).SetEase(Ease.InOutQuad).SetUpdate(true).SetLink(gameObject);

                // Projection Transition (Perspective -> Orthographic)
                Matrix4x4 perspectiveMatrix = _menuCamera.projectionMatrix;
                float aspect = _menuCamera.aspect;
                float orthoSize = _targetOrthographicSize;
                float near = _menuCamera.nearClipPlane;
                float far = _menuCamera.farClipPlane;

                // Calculate target Orthographic Matrix
                Matrix4x4 orthoMatrix = Matrix4x4.Ortho(-orthoSize * aspect, orthoSize * aspect, -orthoSize, orthoSize, near, far);

                // Disable orthographic mode to allow manual matrix manipulation
                _menuCamera.orthographic = false;

                DOVirtual.Float(0f, 1f, _cameraMoveDuration, t =>
                {
                    if (_menuCamera != null)
                        _menuCamera.projectionMatrix = MatrixLerp(perspectiveMatrix, orthoMatrix, t);
                }).SetEase(Ease.InOutQuad).SetUpdate(true).SetLink(gameObject).OnComplete(() =>
                {
                    // Snap to actual orthographic mode at the end
                    if (_menuCamera != null)
                    {
                        _menuCamera.orthographic = true;
                        _menuCamera.orthographicSize = _targetOrthographicSize;
                        _menuCamera.ResetProjectionMatrix();
                    }
                });
            }

            // Wait for camera movement
            float waitTime = (_menuCamera != null && _cameraTarget != null) ? _cameraMoveDuration : _delayBeforeLoad;
            Debug.Log($"[Transition] Step 2: Waiting {waitTime} seconds.");
            yield return new WaitForSecondsRealtime(waitTime);

            // Use the shared SceneTransitionManager
            // Use the shared SceneTransitionManager
            var transitionManager = CinematicSystem.Transitions.SceneTransitionManager.Instance;
            if (transitionManager == null)
            {
                Debug.Log("[MenuManager] SceneTransitionManager missing. Creating one.");
                GameObject go = new GameObject("SceneTransitionManager");
                transitionManager = go.AddComponent<CinematicSystem.Transitions.SceneTransitionManager>();
            }

            transitionManager.CrossfadeToScene(sceneName, _crossFadeDuration);
        }


        public void OnOptionsClicked()
        {
            ShowOptions();
        }

        public void OnResumeClicked()
        {
            CloseMenu();
        }

        public void OnBackFromOptionsClicked()
        {
            HandleBackInput();
        }

        public void OnBackToMenuClicked1()
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene("SC_OptionsMenu", LoadSceneMode.Single);
        }


        public void OnBackToMenuClicked()
        {
            Time.timeScale = 1f;

            SceneManager.LoadScene("SC_OptionsMenu", LoadSceneMode.Single);
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }

        private void SetFirstSelected(GameObject go)
        {
            if (EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(go);
        }


        public void OpenAsPauseMenu()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var music = FindFirstObjectByType<MenuMusicFader>();
            if (music != null)
                music.FadeIn();


            if (InputMapManager.Instance != null)
                InputMapManager.Instance.HandleOpenUI();

            if (_menuCamera != null)
                _menuCamera.gameObject.SetActive(false);
            pauseMenuBlur.OpenMenu();
            DeactivateLightSources();
            StartIn(MenuStartMode.PauseMenu);
        }


        private void DeactivateLightSources()
        {
            foreach (var light in LightSources)
            {
                if (light != null)
                    light.SetActive(false);
            }
        }

        private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float time)
        {
            Matrix4x4 ret = new Matrix4x4();
            for (int i = 0; i < 16; i++)
                ret[i] = Mathf.Lerp(from[i], to[i], time);
            return ret;
        }
    }
}
