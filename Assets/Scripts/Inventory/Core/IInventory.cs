using System;
using System.Collections.Generic;

namespace Inventory.Core
{
    public interface IInventory
    {
        event Action<Item> OnItemAdded;
        event Action<Item> OnItemRemoved;

        bool AddItem(Item item);
        bool RemoveItem(string itemId);
        Item GetItem(string itemId);
        IReadOnlyList<Item> GetAllItems();
    }
}
