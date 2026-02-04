using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI_System.Menus
{
    public class OptionsMenuHandler : MonoBehaviour
    {
        /// <summary>
        /// Establece el límite de FPS según el índice del dropdown.
        /// </summary>
        public void SetFpsLimit(int index)
        {
            int fps = index switch
            {
                0 => 30,
                1 => 60,
                2 => 120,
                _ => -1 // Unlimited
            };

            Application.targetFrameRate = fps;
            PlayerPrefs.SetInt("FpsLimitIndex", index);
        }

        /// <summary>
        /// Establece la sensibilidad del ratón.
        /// </summary>
        public void SetMouseSensitivity(float value)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", value);
            PlayerPrefs.Save();

            if (CameraManager.Instance != null)
                CameraManager.Instance.UpdateCameraSensitivity();
        }

        /// <summary>
        /// Establece la sensibilidad del gamepad.
        /// </summary>
        public void SetGamepadSensitivity(float value)
        {
            PlayerPrefs.SetFloat("GamepadSensitivity", value);
            PlayerPrefs.Save();

            if (CameraManager.Instance != null)
                CameraManager.Instance.UpdateCameraSensitivity();
        }

        /// <summary>
        /// Establece el volumen maestro.
        /// </summary>
        public void SetVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
        }

        /// <summary>
        /// Cambia el idioma según el índice del dropdown.
        /// </summary>
        public void SetLanguage(int index)
        {
            PlayerPrefs.SetInt("LanguageIndex", index);

            if (Localization.LocalizationManager.Instance != null)
                Localization.LocalizationManager.Instance.LoadLanguage((Localization.Language)index);
        }

        public void OnBackClicked(MenuManager menuManager)
        {
            if (menuManager != null)
                menuManager.HandleBackInput();
        }

    }
}
