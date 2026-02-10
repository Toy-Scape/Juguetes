using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using CinematicSystem.Core;
using System.Reflection;

namespace CinematicSystem.Editor
{
    [CustomEditor(typeof(CinematicAsset))]
    public class CinematicAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty scriptProp;
        private SerializedProperty actionsProp;

        private void OnEnable()
        {
            scriptProp = serializedObject.FindProperty("m_Script");
            actionsProp = serializedObject.FindProperty("actions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw Script field disabled
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProp);
            }

            // Draw other properties except 'actions'
            DrawPropertiesExcluding(serializedObject, "m_Script", "actions");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cinematic Sequence", EditorStyles.boldLabel);

            // Draw List
            for (int i = 0; i < actionsProp.arraySize; i++)
            {
                SerializedProperty element = actionsProp.GetArrayElementAtIndex(i);
                
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                // Foldout
                string label = "Action " + i;
                // Try to get type name
                object managedRef = GetManagedReferenceValue(element);
                if (managedRef != null)
                {
                    label += $" ({managedRef.GetType().Name})";
                }
                else
                {
                    label += " (Null)";
                }

                element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, label, true);
                
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    actionsProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    return; // Return to avoid iterator issues
                }
                EditorGUILayout.EndHorizontal();

                if (element.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    // Draw the element with children
                    // Use PropertyField with includeChildren: true
                    EditorGUILayout.PropertyField(element, GUIContent.none, true);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add New Action"))
            {
                ShowAddActionMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowAddActionMenu()
        {
            GenericMenu menu = new GenericMenu();

            // Find all types inheriting from CinematicAction
            var actionTypes = TypeCache.GetTypesDerivedFrom<CinematicAction>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name);

            foreach (var type in actionTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    AddAction(type);
                });
            }

            menu.ShowAsContext();
        }

        private void AddAction(Type type)
        {
            object instance = Activator.CreateInstance(type);
            
            actionsProp.arraySize++;
            SerializedProperty newElement = actionsProp.GetArrayElementAtIndex(actionsProp.arraySize - 1);
            newElement.managedReferenceValue = instance;
            newElement.isExpanded = true;
            
            serializedObject.ApplyModifiedProperties();
        }

        // Helper to get raw object from serialized property (for label)
        private object GetManagedReferenceValue(SerializedProperty property)
        {
            return property.managedReferenceValue;
        }
    }
}
