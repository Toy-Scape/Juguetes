using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WindAffectedSimpleFall : MonoBehaviour
{
    public float fallSpeed = 5f;
    public Vector3 fallDirection = Vector3.down;

    [Header("Wind push offset")]
    public Vector3 forwardOffset = new Vector3(0, 0, 1f); // cuánto se separa de la pared
    public bool useForwardOffset = true;

    public bool hasBeenBlown = false;
    public Action<WindAffectedSimpleFall> OnBlown;

    private Rigidbody rb;
    private bool isFalling = false;
    private bool hasLanded = false;

    private Quaternion initialRotation;
    private Vector3 actualFallDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 🔥 Guardamos la rotación correcta (de pie)
        initialRotation = transform.rotation;

        // Dirección por defecto = vertical
        actualFallDirection = fallDirection.normalized;
    }

    void FixedUpdate()
    {
        // 🔥 FORZAR SIEMPRE ROTACIÓN
        transform.rotation = initialRotation;

        if (isFalling && !hasLanded)
        {
            rb.linearVelocity = actualFallDirection * fallSpeed;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ApplyWind(Vector3 direction, float force)
    {
        if (hasBeenBlown) return;

        hasBeenBlown = true;
        isFalling = true;

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 🔥 FORZAR ROTACIÓN CORRECTA
        transform.rotation = initialRotation;

        // 🔹 Ajustamos la dirección: vertical + pequeño push hacia adelante
        if (useForwardOffset)
        {
            actualFallDirection = (fallDirection + forwardOffset).normalized;
        }
        else
        {
            actualFallDirection = fallDirection.normalized;
        }

        OnBlown?.Invoke(this);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            Land();
        }
    }

    void Land()
    {
        hasLanded = true;
        isFalling = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.rotation = initialRotation;
    }

    public void ResetBlownState()
    {
        hasBeenBlown = false;
        isFalling = false;
        hasLanded = false;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.rotation = initialRotation;

        // 🔹 volver a caída vertical
        actualFallDirection = fallDirection.normalized;
    }
}