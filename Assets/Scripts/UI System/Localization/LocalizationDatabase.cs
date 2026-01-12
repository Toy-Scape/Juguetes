using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "UI System/Localization Database")]
    public class LocalizationDatabase : ScriptableObject
    {
        [System.Serializable]
        public class LocalizationEntry
        {
            public string key;
            [TextArea] public string english;
            [TextArea] public string spanish;
            [TextArea] public string basque;
        }

        [SerializeField] private List<LocalizationEntry> _entries = new List<LocalizationEntry>();

        // Cache for faster lookup at runtime
        private Dictionary<string, LocalizationEntry> _cache;

        public void Initialize()
        {
            _cache = new Dictionary<string, LocalizationEntry>();
            foreach (var entry in _entries)
            {
                if (!_cache.ContainsKey(entry.key))
                {
                    _cache.Add(entry.key, entry);
                }
            }
        }

        public string GetValue(string key, Language language)
        {
            if (_cache == null) Initialize();

            if (_cache.TryGetValue(key, out var entry))
            {
                switch (language)
                {
                    case Language.English: return entry.english;
                    case Language.Spanish: return entry.spanish;
                    case Language.Basque: return entry.basque;
                }
            }
            return key; // Fallback
        }
    }
}
