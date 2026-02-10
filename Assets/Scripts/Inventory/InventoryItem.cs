﻿using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Representa una instancia de un ítem dentro del inventario.
    /// Contiene la referencia al ItemData y la cantidad actual.
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        private const int MinQuantity = 1;

        [SerializeField] private ItemData data;
        [SerializeField] private int quantity;

        public ItemData Data => data;
        public int Quantity => quantity;

        public InventoryItem(ItemData data, int quantity)
        {
            this.data = data;
            this.quantity = Mathf.Max(MinQuantity, quantity);
        }

        /// <summary>
        /// Verifica si este ítem puede apilarse con otro ItemData.
        /// </summary>
        /// <param name="otherData">El ItemData a comparar</param>
        /// <returns>True si puede apilarse, false si el stack está lleno</returns>
        public bool CanStack(ItemData otherData)
        {
            return data == otherData && HasSpaceForMoreItems();
        }

        /// <summary>
        /// Añade cantidad al ítem respetando el límite de stack.
        /// </summary>
        /// <param name="amount">Cantidad a añadir</param>
        /// <returns>La cantidad que no pudo ser añadida (overflow)</returns>
        public int AddQuantity(int amount)
        {
            var spaceAvailable = data.MaxStackSize - quantity;
            var amountToAdd = Mathf.Min(amount, spaceAvailable);
            
            quantity += amountToAdd;
            
            return amount - amountToAdd;
        }

        /// <summary>
        /// Remueve cantidad del ítem.
        /// </summary>
        /// <param name="amount">Cantidad a remover</param>
        /// <returns>True si se removió correctamente, false si no hay suficiente cantidad</returns>
        public bool RemoveQuantity(int amount)
        {
            if (amount > quantity)
                return false;

            quantity -= amount;
            return true;
        }

        /// <summary>
        /// Verifica si el ítem está vacío.
        /// </summary>
        public bool IsEmpty() => quantity <= 0;

        /// <summary>
        /// Verifica si hay espacio disponible para más ítems en el stack.
        /// </summary>
        private bool HasSpaceForMoreItems() => quantity < data.MaxStackSize;
    }
}

