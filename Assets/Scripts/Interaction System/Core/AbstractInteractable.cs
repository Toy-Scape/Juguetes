using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Outline))]
public abstract class AbstractInteractable : MonoBehaviour, IInteractable
{
    protected Outline outline;

    protected virtual void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.enabled = false;
    }

    public static void SetGlobalOutlineProperties(Color color, float width, Outline.Mode mode)
    {
        AbstractInteractable[] interactables = FindObjectsOfType<AbstractInteractable>();
        foreach (var interactable in interactables)
        {
            if (interactable.outline == null)
                interactable.outline = interactable.GetComponent<Outline>();
            interactable.outline.OutlineColor = color;
            interactable.outline.OutlineWidth = width;
            interactable.outline.OutlineMode = mode;
        }
    }

    public abstract void Interact();
    public abstract bool IsInteractable();
}
