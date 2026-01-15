using UnityEngine;

namespace InteractionSystem.Interfaces
{
    public interface IGrabbable
    {
        bool CanBeGrabbed();
        float MoveResistance { get; }
        Dialogue GetFailThought();
        void StartGrab();
        void StopGrab();
    }
}