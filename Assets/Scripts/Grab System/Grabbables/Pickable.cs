using UnityEngine;
using InteractionSystem.Interfaces;
using System;

public class Pickable : MonoBehaviour, IPickable
{
    [SerializeField] private Transform gripPoint;
    [SerializeField] private ConditionSO[] conditions;
    [SerializeField] private Dialogue failureMessage;

    public bool IsPicked { get; private set; }

    private Rigidbody rb;

    private void Awake ()
    {
        rb = GetComponent<Rigidbody>();
        if (gripPoint == null) gripPoint = transform;
    }

    public bool CanBePicked()
    {
        var provider = FindFirstObjectByType<PlayerConditionProvider>();

        foreach (var c in conditions)
        {
            if (!c.TryEvaluate(provider, out failureMessage))
                return false;
        }

        failureMessage = null;
        return true;
    }



    public void Pick (Transform hand)
    {
        IsPicked = true;

        rb.isKinematic = true;
        transform.SetParent(hand);

        transform.localPosition = -gripPoint.localPosition;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop ()
    {
        IsPicked = false;

        transform.SetParent(null);
        rb.isKinematic = false;
    }
    public Dialogue GetFailThought () => failureMessage;
}
