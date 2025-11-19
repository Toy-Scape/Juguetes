using UnityEngine;
using UnityEngine.UI;
using Inventory.Core;

public class InventoryUI : MonoBehaviour
{
    public InventoryHolder playerInventoryHolder;
    public Transform itemsParent;
    public GameObject itemPrefab;

    void Start()
    {
        playerInventoryHolder.Inventory.OnItemAdded += OnItemAdded;
        playerInventoryHolder.Inventory.OnItemRemoved += OnItemRemoved;
        RefreshUI();
    }

    void OnDestroy()
    {
        playerInventoryHolder.Inventory.OnItemAdded -= OnItemAdded;
        playerInventoryHolder.Inventory.OnItemRemoved -= OnItemRemoved;
    }

    private void OnItemAdded(Item item)
    {
        RefreshUI();
    }

    private void OnItemRemoved(Item item)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in itemsParent)
            Destroy(child.gameObject);

        foreach (var item in playerInventoryHolder.Inventory.GetAllItems())
        {
            var go = Instantiate(itemPrefab, itemsParent);
            var text = go.GetComponentInChildren<Text>();
            if (text != null)
                text.text = item.Name;
        }
    }
}

