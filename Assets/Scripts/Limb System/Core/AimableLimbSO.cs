using UnityEngine;

public abstract class AimableLimbSO : LimbSO, IAimable
{
    public virtual void Aim (LimbContext context)
    {
        context.IsAiming = true;
        Debug.Log($"Apuntando con {LimbName}...");
    }

    public virtual void Shoot (LimbContext context)
    {
        if (context.IsAiming)
        {
            Debug.Log($"Disparando con {LimbName}!");
            ActiveAbility?.Activate(context);
        }
        else
        {
            Debug.Log("No estás apuntando, no puedes disparar.");
        }
    }

    public virtual void StopAim (LimbContext context)
    {
        if (context.IsAiming)
        {
            context.IsAiming = false;
            Debug.Log($"Dejaste de apuntar con {LimbName}.");
        }
    }
}
