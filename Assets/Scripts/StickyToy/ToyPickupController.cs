using UnityEngine;
using Domain.StaticNpc;

public class ToyPickupController : MonoBehaviour
{
    public Transform handTarget;
    public Transform windowTarget;
    Vector3 savedWindowPosition;
    Quaternion savedWindowRotation;
    bool hasSavedWindowPosition;
    public StaticNpcActionHandler stickyActions;

    Rigidbody rb;
    WindAffectedSimpleFall wind;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        wind = GetComponent<WindAffectedSimpleFall>();
    }

    public void SaveWindowPose()
    {
        savedWindowPosition = transform.position;
        savedWindowRotation = transform.rotation;
        hasSavedWindowPosition = true;
    }

    public void AttachToHand()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;

        transform.SetParent(handTarget);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void PlaceOnWindow()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        stickyActions.PlayActionsByIdSequence(
            "Idle"
        );

        rb.isKinematic = true;

        transform.SetParent(null, true);

        if (hasSavedWindowPosition)
        {
            transform.position = savedWindowPosition;
            transform.rotation = savedWindowRotation;
        }
        else
        {
            transform.position = windowTarget.position;
            transform.rotation = windowTarget.rotation;
        }

        if (wind != null)
            wind.ResetBlownState();
    }
}