using UnityEngine;

public class InteractionGizmos : MonoBehaviour
{

    void OnDrawGizmos()
    {
        var interactor = GetComponent<PlayerInteractor>();
        if (interactor != null)
        {
            Transform origin = interactor.RayOrigin != null ? interactor.RayOrigin : interactor.transform;
            Gizmos.color = Color.yellow;
            Vector3 start = origin.position;
            Vector3 end = start + origin.forward * interactor.InteractionDistance;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.1f);
        }
    }
}
