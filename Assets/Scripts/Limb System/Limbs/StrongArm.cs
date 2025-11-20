
using UnityEngine;

public class StrongArm : Limb, IAimable
{
    public StrongArm ()
    {
        LimbName = "Brazo Fuerte";
        PassiveAbility = new StrengthPassive();
        SecondaryAbility = new AimAbility(this);
    }

    public void Aim (LimbContext context)
    {
        Debug.Log("Apuntando con objeto pesado...");
    }

    public void Shoot (LimbContext context)
    {
        Debug.Log("Lanzando objeto pesado!");
    }
}