using InteractionSystem.Core;
using UnityEngine;

namespace InteractionSystem.Interactables
{
    [RequireComponent(typeof(SpeakerNPC))]
    public class NPCInteractableBase : InteractableBase
    {
        [SerializeField] private SpeakerNPC speakerNPC;

        public override void Interact (InteractContext context)
        {
            if (speakerNPC != null)
            {
                if (!DialogueBox.Instance.IsOpen)
                {
                    speakerNPC.TriggerDialogue();
                }
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
