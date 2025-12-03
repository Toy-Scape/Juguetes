using UnityEngine;
using InteractionSystem.Interfaces;

public abstract class GrabCondition : ScriptableObject, IGrabCondition
{
    public abstract bool CanGrab();
}
