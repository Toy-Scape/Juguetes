#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SO;

namespace NpcAI.Editor
{
    [CustomEditor(typeof(PatrolRouteSO))]
    public class PatrolRouteSoEditor : UnityEditor.Editor
    {
        private PatrolRouteSO _route;

        private void OnEnable()
        {
            _route = (PatrolRouteSO)target;
            // Ensure Scene GUI callback runs when selection is active
            SceneView.duringSceneGui += OnSceneGUIDuring;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUIDuring;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Validate NavMesh Points"))
            {
                _route.ValidateNavMeshPoints();
                EditorUtility.SetDirty(_route);
            }
        }

        // Wrapper called by SceneView.duringSceneGui
        private void OnSceneGUIDuring(SceneView sv)
        {
            // Only draw when this SO is selected in the Project/Inspector
            if (Selection.activeObject != _route) return;

            DrawSceneHandles();
        }

        // Keep the drawing logic separated so it can be called both from OnSceneGUI and the delegate.
        private void DrawSceneHandles()
        {
            if (_route == null || _route.patrolPoints == null) return;

            Handles.color = Color.cyan;
            for (int i = 0; i < _route.patrolPoints.Length; i++)
            {
                var p = _route.patrolPoints[i];
                Vector3 pos = p.GetPosition();

                // Draw handle for position
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_route, "Move Patrol Point");
                    // If there's a scene reference, move that; otherwise store the position in the SO
                    if (p.sceneReference != null)
                    {
                        p.sceneReference.position = newPos;
                    }
                    else
                    {
                        p.position = newPos;
                        EditorUtility.SetDirty(_route);
                    }
                }

                // Draw label
                Handles.Label(pos + Vector3.up * 0.5f, $"P{i}");

                // Draw line to next
                if (_route.patrolPoints.Length > 1)
                {
                    Vector3 nextPos = _route.patrolPoints[(i + 1) % _route.patrolPoints.Length].GetPosition();
                    Handles.DrawLine(pos, nextPos);
                }
            }

            // If CatmullRom, draw a smoothed curve preview
            if (_route.interpolation == InterpolationType.CatmullRom && _route.patrolPoints.Length >= 4)
            {
                Handles.color = Color.yellow;
                Vector3 prev = _route.patrolPoints[0].GetPosition();
                int steps = 20;
                for (int i = 0; i < _route.patrolPoints.Length; i++)
                {
                    for (int s = 1; s <= steps; s++)
                    {
                        float t = s / (float)steps;
                        // compute point along Catmull-Rom between i and i+1
                        Vector3 cp = Domain.CatmullRomUtils.LerpAround(_route.patrolPoints, i, t);
                        Handles.DrawLine(prev, cp);
                        prev = cp;
                    }
                }
            }
        }

        // Unity may still call this; keep compatibility
        private void OnSceneGUI()
        {
            if (Selection.activeObject == _route)
                DrawSceneHandles();
        }
    }
}
#endif
