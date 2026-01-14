using InteractionSystem.Interfaces;
using UnityEngine;

public class Grabbable : MonoBehaviour, IGrabbable
{
    [SerializeField] private float moveResistance = 1f;
    [SerializeField] private GenericConditionSO[] grabConditions;
    [SerializeField] private Dialogue failThought;

    private Rigidbody rb;
    private Rigidbody anchorRb;
    private bool isGrabbed;
    private Vector3 grabOffset;

    public float MoveResistance => moveResistance;
    public Dialogue GetFailThought () => failThought;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.isKinematic = false;
    }

    public bool CanBeGrabbed ()
    {
        if (grabConditions == null || grabConditions.Length == 0) return true;

        foreach (var c in grabConditions)
        {
            if (c != null && !c.ConditionIsMet())
                return false;
        }

        return true;
    }

    public void StartGrab (Rigidbody grabAnchorRb, Vector3 grabPoint)
    {
        if (!CanBeGrabbed()) return;

        anchorRb = grabAnchorRb;
        grabOffset = rb.position - grabPoint;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //rb.isKinematic = true;

        isGrabbed = true;
    }

    public void StopGrab ()
    {
        isGrabbed = false;
        anchorRb = null;

        //rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate ()
    {
        if (!isGrabbed || anchorRb == null) return;

        float resistance = Mathf.Max(0.1f, moveResistance);
        float speed = 5f / resistance;

        Vector3 targetPos = anchorRb.position + grabOffset;
        Vector3 newPos = Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);
    }
}
