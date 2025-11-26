using UnityEngine;

[CreateAssetMenu(menuName = "Limb/OctopusTentacles")]
public class OctopusTentaclesSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Tentáculos de Pulpo";

        passiveAbility = new CompositePassiveAbility(
            new WallClimbPassive(),
            new SwimPassive()
        );
    }
}

public class WallClimbPassive : IPassiveAbility
{
    public void Apply (LimbContext context) => context.CanClimbWalls = true;
    public void Remove (LimbContext context) => context.CanClimbWalls = false;
}

public class SwimPassive : IPassiveAbility
{
    public void Apply (LimbContext context) => context.CanSwim = true;
    public void Remove (LimbContext context) => context.CanSwim = false;
}
