using UnityEngine;

[CreateAssetMenu(menuName = "Limb/StrongArm")]
public class StrongArmSO : AimableLimbSO
{
    private void OnEnable ()
    {
        PassiveAbility = new StrengthPassive();
        SecondaryAbility = new AimAbility(this);
    }

    public override void Aim (LimbContext context)
    {
        if (context.HasObjectInHand)
        {
            base.Aim(context); 
            Debug.Log("Apuntando con objeto pesado...");
        }
        else
        {
            Debug.Log("No tienes objeto en la mano, no puedes apuntar.");
        }
    }

    public override void Shoot (LimbContext context)
    {
        if (context.IsAiming && context.HasObjectInHand)
        {
            Debug.Log("Lanzando objeto pesado!");
            ActiveAbility?.Activate(context);
            context.IsAiming = false;
        }
        else
        {
            Debug.Log("No estás apuntando o no tienes objeto.");
        }
    }

    public override void StopAim (LimbContext context)
    {
        base.StopAim(context);
        Debug.Log("Dejaste de apuntar con el objeto pesado.");
    }
}
