using UnityEngine;

[System.Serializable]
public class StrengthPassive : IPassiveAbility
{
    public void Apply (LimbContext context) => context.CanGrabObjects = true;
    public void Remove (LimbContext context) => context.CanGrabObjects = false;
}
