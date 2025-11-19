using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Inventory : MonoBehaviour, IInventory
{
    [SerializeField] private int slotLimit = 20;
    private List<InventorySlot> slots = new List<InventorySlot>();

    public event Action OnInventoryChanged;

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        int remaining = amount;

        // Stack en slots existentes
        foreach (var slot in slots)
        {
            if (slot.CanStack(item))
                remaining = slot.Add(remaining);
            if (remaining <= 0) break;
        }

        // Añadir nuevos slots si hay espacio
        while (remaining > 0 && slots.Count < slotLimit)
        {
            int toAdd = Math.Min(item.maxStack, remaining);
            slots.Add(new InventorySlot(item, toAdd));
            remaining -= toAdd;
        }

        bool success = remaining == 0;
        if (success) OnInventoryChanged?.Invoke();
        return success;
    }

    public bool RemoveItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        int remaining = amount;

        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = slots[i];
            if (slot.Item == item)
            {
                remaining = slot.Remove(remaining);
                if (slot.IsEmpty()) slots.RemoveAt(i);
            }
        }

        bool success = remaining == 0;
        if (success) OnInventoryChanged?.Invoke();
        return success;
    }

    public int GetItemCount(Item item)
    {
        int count = 0;
        foreach (var slot in slots)
            if (slot.Item == item) count += slot.Amount;
        return count;
    }

    public int GetFreeSlots() => slotLimit - slots.Count;

    // Opcional: método para obtener snapshot del inventario
    public IReadOnlyList<InventorySlot> GetSlots() => slots.AsReadOnly();
}

