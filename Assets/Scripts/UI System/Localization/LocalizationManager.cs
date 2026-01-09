using System;
using UnityEngine;

namespace Localization
{
    public enum Language
    {
        English = 0,
        Spanish = 1,
        Basque = 2
    }

    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public static event Action OnLanguageChanged;

        [SerializeField] private LocalizationDatabase _database;

        private Language _currentLanguage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (_database != null) _database.Initialize();

            LoadLanguage((Language)PlayerPrefs.GetInt("LanguageIndex", 0));
        }

        public void LoadLanguage(Language language)
        {
            _currentLanguage = language;
            Debug.Log($"Language changed to: {_currentLanguage}");
            OnLanguageChanged?.Invoke();
        }

        public string GetLocalizedValue(string key)
        {
            if (_database == null) return key;
            return _database.GetValue(key, _currentLanguage);
        }

        public Language CurrentLanguage => _currentLanguage;
    }
}
