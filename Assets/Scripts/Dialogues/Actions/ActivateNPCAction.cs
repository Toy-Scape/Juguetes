using UnityEngine;

[CreateAssetMenu(fileName = "ActivateNPCAction", menuName = "Dialogue System/Actions/Activate NPC")]
public class ActivateNPCAction : ActionBase
{
    public override void Execute(DialogueContext context)
    {
        if (context.NPCToActivate != null)
            context.NPCToActivate.SetActive(true);
    }
}