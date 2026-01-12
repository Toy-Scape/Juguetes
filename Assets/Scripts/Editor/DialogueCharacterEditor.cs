using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueCharacter))]
public class DialogueCharacterEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        var character = (DialogueCharacter)target;

        var database = Resources.Load<CharacterDatabase>("CharacterDatabase");
        if (database == null)
        {
            EditorGUILayout.HelpBox("CharacterDatabase not found in Resources folder.", MessageType.Warning);
            DrawDefaultInspector();
            return;
        }

        int index = Mathf.Max(0, database.CharacterIds.IndexOf(character.CharacterId));
        index = EditorGUILayout.Popup("Character ID", index, database.CharacterIds.ToArray());
        character.CharacterId = database.CharacterIds[index];

        DrawDefaultInspector();
    }
}
