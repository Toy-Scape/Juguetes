using UnityEngine;
using Domain.StaticNpc;

[CreateAssetMenu(fileName = "TriggerStaticNpcAction", menuName = "Dialogue/Actions/Trigger Static Npc Action")]
public class TriggerStaticNpcAction : ActionBase
{
    [Tooltip("The ID or comma-separated IDs of the actions to play.")]
    public string actionIds;

    [Tooltip("If true, it will find the ONE StaticNpcActionHandler in the scene and use it. This ignores all Speakers and hierarchy.")]
    public bool findHandlerInScene = true;

    public override void Execute(DialogueContext context)
    {
        Debug.Log($"[TriggerStaticNpcAction] Executing sequence '{actionIds}'.");

        StaticNpcActionHandler handler = null;

        if (findHandlerInScene)
        {
            handler = FindFirstObjectByType<StaticNpcActionHandler>();
        }

        // Only try the old context fallback if we explicitly turned OFF finding it in the scene
        // or if it wasn't found for some reason.
        if (handler == null)
        {
            GameObject targetObj = null;
            if (context.NPCToActivate != null)
            {
                targetObj = context.NPCToActivate;
            }
            else if (context.Speaker != null)
            {
                targetObj = context.Speaker;
            }

            if (targetObj != null)
            {
                handler = targetObj.GetComponentInChildren<StaticNpcActionHandler>();
                if (handler == null) handler = targetObj.GetComponentInParent<StaticNpcActionHandler>();
            }
        }

        if (handler != null)
        {
            Debug.Log($"[TriggerStaticNpcAction] Found Handler on {handler.gameObject.name}. Playing sequence: '{actionIds}'");
            handler.PlayActionsByIdSequence(actionIds);
        }
        else
        {
            Debug.LogWarning("[TriggerStaticNpcAction] CRITICAL: Could not find any StaticNpcActionHandler in the whole scene or in the speaker.");
        }
    }
}
