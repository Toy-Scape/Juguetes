using UnityEngine;

[CreateAssetMenu(menuName = "Conditions/Limb Key Registry")]
public class LimbConditionKeyRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public LimbConditionKey key;
        public string displayName;
        public string description;
    }

    public Entry[] entries;

    public string GetDisplayName(LimbConditionKey key)
    {
        foreach (var e in entries)
            if (e.key == key)
                return string.IsNullOrEmpty(e.displayName) ? key.ToString() : e.displayName;

        return key.ToString();
    }

    public string GetDescription(LimbConditionKey key)
    {
        foreach (var e in entries)
            if (e.key == key)
                return e.description;

        return "";
    }
}
