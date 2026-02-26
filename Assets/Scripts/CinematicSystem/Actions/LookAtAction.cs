using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [System.Serializable]
    public class LookAtAction : CinematicAction
    {
        [SceneReferenceID]
        public string targetId;

        public float lookDuration = 1f;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (context.CameraController == null)
                yield break;

            context.CameraController.LookAt(targetId, lookDuration);

            if (waitForCompletion && lookDuration > 0)
                yield return context.Wait(lookDuration);
        }
    }
}