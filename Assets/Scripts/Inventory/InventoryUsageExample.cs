using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// Ejemplo de cómo usar el InventoryComponent desde el Player.
    /// Este script demuestra las operaciones básicas del inventario.
    /// Usa Unity Messages automáticos del Input System (igual que PlayerController).
    /// </summary>
    public class InventoryUsageExample : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private InventoryComponent inventoryComponent;

        [Header("Items de prueba")]
        [SerializeField] private ItemData testItem;
        [SerializeField] private ItemData testLimb;

        private void Start()
        {
            // Obtener el InventoryComponent si no está asignado
            if (inventoryComponent == null)
                inventoryComponent = GetComponent<InventoryComponent>();

            if (inventoryComponent == null)
            {
                Debug.LogError("No se encontró InventoryComponent. Añádelo al Player.");
                enabled = false;
                return;
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
            CheckInventory();
        }

        #endregion

        private void Update()
        {
            // Ejemplos con teclas numéricas para testing
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AddTestItem();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                RemoveTestItem();
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                DropTestItem();
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ClearInventory();
            }
        }

        private void AddTestItem()
        {
            if (testItem == null)
            {
                Debug.LogWarning("Asigna un ItemData de prueba en el Inspector");
                return;
            }

            bool success = inventoryComponent.AddItem(testItem);
            if (success)
            {
                Debug.Log($"✓ Item añadido: {testItem.name}");
            }
            else
            {
                Debug.LogWarning("✗ No se pudo añadir el item (inventario lleno)");
            }
        }

        private void RemoveTestItem()
        {
            if (testItem == null) return;

            bool success = inventoryComponent.RemoveItem(testItem);
            if (success)
            {
                Debug.Log($"✓ Item eliminado: {testItem.name}");
            }
            else
            {
                Debug.LogWarning("✗ No se pudo eliminar el item (no existe en el inventario)");
            }
        }

        private void CheckInventory()
        {
            Debug.Log("=== Estado del Inventario ===");

            if (testItem != null)
            {
                bool hasItem = inventoryComponent.Contains(testItem);
                int count = inventoryComponent.GetItemCount(testItem);
                Debug.Log($"{testItem.name}: {(hasItem ? "SÍ" : "NO")} - Cantidad: {count}");
            }

            if (testLimb != null)
            {
                bool hasLimb = inventoryComponent.Contains(testLimb);
                int count = inventoryComponent.GetItemCount(testLimb);
                Debug.Log($"{testLimb.name} (Extremidad): {(hasLimb ? "SÍ" : "NO")} - Cantidad: {count}");
            }

            // Mostrar todos los items
            var inventory = inventoryComponent.Inventory;
            Debug.Log($"Items normales: {inventory.Items.Count}/{inventory.MaxCapacity}");
            Debug.Log($"Extremidades: {inventory.Limbs.Count}/{inventory.MaxLimbCapacity}");
        }

        private void DropTestItem()
        {
            if (testItem == null) return;

            bool success = inventoryComponent.DropItem(testItem);
            if (success)
            {
                Debug.Log($"✓ Item dropeado: {testItem.name}");
            }
        }

        private void ClearInventory()
        {
            inventoryComponent.ClearInventory();
            Debug.Log("✓ Inventario limpiado");
        }

        private void OnItemAdded(ItemData item, int quantity)
        {
            Debug.Log($"[Evento] Item añadido: {item.name} x{quantity}");
        }

        private void OnItemRemoved(ItemData item, int quantity)
        {
            Debug.Log($"[Evento] Item eliminado: {item.name} x{quantity}");
        }
    }
}

