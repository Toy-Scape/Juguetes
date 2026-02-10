using Localization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedButton))]
public class LocalizedButtonEditor : Editor
{
    private LocalizationDatabase _db;
    private bool creatingNewKey = false;
    private string newKeyName = "";
    private Language _editorLanguage = Language.Spanish;
    private string keyFilter = "";

    public override void OnInspectorGUI()
    {
        if (_db == null)
            _db = Resources.Load<LocalizationDatabase>("LocalizationDatabase");

        if (_db == null)
        {
            EditorGUILayout.HelpBox("LocalizationDatabase not found in Resources!", MessageType.Error);
            return;
        }

        serializedObject.Update();

        SerializedProperty labelKeyProp = serializedObject.FindProperty("labelKey");
        SerializedProperty labelProp = serializedObject.FindProperty("label");

        EditorGUILayout.PropertyField(labelProp);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Label Key", EditorStyles.boldLabel);

        keyFilter = DrawKeySelector(labelKeyProp, keyFilter, true);

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Label Text Preview / Edit", EditorStyles.boldLabel);
        DrawLocalizationEditor(labelKeyProp.stringValue);

        serializedObject.ApplyModifiedProperties();

        if (target is LocalizedButton button)
            button.Localize();
    }

    private string DrawKeySelector(SerializedProperty keyProp, string filter, bool allowCreate = false)
    {
        filter = EditorGUILayout.TextField("Search Key", filter);

        var allKeys = _db.GetAllKeys();

        // Asegurarse de que la key actual esté siempre en la lista
        List<string> filteredKeys = new List<string>();
        foreach (var k in allKeys)
            if (string.IsNullOrEmpty(filter) || k.ToLower().Contains(filter.ToLower()))
                filteredKeys.Add(k);

        if (!string.IsNullOrEmpty(keyProp.stringValue) && !filteredKeys.Contains(keyProp.stringValue))
            filteredKeys.Insert(0, keyProp.stringValue);

        int index = Mathf.Max(0, filteredKeys.IndexOf(keyProp.stringValue));

        EditorGUILayout.BeginHorizontal();
        int newIndex = EditorGUILayout.Popup("Key", index, filteredKeys.ToArray());

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

        if (newIndex >= 0 && newIndex < filteredKeys.Count)
            keyProp.stringValue = filteredKeys[newIndex];

        return filter;
    }

    private void DrawLocalizationEditor(string key)
    {
        // Crear entry si no existe
        var entry = _db.GetEntry(key);
        if (entry == null)
        {
            entry = new LocalizationDatabase.LocalizationEntry
            {
                key = key,
                english = "",
                spanish = "",
                basque = ""
            };
            _db.AddEntry(entry);
            EditorUtility.SetDirty(_db);
        }

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
}
