using UnityEngine;
using UnityEditor;
using Localization;
using TMPro;

[CustomPropertyDrawer(typeof(LocalizedText))]
public class LocalizedTextDrawer : PropertyDrawer
{
    private static LocalizationDatabase _db;
    private bool creatingNewKey = false;
    private string newKeyName = "";
    private static Localization.Language _editorLanguage = Localization.Language.Spanish;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_db == null)
            _db = Resources.Load<LocalizationDatabase>("LocalizationDatabase");

        if (_db == null)
        {
            EditorGUI.HelpBox(position, "LocalizationDatabase not found in Resources!", MessageType.Error);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty keyProp = property.FindPropertyRelative("_key");
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float y = position.y;

        // Dropdown de keys
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

        // Crear nueva key
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

        // Mostrar preview del texto
        var entryObj = _db.GetEntry(keyProp.stringValue);
        if (entryObj != null)
        {
            string currentText = entryObj.Get(_editorLanguage);
            EditorGUI.LabelField(new Rect(position.x, y, position.width, lineHeight * 2), $"Preview ({_editorLanguage}): {currentText}");
            y += lineHeight * 2 + 4;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 5;
    }
}
