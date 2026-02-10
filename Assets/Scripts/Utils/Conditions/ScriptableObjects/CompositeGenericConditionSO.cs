using UnityEngine;

public enum CompositeType
{
    And,
    Or
}

[CreateAssetMenu(menuName = "Conditions/Composite")]
public class CompositeConditionSO : ConditionSO
{
    [SerializeField] private CompositeType compositeType;
    [SerializeField] private ConditionSO[] conditions;

    public override bool Evaluate(IValueProvider provider)
    {
        if (conditions == null || conditions.Length == 0)
            return true;

        switch (compositeType)
        {
            case CompositeType.And:
                foreach (var c in conditions)
                {
                    if (c == null) continue;
                    if (!c.Evaluate(provider))
                        return false;
                }
                return true;

            case CompositeType.Or:
                foreach (var c in conditions)
                {
                    if (c == null) continue;
                    if (c.Evaluate(provider))
                        return true;
                }
                return false;

            default:
                return false;
        }
    }

    public override bool TryEvaluate(IValueProvider provider, out Dialogue failureMessage)
    {
        failureMessage = null;

        if (conditions == null || conditions.Length == 0)
            return true;

        switch (compositeType)
        {
            case CompositeType.And:
                foreach (var c in conditions)
                {
                    if (c == null) continue;

                    if (!c.TryEvaluate(provider, out failureMessage))
                        return false;
                }
                return true;

            case CompositeType.Or:
                Dialogue lastFailure = null;
                foreach (var c in conditions)
                {
                    if (c == null) continue;

                    if (c.TryEvaluate(provider, out failureMessage))
                        return true;

                    lastFailure = failureMessage;
                }

                failureMessage = lastFailure;
                return false;

            default:
                return false;
        }
    }

}
