using System.Linq;
using InteractionSystem.Interfaces;
using UnityEngine;

public class Pickable : MonoBehaviour, IPickable
{
    [SerializeField] private Transform gripPoint;
    [SerializeField] private GenericConditionSO[] pickConditions;

    public bool IsPicked { get; private set; }

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (gripPoint == null) gripPoint = transform;
    }

    public bool CanBePicked()
    {
        if (pickConditions == null || pickConditions.Length == 0) return true;
        return pickConditions.All(c => c != null && c.ConditionIsMet());
    }


    public void Pick(Transform hand)
    {
        if (!CanBePicked()) return;
        IsPicked = true;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        transform.SetParent(hand);
        transform.rotation = hand.rotation;
        transform.position = hand.position - (gripPoint.position - transform.position);
    }

    public void Drop()
    {
        IsPicked = false;
        transform.SetParent(null);

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = true;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
}
