using System;
using System.Collections.Generic;

namespace Inventory.Core
{
    public class Inventory : IInventory
    {
        private readonly Dictionary<string, Item> _items = new Dictionary<string, Item>();

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;

        public bool AddItem(Item item)
        {
            if (_items.ContainsKey(item.Id)) return false;
            _items[item.Id] = item;
            OnItemAdded?.Invoke(item);
            return true;
        }

        public bool RemoveItem(string itemId)
        {
            if (!_items.TryGetValue(itemId, out var item)) return false;
            _items.Remove(itemId);
            OnItemRemoved?.Invoke(item);
            return true;
        }

        public Item GetItem(string itemId)
        {
            _items.TryGetValue(itemId, out var item);
            return item;
        }

        public IReadOnlyList<Item> GetAllItems()
        {
            return new List<Item>(_items.Values);
        }
    }
}
