using System.Collections.Generic;
using System.Linq;
using Inventory.UI;
using UI_System.Menus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Inventory
{
    /// <summary>
    /// Componente principal del inventario del jugador.
    /// Gestiona la comunicación entre el modelo de inventario y la capa de presentación.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        #region Constantes

        private const string LogPrefix = "[PlayerInventory]";
        private const string WarningNullItem = "ItemData es null";
        private const string WarningFullInventory = "Inventario lleno, no se pudo añadir: {0}";

        #endregion

        #region Campos Serializados

        [Header("Configuración")]
        [SerializeField] private int maxCapacity = 20;
        [SerializeField] private int maxLimbCapacity = 4;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        #endregion

        #region Eventos

        [Header("Eventos")]
        public UnityEvent<ItemData, int> onItemAdded = new UnityEvent<ItemData, int>();
        public UnityEvent<ItemData, int> onItemRemoved = new UnityEvent<ItemData, int>();
        public UnityEvent<ItemData, int> onItemAddedSilent = new UnityEvent<ItemData, int>();
        public UnityEvent<ItemData, int> onItemRemovedSilent = new UnityEvent<ItemData, int>();

        #endregion

        #region Campos Privados

        private Inventory _inventory;
        private InventoryUI _cachedUI;

        #endregion

        #region Propiedades Públicas

        public Inventory Inventory => _inventory;

        #endregion

        #region Métodos Unity

        private void Awake()
        {
            InitializeInventory();
        }

        private void OnToggleInventory()
        {
            if (GamePauseHandler.IsPaused) return;

            if (DialogueBox.Instance != null && DialogueBox.Instance.IsOpen) return;

            if (TryGetOrCacheUI(out var ui))
            {
                ui.ToggleInventory();
            }
            else
            {
                // Fallback: mostrar info de debug si no hay UI disponible
                LogInventoryInfo();
            }
        }

        public void OnNavigate(InputValue value)
        {
            Debug.Log("OnNavigate");
            if (TryGetOrCacheUI(out var ui))
            {
                ui.HandleNavigation(value.Get<Vector2>());
            }
        }

        #endregion

        #region API Pública

        /// <summary>
        /// Añade un ítem al inventario.
        /// </summary>
        /// <param name="itemData">Los datos del ítem a añadir</param>
        /// <param name="quantity">Cantidad a añadir (por defecto 1)</param>
        /// <returns>True si se añadió correctamente, false si el inventario está lleno</returns>
        public bool AddItem (ItemData itemData, int quantity = 1)
        {
            if (!ValidateItemData(itemData)) return false;

            var success = _inventory.AddItem(itemData, quantity);

            if (success)
            {
                OnItemAddedSuccessfully(itemData, quantity);
            }
            else
            {
                LogWarningIfEnabled(string.Format(WarningFullInventory, itemData.name));
            }

            return success;
        }

        public bool AddItemSilent (ItemData itemData, int quantity = 1)
        {
            if (!ValidateItemData(itemData)) return false;

            var success = _inventory.AddItem(itemData, quantity);

            if (success)
            {
                OnItemAddedSilentSuccessfully(itemData, quantity);
            }
            else
            {
                LogWarningIfEnabled(string.Format(WarningFullInventory, itemData.name));
            }

            return success;
        }

        /// <summary>
        /// Elimina un ítem del inventario.
        /// </summary>
        /// <param name="itemData">Los datos del ítem a eliminar</param>
        /// <param name="quantity">Cantidad a eliminar (por defecto 1)</param>
        /// <returns>True si se eliminó correctamente, false si no hay suficientes ítems</returns>
        public bool RemoveItem (ItemData itemData, int quantity = 1)
        {
            if (!ValidateItemData(itemData)) return false;

            var success = _inventory.RemoveItem(itemData, quantity);

            if (success)
            {
                OnItemRemovedSuccessfully(itemData, quantity);
            }

            return success;
        }

        public bool RemoveItemSilent (ItemData itemData, int quantity = 1)
        {
            if (!ValidateItemData(itemData)) return false;

            var success = _inventory.RemoveItem(itemData, quantity);

            if (success)
            {
                OnItemRemovedSilentSuccessfully(itemData, quantity);
            }

            return success;
        }

        /// <summary>
        /// Verifica si el inventario contiene un ítem específico.
        /// </summary>
        public bool Contains(ItemData itemData) => _inventory.Contains(itemData);

        /// <summary>
        /// Obtiene la cantidad total de un ítem en el inventario.
        /// </summary>
        public int GetItemCount(ItemData itemData) => _inventory.GetItemCount(itemData);

        /// <summary>
        /// Obtiene la primera instancia de un ítem del inventario.
        /// </summary>
        public InventoryItem GetItem(ItemData itemData) => _inventory.GetItem(itemData);

        /// <summary>
        /// Limpia completamente el inventario.
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            LogIfEnabled("Inventario limpiado");
        }

        public InventoryItem[] GetAllItems()
        {
            return _inventory.Items.ToArray();
        }

        public ItemData[] GetAllLimbs()
        {
            return _inventory.Limbs.Select(p => p.Data).ToArray();
        }


        public InventoryItem[] GetAllLimbItems()
        {
            return _inventory.Limbs.ToArray();
        }

        public ItemData GetItemDataForLimb(LimbSO limb)
        {
            if (limb == null) return null;

            // Search in Limbs
            foreach (var item in _inventory.Limbs)
            {
                if (item.Data != null && item.Data.LimbSO == limb)
                    return item.Data;
            }

            // Search in Items (just in case)
            foreach (var item in _inventory.Items)
            {
                if (item.Data != null && item.Data.LimbSO == limb)
                    return item.Data;
            }

            return null;
        }

        #endregion

        #region Métodos Privados

        private void InitializeInventory()
        {
            _inventory = new Inventory(maxCapacity, maxLimbCapacity);
        }

        private bool TryGetOrCacheUI(out InventoryUI ui)
        {
            if (_cachedUI != null)
            {
                ui = _cachedUI;
                return true;
            }

            _cachedUI = InventoryUIRegistry.Get();
            ui = _cachedUI;
            return _cachedUI != null;
        }

        private bool ValidateItemData(ItemData itemData)
        {
            if (itemData != null) return true;

            LogWarningIfEnabled(WarningNullItem);
            return false;
        }

        private void OnItemAddedSuccessfully (ItemData itemData, int quantity)
        {
            onItemAdded?.Invoke(itemData, quantity);
            LogIfEnabled($"✓ Añadido: {itemData.name} x{quantity}");
        }


        private void OnItemAddedSilentSuccessfully (ItemData itemData, int quantity)
        {
            onItemAddedSilent?.Invoke(itemData, quantity);
            LogIfEnabled($"✓ Añadido: {itemData.name} x{quantity}");
        }

        private void OnItemRemovedSilentSuccessfully (ItemData itemData, int quantity)
        {
            onItemRemovedSilent?.Invoke(itemData, quantity);
            LogIfEnabled($"✓ Eliminado: {itemData.name} x{quantity}");
        }

        private void OnItemRemovedSuccessfully (ItemData itemData, int quantity)
        {
            onItemRemoved?.Invoke(itemData, quantity);
            LogIfEnabled($"✓ Eliminado: {itemData.name} x{quantity}");
        }

        private void LogInventoryInfo()
        {
            if (!enableDebugLogs) return;

            Debug.Log($"{LogPrefix} === INVENTARIO DEL JUGADOR ===");
            LogItemList("Items normales", _inventory.Items, maxCapacity);
            LogItemList("Extremidades", _inventory.Limbs, maxLimbCapacity);
            Debug.Log($"{LogPrefix} ==============================");
        }

        private void LogItemList(string categoryName, IReadOnlyList<InventoryItem> items, int capacity)
        {
            Debug.Log($"{LogPrefix} {categoryName}: {items.Count}/{capacity}");

            foreach (var item in items)
            {
                Debug.Log($"{LogPrefix}   - {item.Data.ItemName} x{item.Quantity}");
            }
        }

        private void LogIfEnabled(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"{LogPrefix} {message}");
            }
        }

        private void LogWarningIfEnabled(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"{LogPrefix} {message}");
            }
        }

        #endregion
    }
}
