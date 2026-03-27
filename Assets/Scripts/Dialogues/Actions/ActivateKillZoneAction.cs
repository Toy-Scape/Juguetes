using UnityEngine;

[CreateAssetMenu(fileName = "ActivateKillZoneAction", menuName = "Dialogue System/Actions/Activate KillZone")]
public class ActivateKillZoneAction : ActionBase
{
    public override void Execute(DialogueContext context)
    {
        if (context.KillZoneToActivate != null)
        {
            context.KillZoneToActivate.SetActive(true);
        }
    }
}