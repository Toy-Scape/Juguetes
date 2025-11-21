using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Inventory.UI
{
    /// <summary>
    /// UI principal del inventario. Se encarga de mostrar todos los items,
    /// gestionar la navegación entre tabs y mostrar detalles del item seleccionado.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Referencias Principales")]
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private GameObject inventoryPanel;

        [Header("Contenedores de Slots")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform limbsContainer;

        [Header("Prefab de Slot")]
        [SerializeField] private GameObject slotPrefab;

        [Header("Tabs")]
        [SerializeField] private GameObject itemsTab;
        [SerializeField] private GameObject limbsTab;
        [SerializeField] private Button itemsTabButton;
        [SerializeField] private Button limbsTabButton;

        [Header("Panel de Detalles")]
        [SerializeField] private GameObject detailsPanel;
        [SerializeField] private Image detailIcon;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailQuantityText;

        [Header("Configuración")]
        [SerializeField] private int maxSlotsToDisplay = 20;
        [SerializeField] private bool hideOnStart = true;

        private List<InventorySlotUI> itemSlots = new List<InventorySlotUI>();
        private List<InventorySlotUI> limbSlots = new List<InventorySlotUI>();
        private InventorySlotUI selectedSlot;
        private bool isShowingItems = true;
        private bool isInventoryOpen = false;

        private void Awake()
        {
            // Buscar PlayerInventory si no está asignado
            if (playerInventory == null)
            {
                playerInventory = FindObjectOfType<PlayerInventory>();
            }

            InitializeSlots();
            SetupButtons();

            if (hideOnStart && inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (playerInventory != null)
            {
                playerInventory.onItemAdded.AddListener(OnInventoryChanged);
                playerInventory.onItemRemoved.AddListener(OnInventoryChanged);
            }
        }

        private void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.onItemAdded.RemoveListener(OnInventoryChanged);
                playerInventory.onItemRemoved.RemoveListener(OnInventoryChanged);
            }
        }

        #region Input Messages

        void OnToggleInventory()
        {
            ToggleInventory();
        }

        #endregion

        #region Inicialización

        private void InitializeSlots()
        {
            // Crear slots para items normales
            if (itemsContainer != null && slotPrefab != null)
            {
                CreateSlots(itemsContainer, itemSlots, maxSlotsToDisplay);
            }

            // Crear slots para extremidades
            if (limbsContainer != null && slotPrefab != null)
            {
                CreateSlots(limbsContainer, limbSlots, playerInventory != null ? playerInventory.Inventory.MaxLimbCapacity : 4);
            }
        }

        private void CreateSlots(Transform container, List<InventorySlotUI> slotList, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, container);
                InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();

                if (slot != null)
                {
                    slotList.Add(slot);

                    // Añadir evento de click
                    Button button = slotObj.GetComponent<Button>();
                    if (button != null)
                    {
                        InventorySlotUI capturedSlot = slot;
                        button.onClick.AddListener(() => OnSlotClicked(capturedSlot));
                    }
                }
            }
        }

        private void SetupButtons()
        {
            if (itemsTabButton != null)
            {
                itemsTabButton.onClick.AddListener(() => ShowTab(true));
            }

            if (limbsTabButton != null)
            {
                limbsTabButton.onClick.AddListener(() => ShowTab(false));
            }
        }

        #endregion

        #region Actualización UI

        public void RefreshUI()
        {
            if (playerInventory == null || playerInventory.Inventory == null)
                return;

            RefreshItemsTab();
            RefreshLimbsTab();
            RefreshDetailsPanel();
        }

        private void RefreshItemsTab()
        {
            var items = playerInventory.Inventory.Items;

            for (int i = 0; i < itemSlots.Count; i++)
            {
                if (i < items.Count)
                {
                    itemSlots[i].SetItem(items[i]);
                }
                else
                {
                    itemSlots[i].Clear();
                }
            }
        }

        private void RefreshLimbsTab()
        {
            var limbs = playerInventory.Inventory.Limbs;

            for (int i = 0; i < limbSlots.Count; i++)
            {
                if (i < limbs.Count)
                {
                    limbSlots[i].SetItem(limbs[i]);
                }
                else
                {
                    limbSlots[i].Clear();
                }
            }
        }

        private void RefreshDetailsPanel()
        {
            if (detailsPanel == null)
                return;

            if (selectedSlot == null || selectedSlot.IsEmpty)
            {
                detailsPanel.SetActive(false);
                return;
            }

            detailsPanel.SetActive(true);

            var item = selectedSlot.CurrentItem;

            if (detailIcon != null && item.Data.Icon != null)
            {
                detailIcon.sprite = item.Data.Icon;
                detailIcon.enabled = true;
            }
            else if (detailIcon != null)
            {
                detailIcon.enabled = false;
            }

            if (detailNameText != null)
            {
                detailNameText.text = item.Data.ItemName;
            }

            if (detailDescriptionText != null)
            {
                detailDescriptionText.text = item.Data.Description;
            }

            if (detailQuantityText != null)
            {
                detailQuantityText.text = $"Cantidad: {item.Quantity}";
            }
        }

        #endregion

        #region Gestión de Tabs

        private void ShowTab(bool showItems)
        {
            isShowingItems = showItems;

            if (itemsTab != null)
                itemsTab.SetActive(showItems);

            if (limbsTab != null)
                limbsTab.SetActive(!showItems);

            // Deseleccionar slot al cambiar de tab
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
                RefreshDetailsPanel();
            }
        }

        #endregion

        #region Eventos

        private void OnSlotClicked(InventorySlotUI slot)
        {
            if (slot == null || slot.IsEmpty)
                return;

            // Deseleccionar el anterior
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
            }

            // Seleccionar el nuevo
            selectedSlot = slot;
            selectedSlot.SetSelected(true);

            RefreshDetailsPanel();
        }

        private void OnInventoryChanged(ItemData itemData, int quantity)
        {
            if (isInventoryOpen)
            {
                RefreshUI();
            }
        }

        #endregion

        #region Control de Visibilidad

        public void ToggleInventory()
        {
            if (inventoryPanel == null)
                return;

            isInventoryOpen = !isInventoryOpen;

            if (isInventoryOpen)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }

        public void OpenInventory()
        {
            if (inventoryPanel == null)
                return;

            isInventoryOpen = true;
            inventoryPanel.SetActive(true);
            RefreshUI();
            ShowTab(true); // Mostrar items por defecto

            // Pausar el juego o desactivar controles del jugador aquí si es necesario
            // Time.timeScale = 0f;
        }

        public void CloseInventory()
        {
            if (inventoryPanel == null)
                return;

            isInventoryOpen = false;
            inventoryPanel.SetActive(false);

            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }

            // Reanudar el juego aquí si es necesario
            // Time.timeScale = 1f;
        }

        #endregion
    }
}

