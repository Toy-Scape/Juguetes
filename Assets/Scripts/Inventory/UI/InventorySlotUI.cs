using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Inventory.UI
{
    /// <summary>
    /// Representa un slot individual en la UI del inventario.
    /// Muestra el icono, nombre, descripción y cantidad del item.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
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

        private InventoryItem currentItem;
        private bool isSelected;

        public InventoryItem CurrentItem => currentItem;
        public bool IsEmpty => currentItem == null;

        public void SetItem(InventoryItem item)
        {
            currentItem = item;

            if (item == null || item.Data == null)
            {
                Clear();
                return;
            }

            // Actualizar icono
            if (iconImage != null)
            {
                iconImage.sprite = item.Data.Icon;
                iconImage.enabled = item.Data.Icon != null;
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
                backgroundImage.color = isSelected ? selectedColor : filledColor;
            }
        }

        public void Clear()
        {
            currentItem = null;

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

        public void SetSelected(bool selected)
        {
            isSelected = selected;

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
            SetItem(currentItem);
        }
    }
}

