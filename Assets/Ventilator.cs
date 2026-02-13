using UnityEngine;

public class Ventilator : MonoBehaviour
{
    public float windForce = 10f;
    public float maxDistance = 10f;
    public float radius = 1.5f;
    public int rayCount = 20;
    public LayerMask obstacleMask;
    public bool showGizmos = true;

    void FixedUpdate()
    {
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 origin = transform.position + transform.right * randomCircle.x + transform.up * randomCircle.y;
            Vector3 direction = transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, maxDistance))
            {
                if (((1 << hit.collider.gameObject.layer) & obstacleMask) != 0) continue;
                WindAffected windObj = hit.collider.GetComponent<WindAffected>();
                if (windObj != null)
                {
                    float distanceFactor = 1f - (hit.distance / maxDistance);
                    windObj.ApplyWind(direction, windForce * distanceFactor);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < rayCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 origin = transform.position + transform.right * randomCircle.x + transform.up * randomCircle.y;
            Gizmos.DrawRay(origin, transform.forward * maxDistance);
        }
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
