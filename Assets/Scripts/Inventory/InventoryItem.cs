using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Representa una instancia de un ítem dentro del inventario.
    /// Contiene la referencia al ItemData y la cantidad actual.
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        [SerializeField] private ItemData data;
        [SerializeField] private int quantity;

        public ItemData Data => data;
        public int Quantity => quantity;

        public InventoryItem(ItemData data, int quantity)
        {
            this.data = data;
            this.quantity = Mathf.Max(1, quantity);
        }

        public bool CanStack(ItemData otherData)
        {
            return data == otherData && quantity < data.MaxStackSize;
        }

        public int AddQuantity(int amount)
        {
            int spaceAvailable = data.MaxStackSize - quantity;
            int amountToAdd = Mathf.Min(amount, spaceAvailable);
            quantity += amountToAdd;
            return amount - amountToAdd;
        }

        public bool RemoveQuantity(int amount)
        {
            if (amount > quantity)
                return false;

            quantity -= amount;
            return true;
        }

        public bool IsEmpty()
        {
            return quantity <= 0;
        }
    }
}

