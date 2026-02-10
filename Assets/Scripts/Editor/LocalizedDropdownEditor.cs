using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Localization;
using System.Collections.Generic;

[CustomEditor(typeof(LocalizedDropdown))]
public class LocalizedDropdownEditor : Editor
{
    private LocalizationDatabase _db;
    private string labelKeyFilter = "";
    private bool creatingNewKey = false;
    private string newKeyName = "";
    private Language _editorLanguage = Language.Spanish;

    private ReorderableList optionsList;

    private void OnEnable()
    {
        if (_db == null)
            _db = Resources.Load<LocalizationDatabase>("LocalizationDatabase");

        if (_db == null) return;

        SerializedProperty optionKeysProp = serializedObject.FindProperty("optionKeys");

        optionsList = new ReorderableList(serializedObject, optionKeysProp, true, true, true, true);

        optionsList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Options");
        };

        optionsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty keyProp = optionKeysProp.GetArrayElementAtIndex(index);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            rect.y += 2;

            keyProp.stringValue = DrawKeySelectorInline(keyProp, keyProp.stringValue, rect, lineHeight);

            float offsetY = lineHeight + 4;

            if (creatingNewKey)
            {
                Rect createKeyRect = new Rect(rect.x, rect.y + offsetY, rect.width - 60, lineHeight);
                Rect createButtonRect = new Rect(rect.x + rect.width - 55, rect.y + offsetY, 55, lineHeight);

                newKeyName = EditorGUI.TextField(createKeyRect, newKeyName);
                if (GUI.Button(createButtonRect, "Create"))
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

                offsetY += lineHeight + 4;
            }

            Rect textRect = new Rect(rect.x, rect.y + offsetY, rect.width, lineHeight * 4);
            DrawLocalizationEditorInline(keyProp.stringValue, textRect);
        };

        optionsList.elementHeightCallback = index =>
        {
            return creatingNewKey
                ? EditorGUIUtility.singleLineHeight * 9
                : EditorGUIUtility.singleLineHeight * 6;
        };
    }

    public override void OnInspectorGUI()
    {
        if (_db == null)
        {
            EditorGUILayout.HelpBox("LocalizationDatabase not found in Resources!", MessageType.Error);
            return;
        }

        serializedObject.Update();

        SerializedProperty labelKeyProp = serializedObject.FindProperty("labelKey");
        SerializedProperty labelProp = serializedObject.FindProperty("label");
        SerializedProperty dropdownProp = serializedObject.FindProperty("dropdown");

        EditorGUILayout.PropertyField(labelProp);
        EditorGUILayout.PropertyField(dropdownProp);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Label Key", EditorStyles.boldLabel);

        labelKeyFilter = DrawKeySelector(labelKeyProp, labelKeyFilter, true);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Label Text Preview / Edit", EditorStyles.boldLabel);
        DrawLocalizationEditor(labelKeyProp.stringValue);

        EditorGUILayout.Space(10);

        if (optionsList != null)
            optionsList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();

        if (target is LocalizedDropdown dd)
            dd.Localize();
    }

    private string DrawKeySelector(SerializedProperty keyProp, string filter, bool allowCreate = false)
    {
        filter = EditorGUILayout.TextField("Search Key", filter);

        var allKeys = _db.GetAllKeys();
        var filteredKeys = string.IsNullOrEmpty(filter)
            ? allKeys
            : System.Array.FindAll(allKeys, k => k.ToLower().Contains(filter.ToLower()));

        int index = Mathf.Max(0, System.Array.IndexOf(filteredKeys, keyProp.stringValue));

        EditorGUILayout.BeginHorizontal();
        int newIndex = EditorGUILayout.Popup("Key", index, filteredKeys);

        if (allowCreate)
        {
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                creatingNewKey = true;
                newKeyName = "";
            }
        }
        EditorGUILayout.EndHorizontal();

        if (creatingNewKey && allowCreate)
        {
            EditorGUILayout.BeginHorizontal();
            newKeyName = EditorGUILayout.TextField(newKeyName);

            if (GUILayout.Button("Create", GUILayout.Width(60)))
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
                    filter = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (newIndex >= 0 && newIndex < filteredKeys.Length)
            keyProp.stringValue = filteredKeys[newIndex];

        return filter;
    }

    private string DrawKeySelectorInline(SerializedProperty keyProp, string key, Rect rect, float lineHeight)
    {
        var allKeys = _db.GetAllKeys();
        int index = Mathf.Max(0, System.Array.IndexOf(allKeys, key));

        Rect popupRect = new Rect(rect.x, rect.y, rect.width - 30, lineHeight);
        Rect buttonRect = new Rect(rect.x + rect.width - 25, rect.y, 25, lineHeight);

        int newIndex = EditorGUI.Popup(popupRect, index, allKeys);

        if (GUI.Button(buttonRect, "+"))
        {
            creatingNewKey = true;
            newKeyName = "";
        }

        if (newIndex >= 0 && newIndex < allKeys.Length)
            keyProp.stringValue = allKeys[newIndex];

        return keyProp.stringValue;
    }

    private void DrawLocalizationEditor(string key)
    {
        var entry = _db.GetEntry(key);
        if (entry == null) return;

        _editorLanguage = (Language)EditorGUILayout.EnumPopup("Language", _editorLanguage);

        string currentText = entry.Get(_editorLanguage);

        EditorGUI.BeginChangeCheck();
        string newText = EditorGUILayout.TextArea(currentText, GUILayout.MinHeight(40));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_db, "Edit Localization Text");
            entry.Set(_editorLanguage, newText);
            EditorUtility.SetDirty(_db);
        }

        bool missingAny =
            string.IsNullOrEmpty(entry.english) ||
            string.IsNullOrEmpty(entry.spanish) ||
            string.IsNullOrEmpty(entry.basque);

        if (missingAny)
        {
            EditorGUILayout.HelpBox("This key has missing translations", MessageType.Warning);
        }
    }

    private void DrawLocalizationEditorInline(string key, Rect rect)
    {
        var entry = _db.GetEntry(key);
        if (entry == null) return;

        _editorLanguage = (Language)EditorGUI.EnumPopup(
            new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
            "Language",
            _editorLanguage);

        string currentText = entry.Get(_editorLanguage);

        EditorGUI.BeginChangeCheck();
        string newText = EditorGUI.TextArea(
            new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, rect.height - EditorGUIUtility.singleLineHeight - 2),
            currentText);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_db, "Edit Localization Text");
            entry.Set(_editorLanguage, newText);
            EditorUtility.SetDirty(_db);
        }

        bool missingAny =
            string.IsNullOrEmpty(entry.english) ||
            string.IsNullOrEmpty(entry.spanish) ||
            string.IsNullOrEmpty(entry.basque);

        if (missingAny)
        {
            EditorGUI.HelpBox(
                new Rect(rect.x, rect.y + rect.height - 20, rect.width, 20),
                "This key has missing translations",
                MessageType.Warning);
        }
    }
}
