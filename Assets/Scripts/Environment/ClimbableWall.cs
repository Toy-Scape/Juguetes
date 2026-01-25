using UnityEngine;

[RequireComponent(typeof(LedgeGrabSurface))]
public class ClimbableWall : MonoBehaviour
{
    [SerializeField] private ConditionSO[] conditions;

    public bool CanBeClimbed()
    {
        var provider = FindFirstObjectByType<PlayerConditionProvider>();

        foreach (var c in conditions)
        {
            if (!c.Evaluate(provider))
                return false;
        }

        return true;
    }
}
