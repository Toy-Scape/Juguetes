using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Secondary/Aim With Object")]
public class AimWithObjectAbilitySO : SecondaryAbilitySO
{
    public override void Execute(LimbContext context)
    {
        Debug.Log($"Apuntando con el brazo fuerte!");
        context.IsAiming = true;
    }
}
