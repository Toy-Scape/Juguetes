﻿using UnityEngine;

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
        [SerializeField] private string itemName;
        [SerializeField, TextArea(3, 5)] private string description;
        
        [Header("Visual")]
        [SerializeField] private Sprite icon;
        
        [Header("Configuración de Stack")]
        [SerializeField, Min(MinStackSize)] 
        private int maxStackSize = DefaultStackSize;
        
        [Header("Categoría")]
        [SerializeField] private bool isLimb;

        /// <summary>Nombre del ítem que se muestra en la UI</summary>
        public string ItemName => itemName;
        
        /// <summary>Descripción detallada del ítem</summary>
        public string Description => description;
        
        /// <summary>Icono visual del ítem para la UI</summary>
        public Sprite Icon => icon;
        
        /// <summary>Cantidad máxima que puede apilarse en un slot</summary>
        public int MaxStackSize => maxStackSize;
        
        /// <summary>Indica si el ítem es una extremidad (limb)</summary>
        public bool IsLimb => isLimb;

        private void OnValidate()
        {
            maxStackSize = Mathf.Max(MinStackSize, maxStackSize);
        }
    }
}

