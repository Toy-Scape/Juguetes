using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI_System.Menus
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _optionsPanel;

        [Header("Scene Configuration")]
        [SerializeField] private string _gameSceneName = "SC_GamePlay"; // Update with actual Game Scene name

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Determine if we are in Main Menu mode or Pause Menu mode
            // If loaded alone -> Main Menu
            // If loaded additively -> Pause Menu
            if (SceneManager.sceneCount == 1)
            {
                ShowMainMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }

        private void Start()
        {
            // Failsafe: Always unlock cursor when MenuManager starts (it's a menu!)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (InputMapManager.Instance != null)
            {
                InputMapManager.Instance.HandleOpenUI();
            }
            // Fallback if InputMapManager is not present (e.g. testing Menu scene alone without bootstrap)
            // ... already handled above
        }

        private void OnDestroy()
        {
            if (InputMapManager.Instance != null)
            {
                InputMapManager.Instance.HandleCloseUI();
            }
        }

        private void ShowMainMenu()
        {
            _mainMenuPanel.SetActive(true);
            _pauseMenuPanel.SetActive(false);
            _optionsPanel.SetActive(false);
        }

        private void ShowPauseMenu()
        {
            _mainMenuPanel.SetActive(false);
            _pauseMenuPanel.SetActive(true);
            _optionsPanel.SetActive(false);
        }

        // --- Main Menu Functions ---

        public void PlayGame()
        {
            // Load Game Scene in Single mode (replacing the Menu Scene)
            // Or if we are in a dedicated Menu scene, we strictly load the Game Scene.
            SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting Game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // --- Pause Menu Functions ---

        public void ResumeGame()
        {
            // Unload this scene (SC_Menus) to resume the game
            // The GamePauseHandler in the game scene will detect the unload and resume time.
            SceneManager.UnloadSceneAsync(gameObject.scene);
        }

        public void LoadMainMenu()
        {
            // To "Go to Main Menu" from Pause, we essentially reload the Menu Scene as a Single scene
            // This unloads the Game Scene.
            SceneManager.LoadScene(gameObject.scene.name, LoadSceneMode.Single);
            Time.timeScale = 1f; // Ensure time is reset
        }

        // --- Shared Options Functions ---

        public void OpenOptions()
        {
            _optionsPanel.SetActive(true);
            // Keep the previous panel active behind it, or disable it?
            // Usually disable to avoid navigation issues, but for visual overlap keep enabled.
            // Let's disable for simplicity of input.
            if (_mainMenuPanel.activeSelf) _mainMenuPanel.SetActive(false);
            if (_pauseMenuPanel.activeSelf) _pauseMenuPanel.SetActive(false);
        }

        public void BackFromOptions()
        {
            _optionsPanel.SetActive(false);

            // Return to whichever mode we are in (Scene count check again)
            if (SceneManager.sceneCount == 1)
            {
                _mainMenuPanel.SetActive(true);
            }
            else
            {
                _pauseMenuPanel.SetActive(true);
            }
        }
    }
}
