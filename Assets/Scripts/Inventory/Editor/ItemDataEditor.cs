#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Dibujar todos los campos hasta isLimb
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            
            while (iterator.NextVisible(false))
            {
                if (iterator.name == "limbSO")
                {
                    // Solo mostrar limbSO si isLimb es true
                    SerializedProperty isLimbProp = serializedObject.FindProperty("isLimb");
                    if (isLimbProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

