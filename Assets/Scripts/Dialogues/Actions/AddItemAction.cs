using Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "AddItemAction", menuName = "Dialogue System/Actions/Add Item")]
public class AddItemAction : ActionBase
{
    [SerializeField] private ItemData item;
    [SerializeField] private int amount = 1;

    public override void Execute (DialogueContext context)
    {
        var inventory = context.Player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddItem(item, amount);
        }
    }
}