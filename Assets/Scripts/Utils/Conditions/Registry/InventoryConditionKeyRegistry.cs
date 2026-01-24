using UnityEngine;

[CreateAssetMenu(menuName = "Conditions/Inventory Key Registry")]
public class InventoryConditionKeyRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public InventoryConditionKey key;
        public string displayName;
        public string description;
    }

    public Entry[] entries;

    public string GetDisplayName(InventoryConditionKey key)
    {
        foreach (var e in entries)
            if (e.key == key)
                return string.IsNullOrEmpty(e.displayName) ? key.ToString() : e.displayName;

        return key.ToString();
    }

    public string GetDescription(InventoryConditionKey key)
    {
        foreach (var e in entries)
            if (e.key == key)
                return e.description;

        return "";
    }
}
