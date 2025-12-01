using UnityEngine;

namespace InteractionSystem.Interfaces
{
    public interface IGrabbable
    {
        float MoveResistance { get; }
        void StartGrab(Rigidbody grabAnchorRb, Vector3 grabPoint);
        void StopGrab();
    }
}