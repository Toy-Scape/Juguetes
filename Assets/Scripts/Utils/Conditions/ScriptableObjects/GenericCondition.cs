using UnityEngine;
using InteractionSystem.Interfaces;

public abstract class GenericCondition : ScriptableObject, IGenericCondition
{
    public abstract bool ConditionIsMet();
}
