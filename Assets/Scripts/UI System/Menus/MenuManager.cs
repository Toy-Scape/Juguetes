using CheckpointSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace UI_System.Menus
{
    public enum MenuStartMode
    {
        MainMenu,
        PauseMenu
    }

    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _optionsPanel;

        [SerializeField] private GameObject _mainMenuFirstSelected;
        [SerializeField] private GameObject _pauseMenuFirstSelected;
        [SerializeField] private GameObject _optionsFirstSelected;

        [SerializeField] private PauseMenuBlur pauseMenuBlur;
        [SerializeField] private GameObject[] LightSources;

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
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
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
    }
}
