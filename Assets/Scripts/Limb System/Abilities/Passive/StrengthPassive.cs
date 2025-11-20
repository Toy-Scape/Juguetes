using UnityEngine;

public class StrengthPassive : IPassiveAbility
{
    public void Apply (LimbContext context)
    {
        context.StrongArmActive = true;
        Debug.Log("Brazo fuerte activado: puedes mover objetos pesados.");
    }

    public void Remove (LimbContext context)
    {
        context.StrongArmActive = false;
        Debug.Log("Brazo fuerte desactivado.");
    }
}