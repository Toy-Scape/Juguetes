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
    Vector3 basePosition;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // 🔥 Guardamos la rotación correcta (de pie)
        initialRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        // Dirección por defecto = vertical
        actualFallDirection = fallDirection.normalized;
    }

    void FixedUpdate()
    {
        // 🔥 FORZAR SIEMPRE ROTACIÓN
        transform.rotation = initialRotation;

        if (isFalling && !hasLanded)
{
            Vector3 velocity = new Vector3(1f, -fallSpeed, 0f);

            rb.linearVelocity = velocity;
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
        rb.WakeUp();

        transform.rotation = initialRotation;
        basePosition = transform.position;

        // 🔥 OFFSET AQUÍ (SOLO CUANDO EMPIEZA LA CAÍDA)
        if (useForwardOffset)
        {
            Vector3 offset =
                transform.forward * forwardOffset.z +
                transform.up * forwardOffset.y +
                transform.right * forwardOffset.x;

            transform.position = basePosition + offset;
        }
        actualFallDirection = Vector3.down;

        OnBlown?.Invoke(this);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
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
        rb.useGravity = false;

        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.rotation = initialRotation;

        actualFallDirection = Vector3.zero;

        rb.Sleep();

        // 🔥 CLAVE: convertirlo en objeto "estático real"
        rb.interpolation = RigidbodyInterpolation.None;
    }

    public void ResetBlownState()
    {
        hasBeenBlown = false;
        isFalling = false;
        hasLanded = false;

        rb.isKinematic = true;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.constraints = RigidbodyConstraints.FreezeAll;

        transform.rotation = initialRotation;

        actualFallDirection = fallDirection.normalized;

        rb.Sleep();
    }
}