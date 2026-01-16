using UnityEngine;

public class GrabInteractor : MonoBehaviour
{
    [SerializeField] private float grabRadius = 1.0f;
    [SerializeField] private Transform grabOrigin;
    [SerializeField] private LayerMask grabLayer;
    [SerializeField] private Transform holdPoint;

    private Grabbable currentGrabbable;
    private Pickable currentPickable;
    private bool isGrabbing;
    private bool isPicking;
    private Vector3 grabOffset;
    private Quaternion grabRotationOffset;

    private CharacterController characterController;
    private Collider[] playerColliders;

    public bool IsGrabbing => isGrabbing;
    public bool IsPicking => isPicking;

    public Transform GrabbedObjectTransform => currentGrabbable != null ? currentGrabbable.transform : null;

    public float CurrentResistance => currentGrabbable != null ? currentGrabbable.MoveResistance : 0f;


    private void Awake ()
    {
        characterController = GetComponent<CharacterController>();
        playerColliders = GetComponentsInChildren<Collider>();
        if (grabOrigin == null) grabOrigin = transform;
    }

    public bool TryPick ()
    {
        if (isPicking || isGrabbing) return false;

        Collider[] hits = Physics.OverlapSphere(grabOrigin.position, grabRadius, grabLayer);
        float closestDist = float.MaxValue;
        Pickable bestTarget = null;

        foreach (var hit in hits)
        {
            var pickable = hit.GetComponentInParent<Pickable>();
            if (pickable != null)
            {
                float d = (hit.ClosestPoint(grabOrigin.position) - grabOrigin.position).sqrMagnitude;
                if (d < closestDist)
                {
                    closestDist = d;
                    bestTarget = pickable;
                }
            }
        }

        if (bestTarget == null) return false;

        if (!bestTarget.CanBePicked())
        {
            var failThought = bestTarget.GetFailThought();
            if (failThought != null && DialogueBox.Instance != null && !DialogueBox.Instance.IsOpen)
                DialogueBox.Instance.StartDialogue(failThought);
            return false;
        }

        currentPickable = bestTarget;
        isPicking = true;

        currentPickable.Pick(holdPoint);

        var pickedColliders = currentPickable.GetComponentsInChildren<Collider>();

        foreach (var pc in playerColliders)
            foreach (var oc in pickedColliders)
                Physics.IgnoreCollision(pc, oc, true);

        if (characterController != null)
            foreach (var oc in pickedColliders)
                Physics.IgnoreCollision(characterController, oc, true);

        var allGrabbables = FindObjectsByType<Grabbable>(FindObjectsSortMode.None);
        foreach (var grabbable in allGrabbables)
        {
            var grabbableColliders = grabbable.GetComponentsInChildren<Collider>();
            foreach (var oc in pickedColliders)
                foreach (var gc in grabbableColliders)
                    Physics.IgnoreCollision(oc, gc, true);
        }

        return true;
    }

    public void DropPicked ()
    {
        if (!isPicking) return;

        var pickedColliders = currentPickable.GetComponentsInChildren<Collider>();

        foreach (var pc in pickedColliders)
        {
            BoxCollider bc = pc as BoxCollider;

            Vector3 worldCenter = bc.transform.TransformPoint(bc.center);
            Vector3 halfExtents = Vector3.Scale(bc.size * 0.5f, bc.transform.lossyScale);
            Quaternion worldRotation = bc.transform.rotation;

            Collider[] overlaps = Physics.OverlapBox(
                worldCenter,
                halfExtents,
                worldRotation,
                ~0,
                QueryTriggerInteraction.Ignore
            );


            Debug.Log($"[DropDebug] PC: {pc.name}, overlaps: {overlaps.Length}");

            foreach (var hit in overlaps)
            {
                if (hit == pc) continue;
                if (hit.transform.IsChildOf(currentPickable.transform)) continue;
                if (hit.transform.IsChildOf(transform)) continue;

                Debug.Log($"[DropDebug] Blocking drop because of: {hit.name} (layer {hit.gameObject.layer})");
                return;
            }
        }


        var allGrabbables = FindObjectsByType<Grabbable>(FindObjectsSortMode.None);
        foreach (var grabbable in allGrabbables)
        {
            var grabbableColliders = grabbable.GetComponentsInChildren<Collider>();
            foreach (var oc in pickedColliders)
                foreach (var gc in grabbableColliders)
                    Physics.IgnoreCollision(oc, gc, false);
        }

        foreach (var pc in playerColliders)
            foreach (var oc in pickedColliders)
                Physics.IgnoreCollision(pc, oc, false);

        if (characterController != null)
            foreach (var oc in pickedColliders)
                Physics.IgnoreCollision(characterController, oc, false);

        currentPickable.Drop();
        currentPickable = null;
        isPicking = false;
    }

