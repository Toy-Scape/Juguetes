using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    /// <summary>
    /// MonoBehaviour que actúa como puente entre Unity y la lógica del inventario.
    /// Este es el único componente que se añade al Player.
    /// Expone métodos públicos simples y eventos para notificar cambios.
    /// </summary>
    public class InventoryComponent : MonoBehaviour
    {
        [SerializeField] private Inventory inventory = new Inventory();
        [SerializeField] private bool debugMode;

        [Header("Events")]
        public UnityEvent<ItemData, int> onItemAdded;
        public UnityEvent<ItemData, int> onItemRemoved;

        public Inventory Inventory => inventory;

        private void Awake()
        {
            if (inventory == null)
                inventory = new Inventory();
        }

        /// <summary>
        /// Añade un ítem al inventario.
        /// </summary>
        /// <returns>True si se añadió correctamente, false si no hay espacio.</returns>
        public bool AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                if (debugMode)
                    Debug.LogWarning("Intentando añadir un ItemData nulo al inventario.");
                return false;
            }

            bool success = inventory.AddItem(itemData, quantity);

            if (success)
            {
                onItemAdded?.Invoke(itemData, quantity);
                if (debugMode)
                    Debug.Log($"Añadido al inventario: {itemData.name} x{quantity}");
            }
            else
            {
                if (debugMode)
                    Debug.LogWarning($"No se pudo añadir {itemData.name} x{quantity}. Inventario lleno.");
            }

            return success;
        }

        /// <summary>
        /// Verifica si el inventario contiene un ítem específico.
        /// </summary>
        public bool Contains(ItemData itemData)
        {
            return inventory.Contains(itemData);
        }

        /// <summary>
        /// Obtiene la instancia del ítem en el inventario.
        /// </summary>
        /// <returns>El InventoryItem o null si no existe.</returns>
        public InventoryItem GetItem(ItemData itemData)
        {
            return inventory.GetItem(itemData);
        }

        /// <summary>
        /// Obtiene la cantidad total de un ítem en el inventario.
        /// </summary>
        public int GetItemCount(ItemData itemData)
        {
            return inventory.GetItemCount(itemData);
        }

        /// <summary>
        /// Elimina un ítem del inventario.
        /// </summary>
        /// <returns>True si se eliminó correctamente.</returns>
        public bool RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
                return false;

            bool success = inventory.RemoveItem(itemData, quantity);

            if (success)
            {
                onItemRemoved?.Invoke(itemData, quantity);
                if (debugMode)
                    Debug.Log($"Eliminado del inventario: {itemData.name} x{quantity}");
            }

            return success;
        }

        /// <summary>
        /// Suelta un ítem del inventario al mundo (opcional: implementar spawning).
        /// </summary>
        public bool DropItem(ItemData itemData, int quantity = 1)
        {
            if (!Contains(itemData))
                return false;

            bool removed = RemoveItem(itemData, quantity);

            if (removed && debugMode)
            {
                Debug.Log($"Ítem dropeado: {itemData.name} x{quantity}");
                // Aquí se podría instanciar un prefab del ítem en el mundo
            }

            return removed;
        }

        /// <summary>
        /// Limpia todo el inventario.
        /// </summary>
        public void ClearInventory()
        {
            inventory.Clear();
            if (debugMode)
                Debug.Log("Inventario limpiado.");
        }
    }
}

