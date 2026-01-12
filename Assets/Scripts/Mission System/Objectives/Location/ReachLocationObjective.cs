using MissionSystem.Data;
using MissionSystem.Utils; // For MissionLocationTrigger
using UnityEngine;

namespace MissionSystem.Runtime
{
    public class ReachLocationObjective : Objective
    {
        private ReachLocationObjectiveDefinition _def;

        public ReachLocationObjective(ReachLocationObjectiveDefinition definition, Mission mission)
            : base(definition, mission)
        {
            _def = definition;
        }

        public override void Start()
        {
            base.Start();
            MissionLocationTrigger.OnLocationReached += HandleLocationReached;
        }

        private void HandleLocationReached(string locationID)
        {
            if (State == ObjectiveState.Completed) return;

            if (locationID == _def.LocationID)
            {
                Complete();
                MissionLocationTrigger.OnLocationReached -= HandleLocationReached;
            }
        }
    }
}
