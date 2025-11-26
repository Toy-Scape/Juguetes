using UnityEngine;

public class WallClimbPassive : IPassiveAbility
{
    public void Apply (LimbContext context)
    {
        context.CanClimbWalls = true;
        Debug.Log("Tentáculos activados: puedes escalar paredes.");
    }

    public void Remove (LimbContext context)
    {
        context.CanClimbWalls = false;
    }
}