using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Active/Shoot Hook")]
public class ShootHookAbilitySO : ActiveAbilitySO
{
    public override void Execute(LimbContext context)
    {
        if (context.IsAiming)
        {
            Debug.Log("Disparando gancho!");
        }
    }
}
