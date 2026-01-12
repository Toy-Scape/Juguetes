using System.Text;
using MissionSystem.Data;
using MissionSystem.Runtime;
using TMPro; // Using TextMeshPro as it's standard. Change to UnityEngine.UI if needed, but TMP is better.
using UnityEngine;

namespace MissionSystem.UI
{
    public class MissionEntryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI objectivesText;

        private Mission _mission;

        public void Setup(Mission mission)
        {
            _mission = mission;
            Refresh();
        }

        public void Refresh()
        {
            if (_mission == null) return;

            // Update Title
            titleText.text = _mission.Definition.Title;

            // Update Objectives List
            StringBuilder sb = new StringBuilder();
            foreach (var objective in _mission.Objectives)
            {
                string statusIcon = GetStatusIcon(objective.State);
                string description = objective.Definition.description;

                // Optional: Showing progress
                string progress = "";
                if (objective.State == ObjectiveState.InProgress && objective.Progress > 0 && objective.Progress < 1f)
                {
                    progress = $" ({objective.Progress:P0})";
                }

                sb.AppendLine($"{statusIcon} {description}{progress}");
            }

            objectivesText.text = sb.ToString();
        }

        private string GetStatusIcon(ObjectiveState state)
        {
            switch (state)
            {
                case ObjectiveState.Completed: return "<color=green>☑</color>";
                case ObjectiveState.Failed: return "<color=red>☒</color>";
                case ObjectiveState.InProgress: return "☐";
                default: return "☐";
            }
        }
    }
}
