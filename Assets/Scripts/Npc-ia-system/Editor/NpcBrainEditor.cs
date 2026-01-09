using UnityEditor;
using UnityEngine;
using Domain;
using Infrastructure;

namespace Editor
{
    [CustomEditor(typeof(NpcBrain))]
    public class NpcBrainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            NpcBrain brain = (NpcBrain)target;

            // Check if INavigationAgent is present
            var agent = brain.GetComponent<Core.INavigationAgent>();
            if (agent == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Missing INavigationAgent! The NPC needs an adapter to move.", MessageType.Error);
                
                if (GUILayout.Button("Fix: Add NavMeshAgentAdapter"))
                {
                    Undo.AddComponent<NavMeshAgentAdapter>(brain.gameObject);
                }
            }

            // Check if ITargetDetector is present (VisionSensor)
            var vision = brain.GetComponent<Core.ITargetDetector>();
            if (vision == null && brain.vision == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Missing VisionSensor! The NPC cannot see.", MessageType.Error);
                if (GUILayout.Button("Fix: Add VisionSensor"))
                {
                    Undo.AddComponent<Core.VisionSensor>(brain.gameObject);
                }
            }
        }
    }
}
