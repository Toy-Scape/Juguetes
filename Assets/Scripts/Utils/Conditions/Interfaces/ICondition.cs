public interface ICondition
{
    bool Evaluate(IValueProvider provider);

    bool TryEvaluate(IValueProvider provider, out Dialogue failureMessage);
}
