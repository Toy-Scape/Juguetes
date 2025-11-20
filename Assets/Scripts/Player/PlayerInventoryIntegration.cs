using UnityEngine;
using Inventory;

namespace Player
{
    /// <summary>
    /// Integración del sistema de inventario con el PlayerController.
    /// Añade este componente al Player junto con InventoryComponent.
    /// Usa Unity Messages automáticos del Input System (igual que PlayerController).
    /// </summary>
    public class PlayerInventoryIntegration : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private InventoryComponent inventoryComponent;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            // Auto-obtener InventoryComponent si no está asignado
            if (inventoryComponent == null)
            {
                inventoryComponent = GetComponent<InventoryComponent>();
                
                if (inventoryComponent == null)
                {
                    Debug.LogError("PlayerInventoryIntegration requiere un InventoryComponent en el mismo GameObject.");
                    enabled = false;
                    return;
                }
            }

            // Suscribirse a eventos del inventario
            inventoryComponent.onItemAdded.AddListener(OnItemAdded);
            inventoryComponent.onItemRemoved.AddListener(OnItemRemoved);
        }

        private void OnDestroy()
        {
            // Limpiar eventos del inventario
            if (inventoryComponent != null)
            {
                inventoryComponent.onItemAdded.RemoveListener(OnItemAdded);
                inventoryComponent.onItemRemoved.RemoveListener(OnItemRemoved);
            }
        }

        #region Input Actions (Unity Messages - llamados automáticamente por Input System)

        void OnToggleInventory()
        {
            if (showDebugInfo)
            {
                ShowInventoryInfo();
            }
        }

        #endregion

        private void OnItemAdded(ItemData item, int quantity)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[Player] Recogido: {item.name} x{quantity}");
            }
            
            // Aquí puedes añadir:
            // - Mostrar notificación en UI
            // - Reproducir sonido de recogida
            // - Actualizar contador en HUD
        }

        private void OnItemRemoved(ItemData item, int quantity)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[Player] Usado/Eliminado: {item.name} x{quantity}");
            }
            
            // Aquí puedes añadir:
            // - Actualizar UI
            // - Reproducir efectos
        }

        private void ShowInventoryInfo()
        {
            Debug.Log("=== INVENTARIO DEL JUGADOR ===");
            
            var inventory = inventoryComponent.Inventory;
            
            Debug.Log($"Items Normales ({inventory.Items.Count}/{inventory.MaxCapacity}):");
            foreach (var item in inventory.Items)
            {
                Debug.Log($"  - {item.Data.name} x{item.Quantity}");
            }
            
            Debug.Log($"Extremidades ({inventory.Limbs.Count}/{inventory.MaxLimbCapacity}):");
            foreach (var limb in inventory.Limbs)
            {
                Debug.Log($"  - {limb.Data.name} x{limb.Quantity}");
            }
            
            Debug.Log("==============================");
        }


        // Método público para que otros scripts usen el inventario
        public InventoryComponent GetInventory()
        {
            return inventoryComponent;
        }
    }
}

