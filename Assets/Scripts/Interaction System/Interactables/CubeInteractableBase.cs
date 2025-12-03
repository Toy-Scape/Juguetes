using InteractionSystem.Core;
using UnityEngine;
using InteractionSystem.Interfaces;

namespace InteractionSystem.Interactables
{
    public class CubeInteractableBase : InteractableBase
    {
        public override void Interact (InteractContext context)
        {
            Debug.Log("Interact");
        }

        public override bool IsInteractable ()
        {
            return true;
        }
    }
}
