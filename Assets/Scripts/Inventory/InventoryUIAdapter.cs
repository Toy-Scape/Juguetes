using UnityEngine;

public class InventoryUIAdapter : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventoryUI ui; // Asume que tienes un script UI

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (ui != null)
            ui.Refresh(inventory.GetSlots());
    }
}

