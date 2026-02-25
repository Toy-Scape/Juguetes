using System.Collections.Generic;
using UnityEngine;

public class PreferencesManager : MonoBehaviour
{
    public static PreferencesManager Instance { get; private set; }

    public float MasterVolume { get; private set; }
    public float MouseSensitivity { get; private set; }
    public float GamepadSensitivity { get; private set; }
    public int FpsLimit { get; private set; }
    public int LanguageIndex { get; private set; }

    private static readonly Dictionary<int, int> fpsOptions = new()
    {
        { 0, 30 },
        { 1, 60 },
        { 2, 120 }
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPreferences();
    }

    public void LoadPreferences()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = MasterVolume;

        MouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        GamepadSensitivity = PlayerPrefs.GetFloat("GamepadSensitivity", 1f);

        LanguageIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        if (Localization.LocalizationManager.Instance != null)
            Localization.LocalizationManager.Instance.LoadLanguage((Localization.Language)LanguageIndex);

        int fpsIndex = PlayerPrefs.GetInt("FpsLimitIndex", 1);
        FpsLimit = fpsOptions.ContainsKey(fpsIndex) ? fpsOptions[fpsIndex] : -1;
        Application.targetFrameRate = FpsLimit;
    }

    public void SetVolume(float value)
    {
        MasterVolume = value;
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        if (CameraManager.Instance != null)
            CameraManager.Instance.UpdateCameraSensitivity();
    }

    public void SetGamepadSensitivity(float value)
    {
        GamepadSensitivity = value;
        PlayerPrefs.SetFloat("GamepadSensitivity", value);
        PlayerPrefs.Save();
        if (CameraManager.Instance != null)
            CameraManager.Instance.UpdateCameraSensitivity();
    }

    public void SetFpsLimit(int index)
    {
        FpsLimit = fpsOptions.ContainsKey(index) ? fpsOptions[index] : -1;
        Application.targetFrameRate = FpsLimit;
        PlayerPrefs.SetInt("FpsLimitIndex", index);
        PlayerPrefs.Save();
    }

    public void SetLanguage(int index)
    {
        LanguageIndex = index;
        PlayerPrefs.SetInt("LanguageIndex", index);
        PlayerPrefs.Save();
        if (Localization.LocalizationManager.Instance != null)
            Localization.LocalizationManager.Instance.LoadLanguage((Localization.Language)index);
    }
}