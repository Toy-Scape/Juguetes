using UnityEngine;

namespace Domain
{
    public static class CatmullRomUtils
    {
        // LerpAround computes a point on the Catmull-Rom curve between index and index+1
        public static Vector3 LerpAround(SO.PatrolPointData[] pts, int index, float t)
        {
            int count = pts.Length;
            if (count < 4) return pts[index].GetPosition();

            Vector3 p0 = pts[(index - 1 + count) % count].GetPosition();
            Vector3 p1 = pts[index % count].GetPosition();
            Vector3 p2 = pts[(index + 1) % count].GetPosition();
            Vector3 p3 = pts[(index + 2) % count].GetPosition();

            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * ((2f * p1) +
                           (-p0 + p2) * t +
                           (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                           (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }
    }
}

