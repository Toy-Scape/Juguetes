using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WindAffected : MonoBehaviour
{
    [Header("Propiedades físicas")]
    public float windSensitivity = 1f; 
    public float liftFactor = 0f;      
    public float turbulence = 0f;      

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyWind(Vector3 direction, float force)
    {
        Debug.Log($"Aplicando viento a {gameObject.name}: Dirección={direction}, Fuerza={force}");
        Vector3 finalForce = direction * force * windSensitivity;

        finalForce += Vector3.up * force * liftFactor;

        if (turbulence > 0f)
            finalForce += Random.insideUnitSphere * turbulence;

        rb.AddForce(finalForce, ForceMode.Force);
    }
}
