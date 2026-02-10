using System.Collections.Generic;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Infrastructure
{
    public class SceneReferenceResolver : MonoBehaviour, ISceneReferenceResolver
    {
        [System.Serializable]
        public struct ReferenceEntry
        {
            public string id;
            public Transform target;
        }

        [SerializeField] public List<ReferenceEntry> references = new List<ReferenceEntry>();

        private Dictionary<string, Transform> _cache;

        private void Awake()
        {
            InitializeCache();
        }

        private void OnValidate()
        {
            if (references == null) return;

            for (int i = 0; i < references.Count; i++)
            {
                var entry = references[i];
                if (entry.target != null && string.IsNullOrEmpty(entry.id))
                {
                    entry.id = entry.target.name;
                    references[i] = entry;
                }
            }
        }

        private void InitializeCache()
        {
            _cache = new Dictionary<string, Transform>();
            foreach (var entry in references)
            {
                if (!string.IsNullOrEmpty(entry.id) && entry.target != null)
                {
                    if (!_cache.ContainsKey(entry.id))
                    {
                        _cache.Add(entry.id, entry.target);
                    }
                    else
                    {
                        Debug.LogWarning($"[SceneReferenceResolver] Duplicate ID found: {entry.id}");
                    }
                }
            }
        }

        public Transform Resolve(string id)
        {
            if (_cache == null) InitializeCache();

            if (_cache.TryGetValue(id, out var transform))
            {
                return transform;
            }

            Debug.LogWarning($"[SceneReferenceResolver] ID not found: {id}");
            return null;
        }

        public void Register(string id, Transform target)
        {
            if (_cache == null) InitializeCache();

            if (!_cache.ContainsKey(id))
            {
                _cache.Add(id, target);
            }
            else
            {
                _cache[id] = target;
            }
        }
    }
}
