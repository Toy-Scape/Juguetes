using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace Inventory.UI
{
    /// <summary>
    /// UI principal del inventario. Se encarga de mostrar todos los items,
    /// gestionar la navegación entre tabs y mostrar detalles del item seleccionado.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        // Ya no usamos singleton; usamos InventoryUIRegistry para registrar la instancia activa.

        [Header("Referencias Principales")]
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private GameObject inventoryPanel;

        [Header("Contenedores de Slots")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform limbsContainer;

        [Header("Prefab de Slot")]
        [SerializeField] private GameObject slotPrefab;

        [Header("Tooltip (hover)")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipNameText;
        [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(15, -15);

        [Header("Configuración")]
        [SerializeField] private int maxSlotsToDisplay = 20;
        [SerializeField] private bool hideOnStart = true;

        private List<InventorySlotUI> _itemSlots = new List<InventorySlotUI>();
        private List<InventorySlotUI> _limbSlots = new List<InventorySlotUI>();
        private InventorySlotUI _selectedSlot;
        private bool _isInventoryOpen = false;

        private void Awake()
        {
            // Registrar esta instancia en el registro central
            InventoryUIRegistry.Register(this);

            // Buscar PlayerInventory si no está asignado
            if (playerInventory == null)
            {
                playerInventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
            }

            InitializeSlots();

            if (hideOnStart && inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }

            // Aseguramos que el tooltip esté oculto al inicio
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (playerInventory == null)
            {
                playerInventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
            }

            // Asegurarnos de que los slots existen (útil si playerInventory no estaba listo en Awake)
            InitializeSlots();

            if (playerInventory != null)
            {
                playerInventory.onItemAdded.AddListener(OnInventoryChanged);
                playerInventory.onItemRemoved.AddListener(OnInventoryChanged);
            }

            // Asegurarnos de que la UI refleja el estado actual del inventario al activarse.
            RefreshUI();

            // Diagnóstico: si hay limbs en el inventario pero no hay slots suficientes, avisar.
            if (playerInventory != null && playerInventory.Inventory != null)
            {
                int limbCount = playerInventory.Inventory.Limbs.Count;
                if (limbCount > _limbSlots.Count)
                {
                    Debug.LogWarning($"[InventoryUI] Hay {limbCount} limbs en el inventario pero solo {_limbSlots.Count} slots disponibles.");
                }
            }
        }

        private void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.onItemAdded.RemoveListener(OnInventoryChanged);
                playerInventory.onItemRemoved.RemoveListener(OnInventoryChanged);
            }
            // No hacemos Instance = null aquí ya que OnDestroy se encargará.
        }

        private void OnDestroy()
        {
            InventoryUIRegistry.Unregister(this);
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
                CreateSlots(itemsContainer, _itemSlots, maxSlotsToDisplay);
            }

            // Crear slots para extremidades
            if (limbsContainer != null && slotPrefab != null)
            {
                CreateSlots(limbsContainer, _limbSlots, playerInventory != null ? playerInventory.Inventory.MaxLimbCapacity : 4);
            }
        }

        private void CreateSlots(Transform container, List<InventorySlotUI> slotList, int count)
        {
            if (container == null || slotPrefab == null)
                return;

            int toCreate = Mathf.Max(0, count - slotList.Count);

            for (int i = 0; i < toCreate; i++)
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

        #endregion

        #region Actualización UI

        public void RefreshUI()
        {
            if (playerInventory == null || playerInventory.Inventory == null)
                return;

            RefreshItemsTab();
            RefreshLimbsTab();
        }

        private void RefreshItemsTab()
        {
            var items = playerInventory.Inventory.Items;

            // Comprobación: ¿el primer item es limb? (puede indicar containers cruzados)
            if (items.Count > 0 && items[0].Data != null && items[0].Data.IsLimb)
            {
                Debug.LogWarning("[InventoryUI] ¡Parece que los containers están cruzados! El primer item normal es un limb. Revisa la asignación de itemsContainer y limbsContainer en el inspector.");
            }

            for (int i = 0; i < _itemSlots.Count; i++)
            {
                if (i < items.Count)
                {
                    _itemSlots[i].SetItem(items[i]);
                }
                else
                {
                    _itemSlots[i].Clear();
                }
            }
        }

        private void RefreshLimbsTab()
        {
            var limbs = playerInventory.Inventory.Limbs;

            // Comprobación: ¿el primer limb NO es limb? (puede indicar containers cruzados)
            if (limbs.Count > 0 && limbs[0].Data != null && !limbs[0].Data.IsLimb)
            {
                Debug.LogWarning("[InventoryUI] ¡Parece que los containers están cruzados! El primer limb no es limb. Revisa la asignación de itemsContainer y limbsContainer en el inspector.");
            }

            for (int i = 0; i < _limbSlots.Count; i++)
            {
                if (i < limbs.Count)
                {
                    _limbSlots[i].SetItem(limbs[i]);
                }
                else
                {
                    _limbSlots[i].Clear();
                }
            }
        }

        #endregion

        #region Eventos

        private void OnSlotClicked(InventorySlotUI slot)
        {
            if (slot == null || slot.IsEmpty)
                return;

            // Deseleccionar el anterior
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
            }

            // Seleccionar el nuevo
            _selectedSlot = slot;
            _selectedSlot.SetSelected(true);
        }

        private void OnInventoryChanged(ItemData itemData, int quantity)
        {
            // Siempre refresh en memoria; si el panel está abierto, refrescamos la UI visible.
            RefreshUI();
        }

        #endregion

        #region Control de Visibilidad

        public void ToggleInventory()
        {
            if (inventoryPanel == null)
                return;

            _isInventoryOpen = !_isInventoryOpen;

            if (_isInventoryOpen)
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

            _isInventoryOpen = true;
            inventoryPanel.SetActive(true);
            RefreshUI();
            // Mostrar items por defecto

            // Habilitar el cursor del ratón
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Cambiar el modo de acción del PlayerInput a UI si existe
            var playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null && playerInput.currentActionMap.name != "UI")
            {
                var uiActionMap = playerInput.actions.FindActionMap("UI", true);
                if (uiActionMap != null)
                    playerInput.SwitchCurrentActionMap("UI");
            }

            // Pausar el juego o desactivar controles del jugador aquí si es necesario
            // Time.timeScale = 0f;
        }

        public void CloseInventory()
        {
            if (inventoryPanel == null)
                return;

            _isInventoryOpen = false;
            inventoryPanel.SetActive(false);

            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
                _selectedSlot = null;
            }

            // Ocultar y bloquear el cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Cambiar el modo de acción del PlayerInput a Player si existe
            var playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null && playerInput.currentActionMap.name != "Player")
            {
                var playerActionMap = playerInput.actions.FindActionMap("Player", true);
                if (playerActionMap != null)
                    playerInput.SwitchCurrentActionMap("Player");
            }

            // Reanudar el juego aquí si es necesario
            // Time.timeScale = 1f;
        }

        #endregion

        #region Tooltip

        public void ShowTooltip(InventoryItem item, Vector2 screenPosition)
        {
            if (tooltipPanel == null || item == null || item.Data == null)
                return;

            // Actualizar nombre y descripción
            if (tooltipNameText != null)
                tooltipNameText.text = item.Data.ItemName;

            if (tooltipDescriptionText != null)
                tooltipDescriptionText.text = item.Data.Description;

            tooltipPanel.SetActive(true);

            // Posicionar el tooltip cerca del cursor con offset configurable
            RectTransform rt = tooltipPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 anchoredPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt.parent as RectTransform, 
                    screenPosition, 
                    null, 
                    out anchoredPos
                );
                
                // Aplicar offset configurable desde el inspector
                anchoredPos += tooltipOffset;
                rt.anchoredPosition = anchoredPos;
            }
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        #endregion
    }
}

