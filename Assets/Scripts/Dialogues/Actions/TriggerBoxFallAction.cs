using UnityEngine;

[CreateAssetMenu(fileName = "TriggerBoxFallAction", menuName = "Dialogue/Actions/TriggerBoxFall")]
public class TriggerBoxFallAction : ActionBase
{
    public Vector3 finalPosition;
    public Quaternion finalRotation;

    public override void Execute(DialogueContext context)
    {
        BoxWobbleAndFall box = FindFirstObjectByType<BoxWobbleAndFall>();
        if (box != null)
        {
            box.TriggerFall(finalPosition, finalRotation);
        }
    }
}
