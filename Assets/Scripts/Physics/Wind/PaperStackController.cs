using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(WindAffected))]
public class PaperStackController : MonoBehaviour
{
    [Header("Eventos")]
    public UnityEvent onPapersBlown;

    [Header("Referencias de pila")]
    [SerializeField] GameObject paperStack;
    [SerializeField] GameObject smallPaperStack;

    [Header("Spawn de hojas")]
    [SerializeField] GameObject paperPrefab;
    [SerializeField] int maxHojas = 20;
    [SerializeField] float intervalo = 0.2f;
    [SerializeField] int hojasAntesDeReducir = 5;

    [Header("Fuerza de viento")]
    [SerializeField] float fuerzaHorizontal = 3f;
    [SerializeField] float fuerzaVertical = 2f;

    private WindAffected windAffected;
    private bool alreadyTriggered = false;
    private Collider pilaCollider;

    void Awake()
    {
        windAffected = GetComponent<WindAffected>();
        if (paperStack != null)
            pilaCollider = paperStack.GetComponent<Collider>();
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
        onPapersBlown?.Invoke();
        StartCoroutine(SpawnHojas());
    }

    IEnumerator SpawnHojas()
    {
        GetComponent<Collider>().enabled = false;
        if (paperStack != null)
        {
            Collider stackCollider = paperStack.GetComponentInChildren<Collider>();
            if (stackCollider != null)
                stackCollider.enabled = false;
        }

        int hojasSpawned = 0;
        bool pilaReducida = false;

        while (hojasSpawned < maxHojas)
        {
            Vector3 spawnPos = GetTopOfCurrentStack();

            GameObject hoja = Instantiate(
                paperPrefab,
                spawnPos,
                Random.rotation,
                transform
            );

            Rigidbody rb = hoja.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 fuerza = transform.forward * fuerzaHorizontal
                               + Vector3.up * fuerzaVertical;
                rb.AddForce(fuerza, ForceMode.Impulse);
            }

            hojasSpawned++;

            if (!pilaReducida && hojasSpawned >= hojasAntesDeReducir)
            {
                pilaReducida = true;

                if (paperStack != null)
                    paperStack.SetActive(false);

                if (smallPaperStack != null)
                    smallPaperStack.SetActive(true);
            }

            yield return new WaitForSeconds(intervalo);
        }

        if (smallPaperStack != null)
        {
            Collider smallCollider = smallPaperStack.GetComponent<Collider>();
            if (smallCollider != null)
                smallCollider.enabled = true;
        }
    }

    Vector3 GetTopOfCurrentStack()
    {
        GameObject pilaActual = paperStack.activeSelf ? paperStack : smallPaperStack;

        if (pilaActual == null)
            return transform.position;

        Renderer rend = pilaActual.GetComponentInChildren<Renderer>();
        if (rend != null)
            return new Vector3(rend.bounds.center.x, rend.bounds.max.y, rend.bounds.center.z);

        return pilaActual.transform.position;
    }
}