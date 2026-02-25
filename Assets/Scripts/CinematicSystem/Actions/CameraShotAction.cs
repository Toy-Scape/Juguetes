using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [System.Serializable]
    public class CameraShotAction : CinematicAction
    {
        [SceneReferenceID]
        public string targetId;

        [SceneReferenceID]
        public string lookAtId;

        public Vector3 offset = new Vector3(0, 5, -10);
        public float fov = 60f;

        public float duration = 2f;
        public bool instant = false;
        public bool ignoreCollision = false;

        public bool useOrbit = false;
        public float orbitSpeed = 10f;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (context.CameraController == null)
                yield break;

            context.CameraController.SetShot(
                targetId,
                lookAtId,
                offset,
                fov,
                duration,
                useOrbit,
                orbitSpeed,
                instant,
                ignoreCollision);

            if (waitForCompletion && duration > 0)
                yield return context.Wait(duration);
        }
    }
}