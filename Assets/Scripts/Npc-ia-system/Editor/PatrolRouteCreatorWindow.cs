#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using SO;

namespace NpcAI.Editor
{
    public class PatrolRouteCreatorWindow : EditorWindow
    {
        private PatrolRouteSO _currentRoute;
        private bool _isPlacing;

        [MenuItem("Tools/NPC System/Patrol Route Creator")]
        public static void ShowWindow()
        {
            GetWindow<PatrolRouteCreatorWindow>("Patrol Route Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create a new PatrolRoute SO and place points on the Scene (snapped to NavMesh)", EditorStyles.wordWrappedLabel);
            GUILayout.Space(8);

            if (_currentRoute == null)
            {
                if (GUILayout.Button("Create New PatrolRoute SO"))
                {
                    _currentRoute = CreateInstance<PatrolRouteSO>();
                    _currentRoute.patrolPoints = new PatrolPointData[0];
                    string path = EditorUtility.SaveFilePanelInProject("Save PatrolRoute", "NewPatrolRoute", "asset", "Save new patrol route asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(_currentRoute, path);
                        AssetDatabase.SaveAssets();
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = _currentRoute;
                    }
                }
            }
            else
            {
                EditorGUILayout.ObjectField("Route: ", _currentRoute, typeof(PatrolRouteSO), false);
                if (!_isPlacing)
                {
                    if (GUILayout.Button("Start Placing Points (Scene click)"))
                        _isPlacing = true;
                }
                else
                {
                    if (GUILayout.Button("Stop Placing"))
                        _isPlacing = false;
                }

                if (GUILayout.Button("Clear Points"))
                {
                    Undo.RecordObject(_currentRoute, "Clear Patrol Points");
                    _currentRoute.patrolPoints = new PatrolPointData[0];
                    EditorUtility.SetDirty(_currentRoute);
                }
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sv)
        {
            if (!_isPlacing || _currentRoute == null) return;

            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    // Snap to NavMesh
                    if (NavMesh.SamplePosition(hit.point, out NavMeshHit nh, 1.0f, NavMesh.AllAreas))
                    {
                        AddPoint(nh.position);
                        e.Use();
                    }
                    else
                    {
                        Debug.LogWarning("Clicked position is not on NavMesh (within 1m). Point not added.");
                    }
                }
            }
        }

        private void AddPoint(Vector3 pos)
        {
            Undo.RecordObject(_currentRoute, "Add Patrol Point");
            var old = _currentRoute.patrolPoints;
            var list = new System.Collections.Generic.List<PatrolPointData>(old);
            var pp = new PatrolPointData { position = pos, waitTime = 0f };
            list.Add(pp);
            _currentRoute.patrolPoints = list.ToArray();
            EditorUtility.SetDirty(_currentRoute);
        }
    }
}
#endif
