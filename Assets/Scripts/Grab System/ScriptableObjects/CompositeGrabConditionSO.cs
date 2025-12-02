using UnityEngine;

public enum CompositeType
{
    And,
    Or
}

[CreateAssetMenu(menuName = "GrabConditions/Composite")]
public class CompositeGrabConditionSO : GrabConditionSO
{
    [SerializeField] private CompositeType compositeType;
    [SerializeField] private GrabConditionSO[] conditions;

    public override bool CanGrab()
    {
        if (conditions == null || conditions.Length == 0) return true;

        switch (compositeType)
        {
            case CompositeType.And:
                foreach (var c in conditions)
                {
                    if (c == null) continue;
                    if (!c.CanGrab()) return false;
                }
                return true;

            case CompositeType.Or:
                foreach (var c in conditions)
                {
                    if (c == null) continue;
                    if (c.CanGrab()) return true;
                }
                return false;

            default:
                return false;
        }
    }
}
