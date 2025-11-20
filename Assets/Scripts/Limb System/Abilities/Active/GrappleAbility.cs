using UnityEngine;

public class GrappleAbility : IAbility
{
    public void Activate (LimbContext context)
    {
        Debug.Log("Disparando gancho...");
        context.HookArmActive = true;
    }

    public void Deactivate (LimbContext context)
    {
        Debug.Log("Soltando gancho...");
        context.HookArmActive = false;
    }
}