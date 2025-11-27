using InteractionSystem.Interfaces;
using UnityEngine;

public class Pickable : MonoBehaviour, IPickable
{
     private Rigidbody rb;
    [SerializeField] private Transform gripPoint; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (gripPoint == null)
            gripPoint = transform; 
    }

    public void Pick(Transform hand)
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;
        GetComponent<Collider>().enabled = false;

        transform.SetParent(hand);

        transform.position = hand.position - (gripPoint.position - transform.position);
        transform.rotation = hand.rotation;
    }

    public void Drop()
    {
        transform.SetParent(null);

        GetComponent<Collider>().enabled = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
}
