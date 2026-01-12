using Unity.VisualScripting;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// ScriptableObject base para todos los items del juego.
    /// Define las propiedades compartidas de cualquier ítem.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        private const int MinStackSize = 1;
        private const int DefaultStackSize = 1;

        [Header("Información Básica")]
        [Header("Localization Keys")]
        [SerializeField] private string nameKey;
        [SerializeField] private string descriptionKey;

        [Header("Visual")]
        [SerializeField] private Sprite icon;

        [Header("Configuración de Stack")]
        [SerializeField, Min(MinStackSize)]
        private int maxStackSize = DefaultStackSize;

        [Header("Categoría")]
        [SerializeField] private bool isLimb;

        [Header("Limb SO")]
        [SerializeField] private LimbSO limbSO;

        /// <summary>Clave de localización para el nombre</summary>
        public string NameKey => nameKey;

        /// <summary>Clave de localización para la descripción</summary>
        public string DescriptionKey => descriptionKey;

        // Propiedades antiguas marcadas como obsoletas o redirigiendo a la key (temporalmente)
        public string ItemName => nameKey;
        public string Description => descriptionKey;

        /// <summary>Icono visual del ítem para la UI</summary>
        public Sprite Icon => icon;

        /// <summary>Cantidad máxima que puede apilarse en un slot</summary>
        public int MaxStackSize => maxStackSize;

        /// <summary>Indica si el ítem es una extremidad (limb)</summary>
        public bool IsLimb => isLimb;

        private void OnValidate()
        {
            maxStackSize = Mathf.Max(MinStackSize, maxStackSize);

            // Limpiar limbSO si isLimb es false
            if (!isLimb)
            {
                limbSO = null;
            }
        }

        public LimbSO LimbSO => limbSO;
    }
}
