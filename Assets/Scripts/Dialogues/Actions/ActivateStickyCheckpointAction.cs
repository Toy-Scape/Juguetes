using UnityEngine;

[CreateAssetMenu(fileName = "ActivateStickyCheckpoint", menuName = "Dialogue System/Actions/ActivateStickyCheckpoint")]
public class ActivateStickyCheckpointAction : ActionBase
{
    public override void Execute(DialogueContext context)
    {
        if (context.KillZoneToActivate != null)
        {
            context.KillZoneToActivate.SetActive(true);
        }
    }
}