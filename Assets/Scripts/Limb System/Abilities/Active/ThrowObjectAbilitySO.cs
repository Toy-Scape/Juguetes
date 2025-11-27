using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Abilities/Active/Throw Object")]
public class ThrowObjectAbilitySO : ActiveAbilitySO
{
    public override void Execute(LimbContext context)
    {
        if (context.IsAiming)
        {
            Debug.Log($"Objeto lanzado!");
            context.HeldObject = null;
            context.IsAiming = false;
        }
    }
}
