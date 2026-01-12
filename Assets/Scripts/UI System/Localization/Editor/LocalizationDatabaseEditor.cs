using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Localization
{
    [CustomEditor(typeof(LocalizationDatabase))]
    public class LocalizationDatabaseEditor : Editor
    {
        private SerializedProperty _entries;
        private string _searchText = "";
        private Vector2 _scrollPos;

        private void OnEnable()
        {
            _entries = serializedObject.FindProperty("_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header Style
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Localization Database", headerStyle);

            EditorGUILayout.Space();

            // Search Bar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Search:", GUILayout.Width(50));
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // If no search, show default list (reorderable, fully functional)
            if (string.IsNullOrEmpty(_searchText))
            {
                EditorGUILayout.PropertyField(_entries, true);
            }
            else
            {
                // Filtered View
                GUILayout.Label("Search Results:", EditorStyles.boldLabel);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                bool found = false;
                for (int i = 0; i < _entries.arraySize; i++)
                {
                    SerializedProperty entry = _entries.GetArrayElementAtIndex(i);
                    SerializedProperty key = entry.FindPropertyRelative("key");
                    SerializedProperty english = entry.FindPropertyRelative("english");
                    SerializedProperty spanish = entry.FindPropertyRelative("spanish");
                    SerializedProperty basque = entry.FindPropertyRelative("basque");

                    string searchLow = _searchText.ToLower();

                    if (key.stringValue.ToLower().Contains(searchLow) ||
                        english.stringValue.ToLower().Contains(searchLow) ||
                        spanish.stringValue.ToLower().Contains(searchLow) ||
                        basque.stringValue.ToLower().Contains(searchLow))
                    {
                        found = true;
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.PropertyField(entry, new GUIContent(key.stringValue), true);
                        EditorGUILayout.EndVertical();
                    }
                }

                if (!found)
                {
                    GUILayout.Label("No matches found.");
                }

                EditorGUILayout.EndScrollView();
            }

            // Button to add new entry easily at the bottom if searching
            if (!string.IsNullOrEmpty(_searchText))
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Clear Search to Add/Reorder Entries"))
                {
                    _searchText = "";
                    GUI.FocusControl(null);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
