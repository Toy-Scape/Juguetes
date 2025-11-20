using UnityEngine;

public abstract class Limb : MonoBehaviour
{
    public string LimbName { get; protected set; }

    public IAbility ActiveAbility { get; protected set; }
    public IPassiveAbility PassiveAbility { get; protected set; }
    public ISecondaryAbility SecondaryAbility { get; protected set; }

    public virtual void OnEquip (LimbContext context)
    {
        PassiveAbility?.Apply(context);
    }

    public virtual void OnUnequip (LimbContext context)
    {
        PassiveAbility?.Remove(context);
    }
}
