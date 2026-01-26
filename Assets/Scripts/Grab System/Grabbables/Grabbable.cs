using UnityEngine;
using InteractionSystem.Interfaces;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour, IGrabbable
{
    [SerializeField] private float moveResistance = 1f;
    [SerializeField] private ConditionSO[] conditions;
    [SerializeField] private Dialogue failureMessage;
    [SerializeField] private LayerMask collisionLayer = -1;

    public float MoveResistance => moveResistance;

    private Rigidbody rb;
    private Collider[] colliders;
    private BoxCollider boxCollider;
    private bool isGrabbed;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        boxCollider = GetComponent<BoxCollider>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public bool CanBeGrabbed()
    {
        var provider = FindFirstObjectByType<PlayerConditionProvider>();

        foreach (var c in conditions)
        {
            if (!c.TryEvaluate(provider, out failureMessage))
                return false;
        }

        failureMessage = null;
        return true;
    }


    public void StartGrab ()
    {
        if (!CanBeGrabbed()) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isGrabbed = true;
    }

    public void StopGrab ()
    {
        isGrabbed = false;
        rb.WakeUp();
    }

    public bool ValidateMovement (Vector3 translation)
    {
        if (!isGrabbed)
            return true;

        float dist = translation.magnitude;
        if (dist < 0.001f)
            return true;

        RaycastHit[] hits = rb.SweepTestAll(
            translation.normalized,
            dist + 0.05f,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & collisionLayer) == 0)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (Vector3.Dot(hit.normal, Vector3.up) > 0.7f)
                continue;

            if (hit.distance < 0.01f)
            {
                float dot = Vector3.Dot(translation.normalized, hit.normal);
                if (dot < -0.01f)
                    return false;
            }
            else
            {
                if (hit.distance <= dist)
                    return false;
            }
        }

        return true;
    }

    public void ValidateRotation (float proposedAngle, Vector3 playerPosition, out float allowedAngle, out Vector3 effectivePivot)
    {
        allowedAngle = proposedAngle;
        effectivePivot = rb.position;

        if (!isGrabbed || boxCollider == null) return;

        Vector3 scaledSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
        Vector3 extents = scaledSize * 0.5f + Vector3.one * 0.02f;

        Collider[] hits = Physics.OverlapBox(
            rb.position + rb.rotation * boxCollider.center,
            extents,
            rb.rotation,
            collisionLayer,
            QueryTriggerInteraction.Ignore
        );

        Vector3 closestContact = Vector3.zero;
        float closestDist = float.MaxValue;
        bool touching = false;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            Vector3 pt = GetClosestPointSafe(hit, playerPosition);
            float d = Vector3.SqrMagnitude(pt - playerPosition);
            if (d < closestDist)
            {
                closestDist = d;
                closestContact = pt;
                touching = true;
            }
        }

        if (touching)
            effectivePivot = closestContact;

        Vector3[] corners = GetBoxCorners();
        float minRatio = 1f;

        foreach (var corner in corners)
        {
            Vector3 dir = corner - effectivePivot;
            Quaternion rot = Quaternion.Euler(0, proposedAngle, 0);
            Vector3 targetPos = effectivePivot + rot * dir;

            Vector3 moveVec = targetPos - corner;
            float moveDist = moveVec.magnitude;

            if (moveDist > 0.001f)
            {
                if (Physics.Raycast(corner, moveVec.normalized, out RaycastHit hit, moveDist, collisionLayer, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform != transform)
                    {
                        float ratio = hit.distance / moveDist;
                        if (ratio < minRatio)
                            minRatio = ratio;
                    }
                }
            }
        }

        allowedAngle = proposedAngle * minRatio;
        if (minRatio < 1f) allowedAngle *= 0.9f;
    }

    private Vector3 GetClosestPointSafe (Collider col, Vector3 point)
    {
        if (col is BoxCollider || col is SphereCollider || col is CapsuleCollider || (col is MeshCollider mc && mc.convex))
            return col.ClosestPoint(point);

        return col.ClosestPointOnBounds(point);
    }

    private Vector3[] GetBoxCorners ()
    {
        Vector3 scaledSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
        Vector3 center = boxCollider.center;
        Vector3 half = scaledSize * 0.5f;

        Vector3[] corners = new Vector3[8];

        corners[0] = center + new Vector3(half.x, half.y, half.z);
        corners[1] = center + new Vector3(-half.x, half.y, half.z);
        corners[2] = center + new Vector3(half.x, -half.y, half.z);
        corners[3] = center + new Vector3(-half.x, -half.y, half.z);
        corners[4] = center + new Vector3(half.x, half.y, -half.z);
        corners[5] = center + new Vector3(-half.x, half.y, -half.z);
        corners[6] = center + new Vector3(half.x, -half.y, -half.z);
        corners[7] = center + new Vector3(-half.x, -half.y, -half.z);

        for (int i = 0; i < 8; i++)
            corners[i] = transform.TransformPoint(corners[i]);

        return corners;
    }

    public void MoveTo (Vector3 position, Quaternion rotation)
    {
        rb.MovePosition(position);
        rb.MoveRotation(rotation);
    }

    public bool CheckCollision (Vector3 position, Quaternion rotation)
    {
        if (boxCollider == null)
            return false;

        Vector3 scaledSize = Vector3.Scale(boxCollider.size, transform.lossyScale);
        Vector3 extents = scaledSize * 0.5f;
        extents.x *= 0.999f;
        extents.z *= 0.999f;
        extents.y *= 0.90f;

        Collider[] hits = Physics.OverlapBox(
            position + rotation * boxCollider.center,
            extents,
            rotation,
            collisionLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            Vector3 dir = (hit.transform.position - position).normalized;
            if (Vector3.Dot(dir, Vector3.up) > 0.7f)
                continue;

            return true;
        }

        return false;
    }

    public void IgnoreCollisionWith (Collider other, bool ignore)
    {
        foreach (var c in colliders)
            Physics.IgnoreCollision(c, other, ignore);
    }

    public Dialogue GetFailThought () => failureMessage;
}
