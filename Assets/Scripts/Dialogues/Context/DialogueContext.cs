using UnityEngine;

public class DialogueContext
{
    public GameObject Player { get; }
    public GameObject Speaker { get; }
    public GameObject NPCToActivate;

    public DialogueContext (GameObject player, GameObject speaker)
    {
        Player = player;
        Speaker = speaker;
    }
}
