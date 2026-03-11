using InteractionSystem.Interactables;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (dialogue == null) return;

        DialogueBox.Instance.StartDialogue(dialogue);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnBatteryCollected += DisableTrigger;
    }

    private void OnDisable()
    {
        GameEvents.OnBatteryCollected -= DisableTrigger;
    }

    private void DisableTrigger()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}