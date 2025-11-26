using UnityEngine;

[CreateAssetMenu(menuName = "Limb/OctopusTentacles")]
public class OctopusTentaclesSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Tentáculos de Pulpo";

        PassiveAbility = new CompositePassiveAbility(
            new WallClimbPassive(),
            new SwimPassive()
        );
    }
}
