using UnityEngine;
using InteractionSystem.Interfaces;

public class Pickable : MonoBehaviour, IPickable 
{ 
    [SerializeField] private Transform gripPoint; 

    private Rigidbody rb;

    private void Awake () 
    {
        rb = GetComponent<Rigidbody>(); 
        if (gripPoint == null) gripPoint = transform; 
    } 

    public void Pick (Transform hand) 
    { 
        rb.isKinematic = true; 
        rb.interpolation = RigidbodyInterpolation.None; 
        
        // Disable all colliders
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        transform.SetParent(hand); 
        
        // Align rotation first, then position
        transform.rotation = hand.rotation; 
        transform.position = hand.position - (gripPoint.position - transform.position); 
    } 

    public void Drop() 
    { 
        transform.SetParent(null); 
        
        // Enable all colliders
        foreach (var collider in GetComponentsInChildren<Collider>())
            collider.enabled = true;

        rb.isKinematic = false; 
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
    } 
}