using UnityEngine;

namespace Inventory.UI
{
    /// <summary>
    /// Registro ligero para la instancia activa de InventoryUI.
    /// Evita búsquedas globales repetidas y evita el uso de singletons acoplados.
    /// </summary>
    public static class InventoryUIRegistry
    {
        private static InventoryUI _registered;

        public static void Register(InventoryUI ui)
        {
            if (ui == null) return;
            if (_registered != null && _registered != ui)
            {
                Debug.LogWarning("[InventoryUIRegistry] Ya hay una InventoryUI registrada. Se reemplazará la referencia.");
            }
            _registered = ui;
        }

        public static void Unregister(InventoryUI ui)
        {
            if (_registered == ui)
                _registered = null;
        }

        public static InventoryUI Get()
        {
            return _registered;
        }

        /// <summary>
        /// Obtiene la instancia activa de InventoryUI.
        /// Alias de Get() para mayor claridad en la API pública.
        /// </summary>
        public static InventoryUI GetActiveUI()
        {
            return _registered;
        }
    }
}

