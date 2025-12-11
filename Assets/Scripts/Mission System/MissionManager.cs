using System;
using System.Collections.Generic;
using MissionSystem.Data;
using MissionSystem.Runtime;
using UnityEngine;

namespace MissionSystem
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        private List<Mission> _activeMissions = new List<Mission>();
        private List<Mission> _completedMissions = new List<Mission>();

        public IReadOnlyList<Mission> ActiveMissions => _activeMissions;

        public event Action<Mission> OnMissionStarted;
        public event Action<Mission> OnMissionCompleted;
        public event Action<Objective> OnObjectiveCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            for (int i = _activeMissions.Count - 1; i >= 0; i--)
            {
                _activeMissions[i].Update();
            }
        }

        public void StartMission(MissionDefinition missionDef)
        {
            if (IsMissionActive(missionDef) || IsMissionCompleted(missionDef))
            {
                Debug.LogWarning($"[MissionManager] Mission {missionDef.ID} is already active or completed.");
                return;
            }

            var newMission = new Mission(missionDef);
            newMission.OnMissionCompleted += HandleMissionCompleted;
            newMission.OnObjectiveCompleted += HandleObjectiveCompleted;

            _activeMissions.Add(newMission);
            newMission.Start();

            OnMissionStarted?.Invoke(newMission);
            Debug.Log($"[MissionManager] Started Mission: {missionDef.Title}");
        }

        public bool IsMissionActive(MissionDefinition missionDef)
        {
            return _activeMissions.Exists(m => m.Definition.ID == missionDef.ID);
        }

        public bool IsMissionCompleted(MissionDefinition missionDef)
        {
            return _completedMissions.Exists(m => m.Definition.ID == missionDef.ID);
        }

        private void HandleMissionCompleted(Mission mission)
        {
            mission.OnMissionCompleted -= HandleMissionCompleted;
            mission.OnObjectiveCompleted -= HandleObjectiveCompleted;

            _activeMissions.Remove(mission);
            _completedMissions.Add(mission);

            OnMissionCompleted?.Invoke(mission);
            Debug.Log($"[MissionManager] Completed Mission: {mission.Definition.Title}");
        }

        private void HandleObjectiveCompleted(Objective objective)
        {
            OnObjectiveCompleted?.Invoke(objective);
        }
    }
}
