using UnityEngine;

namespace InteractionSystem.Interfaces
{
    public interface IPickable
    {
        bool CanBePicked();
        void Pick(Transform hand);
        void Drop();
    }
}