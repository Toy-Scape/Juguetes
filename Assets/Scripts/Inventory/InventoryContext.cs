using System;
using UnityEngine;

namespace Inventory
{
    public class InventoryContext : MonoBehaviour, IBaseValueProvider
    {
        [SerializeField] private PlayerInventory playerInventory;

        private void Awake()
        {
            if (playerInventory == null)
                playerInventory = GetComponent<PlayerInventory>();
        }

        public bool TryGetBool(string key, object param, out bool value)
        {
            value = default;

            if (!Enum.TryParse(key, out InventoryConditionKey parsed))
                return false;

            switch (parsed)
            {
                case InventoryConditionKey.HasItem:
                    if (param is ItemData item)
                    {
                        value = playerInventory.GetItemCount(item) > 0;
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        public bool TryGetInt(string key, object param, out int value)
        {
            value = default;

            if (!Enum.TryParse(key, out InventoryConditionKey parsed))
                return false;

            switch (parsed)
            {
                case InventoryConditionKey.ItemCount:
                    if (param is ItemData item)
                    {
                        value = playerInventory.GetItemCount(item);
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
