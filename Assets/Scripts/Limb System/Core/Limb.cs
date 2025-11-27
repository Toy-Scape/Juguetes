using UnityEngine;

public abstract class LimbSO : ScriptableObject
{
    [field: SerializeField] public string LimbName { get; protected set; }

    [field: SerializeField] public IAbility ActiveAbility { get; protected set; }
    [field: SerializeField] public IPassiveAbility PassiveAbility { get; protected set; }
    [field: SerializeField] public ISecondaryAbility SecondaryAbility { get; protected set; }

    public virtual void OnEquip (LimbContext context)
    {
        PassiveAbility?.Apply(context);
    }

    public virtual void OnUnequip (LimbContext context)
    {
        PassiveAbility?.Remove(context);
    }
}
