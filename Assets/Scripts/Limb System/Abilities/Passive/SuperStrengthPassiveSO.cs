using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Passive/Super Strength")]
public class SuperStrengthPassiveSO : PassiveAbilitySO
{
    public override void Apply(LimbContext context)
    {
        context.CanLiftHeavyObjects = true;
    }

    public override void Remove(LimbContext context)
    {
        context.CanLiftHeavyObjects = false;
    }
}
