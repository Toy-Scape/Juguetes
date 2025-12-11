using System;
using MissionSystem.Data;
using UnityEngine;

namespace MissionSystem.Runtime
{
    public abstract class Objective
    {
        public ObjectiveDefinition Definition { get; private set; }
        public Mission Mission { get; private set; }
        public ObjectiveState State { get; protected set; }
        public float Progress { get; protected set; }

        public event Action<Objective> OnCompleted;
        public event Action<Objective, float> OnProgressUpdated;

        protected Objective(ObjectiveDefinition definition, Mission mission)
        {
            Definition = definition;
            Mission = mission;
            State = ObjectiveState.NotStarted;
            Progress = 0f;
        }

        public virtual void Start()
        {
            State = ObjectiveState.InProgress;
        }

        public virtual void Update() { }

        protected void Complete()
        {
            if (State == ObjectiveState.Completed) return;

            State = ObjectiveState.Completed;
            Progress = 1f;
            OnCompleted?.Invoke(this);
        }

        protected void UpdateProgress(float newProgress)
        {
            Progress = Mathf.Clamp01(newProgress);
            OnProgressUpdated?.Invoke(this, Progress);
        }
    }
}
