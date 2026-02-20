using System;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class WindAffected : MonoBehaviour
{
    [Header("Propiedades físicas")]
    public float windSensitivity = 1f; 
    public float liftFactor = 0f;      
    public float turbulence = 0f;

    public bool hasBeenBlown = false;
    public Action<WindAffected> OnBlown;

    Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void ApplyWind(Vector3 direction, float force)
    {
        Debug.Log($"Aplicando viento a {gameObject.name}: Dirección={direction}, Fuerza={force}");
        Vector3 finalForce = direction * force * windSensitivity;

        finalForce += Vector3.up * force * liftFactor;

        if (turbulence > 0f)
            finalForce += UnityRandom.insideUnitSphere * turbulence;


        if (!hasBeenBlown)
        {
            hasBeenBlown = true;
            OnBlown?.Invoke(this);
        }
        rb.AddForce(finalForce, ForceMode.Force);
    }
}
