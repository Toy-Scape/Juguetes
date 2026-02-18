using System.Collections.Generic;
using UnityEngine;

namespace MissionSystem.Data
{
    [CreateAssetMenu(fileName = "NewMission", menuName = "Mission/Mission Definition")]
    public class MissionDefinition : ScriptableObject
    {
        [Header("Configuración")]
        [SerializeField] private string id;
        [SerializeField] private string title;
        [TextArea]
        [SerializeField] private string description;

        [Header("Objetivos")]
        [SerializeField] private List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();

        public string ID => id;

        public string Title
        {
            get { 
                string fullText;
                    if (Localization.LocalizationManager.Instance != null)
                    fullText = Localization.LocalizationManager.Instance.GetLocalizedValue(title);
                else
                    fullText = title;
                return fullText;
            }
        }
        public string Description {
            get { 
                string fullText;
                    if (Localization.LocalizationManager.Instance != null)
                    fullText = Localization.LocalizationManager.Instance.GetLocalizedValue(description);
                else
                    fullText = description;
                return fullText;
            }
        }
        public IReadOnlyList<ObjectiveDefinition> Objectives => objectives;

        public void Init(string id, string title, string description, List<ObjectiveDefinition> objectives)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.objectives = objectives;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
        }
    }
}
