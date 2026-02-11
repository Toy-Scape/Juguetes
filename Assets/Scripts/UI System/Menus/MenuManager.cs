using CheckpointSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

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

        private void Awake()
        {
            ShowMainMenu();
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


        public void OnPlayClicked(string scene)
        {
            StartCoroutine(TransitionRoutine(scene));
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

            // Store current scene reference
            Scene currentScene = gameObject.scene;

            // --- TRUE CROSSFADE ---
            // 1. Load the new scene additively (it coexists with the old one)
            Debug.Log($"[Transition] Step 3: Loading scene '{sceneName}' additively.");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return asyncLoad;

            Scene newScene = SceneManager.GetSceneByName(sceneName);

            // 2. Find the new scene's camera and disable it temporarily
            Camera newSceneCamera = null;
            foreach (var root in newScene.GetRootGameObjects())
            {
                newSceneCamera = root.GetComponentInChildren<Camera>(true);
                if (newSceneCamera != null) break;
            }

            if (newSceneCamera != null)
            {
                // Disable the new camera so it doesn't render to screen yet
                newSceneCamera.enabled = false;

                // 3. Create a RenderTexture and make the new camera render to it
                RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
                newSceneCamera.targetTexture = rt;
                newSceneCamera.enabled = true;

                // 4. Create overlay showing the RenderTexture (starts invisible)
                Debug.Log("[Transition] Step 4: Creating crossfade overlay.");
                GameObject overlayObj = new GameObject("CrossfadeOverlay");
                DontDestroyOnLoad(overlayObj);

                Canvas canvas = overlayObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 32767;
                overlayObj.AddComponent<CanvasScaler>();
                overlayObj.AddComponent<GraphicRaycaster>();

                GameObject imageObj = new GameObject("NewSceneImage");
                imageObj.transform.SetParent(overlayObj.transform, false);

                RawImage rawImage = imageObj.AddComponent<RawImage>();
                rawImage.texture = rt;

                RectTransform imageRt = imageObj.GetComponent<RectTransform>();
                imageRt.anchorMin = Vector2.zero;
                imageRt.anchorMax = Vector2.one;
                imageRt.sizeDelta = Vector2.zero;

                CanvasGroup overlayCG = imageObj.AddComponent<CanvasGroup>();
                overlayCG.alpha = 0f; // Starts invisible (old scene is fully visible)
                overlayCG.blocksRaycasts = true;

                // 5. Crossfade: Fade IN the new scene overlay (old scene fades away underneath)
                Debug.Log("[Transition] Step 5: Crossfading to new scene.");
                Tween crossfade = overlayCG.DOFade(1f, _crossFadeDuration).SetEase(Ease.InOutQuad).SetUpdate(true);
                yield return crossfade.WaitForCompletion();

                // 6. Swap: New camera renders to screen, remove overlay
                Debug.Log("[Transition] Step 6: Swapping cameras.");
                newSceneCamera.targetTexture = null; // Render to screen now
                SceneManager.SetActiveScene(newScene);

                // Cleanup
                rt.Release();
                Destroy(rt);
                Destroy(overlayObj);
            }
            else
            {
                // Fallback if no camera found in new scene
                Debug.LogWarning("[Transition] No camera found in new scene, switching directly.");
                SceneManager.SetActiveScene(newScene);
            }

            // 7. Unload old scene
            Debug.Log($"[Transition] Step 7: Unloading old scene '{currentScene.name}'.");
            SceneManager.UnloadSceneAsync(currentScene);
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
