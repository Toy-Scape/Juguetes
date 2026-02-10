using UnityEngine;

public abstract class SecondaryAbilitySO : LimbAbilitySO, ISecondaryAbility
{
    public abstract void Execute(LimbContext context);
    public virtual bool CanExecute(LimbContext context) => true;
}
