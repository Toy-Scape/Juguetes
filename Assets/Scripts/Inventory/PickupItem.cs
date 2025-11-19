using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    public Item item;
    public int amount = 1;

    public bool TryPickup(IInventory inventory)
    {
        if (inventory == null || item == null) return false;
        bool picked = inventory.AddItem(item, amount);
        if (picked) Destroy(gameObject);
        return picked;
    }
}

