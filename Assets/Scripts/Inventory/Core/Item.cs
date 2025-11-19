namespace Inventory.Core
{
    public class Item
    {
        public string Id { get; }
        public string Name { get; }
        public Item(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

