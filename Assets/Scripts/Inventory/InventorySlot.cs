public class InventorySlot
{
    public Item Item { get; private set; }
    public int Amount { get; private set; }

    public InventorySlot(Item item, int amount)
    {
        Item = item;
        Amount = amount;
    }

    public bool CanStack(Item item) => Item == item && Amount < Item.maxStack;

    public int Add(int amount)
    {
        int space = Item.maxStack - Amount;
        int toAdd = System.Math.Min(space, amount);
        Amount += toAdd;
        return amount - toAdd;
    }

    public int Remove(int amount)
    {
        int toRemove = System.Math.Min(Amount, amount);
        Amount -= toRemove;
        return amount - toRemove;
    }

    public bool IsEmpty() => Amount <= 0;
}

