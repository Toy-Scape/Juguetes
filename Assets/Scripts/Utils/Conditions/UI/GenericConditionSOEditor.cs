#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(GenericConditionSO))]
    public class GenericConditionSOEditor : Editor
    {
        public override void OnInspectorGUI ()
        {
            serializedObject.Update();

            // Dibujar todos los campos hasta isLimb
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            while (iterator.NextVisible(false))
            {
                if (iterator.name == "floatVar")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.Float)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "intVar")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.Int)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "comparison")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.Int || (VariableType)variableTypeProp.enumValueIndex == VariableType.Float)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "boolVar")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.Bool)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "boolVar" || iterator.name == "boolValue")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.Bool)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "valueA")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.Int || (VariableType)variableTypeProp.enumValueIndex == VariableType.Float)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "valueB")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    SerializedProperty comparisonTypeProp = serializedObject.FindProperty("comparison");
                    if (
                        ((VariableType)variableTypeProp.enumValueIndex == VariableType.Int || (VariableType)variableTypeProp.enumValueIndex == VariableType.Float) 
                        && (ComparisonType) comparisonTypeProp.enumValueIndex == ComparisonType.Between)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else if (iterator.name == "stringValue" || iterator.name == "stringVar")
                {
                    SerializedProperty variableTypeProp = serializedObject.FindProperty("variableType");
                    if ((VariableType)variableTypeProp.enumValueIndex == VariableType.String)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

