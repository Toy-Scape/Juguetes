using Inventory;
using System;
using UnityEngine;

public enum ComparisonType
{
    EqualTo,
    GreaterThan,
    GreaterOrEqualThan,
    LessThan,
    LessOrEqualThan,
    Between
}

public enum ConditionKeyType
{
    Limb,
    Inventory,
    Player,
}

[CreateAssetMenu(menuName = "Conditions/Generic Comparison")]
public class GenericConditionSO : ConditionSO
{
    [Header("Key Selection")]
    public ConditionKeyType keyType;

    public LimbConditionKey limbKey;
    public InventoryConditionKey inventoryKey;

    [Header("Condition Setup")]
    public ComparisonType comparison;

    public int intParamA;
    public int intParamB;

    public bool boolValue;
    public string stringValue;
    public ItemData itemDataValue;

    //[SerializeField] private Dialogue conditionFailedText;
    

    public override bool Evaluate(IValueProvider provider)
    {
        string keyName = GetKeyName();
        if (keyName == null)
            return false;

        object param = GetParam();

        // 1) BOOL
        if (provider.TryGetBool(keyName, param, out bool b))
            return b == boolValue;

        // 2) INT
        if (provider.TryGetInt(keyName, param, out int i))
            return Compare(i, intParamA, intParamB);

        // 3) STRING
        if (provider.TryGetString(keyName, param, out string s))
            return s == stringValue;

        // 4) FLOAT (si algún día lo usas)
        if (provider.TryGetFloat(keyName, param, out float f))
            return Compare(f, intParamA, intParamB);

        return false;
    }

    private string GetKeyName()
    {
        return keyType switch
        {
            ConditionKeyType.Limb => limbKey.ToString(),
            ConditionKeyType.Inventory => inventoryKey.ToString(),
            _ => null
        };
    }

    private object GetParam()
    {
        // Solo Inventory necesita parámetros (ItemData)
        if (keyType == ConditionKeyType.Inventory)
            return itemDataValue;

        return null;
    }

    private bool Compare<T>(T current, T a, T b) where T : IComparable<T>
    {
        return comparison switch
        {
            ComparisonType.EqualTo => current.CompareTo(a) == 0,
            ComparisonType.GreaterThan => current.CompareTo(a) > 0,
            ComparisonType.GreaterOrEqualThan => current.CompareTo(a) >= 0,
            ComparisonType.LessThan => current.CompareTo(a) < 0,
            ComparisonType.LessOrEqualThan => current.CompareTo(a) <= 0,
            ComparisonType.Between => current.CompareTo(a) >= 0 && current.CompareTo(b) <= 0,
            _ => false,
        };
    }
}
