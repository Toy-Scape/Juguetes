using UnityEngine;
using UI_System.Menus;
using TMPro;
using UnityEngine.Events;

public class OptionsMenuHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UI_SliderField mouseSensitivitySliderField;
    [SerializeField] private UI_SliderField gamepadSensitivitySliderField;
    [SerializeField] private UI_SliderField volumeSliderField;
    [SerializeField] private LocalizedDropdown fpsDropdownPrefab;
    [SerializeField] private LocalizedDropdown languageDropdownPrefab;

    // Listeners con nombre para poder quitarlos sin RemoveAllListeners
    // (RemoveAllListeners también eliminaría los listeners internos de UI_SliderField)
    private UnityAction<float> _onMouseSens;
    private UnityAction<float> _onGamepadSens;
    private UnityAction<float> _onVolume;
    private UnityAction<int> _onFps;
    private UnityAction<int> _onLanguage;

    private void Awake()
    {
        _onMouseSens    = value => PreferencesManager.Instance.SetMouseSensitivity(value);
        _onGamepadSens  = value => PreferencesManager.Instance.SetGamepadSensitivity(value);
        _onVolume       = value => PreferencesManager.Instance.SetVolume(value);
        _onFps          = value => PreferencesManager.Instance.SetFpsLimit(value);
        _onLanguage     = value => PreferencesManager.Instance.SetLanguage(value);
    }

    private void OnEnable()
    {
        // Primero cargar valores sin disparar eventos
        LoadSettings();

        // Luego suscribir listeners para detectar cambios del usuario
        mouseSensitivitySliderField.onValueChanged.AddListener(_onMouseSens);
        gamepadSensitivitySliderField.onValueChanged.AddListener(_onGamepadSens);
        volumeSliderField.onValueChanged.AddListener(_onVolume);
        fpsDropdownPrefab.Dropdown.onValueChanged.AddListener(_onFps);
        languageDropdownPrefab.Dropdown.onValueChanged.AddListener(_onLanguage);
    }

    private void OnDisable()
    {
        // Quitamos solo nuestros listeners, no los internos de UI_SliderField
        mouseSensitivitySliderField.onValueChanged.RemoveListener(_onMouseSens);
        gamepadSensitivitySliderField.onValueChanged.RemoveListener(_onGamepadSens);
        volumeSliderField.onValueChanged.RemoveListener(_onVolume);
        fpsDropdownPrefab.Dropdown.onValueChanged.RemoveListener(_onFps);
        languageDropdownPrefab.Dropdown.onValueChanged.RemoveListener(_onLanguage);
    }

    private void LoadSettings()
    {
        // Usar SetValueWithoutNotify para no disparar el evento y no sobreescribir PreferencesManager
        mouseSensitivitySliderField.SetValueWithoutNotify(PlayerPrefs.GetFloat("MouseSensitivity", 1f));
        gamepadSensitivitySliderField.SetValueWithoutNotify(PlayerPrefs.GetFloat("GamepadSensitivity", 1f));
        volumeSliderField.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 1f));

        fpsDropdownPrefab.SetValueWithoutNotify(PlayerPrefs.GetInt("FpsLimitIndex", 1));
        languageDropdownPrefab.SetValueWithoutNotify(PlayerPrefs.GetInt("LanguageIndex", 0));
    }

    public void OnBackClicked(MenuManager menuManager)
    {
        if (menuManager != null)
            menuManager.HandleBackInput();
    }
}