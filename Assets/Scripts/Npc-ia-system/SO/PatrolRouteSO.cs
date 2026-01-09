using UnityEngine;
using UnityEngine.Events;

namespace SO
{
    public enum InterpolationType { Linear, CatmullRom }

    [System.Serializable]
    public class PatrolPointData
    {
        [Tooltip("World position of the patrol point. If a Scene Transform is assigned, that position will be used at runtime.")]
        public Vector3 position;

        [Tooltip("Optional: reference to a scene Transform. If assigned, its position will override the stored position at runtime.")]
        public Transform sceneReference;

        public float waitTime;
        public UnityEvent onReachPoint;

        [Header("Movement")]
        [Tooltip("If true uses moveSpeed, otherwise default speed from route is used.")]
        public bool overrideSpeed;
        public float moveSpeed = 3f;

        public Vector3 GetPosition()
        {
            if (sceneReference != null)
                return sceneReference.position;
            return position;
        }
    }

    [CreateAssetMenu(fileName = "PatrolRoute", menuName = "NPC System/PatrolRoute")]
    public class PatrolRouteSO : ScriptableObject
    {
        public bool randomPatrol;
        public PatrolPointData[] patrolPoints;

        [Header("Route Settings")]
        public InterpolationType interpolation = InterpolationType.Linear;

        [Tooltip("If true, uses defaultSpeed from this route. If false, uses the global patrolSpeed from NpcBrain.")]
        public bool useRouteSpeed = false;
        public float defaultSpeed = 3f;

        [Header("Spline Settings")]
        [Range(2, 50)] public int curveResolution = 10;
        [Tooltip("0 = Standard Catmull-Rom (Smooth). 1 = Linear (Straight). Negative = Floppy/Looser.")]
        [Range(-1f, 1f)] public float curveTension = 0f;

        private void OnValidate()
        {
            ValidateNavMeshPoints();
        }

        /// <summary>
        /// Valida los puntos de la ruta contra el NavMesh y asegura que el array no sea null.
        /// Puede llamarse desde código o desde editores sin depender de la visibilidad de OnValidate.
        /// </summary>
        public void ValidateNavMeshPoints()
        {
            if (patrolPoints == null)
                patrolPoints = new PatrolPointData[0];

#if UNITY_EDITOR
            // Basic navmesh validation: ensure points are near a NavMesh position
            UnityEngine.AI.NavMeshHit hit;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                var p = patrolPoints[i];
                Vector3 pos = p.GetPosition();
                bool valid = UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas);
                if (!valid)
                {
                    Debug.LogWarning($"PatrolRouteSO '{name}': patrol point {i} at {pos} is not on NavMesh (within 1m). Consider adjusting or assigning a scene reference.");
                }
            }
#endif
        }

        public System.Collections.Generic.List<PatrolPointData> GetPathPoints()
        {
            var list = new System.Collections.Generic.List<PatrolPointData>();
            if (patrolPoints == null || patrolPoints.Length == 0) return list;

            // Simple Linear or Random -> Return original points
            if (randomPatrol || interpolation == InterpolationType.Linear || patrolPoints.Length < 2)
            {
                list.AddRange(patrolPoints);
                return list;
            }

            // Cardinal Spline (Generalization of Catmull-Rom)
            // We assume a closed loop for the spline to work effectively on circular patrols
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                // Control points
                Vector3 p0 = GetPointPos(i - 1);
                Vector3 p1 = GetPointPos(i);
                Vector3 p2 = GetPointPos(i + 1);
                Vector3 p3 = GetPointPos(i + 2);

                // Add the main control point (p1)
                list.Add(patrolPoints[i]);

                // Add intermediate points between p1 and p2
                for (int j = 1; j < curveResolution; j++)
                {
                    float t = j / (float)curveResolution;
                    Vector3 pos = EvaluateCardinal(p0, p1, p2, p3, t, curveTension);

                    // Intermediate point usually shouldn't wait
                    var point = new PatrolPointData();
                    point.position = pos;
                    point.waitTime = 0f;
                    list.Add(point);
                }
            }

            return list;
        }

        private Vector3 GetPointPos(int i)
        {
            int len = patrolPoints.Length;
            // Handle wrap around
            int index = (i % len + len) % len;
            return patrolPoints[index].GetPosition();
        }

        private Vector3 EvaluateCardinal(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension)
        {
            // Cardinal Spline logic
            // s = (1 - tension) / 2
            float s = (1f - tension) * 0.5f;

            Vector3 m1 = s * (p2 - p0);
            Vector3 m2 = s * (p3 - p1);

            float t2 = t * t;
            float t3 = t2 * t;

            // Hermite Basis functions
            float h1 = 2f * t3 - 3f * t2 + 1f;          // (2t^3 - 3t^2 + 1)
            float h2 = t3 - 2f * t2 + t;            // (t^3 - 2t^2 + t)
            float h3 = -2f * t3 + 3f * t2;          // (-2t^3 + 3t^2)
            float h4 = t3 - t2;                     // (t^3 - t^2)

            Vector3 ret = (h1 * p1) + (h2 * m1) + (h3 * p2) + (h4 * m2);
            return ret;
        }
    }
}
