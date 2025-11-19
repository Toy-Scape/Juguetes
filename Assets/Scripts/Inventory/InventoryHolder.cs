using UnityEngine;
using Inventory.Core;
using Inventory.Services;

public class InventoryHolder : MonoBehaviour
{
    public IInventory Inventory { get; private set; }
    public InventoryService Service { get; private set; }

    void Awake()
    {
        Inventory = new Inventory.Core.Inventory();
        Service = new InventoryService(Inventory);
    }
}
