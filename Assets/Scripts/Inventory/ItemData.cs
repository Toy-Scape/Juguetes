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
        [SerializeField] private string itemName;
        [SerializeField, TextArea(3, 5)] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private int maxStackSize = 1;
        [SerializeField] private bool isLimb;

        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStackSize => maxStackSize;
        public bool IsLimb => isLimb;

        private void OnValidate()
        {
            if (maxStackSize < 1)
                maxStackSize = 1;
        }
    }
}

