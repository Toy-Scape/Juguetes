using System.Collections;
using UnityEngine;

namespace CinematicSystem.Core
{
    public interface ICinematicContext
    {
        ICameraController CameraController { get; }
        ISceneReferenceResolver Resolver { get; }
        // Potentially Input blocking interface can be added here
        void SetPlayerInput(bool enabled);
        IEnumerator Wait(float duration);
    }

    public interface ICameraController
    {
        void SetActive(bool active, bool instant = false);
        void MoveTo(string targetId, float duration, bool smooth = true);
        void LookAt(string targetId, float duration);
        void ResetCamera(bool instant = false);

        // New consolidated method
        // New consolidated method
        // New consolidated method
        void SetShot(string targetId, string lookAtId, Vector3 offset, float fov, float duration, bool useOrbit, float orbitSpeed, bool instant = false, bool ignoreCollision = false);
        IEnumerator WaitForBlend();
    }

    public interface ISceneReferenceResolver
    {
        Transform Resolve(string id);
    }

    public interface ICinematicPlayer
    {
        void Play(CinematicAsset cinematic);
        void Stop();
        bool IsPlaying { get; }
    }
}
