using MissionSystem.Data;
using UnityEngine;

namespace MissionSystem.Utils
{
    [RequireComponent(typeof(Collider))]
    public class MissionGiver : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MissionDefinition missionToStart;
        [SerializeField] private bool oneShot = true;
        [SerializeField] private bool autoStartOnEnter = true;

        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered && oneShot) return;
            if (!autoStartOnEnter) return;

            if (other.GetComponent<PlayerController>() != null)
            {
                GiveMission();
            }
        }

        public void GiveMission()
        {
            if (MissionManager.Instance != null && missionToStart != null)
            {
                // Optional: Check if already completed? MissionManager handles active check.
                if (!MissionManager.Instance.IsMissionCompleted(missionToStart))
                {
                    _hasTriggered = true;
                    MissionManager.Instance.StartMission(missionToStart);
                }
            }
            else
            {
                Debug.LogWarning("[MissionGiver] Missing Manager or Mission Definition.");
            }
        }

        private void OnValidate()
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }
        }
    }
}
