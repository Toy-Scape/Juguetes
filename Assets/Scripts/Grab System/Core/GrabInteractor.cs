using InteractionSystem.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabInteractor : MonoBehaviour
{
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform grabAnchor; 
    [SerializeField] private Transform player;
    [SerializeField] private float grabOffset = 1f;


    private Rigidbody grabAnchorRb;
    private IGrabbable currentGrabbed;
    private IPickable currentPicked;
    private Transform grabbedTransform;
    private bool grabButtonHeld;

    private void Awake()
    {
        if (rayOrigin == null)
            rayOrigin = Camera.main.transform;

        if (grabAnchor != null)
        {
            grabAnchorRb = grabAnchor.GetComponent<Rigidbody>();
            if (grabAnchorRb == null)
            {
                grabAnchorRb = grabAnchor.gameObject.AddComponent<Rigidbody>();
                grabAnchorRb.isKinematic = true;
                grabAnchorRb.useGravity = false;
            }
        }
        else
        {
            Debug.LogError("GrabInteractor: asigna GrabAnchor en inspector");
        }
    }

    private void FixedUpdate()
    {
        if (currentGrabbed != null && grabbedTransform != null)
        {
            holdPoint.position = grabbedTransform.position + player.forward * grabOffset;
            holdPoint.rotation = player.rotation;
        }


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

        if (!grabButtonHeld) return;

        if (currentPicked != null)
        {
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
            currentPicked = pickable;
            pickable.Pick(holdPoint);
            return true;
        }

        return false;
    }

    private void TryStartGrab()
    {
        if (!Raycast(out RaycastHit hit)) return;

        if (hit.collider.TryGetComponent<IGrabbable>(out var grabbable))
        {
            currentGrabbed = grabbable;

            MovePlayerToGrabPosition(hit.collider.transform);

            RotatePlayerToFaceObject(hit.collider.transform);

            currentGrabbed.StartGrab(grabAnchorRb, hit.point);

            grabbedTransform = hit.collider.transform;
        }
    }

    private void MovePlayerToGrabPosition(Transform target)
    {
        Vector3 dir = (player.position - target.position).normalized;
        Vector3 targetPos = target.position + dir * grabOffset;

        player.position = targetPos; 
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
        currentGrabbed?.StopGrab();
        currentGrabbed = null;
    }

    private bool Raycast(out RaycastHit hit)
    {
        return Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, grabDistance);
    }
}
