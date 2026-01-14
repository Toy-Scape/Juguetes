using InteractionSystem.Interfaces;
using UnityEngine;

public class Pickable : MonoBehaviour, IPickable
{
    [SerializeField] private Transform gripPoint;
    [SerializeField] private GenericConditionSO[] pickConditions;

    public bool IsPicked { get; private set; }

    private Rigidbody rb;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();
        if (gripPoint == null) gripPoint = transform;
    }

    public bool CanBePicked ()
    {
        if (pickConditions == null || pickConditions.Length == 0) return true;
        foreach (var c in pickConditions)
            if (c != null && !c.ConditionIsMet())
                return false;
        return true;
    }

    public void Pick (Transform hand)
    {
        IsPicked = true;

        rb.isKinematic = true;
        transform.SetParent(hand);

        // Ajuste de posición y rotación
        transform.localPosition = -gripPoint.localPosition;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop ()
    {
        IsPicked = false;

        transform.SetParent(null);
        rb.isKinematic = false;
    }
}
