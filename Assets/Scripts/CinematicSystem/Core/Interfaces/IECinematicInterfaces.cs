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
    }

    public interface ICameraController
    {
        void SetActive(bool active);
        void MoveTo(string targetId, float duration, bool smooth = true);
        void LookAt(string targetId, float duration);
        void ResetCamera();
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
