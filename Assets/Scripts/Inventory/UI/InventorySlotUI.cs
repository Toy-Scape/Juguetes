﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Inventory.UI
{
    /// <summary>
    /// Representa un slot individual en la UI del inventario.
    /// Muestra el icono, nombre, descripción y cantidad del item.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [Header("Referencias UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private GameObject quantityContainer;
        [SerializeField] private Image backgroundImage;

        [Header("Colores")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color filledColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color selectedColor = new Color(0.5f, 0.7f, 1f, 0.9f);

        [Header("Fallbacks")]
        [SerializeField] private Sprite placeholderIcon;

        private InventoryItem _currentItem;
        private bool _isSelected;

        public InventoryItem CurrentItem => _currentItem;
        public bool IsEmpty => _currentItem == null;

        public void SetItem(InventoryItem item)
        {
            _currentItem = item;

            if (item == null || item.Data == null)
            {
                Clear();
                return;
            }

            // Actualizar icono
            if (iconImage != null)
            {
                var spriteToUse = item.Data.Icon != null ? item.Data.Icon : placeholderIcon;
                iconImage.sprite = spriteToUse;
                iconImage.enabled = spriteToUse != null;
                iconImage.color = Color.white;
            }

            // Actualizar nombre
            if (nameText != null)
            {
                nameText.text = item.Data.ItemName;
                nameText.enabled = true;
            }

            // Actualizar cantidad
            if (quantityText != null)
            {
                quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : "";
                quantityText.enabled = item.Quantity > 1;
            }

            if (quantityContainer != null)
            {
                quantityContainer.SetActive(item.Quantity > 1);
            }

            // Actualizar fondo
            if (backgroundImage != null)
            {
                backgroundImage.color = _isSelected ? selectedColor : filledColor;
            }
        }

        public void Clear()
        {
            _currentItem = null;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = "";
                nameText.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }

            if (quantityContainer != null)
            {
                quantityContainer.SetActive(false);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }
        }

        private void OnDisable()
        {
            // Asegurar que tooltip se oculte si el slot se desactiva
            var ui = InventoryUIRegistry.Get();
            if (ui != null)
                ui.HideTooltip();
        }

        // IPointer handlers para mostrar tooltip
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_currentItem == null || _currentItem.Data == null)
                return;

            var ui = InventoryUIRegistry.Get();
            if (ui != null)
            {
                // Usar eventData.position para evitar problemas con el nuevo Input System
                ui.ShowTooltip(_currentItem, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var ui = InventoryUIRegistry.Get();
            if (ui != null)
            {
                ui.HideTooltip();
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            // Actualizar posición del tooltip cuando el ratón se mueva
            if (_currentItem == null)
                return;

            var ui = InventoryUIRegistry.Get();
            if (ui != null)
            {
                // Usar eventData.position para mejor precisión
                ui.ShowTooltip(_currentItem, eventData.position);
            }
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (backgroundImage != null)
            {
                if (IsEmpty)
                    backgroundImage.color = emptyColor;
                else
                    backgroundImage.color = selected ? selectedColor : filledColor;
            }
        }

        public void RefreshDisplay()
        {
            SetItem(_currentItem);
        }
    }
}
