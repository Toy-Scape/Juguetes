using UnityEngine;

public class WindSource : MonoBehaviour
{
    public float windForce = 10f;
    public float maxDistance = 10f;

    // Tamaño del área del viento
    public Vector3 boxSize = new Vector3(2f, 2f, 10f);

    // Offset local para ajustar la posición del viento
    public Vector3 windOffset = new Vector3(0f, 0f, 0.5f);

    public LayerMask obstacleMask;
    public bool showGizmos = true;

    void FixedUpdate()
    {
        // Centro del área del viento con offset
        Vector3 center = transform.TransformPoint(windOffset + new Vector3(0, 0, boxSize.z * 0.5f));

        Collider[] hits = Physics.OverlapBox(center, boxSize * 0.5f, transform.rotation);

        foreach (Collider col in hits)
        {
            if (((1 << col.gameObject.layer) & obstacleMask) != 0)
                continue;

            WindAffected windObj = col.GetComponent<WindAffected>();
            if (windObj != null)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                float distanceFactor = 1f - Mathf.Clamp01(dist / maxDistance);

                windObj.ApplyWind(transform.forward, windForce * distanceFactor);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = new Color(0, 1, 1, 0.3f);

        Vector3 center = transform.TransformPoint(windOffset + new Vector3(0, 0, boxSize.z * 0.5f));

        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}
