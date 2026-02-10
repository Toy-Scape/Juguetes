using UnityEditor;
using UnityEngine;
using Localization;

[CustomEditor(typeof(LocalizedText))]
public class LocalizedTextEditor : Editor
{
    private LocalizationDatabase _db;
    private bool creatingNewKey;
    private string newKeyName = "";
    private string keyFilter = "";
    private Language _editorLanguage = Language.Spanish;

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
        SerializedProperty keyProp = serializedObject.FindProperty("_key");

        keyFilter = EditorGUILayout.TextField("Search Key", keyFilter);

        var allKeys = _db.GetAllKeys();
        var filteredKeys = string.IsNullOrEmpty(keyFilter)
            ? allKeys
            : System.Array.FindAll(allKeys, k =>
                k.ToLower().Contains(keyFilter.ToLower()));

        int index = Mathf.Max(0, System.Array.IndexOf(filteredKeys, keyProp.stringValue));

        EditorGUILayout.BeginHorizontal();
        int newIndex = EditorGUILayout.Popup("Key", index, filteredKeys);

        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            creatingNewKey = true;
            newKeyName = "";
        }
        EditorGUILayout.EndHorizontal();

        if (newIndex >= 0 && newIndex < filteredKeys.Length)
            keyProp.stringValue = filteredKeys[newIndex];

        if (creatingNewKey)
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
                    keyFilter = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        var entryObj = _db.GetEntry(keyProp.stringValue);
        if (entryObj != null)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Localized Text", EditorStyles.boldLabel);

            _editorLanguage = (Language)EditorGUILayout.EnumPopup("Language", _editorLanguage);

            string currentText = entryObj.Get(_editorLanguage);

            EditorGUI.BeginChangeCheck();
            string newText = EditorGUILayout.TextArea(currentText, GUILayout.MinHeight(60));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_db, "Edit Localization Text");
                entryObj.Set(_editorLanguage, newText);
                EditorUtility.SetDirty(_db);
            }

            bool missingAny =
                string.IsNullOrEmpty(entryObj.english) ||
                string.IsNullOrEmpty(entryObj.spanish) ||
                string.IsNullOrEmpty(entryObj.basque);

            if (missingAny)
            {
                EditorGUILayout.HelpBox("This key has missing translations", MessageType.Warning);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
