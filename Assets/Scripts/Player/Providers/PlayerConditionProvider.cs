using Inventory;
using UnityEngine;

public class PlayerConditionProvider : MonoBehaviour, IBaseValueProvider
{
    [SerializeField] private InventoryContext inventory;
    [SerializeField] private LimbContext limb;

    public bool TryGetBool(string key, object param, out bool value)
    {
        // Inventario
        if (inventory != null && inventory.TryGetBool(key, param, out value))
            return true;

        // Extremidades
        if (limb != null && limb.TryGetBool(key, param, out value))
            return true;

        value = default;
        return false;
    }

    public bool TryGetInt(string key, object param, out int value)
    {
        if (inventory != null && inventory.TryGetInt(key, param, out value))
            return true;

        value = default;
        return false;
    }
}