using MissionSystem.Runtime;
using UnityEngine;

namespace MissionSystem.Data
{
    public abstract class ObjectiveDefinition : ScriptableObject
    {
        [TextArea]
        public string description;

        /// <summary>
        /// Crea una instancia en tiempo de ejecución de este objetivo.
        /// </summary>
        public abstract Objective CreateRuntimeInstance(Mission mission);
    }
}
