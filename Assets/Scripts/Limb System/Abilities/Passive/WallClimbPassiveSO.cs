using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Passive/Wall Climb")]
public class WallClimbPassiveSO : PassiveAbilitySO
{
    public override void Apply(LimbContext context) => context.CanClimbWalls = true;
    public override void Remove(LimbContext context) => context.CanClimbWalls = false;
}
