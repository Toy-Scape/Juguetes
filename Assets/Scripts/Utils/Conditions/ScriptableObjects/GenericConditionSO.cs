using System;
using UnityEngine;

public enum VariableType
{
    Float,
    Int,
    Bool,
    String
}

public enum ComparisonType
{
    EqualTo,
    GreaterThan,
    GreaterOrEqualThan,
    LessThan,
    LessOrEqualThan,
    Between
}

[CreateAssetMenu(menuName = "Conditions/Generic Comparison")]
public class GenericConditionSO : GenericCondition
{
    [SerializeField] private VariableType variableType;

    [SerializeField] private FloatVariableSO floatVar;
    [SerializeField] private IntVariableSO intVar;
    [SerializeField] private BoolVariableSO boolVar;
    [SerializeField] private StringVariableSO stringVar;

    [SerializeField] private ComparisonType comparison;
    [SerializeField] private string valueA; 
    [SerializeField] private string valueB;

    [SerializeField] private bool boolValue;
    [SerializeField] private string stringValue;

    public override bool ConditionIsMet()
    {
        switch (variableType)
        {
            case VariableType.Float:
                if (floatVar == null) return false;
                return Compare(floatVar.Value, float.Parse(valueA), float.Parse(valueB));

            case VariableType.Int:
                if (intVar == null) return false;
                return Compare(intVar.Value, int.Parse(valueA), int.Parse(valueB));

            case VariableType.Bool:
                if (boolVar == null) return false;
                return boolVar.Value == boolValue;

            case VariableType.String:
                if (stringVar == null) return false;
                return stringVar.Value == stringValue;

            default:
                return false;
        }
    }

    private bool Compare<T>(T current, T a, T b) where T : IComparable<T>
    {
        switch (comparison)
        {
            case ComparisonType.EqualTo:
                return current.CompareTo(a) == 0;

            case ComparisonType.GreaterThan:
                return current.CompareTo(a) > 0;

            case ComparisonType.GreaterOrEqualThan:
                return current.CompareTo(a) >= 0;

            case ComparisonType.LessThan:
                return current.CompareTo(a) < 0;

            case ComparisonType.LessOrEqualThan:
                return current.CompareTo(a) <= 0;

            case ComparisonType.Between:
                return current.CompareTo(a) >= 0 && current.CompareTo(b) <= 0;

            default:
                return false;
        }
    }
}
