using UnityEngine;
using Inventory.Core;
using Inventory.Services;
using InteractionSystem.Interfaces;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public string itemId;
    public string itemName;

    public void Interact()
    {
        var player = FindObjectOfType<InventoryHolder>();
        if (player != null)
        {
            var item = new Item(itemId, itemName);
            if (player.Service.PickUp(item))
            {
                Destroy(gameObject);
            }
        }
    }

    public bool IsInteractable()
    {
        return true;
    }
}
