using Inventory;
using MissionSystem;
using MissionSystem.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "GiveQuestAction", menuName = "Dialogue System/Actions/Give Quest")]
public class GiveQuestAction : ActionBase
{
    [SerializeField] private MissionDefinition missionToStart;

    public override void Execute (DialogueContext context)
    {
        Debug.Log("[GiveQuestAction] Executing GiveMission action.");
        var controller = context.Player.GetComponent<PlayerController>();
        if (controller != null)
        {
            GiveMission();
        }
    }


    public void GiveMission ()
    {
        if (MissionManager.Instance != null && missionToStart != null)
        {
            MissionManager.Instance.StartMission(missionToStart);
        }
        else
        {
            Debug.LogWarning("[MissionGiver] Missing Manager or Mission Definition.");
        }
    }
}