using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UI_System.Menus
{
    public class OptionsMenuHandler : MonoBehaviour
    {

        /// <summary>
        /// Establece el límite de FPS según el índice del dropdown.
        /// </summary>
        public void SetFpsLimit(TMP_Dropdown change)
        {
            int fps = change.value switch
            {
                0 => 100,
                1 => 120,
                2 => 144,
                _ => -1 // Unlimited
            };

            Application.targetFrameRate = fps;
            PlayerPrefs.SetInt("FpsLimitIndex", change.value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Establece la sensibilidad del ratón.
        /// </summary>
        public void SetMouseSensitivity(Slider change)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", change.value);
            PlayerPrefs.Save();

            if (CameraManager.Instance != null)
                CameraManager.Instance.UpdateCameraSensitivity();
        }

        /// <summary>
        /// Establece la sensibilidad del gamepad.
        /// </summary>
        public void SetGamepadSensitivity(Slider change)
        {
            PlayerPrefs.SetFloat("GamepadSensitivity", change.value);
            PlayerPrefs.Save();

            if (CameraManager.Instance != null)
                CameraManager.Instance.UpdateCameraSensitivity();
        }

        /// <summary>
        /// Establece el volumen maestro.
        /// </summary>
        public void SetVolume(Slider change)
        {
            AudioListener.volume = change.value;
            PlayerPrefs.SetFloat("MasterVolume", change.value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Cambia el idioma según el índice del dropdown.
        /// </summary>
        public void SetLanguage(TMP_Dropdown change)
        {
            PlayerPrefs.SetInt("LanguageIndex", change.value);
            PlayerPrefs.Save();

            if (Localization.LocalizationManager.Instance != null)
                Localization.LocalizationManager.Instance.LoadLanguage((Localization.Language)change.value);
        }

        public void OnBackClicked(MenuManager menuManager)
        {
            if (menuManager != null)
                menuManager.HandleBackInput();
        }



    }
}