    public bool TryGrab ()
    {
        if (isPicking || isGrabbing) return false;

        Collider[] hits = Physics.OverlapSphere(grabOrigin.position, grabRadius, grabLayer);
        float closestDist = float.MaxValue;
        Grabbable bestTarget = null;
        Vector3 bestPoint = Vector3.zero;

        foreach (var hit in hits)
        {
            var grabbable = hit.GetComponentInParent<Grabbable>();
            if (grabbable != null)
            {
                float d = (hit.ClosestPoint(grabOrigin.position) - grabOrigin.position).sqrMagnitude;
                if (d < closestDist)
                {
                    closestDist = d;
                    bestTarget = grabbable;
                    bestPoint = hit.ClosestPoint(grabOrigin.position);
                }
            }
        }

        if (bestTarget == null) return false;

        if (!bestTarget.CanBeGrabbed())
        {
            var failThought = bestTarget.GetFailThought();
            if (failThought != null && DialogueBox.Instance != null && !DialogueBox.Instance.IsOpen)
                DialogueBox.Instance.StartDialogue(failThought);
            return false;
        }

        StartGrab(bestTarget, bestPoint);
        return true;
    }

    public void ReleaseGrab ()
    {
        if (!isGrabbing) return;

        if (currentGrabbable != null)
        {
            foreach (var pc in playerColliders)
                currentGrabbable.IgnoreCollisionWith(pc, false);

            if (characterController != null)
                currentGrabbable.IgnoreCollisionWith(characterController, false);

            currentGrabbable.StopGrab();
        }

        currentGrabbable = null;
        isGrabbing = false;
    }

    private void StartGrab (Grabbable grabbable, Vector3 hitPoint)
    {
        currentGrabbable = grabbable;
        isGrabbing = true;

        Vector3 direction = hitPoint - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);

        grabOffset = transform.InverseTransformPoint(currentGrabbable.transform.position);
        grabRotationOffset = Quaternion.Inverse(transform.rotation) * currentGrabbable.transform.rotation;

        foreach (var pc in playerColliders)
            currentGrabbable.IgnoreCollisionWith(pc, true);

        if (characterController != null)
            currentGrabbable.IgnoreCollisionWith(characterController, true);

        currentGrabbable.StartGrab();
    }

    public bool CheckMove (Vector3 movement)
    {
        if (!isGrabbing || currentGrabbable == null)
            return true;

        if (!currentGrabbable.ValidateMovement(movement))
            return false;

        Vector3 targetPos = transform.TransformPoint(grabOffset) + movement;
        Quaternion targetRot = transform.rotation * grabRotationOffset;

        if (currentGrabbable.CheckCollision(targetPos, targetRot))
            return false;

        return true;
    }

    public void ValidateRotation (float proposedAngle, Vector3 playerPos, out float allowedAngle, out Vector3 effectivePivot)
    {
        if (!isGrabbing || currentGrabbable == null)
        {
            allowedAngle = proposedAngle;
            effectivePivot = Vector3.zero;
            return;
        }

        currentGrabbable.ValidateRotation(proposedAngle, playerPos, out allowedAngle, out effectivePivot);
    }

    public bool IsConfigurationValid (Vector3 playerPosition, Quaternion playerRotation)
    {
        if (!isGrabbing || currentGrabbable == null) return true;

        Vector3 targetPos = playerPosition + (playerRotation * grabOffset);
        Quaternion targetRot = playerRotation * grabRotationOffset;

        return !currentGrabbable.CheckCollision(targetPos, targetRot);
    }

    public void UpdateObjectPosition ()
    {
        if (!isGrabbing || currentGrabbable == null) return;

        Vector3 targetPos = transform.TransformPoint(grabOffset);
        Quaternion targetRot = transform.rotation * grabRotationOffset;

        if (currentGrabbable.CheckCollision(targetPos, targetRot))
            return;

        currentGrabbable.MoveTo(targetPos, targetRot);
    }

    private void OnDrawGizmosSelected ()
    {
        if (grabOrigin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grabOrigin.position, grabRadius);
        }
    }
}
