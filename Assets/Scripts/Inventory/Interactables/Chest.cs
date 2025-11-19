using InteractionSystem.Interfaces;
using UnityEngine;
using Inventory.Core;
using Inventory.Services;

public class Chest : MonoBehaviour, IInteractable
{
    public string itemId;
    public string itemName;

    private bool isOpened = false;
    
    public void Interact()
    {
        if (isOpened) return;
        var inventoryHolder = FindObjectOfType<InventoryHolder>();
        if (inventoryHolder != null)
        {
            var item = new Item(itemId, itemName);
            if (inventoryHolder.Service.PickUp(item))
            {
                isOpened = true;
                // Aquí podrías mostrar animación de apertura
                Debug.Log("Cofre abierto y item entregado");
            }
        }
    }

    public bool IsInteractable()
    {
        return !isOpened;
    }
}
