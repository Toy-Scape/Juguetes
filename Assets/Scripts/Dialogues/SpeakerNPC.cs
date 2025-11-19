using UnityEditor.SceneManagement;
using UnityEngine;

public class SpeakerNPC : MonoBehaviour
{
    public Dialogue dialogue;
    public void TriggerDialogue()
    {
        if (!dialogue)
        {
            return;
        }
        
        DialogueBox.Instance.StartDialogue(dialogue);
    }
}
