using UnityEditor;
using UnityEngine;
using Localization;

[CustomPropertyDrawer(typeof(Dialogue.Line))]
public class LineDrawer : PropertyDrawer
{
    private static LocalizationDatabase _db;
    private static Language _editorLanguage = Language.Spanish;

    private bool creatingNewKey = false;
    private string newKeyName = "";

    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        if (_db == null)
            _db = Resources.Load<LocalizationDatabase>("LocalizationDatabase");

        if (_db == null)
        {
            EditorGUI.HelpBox(position, "LocalizationDatabase not found in Resources!", MessageType.Error);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        var keyProp = property.FindPropertyRelative("Key");
        var characterProp = property.FindPropertyRelative("Character");
        var actionsProp = property.FindPropertyRelative("Actions");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = position.y;

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), characterProp);
        y += lineHeight + 4;

        _editorLanguage = (Language)EditorGUI.EnumPopup(
            new Rect(position.x, y, position.width, lineHeight),
            "Editor Language",
            _editorLanguage
        );
        y += lineHeight + 4;

        var keys = _db.GetAllKeys();
        int index = Mathf.Max(0, System.Array.IndexOf(keys, keyProp.stringValue));

        Rect popupRect = new Rect(position.x, y, position.width - 30, lineHeight);
        Rect addButtonRect = new Rect(position.x + position.width - 25, y, 25, lineHeight);

        int newIndex = EditorGUI.Popup(popupRect, "Key", index, keys);

        if (GUI.Button(addButtonRect, "+"))
        {
            creatingNewKey = true;
            newKeyName = "";
        }

        if (newIndex != index)
            keyProp.stringValue = keys[newIndex];

        y += lineHeight + 4;

        if (creatingNewKey)
        {
            Rect textRect = new Rect(position.x, y, position.width - 60, lineHeight);
            Rect createRect = new Rect(position.x + position.width - 55, y, 55, lineHeight);

            newKeyName = EditorGUI.TextField(textRect, newKeyName);

            if (GUI.Button(createRect, "Create"))
            {
                if (!string.IsNullOrEmpty(newKeyName))
                {
                    Undo.RecordObject(_db, "Add Localization Key");

                    var entry = new LocalizationDatabase.LocalizationEntry
                    {
                        key = newKeyName,
                        english = "",
                        spanish = "",
                        basque = ""
                    };

                    _db.AddEntry(entry);
                    EditorUtility.SetDirty(_db);

                    keyProp.stringValue = newKeyName;
                    creatingNewKey = false;
                }
            }

            y += lineHeight + 4;
        }

        var entryObj = _db.GetEntry(keyProp.stringValue);

        if (entryObj != null)
        {
            string currentText = entryObj.Get(_editorLanguage);
            string newText = EditorGUI.TextArea(
                new Rect(position.x, y, position.width, lineHeight * 3),
                currentText
            );

            if (newText != currentText)
            {
                Undo.RecordObject(_db, "Edit Localization Entry");
                entryObj.Set(_editorLanguage, newText);
                EditorUtility.SetDirty(_db);
            }

            y += lineHeight * 3 + 4;
        }
        else
        {
            EditorGUI.HelpBox(new Rect(position.x, y, position.width, lineHeight * 2),
                "Key not found in Localization Database", MessageType.Warning);
            y += lineHeight * 2 + 4;
        }

        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), actionsProp, true);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 12;
    }
}
