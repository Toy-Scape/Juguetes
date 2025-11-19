using System;

public interface IInventory
{
    event Action OnInventoryChanged;
    bool AddItem(Item item, int amount = 1);
    bool RemoveItem(Item item, int amount = 1);
    int GetItemCount(Item item);
    int GetFreeSlots();
}

