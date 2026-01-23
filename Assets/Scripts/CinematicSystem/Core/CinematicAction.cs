using System.Collections;
using UnityEngine;

namespace CinematicSystem.Core
{
    public abstract class CinematicAction : ScriptableObject
    {
        [Tooltip("If true, the cinematic player waits for this action to finish before starting the next one.")]
        public bool waitForCompletion = true;

        [TextArea]
        public string description;

        /// <summary>
        /// Executes the action logic.
        /// </summary>
        /// <param name="context">Provides access to infrastructure (Camera, Resolver, etc)</param>
        public abstract IEnumerator Execute(ICinematicContext context);
    }
}
