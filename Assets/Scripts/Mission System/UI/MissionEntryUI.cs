using MissionSystem.Data;
using MissionSystem.Runtime;
using TMPro;
using UnityEngine;

namespace MissionSystem.UI
{
    public class MissionEntryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform objectivesContainer;
        [SerializeField] private GameObject objectiveEntryPrefab;

        private Mission _mission;

        public void Setup (Mission mission)
        {
            _mission = mission;

            titleText.text = mission.Definition.Title;
            descriptionText.text = mission.Definition.Description;

            Refresh();

            foreach (var obj in mission.Objectives)
                obj.OnProgressUpdated += HandleObjectiveUpdated;
        }

        private void OnDestroy ()
        {
            if (_mission == null) return;

            foreach (var obj in _mission.Objectives)
                obj.OnProgressUpdated -= HandleObjectiveUpdated;
        }

        private void HandleObjectiveUpdated (Objective obj, float progress)
        {
            Refresh();
        }

        public void Refresh ()
        {
            if (_mission == null) return;

            foreach (Transform child in objectivesContainer)
                Destroy(child.gameObject);

            foreach (var objective in _mission.Objectives)
            {
                var entry = Instantiate(objectiveEntryPrefab, objectivesContainer);
                entry.GetComponent<ObjectiveEntryUI>().Bind(objective);
            }
        }
    }
}
