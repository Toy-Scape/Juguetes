using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform rayOrigin;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineAll;

    private IInteractable focusedInteractable;
    private GameObject focusedGameObject;
    private Dictionary<int, IInteractable> interactableCache = new Dictionary<int, IInteractable>();
    private Outline focusedOutline;

    void Awake()
    {
        Debug.Log("PlayerInteractor Awake. RayOrigin: " + rayOrigin);
        if (rayOrigin == null)
        {
            if (Camera.main != null)
            {
                rayOrigin = Camera.main.transform;
            }
            else
            {
                rayOrigin = transform;
            }
        }
        AbstractInteractable.SetGlobalOutlineProperties(outlineColor, outlineWidth, outlineMode);
    }

    void OnValidate()
    {
        AbstractInteractable.SetGlobalOutlineProperties(outlineColor, outlineWidth, outlineMode);
    }

    void Update()
    {
        CheckForInteractable();
    }

    // Debe ser exactamente así para el Input System
    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log($"OnInteract llamado. Context phase: {context.phase}, performed: {context.performed}, FocusedInteractable: {focusedInteractable}, FocusedGameObject: {focusedGameObject}");
        if (context.performed && focusedInteractable != null)
        {
            Debug.Log($"Interact ejecutado en: {focusedGameObject?.name}");
            focusedInteractable.Interact();
        }
        else if (context.performed)
        {
            Debug.Log("No hay interactable enfocado al intentar interactuar.");
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;
        bool foundInteractable = false;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            int id = hitObject.GetInstanceID();
            IInteractable interactable;
            if (!interactableCache.TryGetValue(id, out interactable))
            {
                interactable = FindInteractableInParents(hitObject);
                interactableCache[id] = interactable;
                Debug.Log($"Interactable cacheado: {hitObject.name} ({id}) => {interactable}");
            }

            if (interactable != null && interactable.IsInteractable())
            {
                foundInteractable = true;
                if (focusedGameObject != hitObject)
                {
                    Debug.Log($"Nuevo interactable enfocado: {hitObject.name}");
                    ClearOutline();
                    focusedInteractable = interactable;
                    focusedGameObject = hitObject;
                    ApplyOutline(focusedGameObject);
                }
            }
            else if (interactable != null)
            {
                Debug.Log($"Interactable encontrado pero no interactuable: {hitObject.name}");
            }
        }

        if (!foundInteractable && focusedGameObject != null)
        {
            Debug.Log("Interactable enfocado perdido, limpiando outline.");
            ClearOutline();
        }
    }

    private IInteractable FindInteractableInParents(GameObject start)
    {
        Transform t = start.transform;
        while (t != null)
        {
            if (t.TryGetComponent<IInteractable>(out var interactable))
            {
                return interactable;
            }
            t = t.parent;
        }
        return null;
    }

    private void ApplyOutline(GameObject target)
    {
        if (target == null) return;
        focusedOutline = target.GetComponent<Outline>();
        if (focusedOutline == null)
        {
            Debug.LogWarning($"El objeto {target.name} no tiene componente Outline. Añádelo manualmente para personalizar el borde desde el inspector.");
            return;
        }
        focusedOutline.enabled = true;
    }

    private void ClearOutline()
    {
        if (focusedOutline != null)
        {
            focusedOutline.enabled = false;
            focusedOutline = null;
        }
        focusedInteractable = null;
        focusedGameObject = null;
    }

    public float InteractionDistance => interactionDistance;
    public Transform RayOrigin => rayOrigin;
}
