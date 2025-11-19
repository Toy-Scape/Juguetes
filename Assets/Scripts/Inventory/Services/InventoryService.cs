using Inventory.Core;

namespace Inventory.Services
{
    /// <summary>
    /// Servicio para operaciones comunes sobre el inventario.
    /// </summary>
    public class InventoryService
    {
        private readonly IInventory _inventory;

        public InventoryService(IInventory inventory)
        {
            _inventory = inventory;
        }

        public bool PickUp(Item item)
        {
            return _inventory.AddItem(item);
        }

        public bool Drop(string itemId)
        {
            return _inventory.RemoveItem(itemId);
        }

        public Item GetItem(string itemId)
        {
            return _inventory.GetItem(itemId);
        }
    }
}
