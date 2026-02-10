#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenericConditionSO))]
public class GenericConditionSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var keyTypeProp = serializedObject.FindProperty("keyType");
        EditorGUILayout.PropertyField(keyTypeProp);

        var keyType = (ConditionKeyType)keyTypeProp.enumValueIndex;

        SerializedProperty limbKeyProp = serializedObject.FindProperty("limbKey");
        SerializedProperty inventoryKeyProp = serializedObject.FindProperty("inventoryKey");

        // Mostrar clave según categoría
        switch (keyType)
        {
            case ConditionKeyType.Limb:
                EditorGUILayout.PropertyField(limbKeyProp);
                break;

            case ConditionKeyType.Inventory:
                EditorGUILayout.PropertyField(inventoryKeyProp);
                break;
        }

        EditorGUILayout.Space();

        // Detectar tipo de dato según la clave seleccionada
        System.Type valueType = GetValueType(keyType, limbKeyProp, inventoryKeyProp);

        // Detectar si requiere parámetro (ItemData)
        bool requiresItemData = RequiresItemData(keyType, inventoryKeyProp);

        // Mostrar parámetro si hace falta
        if (requiresItemData)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDataValue"));
        }

        // Mostrar campos según tipo real
        if (valueType == typeof(bool))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("boolValue"));
        }
        else if (valueType == typeof(int))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comparison"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intParamA"));

            var comparisonProp = serializedObject.FindProperty("comparison");
            var comparison = (ComparisonType)comparisonProp.enumValueIndex;

            if (comparison == ComparisonType.Between)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("intParamB"));
            }
        }
        else if (valueType == typeof(string))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stringValue"));
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("conditionFailedText"));

        serializedObject.ApplyModifiedProperties();
    }

    // --- Helpers ---

    private System.Type GetValueType(ConditionKeyType keyType, SerializedProperty limbKeyProp, SerializedProperty inventoryKeyProp)
    {
        switch (keyType)
        {
            case ConditionKeyType.Limb:
                var limbKey = (LimbConditionKey)limbKeyProp.enumValueIndex;
                return typeof(bool); // todas las limb conditions devuelven bool

            case ConditionKeyType.Inventory:
                var invKey = (InventoryConditionKey)inventoryKeyProp.enumValueIndex;
                switch (invKey)
                {
                    case InventoryConditionKey.HasItem:
                        return typeof(bool);

                    case InventoryConditionKey.ItemCount:
                        return typeof(int);
                }
                break;
        }

        return typeof(bool);
    }

    private bool RequiresItemData(ConditionKeyType keyType, SerializedProperty inventoryKeyProp)
    {
        if (keyType != ConditionKeyType.Inventory)
            return false;

        var invKey = (InventoryConditionKey)inventoryKeyProp.enumValueIndex;

        return invKey == InventoryConditionKey.HasItem || invKey == InventoryConditionKey.ItemCount;
    }
}
#endif
