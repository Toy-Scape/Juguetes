using System.Collections.Generic;
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
            if (itemData == null || quantity <= 0)
                return false;

            List<InventoryItem> targetList = itemData.IsLimb ? limbs : items;
            int maxCap = itemData.IsLimb ? maxLimbCapacity : maxCapacity;

            int remainingQuantity = quantity;

            // Intentar stackear en items existentes
            foreach (var existingItem in targetList)
            {
                if (existingItem.CanStack(itemData))
                {
                    remainingQuantity = existingItem.AddQuantity(remainingQuantity);
                    if (remainingQuantity <= 0)
                        return true;
                }
            }

            // Crear nuevos slots si es necesario
            while (remainingQuantity > 0 && targetList.Count < maxCap)
            {
                int amountForNewStack = Mathf.Min(remainingQuantity, itemData.MaxStackSize);
                targetList.Add(new InventoryItem(itemData, amountForNewStack));
                remainingQuantity -= amountForNewStack;
            }

            return remainingQuantity <= 0;
        }

        public bool Contains(ItemData itemData)
        {
            if (itemData == null)
                return false;

            List<InventoryItem> targetList = itemData.IsLimb ? limbs : items;

            foreach (var item in targetList)
            {
                if (item.Data == itemData)
                    return true;
            }

            return false;
        }

        public InventoryItem GetItem(ItemData itemData)
        {
            if (itemData == null)
                return null;

            List<InventoryItem> targetList = itemData.IsLimb ? limbs : items;

            foreach (var item in targetList)
            {
                if (item.Data == itemData)
                    return item;
            }

            return null;
        }

        public bool RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0)
                return false;

            List<InventoryItem> targetList = itemData.IsLimb ? limbs : items;
            int remainingToRemove = quantity;

            for (int i = targetList.Count - 1; i >= 0; i--)
            {
                if (targetList[i].Data == itemData)
                {
                    int itemQuantity = targetList[i].Quantity;
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
            }

            return false;
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

            List<InventoryItem> targetList = itemData.IsLimb ? limbs : items;
            int count = 0;

            foreach (var item in targetList)
            {
                if (item.Data == itemData)
                    count += item.Quantity;
            }

            return count;
        }
    }
}

