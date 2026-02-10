using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Passive/Swim")]
public class SwimPassiveSO : PassiveAbilitySO
{
    public override void Apply(LimbContext context) => context.CanSwim = true;
    public override void Remove(LimbContext context) => context.CanSwim = false;
}
