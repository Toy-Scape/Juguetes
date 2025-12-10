using InteractionSystem.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabInteractor : MonoBehaviour
{
    [SerializeField] private float grabDistance = 3f;
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
        if (!Raycast(out RaycastHit hit)) return false;

        if (hit.collider.TryGetComponent<IPickable>(out var pickable))
        {
            if (!pickable.CanBePicked())
                return false;
            currentPicked = pickable;
            // Place the picked object at the player's hold point so its gripPoint aligns there
            pickable.Pick(holdPoint);

            // Ignore collisions between picked object and player
            var pickedColliders = hit.collider.GetComponentsInChildren<Collider>();
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

        if (!Raycast(out RaycastHit hit)) return;

        if (hit.collider.TryGetComponent<IGrabbable>(out var grabbable))
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

            MovePlayerToGrabPosition(hit);
            RotatePlayerToFaceObject(hit.collider.transform);

            currentGrabbed.StartGrab(grabAnchorRb, hit.point);

            grabbedTransform = hit.collider.transform;

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

    private void MovePlayerToGrabPosition(RaycastHit hit)
    {
        Vector3 dir = (player.position - hit.point).normalized;
        Vector3 targetPos = hit.point + dir * grabOffset;
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

    private bool Raycast(out RaycastHit hit)
    {
        hit = new RaycastHit();
        foreach (var origin in rayOrigins)
        {
            if (origin == null) continue;
            if (Physics.Raycast(origin.position, origin.forward, out hit, grabDistance))
            {
                return true;
            }
        }
        return false;
    }
}
