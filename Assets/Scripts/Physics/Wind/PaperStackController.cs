using UnityEngine;

[RequireComponent(typeof(WindAffected))]
public class PaperStackController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Modelo compacto de la pila")]
    public GameObject pilaVisual;

    [Tooltip("Contenedor con las hojas individuales (debe empezar desactivado)")]
    public GameObject hojasContainer;

    private WindAffected windAffected;
    private bool alreadyTriggered = false;

    void Awake()
    {
        windAffected = GetComponent<WindAffected>();
    }

    void OnEnable()
    {
        windAffected.OnBlown += HandleBlown;
    }

    void OnDisable()
    {
        windAffected.OnBlown -= HandleBlown;
    }

    void HandleBlown(WindAffected obj)
    {
        if (alreadyTriggered) return;
        alreadyTriggered = true;

        Debug.Log("La pila fue soplada. Activando hojas...");

        if (pilaVisual != null)
            pilaVisual.SetActive(false);

        if (hojasContainer != null)
            hojasContainer.SetActive(true);
    }
}