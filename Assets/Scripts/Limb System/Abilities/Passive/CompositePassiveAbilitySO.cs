using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Limb/Abilities/Passive/Composite")]
public class CompositePassiveAbilitySO : PassiveAbilitySO
{
    [SerializeField] private List<PassiveAbilitySO> abilities = new List<PassiveAbilitySO>();

    public override void Apply(LimbContext context)
    {
        foreach (var ability in abilities)
        {
            if (ability != null) ability.Apply(context);
        }
    }

    public override void Remove(LimbContext context)
    {
        foreach (var ability in abilities)
        {
            if (ability != null) ability.Remove(context);
        }
    }
}
