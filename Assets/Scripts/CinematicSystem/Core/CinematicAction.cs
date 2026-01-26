using System.Collections;
using UnityEngine;

namespace CinematicSystem.Core
{
    [System.Serializable]
    public abstract class CinematicAction
    {
        [Tooltip("If true, the cinematic player waits for this action to finish before starting the next one.")]
        public bool waitForCompletion = true;
        [Tooltip("If true, pauses the cinematic after this action finishes, until Advance (e.g. Dialogue Next) is called.")]
        public bool holdAtEnd = false;

        /// <summary>
        /// Executes the action logic.
        /// </summary>
        /// <param name="context">Provides access to infrastructure (Camera, Resolver, etc)</param>
        public abstract IEnumerator Execute(ICinematicContext context);
    }
}
