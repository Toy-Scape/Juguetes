using SO;
using UnityEditor;
using UnityEngine;

namespace NPCEditors
{
    [CustomEditor(typeof(PatrolRouteSO))]
    public class PatrolRouteEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI()
        {
            // Update serialized object
            serializedObject.Update();

            // Draw default inspector
            DrawDefaultInspector();

            // Apply changes and repaint scene
            if (serializedObject.ApplyModifiedProperties())
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            PatrolRouteSO route = (PatrolRouteSO)target;

            if (route == null || route.patrolPoints == null || route.patrolPoints.Length < 2)
            {
                return;
            }

            // Draw Controls Points
            Handles.color = Color.yellow;
            for (int i = 0; i < route.patrolPoints.Length; i++)
            {
                var point = route.patrolPoints[i];
                Vector3 pos = point.GetPosition();

                Handles.SphereHandleCap(0, pos, Quaternion.identity, 0.3f, EventType.Repaint);
                Handles.Label(pos + Vector3.up * 0.5f, $"Pt {i}");
            }

            // Draw Generated Path
            var generatedPoints = route.GetPathPoints();
            if (generatedPoints != null && generatedPoints.Count > 1)
            {
                Vector3[] linePoints = new Vector3[generatedPoints.Count + 1]; // +1 to close loop visually if needed, but GetPathPoints already handles logic?
                                                                               // Actually GetPathPoints handles segments. Let's just draw lines between them.

                Handles.color = Color.cyan;

                for (int i = 0; i < generatedPoints.Count; i++)
                {
                    Vector3 p1 = generatedPoints[i].GetPosition();
                    Vector3 p2 = generatedPoints[(i + 1) % generatedPoints.Count].GetPosition();

                    Handles.DrawLine(p1, p2);

                    // Optional: Draw small dots for resolution check
                    // Handles.DotHandleCap(0, p1, Quaternion.identity, 0.05f, EventType.Repaint);
                }
            }
        }
    }
}
