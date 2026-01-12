using UnityEngine;

public class DialogueContext
{
    public GameObject Player { get; }

    public DialogueContext (GameObject player)
    {
        Player = player;
    }
}