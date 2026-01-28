using System;
using System.Collections.Generic;
using System.Linq;
using CinematicSystem.Core;
using UnityEditor;
using UnityEngine;

namespace CinematicSystem.Editor
{
    [CustomPropertyDrawer(typeof(CinematicAction))]
    public class CinematicActionDrawer : PropertyDrawer
    {
        private static List<Type> actionTypes;
        private static string[] typeNames;

        static CinematicActionDrawer()
        {
            // Cache types
            var baseType = typeof(CinematicAction);
            actionTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name)
                .ToList();

            typeNames = actionTypes.Select(t => ObjectNames.NicifyVariableName(t.Name)).ToArray();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Height for the Type Selector Popup
            float h = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // If not null, add height of the children
            if (property.managedReferenceValue != null)
            {
                // We assume expanded to show fields
                if (property.isExpanded)
                {
                    h += EditorGUI.GetPropertyHeight(property, true);
                }
            }
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw Type Selector
            Rect dropDownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            int currentIndex = -1;
            object managedRef = property.managedReferenceValue;
            if (managedRef != null)
            {
                Type currentType = managedRef.GetType();
                currentIndex = actionTypes.IndexOf(currentType);
            }

            EditorGUI.BeginChangeCheck();

            // "Select Action Type" if null, otherwise show current type name
            string title = currentIndex == -1 ? "Select Action Type..." : typeNames[currentIndex];
            int newIndex = EditorGUI.Popup(dropDownRect, "Action Type", currentIndex, typeNames);

            if (EditorGUI.EndChangeCheck())
            {
                if (newIndex >= 0 && newIndex < actionTypes.Count)
                {
                    Type newType = actionTypes[newIndex];
                    property.managedReferenceValue = Activator.CreateInstance(newType);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // Draw Properties if strictly not null
            if (property.managedReferenceValue != null)
            {
                // Shift down
                Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);

                // Draw the object properties
                // property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);

                // Better UX: Just draw it recursively. 
                // We use includeChildren: true to draw all fields of the concrete class.
                EditorGUI.PropertyField(contentRect, property, new GUIContent("Properties"), true);
            }

            EditorGUI.EndProperty();
        }
    }
}
