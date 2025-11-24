using UnityEngine;
using UnityEngine.Events;
using Inventory.UI;

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
        // Inicializamos para que no sean null y otros componentes puedan suscribirse sin hacerlo en el Inspector.
        public UnityEvent<ItemData, int> onItemAdded = new UnityEvent<ItemData, int>();
        public UnityEvent<ItemData, int> onItemRemoved = new UnityEvent<ItemData, int>();
        private Inventory _inventory;

        public Inventory Inventory => _inventory;

        private void Awake()
        {
            _inventory = new Inventory(maxCapacity, maxLimbCapacity);
        }

        #region Input Actions (Unity Messages)

        void OnToggleInventory()
        {
            // Localizar la UI registrada mediante el registro central.
            var ui = InventoryUIRegistry.Get();
            if (ui != null)
            {
                ui.ToggleInventory();
                return;
            }

            // Si no hay UI registrada, dejamos el comportamiento de debug para diagnóstico.
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

            bool success = _inventory.AddItem(itemData, quantity);

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

            bool success = _inventory.RemoveItem(itemData, quantity);

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
            return _inventory.Contains(itemData);
        }

        public int GetItemCount(ItemData itemData)
        {
            return _inventory.GetItemCount(itemData);
        }

        public InventoryItem GetItem(ItemData itemData)
        {
            return _inventory.GetItem(itemData);
        }

        public void ClearInventory()
        {
            _inventory.Clear();
            if (showDebugLogs)
                Debug.Log("Inventario limpiado");
        }

        #endregion

        private void ShowInventoryInfo()
        {
            Debug.Log("=== INVENTARIO DEL JUGADOR ===");
            Debug.Log($"Items normales: {_inventory.Items.Count}/{maxCapacity}");
            
            foreach (var item in _inventory.Items)
            {
                Debug.Log($"  - {item.Data.ItemName} x{item.Quantity}");
            }
            
            Debug.Log($"Extremidades: {_inventory.Limbs.Count}/{maxLimbCapacity}");
            
            foreach (var limb in _inventory.Limbs)
            {
                Debug.Log($"  - {limb.Data.ItemName} x{limb.Quantity}");
            }
            
            Debug.Log("==============================");
        }
    }
}
