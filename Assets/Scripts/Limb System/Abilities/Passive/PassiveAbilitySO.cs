using UnityEngine;

public abstract class PassiveAbilitySO : LimbAbilitySO, IPassiveAbility
{
    public abstract void Apply(LimbContext context);
    public abstract void Remove(LimbContext context);
}
