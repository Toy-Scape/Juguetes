using System.Collections.Generic;
using Inventory;
using MissionSystem.Data;
using MissionSystem.Runtime;
using UnityEngine;

namespace MissionSystem.Tests
{
    public class MissionDebugger : MonoBehaviour
    {
        [Header("Test Config")]
        [SerializeField] private MissionDefinition missionToStart;
        [SerializeField] private KeyCode startKey = KeyCode.T;

        private void Update()
        {
            if (Input.GetKeyDown(startKey))
            {
                StartMission();
            }
        }

        [ContextMenu("Start Mission")]
        public void StartMission()
        {
            if (MissionManager.Instance == null)
            {
                Debug.LogError("MissionManager instance not found!");
                return;
            }

            if (missionToStart == null)
            {
                Debug.LogWarning("No MissionDefinition assigned to MissionDebugger!");
                return;
            }

            MissionManager.Instance.StartMission(missionToStart);
            Debug.Log($"Requested start of mission: {missionToStart.name}");
        }
    }
}
