using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_System.Menus
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("Video")]
        [SerializeField] private TMP_Dropdown _fpsLimitDropdown;

        [Header("Controls")]
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI _mouseSensitivityValueText;
        [SerializeField] private Slider _gamepadSensitivitySlider;
        [SerializeField] private TextMeshProUGUI _gamepadSensitivityValueText;

        [Header("Audio")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private TextMeshProUGUI _volumeValueText;

        [Header("Language")]
        [SerializeField] private TMP_Dropdown _languageDropdown;

        private void Start()
        {
            // Populate FPS Dropdown
            if (_fpsLimitDropdown != null)
            {
                _fpsLimitDropdown.ClearOptions();
                var fpsOptions = new System.Collections.Generic.List<string> { "30 FPS", "60 FPS", "120 FPS", "Unlimited" };
                _fpsLimitDropdown.AddOptions(fpsOptions);

                int savedFpsIndex = PlayerPrefs.GetInt("FpsLimitIndex", 3); // Default to Unlimited (index 3)
                _fpsLimitDropdown.value = savedFpsIndex;
                _fpsLimitDropdown.onValueChanged.AddListener(SetFpsLimit);

                // Apply immediately on start
                SetFpsLimit(savedFpsIndex);
            }

            // Initialize Sensitivity
            if (_mouseSensitivitySlider != null)
            {
                _mouseSensitivitySlider.minValue = 0.1f;
                _mouseSensitivitySlider.maxValue = 5.0f;
                float savedMouseSens = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
                _mouseSensitivitySlider.value = savedMouseSens;
                _mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
                UpdateMouseSensitivityText(savedMouseSens);
            }

            if (_gamepadSensitivitySlider != null)
            {
                _gamepadSensitivitySlider.minValue = 0.1f;
                _gamepadSensitivitySlider.maxValue = 5.0f;
                float savedGamepadSens = PlayerPrefs.GetFloat("GamepadSensitivity", 1f);
                _gamepadSensitivitySlider.value = savedGamepadSens;
                _gamepadSensitivitySlider.onValueChanged.AddListener(SetGamepadSensitivity);
                UpdateGamepadSensitivityText(savedGamepadSens);
            }

            // Populate Language Dropdown
            if (_languageDropdown != null)
            {
                _languageDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string> { "English", "Español", "Euskera" };
                _languageDropdown.AddOptions(options);

                int savedLanguage = PlayerPrefs.GetInt("LanguageIndex", 0);
                _languageDropdown.value = savedLanguage;
                _languageDropdown.onValueChanged.AddListener(SetLanguage);
            }

            // Initialize Volume
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (_volumeSlider != null)
            {
                _volumeSlider.value = savedVolume;
                _volumeSlider.onValueChanged.AddListener(SetVolume);
                UpdateVolumeText(savedVolume);
            }
        }

        public void SetFpsLimit(int index)
        {
            int targetFps = -1;
            switch (index)
            {
                case 0: targetFps = 30; break;
                case 1: targetFps = 60; break;
                case 2: targetFps = 120; break;
                case 3: targetFps = -1; break;
            }

            Application.targetFrameRate = targetFps;
            PlayerPrefs.SetInt("FpsLimitIndex", index);
        }

        public void SetMouseSensitivity(float value)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", value);
            PlayerPrefs.Save();
            UpdateMouseSensitivityText(value);

            // Validate changes immediately if in-game
            if (CameraManager.Instance != null) CameraManager.Instance.UpdateCameraSensitivity();
        }

        private void UpdateMouseSensitivityText(float value)
        {
            if (_mouseSensitivityValueText != null)
                _mouseSensitivityValueText.text = value.ToString("F1");
        }

        public void SetGamepadSensitivity(float value)
        {
            PlayerPrefs.SetFloat("GamepadSensitivity", value);
            PlayerPrefs.Save();
            UpdateGamepadSensitivityText(value);

            // Validate changes immediately if in-game
            if (CameraManager.Instance != null) CameraManager.Instance.UpdateCameraSensitivity();
        }

        private void UpdateGamepadSensitivityText(float value)
        {
            if (_gamepadSensitivityValueText != null)
                _gamepadSensitivityValueText.text = value.ToString("F1");
        }

        public void SetVolume(float volume)
        {
            // Here you would connect to an AudioMixer or AudioListener
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("MasterVolume", volume);
            UpdateVolumeText(volume);
        }

        private void UpdateVolumeText(float volume)
        {
            if (_volumeValueText != null)
            {
                _volumeValueText.text = Mathf.RoundToInt(volume * 100) + "%";
            }
        }

        public void SetLanguage(int index)
        {
            if (Localization.LocalizationManager.Instance != null)
            {
                Localization.LocalizationManager.Instance.LoadLanguage((Localization.Language)index);
            }

            Debug.Log($"Language set to index: {index}");
            PlayerPrefs.SetInt("LanguageIndex", index);
        }

        public void OnBackButtonClicked()
        {
            MenuManager.Instance.BackFromOptions();
        }
    }
}

