using UnityEngine;
using Inventory.UI;

namespace Inventory
{
    /// <summary>
    /// Ejemplo de uso del sistema de inventario.
    /// Demuestra cómo interactuar con el inventario del jugador para verificar,
    /// añadir y eliminar items, así como controlar la UI.
    /// </summary>
    /// <remarks>
    /// Este script es un ejemplo educativo que muestra las principales funcionalidades
    /// del sistema de inventario. Puedes usarlo como referencia para crear tus propios
    /// scripts de gameplay que interactúen con el inventario.
    /// 
    /// CÓMO USAR ESTE EJEMPLO:
    /// 1. Adjunta este script a un GameObject en tu escena (por ejemplo, un trigger o un NPC)
    /// 2. Asigna los ItemData que quieras comprobar desde el Inspector
    /// 3. El script verificará automáticamente al inicio si el jugador tiene esos items
    /// 4. Puedes llamar a los métodos públicos desde el Inspector o desde otros scripts
    /// </remarks>
    public class InventoryExample : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Referencia al inventario del jugador. Se busca automáticamente si no se asigna.")]
        [SerializeField] private PlayerInventory playerInventory;

        [Header("Items de Prueba")]
        [Tooltip("Item que se verificará al inicio del juego")]
        [SerializeField] private ItemData itemToCheck;
        
        [Tooltip("Item que se añadirá al llamar AddTestItem()")]
        [SerializeField] private ItemData itemToAdd;
        
        [Tooltip("Cantidad del item a añadir")]
        [SerializeField] private int quantityToAdd = 1;

        [Header("Configuración")]
        [Tooltip("Si está activado, ejecuta las pruebas al iniciar el juego")]
        [SerializeField] private bool runTestsOnStart = true;

        private void Start()
        {
            // Buscar el PlayerInventory si no está asignado
            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();
                
                if (playerInventory == null)
                {
                    Debug.LogError("[InventoryExample] No se encontró PlayerInventory en la escena. Asegúrate de que existe un objeto con el componente PlayerInventory.");
                    return;
                }
            }

