using InteractionSystem.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabInteractor : MonoBehaviour
{
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float sphereCastOffset = 0.5f;
    [SerializeField] private Transform[] rayOrigins;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform grabAnchor;
    [SerializeField] private Transform player;
    [SerializeField] private Collider[] playerBodyColliders;

    private Rigidbody grabAnchorRb;
    private IGrabbable currentGrabbed;
    private IPickable currentPicked;
    private Transform grabbedTransform;
    private Collider[] objectColliders;
    private bool grabButtonHeld;
    private Assets.Scripts.AntiGravityController.AntiGravityPlayerController playerController;

    private void Awake ()
    {
        if (rayOrigins == null || rayOrigins.Length == 0)
            rayOrigins = new Transform[] { Camera.main.transform };

        if (player != null)
            playerController = player.GetComponent<Assets.Scripts.AntiGravityController.AntiGravityPlayerController>();

        if (grabAnchor != null)
        {
            grabAnchorRb = grabAnchor.GetComponent<Rigidbody>();
            if (grabAnchorRb == null)
                grabAnchorRb = grabAnchor.gameObject.AddComponent<Rigidbody>();

            grabAnchorRb.isKinematic = true;
            grabAnchorRb.useGravity = false;
        }
    }

    private void FixedUpdate ()
    {
        if (grabAnchorRb != null && holdPoint != null)
        {
            grabAnchorRb.MovePosition(holdPoint.position);
            grabAnchorRb.MoveRotation(holdPoint.rotation);
        }
    }


    public void OnGrab (InputValue value)
    {
        grabButtonHeld = value.isPressed;

        if (!grabButtonHeld)
        {
            if (currentGrabbed != null)
                StopGrab();
            return;
        }

        if (currentPicked != null)
        {
            DropPickedObject();
            return;
        }

        if (TryPick()) return;

        TryStartGrab();
    }

    private void DropPickedObject ()
    {
        var pickedComponent = currentPicked as Component;
        if (pickedComponent != null)
        {
            var pickedColliders = pickedComponent.GetComponentsInChildren<Collider>();

            foreach (var pc in playerBodyColliders)
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(pc, oc, false);

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(characterController, oc, false);

            currentPicked.Drop();
            currentPicked = null;

            if (playerController != null)
                playerController.SetPickState(false);
        }
    }

    private bool TryPick ()
    {
        if (!DetectBestTarget<IPickable>(out var pickableCollider, out var hitPoint)) return false;

        if (pickableCollider.TryGetComponent<IPickable>(out var pickable))
        {
            if (!pickable.CanBePicked())
                return false;

            currentPicked = pickable;
            pickable.Pick(holdPoint);

            var pickedColliders = pickableCollider.GetComponentsInChildren<Collider>();
            foreach (var pc in playerBodyColliders)
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(pc, oc, true);

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(characterController, oc, true);

            if (playerController != null)
                playerController.SetPickState(true);

            return true;
        }

        return false;
    }

    private void TryStartGrab ()
    {
        if (currentPicked != null) return;

        if (!DetectBestTarget<IGrabbable>(out var grabbableCollider, out var hitPoint)) return;

        if (!grabbableCollider.TryGetComponent<IGrabbable>(out var grabbable)) return;

        if (!grabbable.CanBeGrabbed())
        {
            var failThought = grabbable.GetFailThought();
            if (failThought != null && DialogueBox.Instance != null && !DialogueBox.Instance.IsOpen)
                DialogueBox.Instance.StartDialogue(failThought);
            return;
        }

        currentGrabbed = grabbable;

        RotatePlayerToFaceObject(grabbableCollider.transform);

        currentGrabbed.StartGrab(grabAnchorRb, hitPoint);

        grabbedTransform = grabbableCollider.transform;
        objectColliders = grabbedTransform.GetComponentsInChildren<Collider>();

        foreach (var pc in playerBodyColliders)
            foreach (var oc in objectColliders)
                Physics.IgnoreCollision(pc, oc, true);

        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
            foreach (var oc in objectColliders)
                Physics.IgnoreCollision(characterController, oc, true);

        if (playerController != null)
            playerController.SetGrabState(true, grabbable.MoveResistance, grabbableCollider.transform);
    }

    private void RotatePlayerToFaceObject (Transform target)
    {
        if (player == null || target == null) return;

        Vector3 lookDir = target.position - player.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
            player.rotation = Quaternion.LookRotation(lookDir);
    }

    private void StopGrab ()
    {
        if (objectColliders != null)
        {
            foreach (var pc in playerBodyColliders)
                foreach (var oc in objectColliders)
                    Physics.IgnoreCollision(pc, oc, false);

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
                foreach (var oc in objectColliders)
                    Physics.IgnoreCollision(characterController, oc, false);
        }

        currentGrabbed?.StopGrab();
        grabbedTransform = null;
        currentGrabbed = null;

        if (playerController != null)
            playerController.SetGrabState(false, 1f, null);
    }

    private bool DetectBestTarget<T> (out Collider bestCollider, out Vector3 hitPoint) where T : class
    {
        bestCollider = null;
        hitPoint = Vector3.zero;
        float closestDistanceSqr = float.MaxValue;

        foreach (var origin in rayOrigins)
        {
            if (origin == null) continue;

            Vector3 sphereCenter = origin.position + origin.forward * sphereCastOffset;
            Collider[] hits = Physics.OverlapSphere(sphereCenter, sphereRadius);

            foreach (var hit in hits)
            {
                if (hit.GetComponent<T>() != null)
                {
                    Vector3 closestPoint = hit.ClosestPoint(player.position);
                    float distSqr = (player.position - closestPoint).sqrMagnitude;

                    if (distSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distSqr;
                        bestCollider = hit;
                        hitPoint = closestPoint;
                    }
                }
            }
        }

        return bestCollider != null;
    }

    private void OnDrawGizmosSelected ()
    {
        if (rayOrigins == null) return;

        Gizmos.color = Color.yellow;
        foreach (var origin in rayOrigins)
        {
            if (origin != null)
            {
                Vector3 sphereCenter = origin.position + origin.forward * sphereCastOffset;
                Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
            }
        }
    }
}
