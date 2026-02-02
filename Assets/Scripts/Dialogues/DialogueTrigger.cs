using InteractionSystem.Interactables;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    private void OnTriggerEnter(Collider other)
    {
        if (!dialogue)
        {
            return;
        }

        DialogueBox.Instance.StartDialogue(dialogue);
        this.enabled = false;
    }    
}
