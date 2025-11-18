using InteractionSystem.Core;
using UnityEngine;

namespace InteractionSystem.Core
{
    public class InteractionGizmos : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            var interactor = GetComponent<PlayerInteractor>();

            if (interactor == null)
                return;

            Transform[] origins = interactor.RayOrigins;
            int rayCount = origins.Length;

            for (int i = 0; i < rayCount; i++)
            {
                Transform origin = origins[i] != null ? origins[i] : transform;

                // Genera un color único usando HSV
                Color gizmoColor = Color.HSVToRGB((float)i / Mathf.Max(rayCount, 1), 0.8f, 1f);

                Gizmos.color = gizmoColor;

                Vector3 start = origin.position;
                Vector3 end = start + origin.forward * interactor.InteractionDistance;

                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.05f);
            }
        }
    }
}
