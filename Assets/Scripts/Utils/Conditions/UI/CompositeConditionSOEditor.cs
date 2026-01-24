#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CompositeConditionSO))]
public class CompositeConditionSOEditor : Editor
{
    private SerializedProperty compositeTypeProp;
    private SerializedProperty conditionsProp;

    private void OnEnable()
    {
        compositeTypeProp = serializedObject.FindProperty("compositeType");
        conditionsProp = serializedObject.FindProperty("conditions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Composite Condition", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(compositeTypeProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

        // Botón añadir
        if (GUILayout.Button("Add Condition"))
        {
            conditionsProp.arraySize++;
        }

        EditorGUILayout.Space();

        // Dibujar cada condición con su inspector
        for (int i = 0; i < conditionsProp.arraySize; i++)
        {
            SerializedProperty element = conditionsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Condition {i + 1}", EditorStyles.boldLabel);

            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                conditionsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(element);

            // Dibujar inspector interno si hay asset
            if (element.objectReferenceValue != null)
            {
                Editor editor = CreateEditor(element.objectReferenceValue);
                editor.OnInspectorGUI();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
