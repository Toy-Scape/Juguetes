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

    private FixedJoint joint;

    public void Pick(Transform hand)
    {
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Do NOT disable collider
        
        // Align to hand
        transform.position = hand.position - (gripPoint.position - transform.position);
        transform.rotation = hand.rotation;

        // Create joint
        if (joint != null) Destroy(joint);
        joint = gameObject.AddComponent<FixedJoint>();
        
        Rigidbody handRb = hand.GetComponent<Rigidbody>();
        if (handRb != null)
        {
            joint.connectedBody = handRb;
        }
        else
        {
            // Fallback if hand has no RB (should not happen with GrabInteractor update)
            joint.connectedAnchor = hand.position;
        }
    }

    public void Drop()
    {
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

        // Collider is already enabled
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
}