            if (runTestsOnStart)
            {
                RunInventoryTests();
            }
        }

        /// <summary>
        /// Ejecuta una serie de pruebas básicas del inventario.
        /// </summary>
        /// <remarks>
        /// Este método demuestra las operaciones más comunes del inventario:
        /// - Verificar si existe un item
        /// - Obtener la cantidad de un item
        /// - Añadir items
        /// - Eliminar items
        /// </remarks>
        [ContextMenu("Ejecutar Pruebas de Inventario")]
        public void RunInventoryTests()
        {
            Debug.Log("=== INICIO DE PRUEBAS DE INVENTARIO ===");

            // EJEMPLO 1: Verificar si existe un item específico
            CheckIfItemExists();

            // EJEMPLO 2: Contar cantidad de un item
            CountItemQuantity();

            // EJEMPLO 3: Añadir un item y verificar
            AddItemExample();

            // EJEMPLO 4: Eliminar un item
            RemoveItemExample();

            Debug.Log("=== FIN DE PRUEBAS DE INVENTARIO ===");
        }

        /// <summary>
        /// EJEMPLO 1: Verifica si el jugador tiene un item específico.
        /// </summary>
        /// <remarks>
        /// Usa este patrón cuando necesites verificar si el jugador tiene un item
        /// antes de permitir una acción, como abrir una puerta con una llave o
        /// usar un item para crafting.
        /// </remarks>
        [ContextMenu("Verificar si Existe Item")]
        public void CheckIfItemExists()
        {
            if (itemToCheck == null)
            {
                Debug.LogWarning("[InventoryExample] No se ha asignado un ItemData para verificar.");
                return;
            }

            // Verificar si el item existe en el inventario
            bool hasItem = playerInventory.Contains(itemToCheck);

            if (hasItem)
            {
                Debug.Log($"✓ El jugador TIENE '{itemToCheck.ItemName}' en el inventario.");
                
                // ACCIÓN SI TIENE EL ITEM
                // Por ejemplo: abrir una puerta, permitir crafting, activar un diálogo especial, etc.
                OnPlayerHasItem(itemToCheck);
            }
            else
            {
                Debug.Log($"✗ El jugador NO TIENE '{itemToCheck.ItemName}' en el inventario.");
                
                // ACCIÓN SI NO TIENE EL ITEM
                // Por ejemplo: mostrar mensaje de error, bloquear acceso, etc.
                OnPlayerDoesNotHaveItem(itemToCheck);
            }
        }

        /// <summary>
        /// EJEMPLO 2: Cuenta cuántas unidades de un item tiene el jugador.
        /// </summary>
        /// <remarks>
        /// Usa este patrón cuando necesites saber la cantidad exacta de un item,
        /// por ejemplo para verificar si tiene suficientes recursos para craftear
        /// o para mostrar la cantidad en una interfaz de tienda.
        /// </remarks>
        [ContextMenu("Contar Cantidad de Item")]
        public void CountItemQuantity()
        {
            if (itemToCheck == null)
            {
                Debug.LogWarning("[InventoryExample] No se ha asignado un ItemData para contar.");
                return;
            }

            // Obtener la cantidad total del item en el inventario
            int quantity = playerInventory.GetItemCount(itemToCheck);

            Debug.Log($"El jugador tiene {quantity} unidades de '{itemToCheck.ItemName}'");

            // Ejemplo: verificar si tiene suficiente cantidad para una receta
            int requiredAmount = 5;
            if (quantity >= requiredAmount)
            {
                Debug.Log($"✓ El jugador tiene suficientes '{itemToCheck.ItemName}' (necesita {requiredAmount})");
            }
            else
            {
                Debug.Log($"✗ El jugador NO tiene suficientes '{itemToCheck.ItemName}' (tiene {quantity}, necesita {requiredAmount})");
            }
        }

        /// <summary>
        /// EJEMPLO 3: Añade un item al inventario del jugador.
        /// </summary>
        /// <remarks>
        /// Usa este patrón cuando el jugador recoja un item del mundo,
        /// complete una misión, compre algo de una tienda, etc.
        /// </remarks>
        [ContextMenu("Añadir Item de Prueba")]
        public void AddItemExample()
        {
            if (itemToAdd == null)
            {
                Debug.LogWarning("[InventoryExample] No se ha asignado un ItemData para añadir.");
                return;
            }

            Debug.Log($"Intentando añadir {quantityToAdd} unidades de '{itemToAdd.ItemName}'...");

            // Intentar añadir el item al inventario
            bool success = playerInventory.AddItem(itemToAdd, quantityToAdd);

            if (success)
            {
                Debug.Log($"✓ Se añadió correctamente '{itemToAdd.ItemName}' x{quantityToAdd} al inventario.");
                
                // ACCIÓN DESPUÉS DE AÑADIR
                // Por ejemplo: mostrar notificación UI, reproducir sonido, etc.
                OnItemAdded(itemToAdd, quantityToAdd);
            }
            else
            {
                Debug.LogWarning($"✗ No se pudo añadir '{itemToAdd.ItemName}'. El inventario está lleno.");
                
                // ACCIÓN SI EL INVENTARIO ESTÁ LLENO
                // Por ejemplo: mostrar mensaje "Inventario lleno", etc.
                OnInventoryFull();
            }
        }

        /// <summary>
        /// EJEMPLO 4: Elimina un item del inventario del jugador.
        /// </summary>
        /// <remarks>
        /// Usa este patrón cuando el jugador consuma un item, lo use en crafting,
        /// lo venda, etc.
        /// </remarks>
        [ContextMenu("Eliminar Item de Prueba")]
        public void RemoveItemExample()
        {
            if (itemToCheck == null)
            {
                Debug.LogWarning("[InventoryExample] No se ha asignado un ItemData para eliminar.");
                return;
            }

            int amountToRemove = 1;
            Debug.Log($"Intentando eliminar {amountToRemove} unidad(es) de '{itemToCheck.ItemName}'...");

            // Intentar eliminar el item del inventario
            bool success = playerInventory.RemoveItem(itemToCheck, amountToRemove);

            if (success)
            {
                Debug.Log($"✓ Se eliminó correctamente '{itemToCheck.ItemName}' x{amountToRemove} del inventario.");
                
                // ACCIÓN DESPUÉS DE ELIMINAR
                // Por ejemplo: aplicar efecto del item, actualizar quest, etc.
                OnItemRemoved(itemToCheck, amountToRemove);
            }
            else
            {
                Debug.LogWarning($"✗ No se pudo eliminar '{itemToCheck.ItemName}'. El jugador no tiene suficientes unidades.");
                
                // ACCIÓN SI NO HAY SUFICIENTES ITEMS
                OnNotEnoughItems(itemToCheck);
            }
        }

        /// <summary>
        /// EJEMPLO 5: Obtiene información detallada de un item.
        /// </summary>
        /// <remarks>
        /// Usa este patrón cuando necesites acceder a la información completa
        /// de un item que está en el inventario, como su cantidad actual o sus datos.
        /// </remarks>
        [ContextMenu("Obtener Información del Item")]
        public void GetItemInfo()
        {
            if (itemToCheck == null)
            {
                Debug.LogWarning("[InventoryExample] No se ha asignado un ItemData para obtener información.");
                return;
            }

            // Obtener el item del inventario
            InventoryItem item = playerInventory.GetItem(itemToCheck);

            if (item != null)
            {
                Debug.Log($"=== INFORMACIÓN DE ITEM ===");
                Debug.Log($"Nombre: {item.Data.ItemName}");
                Debug.Log($"Descripción: {item.Data.Description}");
                Debug.Log($"Cantidad en este stack: {item.Quantity}");
                Debug.Log($"Cantidad total en inventario: {playerInventory.GetItemCount(itemToCheck)}");
                Debug.Log($"Max Stack Size: {item.Data.MaxStackSize}");
                Debug.Log($"Es extremidad: {item.Data.IsLimb}");
            }
            else
            {
                Debug.Log($"El item '{itemToCheck.ItemName}' no está en el inventario.");
            }
        }

        /// <summary>
        /// EJEMPLO 6: Controla la UI del inventario.
        /// </summary>
        /// <remarks>
        /// Usa estos métodos cuando necesites abrir/cerrar el inventario programáticamente,
        /// por ejemplo en tutoriales, cinemáticas, o al interactuar con objetos.
        /// </remarks>
        [ContextMenu("Abrir Inventario")]
        public void OpenInventoryUI()
        {
            var inventoryUI = InventoryUIRegistry.GetActiveUI();
            
            if (inventoryUI != null)
            {
                inventoryUI.OpenInventory();
                Debug.Log("Inventario abierto mediante script.");
            }
            else
            {
                Debug.LogWarning("No se encontró InventoryUI en la escena.");
            }
        }

        /// <summary>
        /// Cierra la UI del inventario.
        /// </summary>
        [ContextMenu("Cerrar Inventario")]
        public void CloseInventoryUI()
        {
            var inventoryUI = InventoryUIRegistry.GetActiveUI();
            
            if (inventoryUI != null)
            {
                inventoryUI.CloseInventory();
                Debug.Log("Inventario cerrado mediante script.");
            }
        }

        /// <summary>
        /// Alterna la visibilidad del inventario.
        /// </summary>
        [ContextMenu("Alternar Inventario")]
        public void ToggleInventoryUI()
        {
            var inventoryUI = InventoryUIRegistry.GetActiveUI();
            
            if (inventoryUI != null)
            {
                inventoryUI.ToggleInventory();
                Debug.Log("Inventario alternado mediante script.");
            }
        }

        #region Callbacks de Ejemplo

        // Estos métodos son callbacks de ejemplo que puedes implementar en tu juego
        // según tus necesidades específicas.

        private void OnPlayerHasItem(ItemData _item)
        {
            // Implementa aquí lo que debe pasar cuando el jugador TIENE el item
            // Ejemplos:
            // - Abrir una puerta cerrada
            // - Permitir avanzar en una quest
            // - Activar un diálogo especial con un NPC
            // - Desbloquear una habilidad
        }

        private void OnPlayerDoesNotHaveItem(ItemData _item)
        {
            // Implementa aquí lo que debe pasar cuando el jugador NO TIENE el item
            // Ejemplos:
            // - Mostrar mensaje: "Necesitas una llave para abrir esta puerta"
            // - Marcar una ubicación en el mapa donde puede encontrar el item
            // - Dar una pista al jugador
        }

        private void OnItemAdded(ItemData _item, int _quantity)
        {
            // Implementa aquí lo que debe pasar cuando se añade un item
            // Ejemplos:
            // - Mostrar notificación en pantalla: "+1 Espada"
            // - Reproducir sonido de recolección
            // - Actualizar objetivos de quest
            // - Guardar el juego automáticamente
        }

        private void OnInventoryFull()
        {
            // Implementa aquí lo que debe pasar cuando el inventario está lleno
            // Ejemplos:
            // - Mostrar mensaje: "Inventario lleno"
            // - Sugerir al jugador que elimine items
            // - Ofrecer expandir el inventario (si tu juego lo permite)
        }

        private void OnItemRemoved(ItemData _item, int _quantity)
        {
            // Implementa aquí lo que debe pasar cuando se elimina un item
            // Ejemplos:
            // - Aplicar el efecto del item (si es consumible)
            // - Actualizar estadísticas del jugador
            // - Avanzar en una quest de crafting
            // - Reproducir animación o efecto visual
        }

        private void OnNotEnoughItems(ItemData _item)
        {
            // Implementa aquí lo que debe pasar cuando no hay suficientes items
            // Ejemplos:
            // - Mostrar mensaje: "No tienes suficientes materiales"
            // - Cancelar la acción de crafting
            // - Sugerir dónde conseguir más items
        }

        #endregion
    }
}

