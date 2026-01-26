using UnityEngine;

public abstract class ConditionSO : ScriptableObject, ICondition
{
    [SerializeField] private Dialogue conditionFailedText;
    public Dialogue ConditionNotMetText => conditionFailedText;

    public abstract bool Evaluate(IValueProvider provider);

    public virtual bool TryEvaluate(IValueProvider provider, out Dialogue failureMessage)
    {
        if (Evaluate(provider))
        {
            failureMessage = null;
            return true;
        }

        failureMessage = conditionFailedText;
        return false;
    }
}
