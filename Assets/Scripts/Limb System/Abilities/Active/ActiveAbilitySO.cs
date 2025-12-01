using UnityEngine;

public abstract class ActiveAbilitySO : LimbAbilitySO, IActiveAbility
{
    public abstract void Execute(LimbContext context);
}
