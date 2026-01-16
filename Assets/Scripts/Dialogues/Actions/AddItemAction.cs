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
            //TODO:: Cada objeto debería tener una cantidad máxima.
            // Actualmente esto solo sirve para evitar duplicados en inventarios de objetos únicos.
            // Pero si un NPC nos da 5 de un objeto que ya tenemos, no recibiremos ninguno.
            var itemAdded = inventory.GetItemCount(item) < 1 ? inventory.AddItem(item, amount): false;
            Debug.Log($"[AddItemAction] Item '{item.name}' added: {itemAdded}");
        }
    }
}