using System.Collections.Generic;
using MissionSystem.Runtime;
using UnityEngine;

namespace MissionSystem.UI
{
    public class MissionTrackerUI : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Transform container;
        [SerializeField] private MissionEntryUI missionEntryPrefab;

        private Dictionary<Mission, MissionEntryUI> _activeEntries = new Dictionary<Mission, MissionEntryUI>();

        private void Start()
        {
            Debug.Log("[MissionTrackerUI] Start called");

            if (container == null)
            {
                Debug.LogError("[MissionTrackerUI] Container is not assigned!");
            }

            if (missionEntryPrefab == null)
            {
                Debug.LogError("[MissionTrackerUI] MissionEntryPrefab is not assigned!");
            }

            if (MissionManager.Instance != null)
            {
                Debug.Log("[MissionTrackerUI] MissionManager found, subscribing to events");
                MissionManager.Instance.OnMissionStarted += HandleMissionStarted;
                MissionManager.Instance.OnMissionCompleted += HandleMissionCompleted;
                MissionManager.Instance.OnObjectiveCompleted += HandleObjectiveCompleted;

                // Sync existing missions
                foreach (var mission in MissionManager.Instance.ActiveMissions)
                {
                    Debug.Log($"[MissionTrackerUI] Syncing existing mission: {mission.Definition.Title}");
                    CreateEntry(mission);
                }
            }
            else
            {
                Debug.LogError("[MissionTrackerUI] MissionManager Instance is NULL!");
            }
        }

        private void OnDestroy()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionStarted -= HandleMissionStarted;
                MissionManager.Instance.OnMissionCompleted -= HandleMissionCompleted;
                MissionManager.Instance.OnObjectiveCompleted -= HandleObjectiveCompleted;
            }
        }

        private void HandleMissionStarted(Mission mission)
        {
            Debug.Log($"[MissionTrackerUI] HandleMissionStarted called for: {mission.Definition.Title}");
            CreateEntry(mission);
        }

        private void HandleMissionCompleted(Mission mission)
        {
            Debug.Log($"[MissionTrackerUI] HandleMissionCompleted called for: {mission.Definition.Title}");
            if (_activeEntries.TryGetValue(mission, out var entry))
            {
                entry.Refresh();
                StartCoroutine(RemoveEntryDelayed(mission, 5f));
            }
        }

        private System.Collections.IEnumerator RemoveEntryDelayed(Mission mission, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_activeEntries.TryGetValue(mission, out var entry))
            {
                if (entry != null) Destroy(entry.gameObject);
                _activeEntries.Remove(mission);
            }
        }

        private void HandleObjectiveCompleted(Objective objective)
        {
            Debug.Log($"[MissionTrackerUI] HandleObjectiveCompleted called");
            if (_activeEntries.TryGetValue(objective.Mission, out var entry))
            {
                entry.Refresh();
            }
        }

        private void CreateEntry(Mission mission)
        {
            Debug.Log($"[MissionTrackerUI] CreateEntry called for: {mission.Definition.Title}");

            if (_activeEntries.ContainsKey(mission))
            {
                Debug.LogWarning($"[MissionTrackerUI] Mission entry already exists for: {mission.Definition.Title}");
                return;
            }

            if (missionEntryPrefab == null)
            {
                Debug.LogError("[MissionTrackerUI] Cannot create entry - missionEntryPrefab is null!");
                return;
            }

            if (container == null)
            {
                Debug.LogError("[MissionTrackerUI] Cannot create entry - container is null!");
                return;
            }

            var instance = Instantiate(missionEntryPrefab, container);
            Debug.Log($"[MissionTrackerUI] Instantiated prefab: {instance.name}");
            instance.Setup(mission);
            _activeEntries.Add(mission, instance);
            Debug.Log($"[MissionTrackerUI] Entry created successfully. Active entries count: {_activeEntries.Count}");
        }
    }
}
