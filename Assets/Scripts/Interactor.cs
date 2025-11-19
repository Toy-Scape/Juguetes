using UnityEngine;

public class Interactor : MonoBehaviour
{
    private void OnInteract()
    {
        Interact();
    }

    private void Interact()
    {
        if (DialogueBox.Instance.IsOpen)
        {
            DialogueBox.Instance.Next();
        }
    }
}
