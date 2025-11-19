using InteractionSystem.Core;
using UnityEngine;
using InteractionSystem.Interfaces;

namespace InteractionSystem.Interactables
{
    public class NPCInteractableBase : InteractableBase
    {
        [SerializeField] private SpeakerNPC speakerNPC;

        public override void Interact ()
        {
            if (speakerNPC != null)
            {
                speakerNPC.TriggerDialogue();
            }
            else
            {
                Debug.LogWarning($"SpeakerNPC not found on '{gameObject.name}'. Attach a SpeakerNPC component or assign it in the inspector.");
            }
        }

        public override bool IsInteractable ()
        {
            return true;
        }
    }
}
