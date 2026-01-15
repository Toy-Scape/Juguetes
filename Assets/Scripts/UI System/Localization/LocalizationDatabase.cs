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

            public string Get (Language lang)
            {
                return lang switch
                {
                    Language.English => english,
                    Language.Spanish => spanish,
                    Language.Basque => basque,
                    _ => spanish
                };
            }

            public void Set (Language lang, string value)
            {
                switch (lang)
                {
                    case Language.English: english = value; break;
                    case Language.Spanish: spanish = value; break;
                    case Language.Basque: basque = value; break;
                }
            }
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

        public string[] GetAllKeys ()
        {
            var list = new List<string>();
            foreach (var e in _entries)
                list.Add(e.key);
            return list.ToArray();
        }

        public LocalizationEntry GetEntry (string key)
        {
            Initialize();
            _cache.TryGetValue(key, out var entry);
            return entry;
        }

        public string Get (Language lang, LocalizationEntry entry)
        {
            return lang switch
            {
                Language.English => entry.english,
                Language.Spanish => entry.spanish,
                Language.Basque => entry.basque,
                _ => entry.spanish
            };
        }

        public void Set (Language lang, LocalizationEntry entry, string value)
        {
            switch (lang)
            {
                case Language.English: entry.english = value; break;
                case Language.Spanish: entry.spanish = value; break;
                case Language.Basque: entry.basque = value; break;
            }
        }

        public void AddEntry (LocalizationEntry entry)
        {
            _entries.Add(entry);
            Initialize(); // reconstruye el cache
        }

    }
}
