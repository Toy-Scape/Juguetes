using InteractionSystem.Interactables;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!dialogue)
        {
            return;
        }

        DialogueBox.Instance.StartDialogue(dialogue);
        this.gameObject.SetActive(false);
    }    
}
