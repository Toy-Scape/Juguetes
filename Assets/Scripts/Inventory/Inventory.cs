﻿using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Lógica pura del inventario sin dependencias de MonoBehaviour.
    /// Maneja dos listas separadas: items normales y extremidades (limbs).
    /// Facilita testing y reutilización.
    /// </summary>
    [System.Serializable]
    public class Inventory
    {
        [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();
        [SerializeField] private List<InventoryItem> limbs = new List<InventoryItem>();
        [SerializeField] private int maxCapacity;
        [SerializeField] private int maxLimbCapacity;

        public IReadOnlyList<InventoryItem> Items => items;
        public IReadOnlyList<InventoryItem> Limbs => limbs;
        public int MaxCapacity => maxCapacity;
        public int MaxLimbCapacity => maxLimbCapacity;

        public Inventory(int maxCapacity = 20, int maxLimbCapacity = 10)
        {
            this.maxCapacity = maxCapacity;
            this.maxLimbCapacity = maxLimbCapacity;
        }

        public bool AddItem(ItemData itemData, int quantity)
        {
            if (!IsValidInput(itemData, quantity)) 
                return false;

            var targetList = GetTargetList(itemData);
            var maxCap = GetMaxCapacity(itemData);
            var remainingQuantity = TryStackToExistingItems(targetList, itemData, quantity);

            return CreateNewStacks(targetList, itemData, remainingQuantity, maxCap);
        }

        public bool Contains(ItemData itemData)
        {
            if (itemData == null) 
                return false;

            return FindItem(GetTargetList(itemData), itemData) != null;
        }

        public InventoryItem GetItem(ItemData itemData)
        {
            if (itemData == null) 
                return null;

            return FindItem(GetTargetList(itemData), itemData);
        }

        public bool RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (!IsValidInput(itemData, quantity)) 
                return false;

            var targetList = GetTargetList(itemData);
            return RemoveItemsFromList(targetList, itemData, quantity);
        }

        public void Clear()
        {
            items.Clear();
            limbs.Clear();
        }

        public int GetItemCount(ItemData itemData)
        {
            if (itemData == null) 
                return 0;

            return CalculateTotalQuantity(GetTargetList(itemData), itemData);
        }

        #region Private Helper Methods

        private bool IsValidInput(ItemData itemData, int quantity)
        {
            return itemData != null && quantity > 0;
        }

        private List<InventoryItem> GetTargetList(ItemData itemData)
        {
            return itemData.IsLimb ? limbs : items;
        }

        private int GetMaxCapacity(ItemData itemData)
        {
            return itemData.IsLimb ? maxLimbCapacity : maxCapacity;
        }

        private int TryStackToExistingItems(List<InventoryItem> targetList, ItemData itemData, int quantity)
        {
            var remainingQuantity = quantity;

            foreach (var existingItem in targetList)
            {
                if (!existingItem.CanStack(itemData)) 
                    continue;

                remainingQuantity = existingItem.AddQuantity(remainingQuantity);
                if (remainingQuantity <= 0) 
                    break;
            }

            return remainingQuantity;
        }

        private bool CreateNewStacks(List<InventoryItem> targetList, ItemData itemData, int quantity, int maxCap)
        {
            var remainingQuantity = quantity;

            while (remainingQuantity > 0 && targetList.Count < maxCap)
            {
                var amountForNewStack = Mathf.Min(remainingQuantity, itemData.MaxStackSize);
                targetList.Add(new InventoryItem(itemData, amountForNewStack));
                remainingQuantity -= amountForNewStack;
            }

            return remainingQuantity <= 0;
        }

        private InventoryItem FindItem(List<InventoryItem> targetList, ItemData itemData)
        {
            foreach (var item in targetList)
            {
                if (item.Data == itemData)
                    return item;
            }

            return null;
        }

        private bool RemoveItemsFromList(List<InventoryItem> targetList, ItemData itemData, int quantity)
        {
            var remainingToRemove = quantity;

            for (var i = targetList.Count - 1; i >= 0; i--)
            {
                if (targetList[i].Data != itemData) 
                    continue;

                var itemQuantity = targetList[i].Quantity;
                
                if (itemQuantity <= remainingToRemove)
                {
                    remainingToRemove -= itemQuantity;
                    targetList.RemoveAt(i);
                }
                else
                {
                    targetList[i].RemoveQuantity(remainingToRemove);
                    remainingToRemove = 0;
                }

                if (remainingToRemove <= 0)
                    return true;
            }

            return false;
        }

        private int CalculateTotalQuantity(List<InventoryItem> targetList, ItemData itemData)
        {
            var count = 0;

            foreach (var item in targetList)
            {
                if (item.Data == itemData)
                    count += item.Quantity;
            }

            return count;
        }

        #endregion
    }
}

