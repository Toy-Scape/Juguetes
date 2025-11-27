using UnityEngine;

public class SwimPassive : IPassiveAbility
{
    public void Apply (LimbContext context)
    {
        context.CanSwim = true;
        Debug.Log("Tentáculos activados: puedes nadar.");
    }

    public void Remove (LimbContext context)
    {
        context.CanSwim = false;
    }
}