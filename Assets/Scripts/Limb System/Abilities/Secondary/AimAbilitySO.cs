using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Secondary/Aim")]
public class AimAbilitySO : SecondaryAbilitySO
{
    public override void Execute(LimbContext context)
    {
        context.IsAiming = true;
        Debug.Log("Apuntando gancho!");
    }
}
