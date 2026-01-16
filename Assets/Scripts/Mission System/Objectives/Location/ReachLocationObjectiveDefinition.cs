using MissionSystem.Runtime;
using UnityEngine;

namespace MissionSystem.Data
{
    [CreateAssetMenu(fileName = "ReachLocationObjective", menuName = "Mission/Objectives/Reach Location")]
    public class ReachLocationObjectiveDefinition : ObjectiveDefinition
    {
        [Header("Location Config")]
        [SerializeField] private string locationID;

        public string LocationID => locationID;

        public void Init(string desc, string locID)
        {
            this.description = desc;
            this.locationID = locID;
        }

        public override Objective CreateRuntimeInstance(Mission mission)
        {
            return new ReachLocationObjective(this, mission);
        }
    }
}
