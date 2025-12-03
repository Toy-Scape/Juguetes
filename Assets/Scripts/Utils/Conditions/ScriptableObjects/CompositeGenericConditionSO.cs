using UnityEngine;

public enum CompositeType
{
    And,
    Or
}

[CreateAssetMenu(menuName = "Conditions/Composite")]
public class CompositeGenericConditionSO : GenericCondition
{
    [SerializeField] private CompositeType compositeType;
    [SerializeField] private GenericConditionSO[] conditions;

    public override bool ConditionIsMet()
    {
        if (conditions == null || conditions.Length == 0) return true;

        switch (compositeType)
        {
            case CompositeType.And:
                foreach (var c in conditions)
                {
                    if (c == null) continue;
                    if (!c.ConditionIsMet()) return false;
                }
                return true;

            case CompositeType.Or:
                foreach (var c in conditions)
                {
                    if (c == null) continue;
                    if (c.ConditionIsMet()) return true;
                }
                return false;

            default:
                return false;
        }
    }
}
