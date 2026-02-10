using InteractionSystem.Interactables;
using UnityEngine;

[RequireComponent(typeof(NPCInteractableBase))]
public class SpeakerNPC : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;
    public void TriggerDialogue ()
    {
        if (!dialogue)
        {
            return;
        }
        
        DialogueBox.Instance.StartDialogue(dialogue);
    }
}
