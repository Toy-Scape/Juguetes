using InteractionSystem.Core;
using UnityEngine;

namespace InteractionSystem.Interactables
{
    public class ThoughtInteractableBase : InteractableBase
    {
        [SerializeField] private SpeakerNPC speakerNPC;
        [SerializeField] private bool requiredItem;
        public override void Interact ()
        {
            if (speakerNPC != null && requiredItem)
            {
                
            }
            else
            {
                speakerNPC.TriggerDialogue();
            }
        }

        public override bool IsInteractable ()
        {
            return true;
        }
    }
}
