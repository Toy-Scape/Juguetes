using UnityEngine;
using UnityEngine.Events;

namespace Inventory
{
    /// <summary>
    /// Componente principal del inventario del jugador.
    /// Añádelo al Player y todo funciona automáticamente.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private int maxCapacity = 20;
        [SerializeField] private int maxLimbCapacity = 4;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Eventos C# (no aparecen en el Inspector)
        [Header("Eventos")]
        public UnityEvent<ItemData, int> onItemAdded;
        public UnityEvent<ItemData, int> onItemRemoved;
        private Inventory inventory;

        public Inventory Inventory => inventory;

        private void Awake()
        {
            inventory = new Inventory(maxCapacity, maxLimbCapacity);
        }

        #region Input Actions (Unity Messages)

        void OnToggleInventory()
        {
            ShowInventoryInfo();
        }

        #endregion

        #region Public API

        public bool AddItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null)
            {
                Debug.LogWarning("[PlayerInventory] ItemData es null");
                return false;
            }

            bool success = inventory.AddItem(itemData, quantity);

            if (success)
            {
                onItemAdded?.Invoke(itemData, quantity);
                
                if (showDebugLogs)
                    Debug.Log($"✓ Añadido: {itemData.name} x{quantity}");
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"✗ Inventario lleno, no se pudo añadir: {itemData.name}");
            }

            return success;
        }

        public bool RemoveItem(ItemData itemData, int quantity = 1)
        {
            if (itemData == null) return false;

            bool success = inventory.RemoveItem(itemData, quantity);

            if (success)
            {
                onItemRemoved?.Invoke(itemData, quantity);
                
                if (showDebugLogs)
                    Debug.Log($"✓ Eliminado: {itemData.name} x{quantity}");
            }

            return success;
        }

        public bool Contains(ItemData itemData)
        {
            return inventory.Contains(itemData);
        }

        public int GetItemCount(ItemData itemData)
        {
            return inventory.GetItemCount(itemData);
        }

        public InventoryItem GetItem(ItemData itemData)
        {
            return inventory.GetItem(itemData);
        }

        public void ClearInventory()
        {
            inventory.Clear();
            if (showDebugLogs)
                Debug.Log("Inventario limpiado");
        }

        #endregion

        private void ShowInventoryInfo()
        {
            Debug.Log("=== INVENTARIO DEL JUGADOR ===");
            Debug.Log($"Items normales: {inventory.Items.Count}/{maxCapacity}");
            
            foreach (var item in inventory.Items)
            {
                Debug.Log($"  - {item.Data.ItemName} x{item.Quantity}");
            }
            
            Debug.Log($"Extremidades: {inventory.Limbs.Count}/{maxLimbCapacity}");
            
            foreach (var limb in inventory.Limbs)
            {
                Debug.Log($"  - {limb.Data.ItemName} x{limb.Quantity}");
            }
            
            Debug.Log("==============================");
        }
    }
}

