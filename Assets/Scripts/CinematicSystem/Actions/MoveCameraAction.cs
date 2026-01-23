using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [CreateAssetMenu(fileName = "MoveCameraAction", menuName = "Cinematic System/Actions/Move Camera")]
    public class MoveCameraAction : CinematicAction
    {
        [Tooltip("ID of the transform defined in SceneReferenceResolver")]
        public string targetId;
        public float moveDuration = 2f;
        public bool smooth = true;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (context.CameraController != null)
            {
                context.CameraController.MoveTo(targetId, moveDuration, smooth);

                // If this action handles the waiting itself, we yield.
                // But usually the action just triggers the move. 
                // However, CinematicAction has a 'waitForCompletion' flag.
                // Implementation depend on if camera controller blocks.
                // For now, let's assume we wait for result duration if flagged.

                if (waitForCompletion && moveDuration > 0)
                {
                    yield return new WaitForSeconds(moveDuration);
                }
            }
        }
    }
}
