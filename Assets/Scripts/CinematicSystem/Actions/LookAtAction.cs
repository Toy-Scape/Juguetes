using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [System.Serializable]
    public class LookAtAction : CinematicAction
    {
        [Tooltip("ID of the transform defined in SceneReferenceResolver")]
        [SceneReferenceID]
        public string targetId;
        public float lookDuration = 1f;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (context.CameraController != null)
            {
                context.CameraController.LookAt(targetId, lookDuration);

                if (waitForCompletion && lookDuration > 0)
                {
                    yield return new WaitForSeconds(lookDuration);
                }
            }
        }
    }
}
