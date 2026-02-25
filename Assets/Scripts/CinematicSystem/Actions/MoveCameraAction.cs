using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [System.Serializable]
    public class MoveCameraAction : CinematicAction
    {
        [SceneReferenceID]
        public string targetId;

        public float moveDuration = 2f;
        public bool smooth = true;

        public bool instant = false;
        public bool ignoreCollision = false;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (context.CameraController == null)
                yield break;

            context.CameraController.MoveTo(
                targetId,
                moveDuration,
                smooth,
                instant,
                ignoreCollision);

            if (waitForCompletion && moveDuration > 0)
                yield return context.Wait(moveDuration);
        }
    }
}