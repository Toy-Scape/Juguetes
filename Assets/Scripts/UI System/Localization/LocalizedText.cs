using TMPro;
using UnityEngine;

namespace Localization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;

        private TextMeshProUGUI _textComponent;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateText();
        }

        private void OnEnable()
        {
            LocalizationManager.OnLanguageChanged += UpdateText;
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationManager.OnLanguageChanged -= UpdateText;
        }

        public void UpdateText()
        {
            if (_textComponent != null && LocalizationManager.Instance != null)
            {
                _textComponent.text = LocalizationManager.Instance.GetLocalizedValue(_key);
            }
        }

        // Helper to set key dynamically if needed
        public void SetKey(string key)
        {
            _key = key;
            UpdateText();
        }
    }
}
