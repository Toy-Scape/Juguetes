using System.Collections.Generic;
using System.Linq;
using CinematicSystem.Infrastructure;
using UnityEditor;
using UnityEngine;

namespace CinematicSystem.Editor
{
    [CustomPropertyDrawer(typeof(Core.SceneReferenceIDAttribute))]
    public class SceneReferenceIDDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [SceneReferenceID] with string.");
                return;
            }

            // Find resolver in the scene
            var resolver = Object.FindFirstObjectByType<SceneReferenceResolver>();

            if (resolver == null)
            {
                EditorGUI.PropertyField(position, property, label); // Fallback to text field
                return;
            }

            List<string> ids = new List<string>();
            ids.Add("None");

            if (resolver.references != null)
            {
                ids.AddRange(resolver.references.Select(r => r.id).Where(id => !string.IsNullOrEmpty(id)));
            }

            string currentId = property.stringValue;
            int currentIndex = ids.IndexOf(currentId);
            if (currentIndex == -1) currentIndex = 0; // Default to None if not found

            // Draw popup
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, ids.ToArray());

            if (newIndex > 0)
            {
                property.stringValue = ids[newIndex];
            }
            else
            {
                property.stringValue = "";
            }
        }
    }
}
