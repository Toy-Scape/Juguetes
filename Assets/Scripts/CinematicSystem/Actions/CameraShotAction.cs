using System.Collections;
using CinematicSystem.Core;
using DG.Tweening;
using UnityEngine;

namespace CinematicSystem.Actions
{
    [System.Serializable]
    public class CameraShotAction : CinematicAction
    {
        [Header("Targets")]
        [Tooltip("Object to follow/anchor around")]
        [SceneReferenceID]
        public string targetId;

        [Tooltip("Optional: Object to look at. If empty, looks at targetId.")]
        [SceneReferenceID]
        public string lookAtId;

        [Header("Positioning")]
        public Vector3 offset = new Vector3(0, 5, -10);

        [Header("Lens")]
        [Range(1, 179)]
        public float fov = 60f;

        [Header("Movement")]
        public float duration = 2f;
        public bool smooth = true;
        
        [Tooltip("If true, the camera teleports instantly to the start position, skipping any blend.")]
        public bool instant = false;
        [Tooltip("If true, disables Cinemachine Collider/Confiner to prevent auto-zooming near walls.")]
        public bool ignoreCollision = false;

        [Header("Orbit")]
        public bool useOrbit = false;
        public float orbitSpeed = 10f;

        public override IEnumerator Execute(ICinematicContext context)
        {
            if (context.CameraController != null)
            {
                context.CameraController.SetShot(targetId, lookAtId, offset, fov, duration, useOrbit, orbitSpeed, instant, ignoreCollision);

                if (waitForCompletion && duration > 0)
                {
                    yield return context.Wait(duration);
                }
            }
        }
    }
}
