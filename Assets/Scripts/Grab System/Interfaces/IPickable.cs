using UnityEngine;

namespace InteractionSystem.Interfaces
{
    public interface IPickable
    {
        void Pick(Transform hand);
        void Drop();
    }
}