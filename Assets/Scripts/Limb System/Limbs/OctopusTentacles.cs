using UnityEngine;

[CreateAssetMenu(menuName = "Limb/OctopusTentacles")]
public class OctopusTentaclesSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Tentáculos de Pulpo";

        // Agrupamos varias pasivas en una sola composite
        PassiveAbility = new CompositePassiveAbility(
            new WallClimbPassive(),
            new SwimPassive()
        );
    }
}
