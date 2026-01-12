using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterManager))]
public class CharacterManagerEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        var manager = (CharacterManager)target;

        // Cargar la base de datos
        var database = Resources.Load<CharacterDatabase>("CharacterDatabase");
        if (database == null)
        {
            EditorGUILayout.HelpBox("CharacterDatabase not found in Resources folder.", MessageType.Warning);
            DrawDefaultInspector();
            return;
        }

        // Dibujar lista de personajes
        serializedObject.Update();

        var charactersProp = serializedObject.FindProperty("characters");

        for (int i = 0; i < charactersProp.arraySize; i++)
        {
            var entry = charactersProp.GetArrayElementAtIndex(i);
            var idProp = entry.FindPropertyRelative("id");
            var modelProp = entry.FindPropertyRelative("model");

            EditorGUILayout.BeginVertical("box");

            // Popup de IDs
            int index = Mathf.Max(0, database.CharacterIds.IndexOf(idProp.stringValue));
            index = EditorGUILayout.Popup("Character ID", index, database.CharacterIds.ToArray());
            idProp.stringValue = database.CharacterIds[index];

            // Modelo
            EditorGUILayout.PropertyField(modelProp);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Character"))
        {
            charactersProp.InsertArrayElementAtIndex(charactersProp.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
