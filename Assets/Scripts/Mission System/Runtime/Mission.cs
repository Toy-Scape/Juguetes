using System;
using System.Collections.Generic;
using System.Linq;
using MissionSystem.Data;
using UnityEngine;

namespace MissionSystem.Runtime
{
    public class Mission
    {
        public MissionDefinition Definition { get; private set; }
        public MissionState State { get; private set; }
        public IReadOnlyList<Objective> Objectives { get; private set; }

        public event Action<Mission> OnMissionCompleted;
        public event Action<Objective> OnObjectiveCompleted;

        public Mission(MissionDefinition definition)
        {
            Definition = definition;
            State = MissionState.NotStarted;

            var objectivesList = new List<Objective>();
            foreach (var objDef in definition.Objectives)
            {
                objectivesList.Add(objDef.CreateRuntimeInstance(this));
            }
            Objectives = objectivesList;
        }

        public void Start()
        {
            if (State != MissionState.NotStarted) return;

            State = MissionState.InProgress;

            foreach (var objective in Objectives)
            {
                objective.OnCompleted += HandleObjectiveCompleted;
                objective.Start();
            }

            CheckCompletion();
        }

        public void Update()
        {
            if (State != MissionState.InProgress) return;

            foreach (var objective in Objectives)
            {
                if (objective.State == ObjectiveState.InProgress)
                {
                    objective.Update();
                }
            }
        }

        private void HandleObjectiveCompleted(Objective objective)
        {
            OnObjectiveCompleted?.Invoke(objective);
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (Objectives.All(o => o.State == ObjectiveState.Completed))
            {
                Complete();
            }
        }

        private void Complete()
        {
            State = MissionState.Completed;
            OnMissionCompleted?.Invoke(this);

            // Clean up events
            foreach (var objective in Objectives)
            {
                objective.OnCompleted -= HandleObjectiveCompleted;
            }
        }
    }
}
