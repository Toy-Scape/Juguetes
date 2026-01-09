using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_System.Menus
{
    public class OptionsMenu : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private TextMeshProUGUI _volumeValueText;

        [Header("Language")]
        [SerializeField] private TMP_Dropdown _languageDropdown;

        private void Start()
        {
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
