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
    [SerializeField] private Collider pushCollider;
    [SerializeField] private float grabOffset = 1f;
    [SerializeField] private Collider[] playerBodyColliders;

    private Rigidbody grabAnchorRb;
    private IGrabbable currentGrabbed;
    private IPickable currentPicked;
    private Transform grabbedTransform;
    private Collider[] objectColliders;
    private bool grabButtonHeld;
    private PlayerController playerController;

    private void Awake()
    {
        if (rayOrigins == null || rayOrigins.Length == 0)
            rayOrigins = new Transform[] { Camera.main.transform };

        if (player != null)
            playerController = player.GetComponent<PlayerController>();

        if (grabAnchor != null)
        {
            grabAnchorRb = grabAnchor.GetComponent<Rigidbody>();
            if (grabAnchorRb == null)
            {
                grabAnchorRb = grabAnchor.gameObject.AddComponent<Rigidbody>();
            }
            grabAnchorRb.isKinematic = false;
            grabAnchorRb.useGravity = false;
        }
        else
        {
            Debug.LogError("GrabInteractor: asigna GrabAnchor en inspector");
        }

        if (pushCollider != null)
        {
            pushCollider.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (grabAnchor != null && holdPoint != null)
        {
            grabAnchor.position = holdPoint.position;
            grabAnchor.rotation = holdPoint.rotation;
        }
    }


    void Update()
    {
        if (grabButtonHeld)
        {
            if (currentGrabbed == null)
                TryStartGrab();
        }
        else
        {
            if (currentGrabbed != null)
                StopGrab();
        }
    }

    public void OnGrab(InputValue value)
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

    private void DropPickedObject()
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
            return;
        }

        if (TryPick()) return;
    }

    private bool TryPick()
    {
        if (!DetectBestTarget<IPickable>(out var pickableCollider, out var hitPoint)) return false;

        if (pickableCollider.TryGetComponent<IPickable>(out var pickable))
        {
            if (!pickable.CanBePicked())
                return false;
            currentPicked = pickable;
            // Place the picked object at the player's hold point so its gripPoint aligns there
            pickable.Pick(holdPoint);

            // Ignore collisions between picked object and player
            var pickedColliders = pickableCollider.GetComponentsInChildren<Collider>();
            foreach (var pc in playerBodyColliders)
            {
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(pc, oc, true);
            }

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                foreach (var oc in pickedColliders)
                    Physics.IgnoreCollision(characterController, oc, true);
            }

            return true;
        }

        return false;
    }

    private void TryStartGrab()
    {
        if (currentPicked != null) return;

        if (!DetectBestTarget<IGrabbable>(out var grabbableCollider, out var hitPoint)) return;

        if (grabbableCollider.TryGetComponent<IGrabbable>(out var grabbable))
        {
            if (!grabbable.CanBeGrabbed())
            {
                var failThought = grabbable.GetFailThought();
                if (failThought != null && DialogueBox.Instance != null && !DialogueBox.Instance.IsOpen)
                {
                    DialogueBox.Instance.StartDialogue(failThought);
                }
                return;
            }
            currentGrabbed = grabbable;

            MovePlayerToGrabPosition(hitPoint);
            RotatePlayerToFaceObject(grabbableCollider.transform);

            currentGrabbed.StartGrab(grabAnchorRb, hitPoint);

            grabbedTransform = grabbableCollider.transform;

            objectColliders = grabbedTransform.GetComponentsInChildren<Collider>();
            pushCollider.enabled = true;

            // Ignorar colisiones entre objeto y el cuerpo
            foreach (var pc in playerBodyColliders)
            {
                foreach (var oc in objectColliders)
                    Physics.IgnoreCollision(pc, oc, true);
            }

            // Ignorar colisiones con el CharacterController explícitamente
            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                foreach (var oc in objectColliders)
                    Physics.IgnoreCollision(characterController, oc, true);
            }

            // Habilitar colisiones pushCollider <-> objeto
            pushCollider.enabled = true;

            if (playerController != null)
                playerController.SetGrabState(true, grabbable.MoveResistance);
        }
    }

    private void MovePlayerToGrabPosition(Vector3 grabPoint)
    {
        Vector3 dir = (player.position - grabPoint).normalized;
        Vector3 targetPos = grabPoint + dir * grabOffset;
        Vector3 moveDir = targetPos - player.position;
        grabAnchorRb.MovePosition(player.position + moveDir * Time.fixedDeltaTime);
    }

    private void RotatePlayerToFaceObject(Transform target)
    {
        Vector3 lookDir = target.position - player.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
            player.rotation = Quaternion.LookRotation(lookDir);
    }

    private void StopGrab()
    {
        if (objectColliders != null)
        {
            foreach (var pc in playerBodyColliders)
            {
                foreach (var oc in objectColliders)
                    Physics.IgnoreCollision(pc, oc, false);
            }

            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                foreach (var oc in objectColliders)
                    Physics.IgnoreCollision(characterController, oc, false);
            }
        }

        pushCollider.enabled = false;
        currentGrabbed?.StopGrab();
        grabbedTransform = null;
        currentGrabbed = null;

        if (playerController != null)
            playerController.SetGrabState(false, 1f);

    }

    private bool DetectBestTarget<T>(out Collider bestCollider, out Vector3 hitPoint) where T : class
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

    private void OnDrawGizmosSelected()
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
