using UnityEngine;
using InteractionSystem.Interfaces;
using System.Linq;

public class Pickable : MonoBehaviour, IPickable
{
    [SerializeField] private Transform gripPoint;
    [SerializeField] private GrabConditionSO[] pickConditions;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (gripPoint == null) gripPoint = transform;
    }

    public bool CanBePicked()
    {
        if (pickConditions == null || pickConditions.Length == 0) return true;
        return pickConditions.All(c => c != null && c.CanGrab());
    }


    public void Pick(Transform hand)
    {
        if (!CanBePicked()) return;
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
        transform.SetParent(null);

        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = true;

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
}
