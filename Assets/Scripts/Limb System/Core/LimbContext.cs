using System;
using UnityEngine;

public class LimbContext : MonoBehaviour, IBaseValueProvider
{
    public bool CanLiftHeavyObjects;
    public bool CanClimbWalls;
    public bool CanSwim;
    public bool IsAiming;

    public GameObject HeldObject;

    public bool TryGetBool(string key, object param, out bool value)
    {
        value = default;

        if (!Enum.TryParse(key, out LimbConditionKey parsed))
            return false;

        switch (parsed)
        {
            case LimbConditionKey.CanLiftHeavyObjects:
                value = CanLiftHeavyObjects;
                return true;

            case LimbConditionKey.CanClimbWalls:
                value = CanClimbWalls;
                return true;

            case LimbConditionKey.CanSwim:
                value = CanSwim;
                return true;

            case LimbConditionKey.IsAiming:
                value = IsAiming;
                return true;

            case LimbConditionKey.HeldObjectExists:
                value = HeldObject != null;
                return true;

            default:
                return false;
        }
    }
}
