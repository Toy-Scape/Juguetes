using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        [SerializeField] private TMP_Text inventoryTitle;

        [Header("Contenedores de Slots")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private Transform limbsContainer;

        [Header("Prefab de Slot")]
        [SerializeField] private GameObject slotPrefab;

        [Header("Tooltip (hover)")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private Image tooltipImage;
        [SerializeField] private TextMeshProUGUI tooltipNameText;
        [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
        [SerializeField] private Vector2 tooltipOffset = new Vector2(15, -15);

        [Header("Configuración")]
        [SerializeField] private bool hideOnStart = true;
        [SerializeField] private int itemsGridColumns = 5;
        [SerializeField] private int limbsGridColumns = 1;
        [SerializeField] private Color selectedSlotColor = new Color(0.5f, 0.7f, 1f, 0.9f);

        [Header("Message tooltips")]
        [SerializeField] private GameObject ItemMessage;
        [SerializeField] private GameObject ItemMessageContainer;

        private List<InventorySlotUI> _itemSlots = new List<InventorySlotUI>();
        private List<InventorySlotUI> _limbSlots = new List<InventorySlotUI>();
        private InventorySlotUI _selectedSlot;
        private bool _isInventoryOpen;

        public static event Action OnInventoryOpened;
        public static event Action OnInventoryClosed;

        private void Awake()
        {
            // Registrar esta instancia en el registro central
            InventoryUIRegistry.Register(this);

            // Ocultar el panel PRIMERO antes de cualquier otra cosa
            if (hideOnStart && inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
                _isInventoryOpen = false;
            }

            

            // Aseguramos que el tooltip esté oculto al inicio
            //if (tooltipPanel != null)
            //    tooltipPanel.SetActive(false);

            // Buscar PlayerInventory si no está asignado
            if (playerInventory == null)
            {
                playerInventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
            }
        }

        void Start()
        {
            // Inicializar slots en Start para garantizar que PlayerInventory.Awake() ya se ejecutó
            InitializeSlots();

            // Asegurar que el cursor esté correctamente configurado al inicio
            if (!_isInventoryOpen)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnEnable()
        {
            if (playerInventory == null)
            {
                playerInventory = UnityEngine.Object.FindFirstObjectByType<PlayerInventory>();
            }

            // Suscribirse a eventos
            if (playerInventory != null)
            {
                playerInventory.onItemAdded.AddListener(OnItemAdded);
                playerInventory.onItemRemoved.AddListener(OnItemRemoved);
                playerInventory.onItemAddedSilent.AddListener(OnItemAddedSilent);
                playerInventory.onItemRemovedSilent.AddListener(OnItemRemovedSilent);
            }

            // Solo refrescar la UI si el inventario está abierto
            if (_isInventoryOpen)
            {
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.onItemAdded.RemoveListener(OnItemAdded);
                playerInventory.onItemRemoved.RemoveListener(OnItemRemoved);
                playerInventory.onItemAddedSilent.RemoveListener(OnItemAddedSilent);
                playerInventory.onItemRemovedSilent.RemoveListener(OnItemRemovedSilent);
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

        public void HandleNavigation(Vector2 input)
        {
            if (!_isInventoryOpen) return;

            // Normalizar input para evitar movimientos diagonales accidentales y ruido
            if (input.magnitude > 0.5f)
            {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                    input = new Vector2(Mathf.Sign(input.x), 0);
                else
                    input = new Vector2(0, Mathf.Sign(input.y));

                MoveSelection(input);
            }
        }

        private void MoveSelection(Vector2 direction)
        {
            // Si no hay nada seleccionado, seleccionar el primero
            if (_selectedSlot == null)
            {
                if (_itemSlots.Count > 0) SelectSlot(_itemSlots[0]);
                else if (_limbSlots.Count > 0) SelectSlot(_limbSlots[0]);
                return;
            }

            // Determinar en qué lista estamos
            List<InventorySlotUI> currentList = null;
            int currentIndex = -1;
            int currentColumns = 1;

            if (_itemSlots.Contains(_selectedSlot))
            {
                currentList = _itemSlots;
                currentIndex = _itemSlots.IndexOf(_selectedSlot);
                currentColumns = itemsGridColumns;
            }
            else if (_limbSlots.Contains(_selectedSlot))
            {
                currentList = _limbSlots;
                currentIndex = _limbSlots.IndexOf(_selectedSlot);
                currentColumns = limbsGridColumns;
            }

            if (currentList == null || currentIndex == -1) return;

            int newIndex = currentIndex;

            // Movimiento Horizontal
            if (direction.x != 0)
            {
                // Calcular fila y columna actual
                int row = currentIndex / currentColumns;
                int col = currentIndex % currentColumns;

                // Intentar mover horizontalmente
                int newCol = col + (int)direction.x;

                // Lógica de salto entre grids
                if (currentList == _itemSlots)
                {
                    // Si estamos en Items y nos salimos por la derecha -> Ir a Limbs
                    if (newCol >= currentColumns)
                    {
                        if (_limbSlots.Count > 0)
                        {
                            // Intentar mantener la fila relativa
                            int targetIndex = Mathf.Min(row, _limbSlots.Count - 1);
                            SelectSlot(_limbSlots[targetIndex]);
                        }
                        return;
                    }
                }
                else if (currentList == _limbSlots)
                {
                    // Si estamos en Limbs y nos salimos por la izquierda -> Ir a Items
                    if (newCol < 0)
                    {
                        if (_itemSlots.Count > 0)
                        {
                            // Ir a la última columna de la misma fila en Items
                            int targetRowStart = row * itemsGridColumns;
                            if (targetRowStart < _itemSlots.Count)
                            {
                                int targetIndex = Mathf.Min(targetRowStart + (itemsGridColumns - 1), _itemSlots.Count - 1);
                                SelectSlot(_itemSlots[targetIndex]);
                            }
                            else
                            {
                                SelectSlot(_itemSlots[_itemSlots.Count - 1]);
                            }
                        }
                        return;
                    }
                }

                // Movimiento normal dentro del grid
                if (newCol >= 0 && newCol < currentColumns)
                {
                    newIndex = row * currentColumns + newCol;
                }
                else
                {
                    return;
                }
            }
            // Movimiento Vertical
            else if (direction.y != 0)
            {
                newIndex -= (int)direction.y * currentColumns;
            }

            // Validar límites generales del índice
            if (newIndex >= 0 && newIndex < currentList.Count)
            {
                SelectSlot(currentList[newIndex]);
            }
        }

        private void SelectSlot (InventorySlotUI slot)
        {
            if (slot == null) return;

            if (_selectedSlot != null)
                _selectedSlot.SetSelected(false);

            _selectedSlot = slot;
            _selectedSlot.SetSelected(true);

            //if (!slot.IsEmpty)
            //{
            //    RectTransform slotRect = slot.GetComponent<RectTransform>();
            //    if (slotRect != null)
            //    {
            //        Vector3 worldCenter = slotRect.TransformPoint(slotRect.rect.center);
            //        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldCenter);

            //        ShowTooltip(slot.CurrentItem, screenPos);
            //    }
            //}
            //else
            //{
            //    HideTooltip();
            //}
        }


        #endregion

        #region Inicialización

        private void InitializeSlots()
        {
            // Verificar que todo esté listo antes de crear slots
            if (playerInventory == null || playerInventory.Inventory == null)
            {
                Debug.LogWarning("[InventoryUI] PlayerInventory o su Inventory aún no están inicializados. Se intentará más tarde.");
                return;
            }

            if (slotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] slotPrefab es NULL! Asígnalo en el Inspector.");
                return;
            }

            Debug.Log($"[InventoryUI] InitializeSlots() - itemsContainer: {(itemsContainer != null ? itemsContainer.name : "NULL")}, limbsContainer: {(limbsContainer != null ? limbsContainer.name : "NULL")}, slotPrefab: OK");

            // Crear slots para items normales usando la capacidad del inventario
            if (itemsContainer != null)
            {
                // Configurar Grid Layout
                GridLayoutGroup grid = itemsContainer.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = itemsGridColumns;
                }

                int itemCapacity = playerInventory.Inventory.MaxCapacity;
                Debug.Log($"[InventoryUI] Creando slots para items. Capacidad configurada: {itemCapacity}");
                CreateSlots(itemsContainer, _itemSlots, itemCapacity);
            }
            else
            {
                Debug.LogWarning("[InventoryUI] itemsContainer es NULL! Asígnalo en el Inspector.");
            }

            // Crear slots para extremidades usando la capacidad del inventario
            if (limbsContainer != null)
            {
                // Configurar Grid Layout
                GridLayoutGroup grid = limbsContainer.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = limbsGridColumns;
                }

                int limbCapacity = playerInventory.Inventory.MaxLimbCapacity;
                Debug.Log($"[InventoryUI] Creando slots para limbs. Capacidad configurada: {limbCapacity}");
                CreateSlots(limbsContainer, _limbSlots, limbCapacity);
            }
            else
            {
                Debug.LogWarning("[InventoryUI] limbsContainer es NULL! Asígnalo en el Inspector.");
            }

            // Refrescar la UI después de crear los slots para mostrar items que ya existan
            RefreshUI();
        }

        private void CreateSlots(Transform container, List<InventorySlotUI> slotList, int count)
        {
            if (container == null)
            {
                Debug.LogWarning("[InventoryUI] Container es null, no se pueden crear slots");
                return;
            }

            if (slotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] SlotPrefab es null, no se pueden crear slots");
                return;
            }

            int toCreate = Mathf.Max(0, count - slotList.Count);

            if (toCreate > 0)
            {
                Debug.Log($"[InventoryUI] Creando {toCreate} slots en {container.name}. Capacidad total: {count}");
            }

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

                    // Configurar color de selección
                    slot.SetSelectedColor(selectedSlotColor);
                }
                else
                {
                    Debug.LogWarning("[InventoryUI] El prefab de slot no tiene el componente InventorySlotUI");
                }
            }
        }

        /// <summary>
        /// Refresca toda la interfaz del inventario para mostrar los items actuales.
        /// </summary>
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

            if (limbs.Count > 0 && _limbSlots.Count == 0)
            {
                Debug.LogWarning($"[InventoryUI] Hay {limbs.Count} limbs en el inventario pero NO hay slots creados! Verifica que limbsContainer esté asignado en el Inspector.");
            }

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

            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
            }

            _selectedSlot = slot;
            _selectedSlot.SetSelected(true);
        }

        private void OnInventoryChanged (ItemData itemData, int quantity)
        {
            RefreshUI();
        }

        private void OnItemAdded (ItemData itemData, int quantity)
        {
            var Message = Instantiate(ItemMessage, ItemMessageContainer.transform);
            Message.GetComponent<ItemMessageTooltip>().SetData(ItemMessageType.Added, itemData.Icon, itemData.ItemName, quantity);
            Debug.Log("Added item");
            RefreshUI();
        }

        private void OnItemRemoved (ItemData itemData, int quantity)
        {
            var Message = Instantiate(ItemMessage, ItemMessageContainer.transform);
            Message.GetComponent<ItemMessageTooltip>().SetData(ItemMessageType.Removed, itemData.Icon, itemData.ItemName, quantity);
            Debug.Log("Removed item");

            RefreshUI();
        }

        private void OnItemAddedSilent (ItemData itemData, int quantity)
        {
            // No mostramos mensaje, solo refrescamos UI
            RefreshUI();
        }

        private void OnItemRemovedSilent (ItemData itemData, int quantity)
        {
            // No mostramos mensaje, solo refrescamos UI
            RefreshUI();
        }

        #endregion

        #region Control de Visibilidad

        private int _lastToggleFrame = -1;

        public void ToggleInventory()
        {
            if (inventoryPanel == null)
                return;

            // Prevent doubleinput (e.g. switching action maps triggering input again in same frame)
            if (Time.frameCount == _lastToggleFrame)
            {
                Debug.Log($"[InventoryUI] ToggleInventory ignored (Duplicate call in Frame {Time.frameCount})");
                return;
            }
            _lastToggleFrame = Time.frameCount;

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

            inventoryTitle.text = Localization.LocalizationManager.Instance != null
                ? Localization.LocalizationManager.Instance.GetLocalizedValue("inventory-title")
                : "Inventory";

            _isInventoryOpen = true;
            Debug.Log($"[InventoryUI] OpenInventory. Frame: {Time.frameCount}");
            
            // Revert debug name if needed, or just keep original name
            if (inventoryPanel.name.Contains("[ACTIVE"))
            {
                inventoryPanel.name = "Inventory Panel";
            }
            
            inventoryPanel.SetActive(true);

            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Ensure correct sorting and visibility
                canvas.sortingOrder = 999;
                if (!canvas.enabled) canvas.enabled = true;
                
                var group = inventoryPanel.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 1f;
                    group.blocksRaycasts = true;
                }
            }

            RefreshUI();
            OnInventoryOpened?.Invoke();
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
            OnInventoryClosed?.Invoke();
            Debug.Log($"[InventoryUI] CloseInventory. Frame: {Time.frameCount}");
        }

        #endregion

        #region Tooltip

        public void ShowTooltip (InventoryItem item, Vector2 screenPosition)
        {
            if (tooltipPanel == null || item == null || item.Data == null)
                return;
            tooltipImage.sprite = item.Data.Icon;
            var color = tooltipImage.color;
            color.a = item.Data.Icon != null ? Color.white.a : 0f;
            tooltipImage.color = color;
            tooltipNameText.text = Localization.LocalizationManager.Instance != null
                ? Localization.LocalizationManager.Instance.GetLocalizedValue(item.Data.NameKey)
                : item.Data.NameKey;

            tooltipDescriptionText.text = Localization.LocalizationManager.Instance != null
                ? Localization.LocalizationManager.Instance.GetLocalizedValue(item.Data.DescriptionKey)
                : item.Data.DescriptionKey;

            tooltipPanel.SetActive(true);

            //RectTransform rt = tooltipPanel.GetComponent<RectTransform>();
            //Canvas rootCanvas = tooltipPanel.GetComponentInParent<Canvas>();
            //RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();

            //Vector2 localPos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //    canvasRect,
            //    screenPosition,
            //    rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            //    out localPos
            //);

            //rt.anchoredPosition = localPos + tooltipOffset;
        }


        public void HideTooltip()
        {
            //if (tooltipPanel != null)
            //    tooltipPanel.SetActive(false);
        }

        #endregion
    }
}