using UnityEngine;
using InteractionSystem.Interfaces;

namespace InteractionSystem.Core
{
    [RequireComponent(typeof(Outline))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        protected Outline outline;

        protected virtual void Awake ()
        {
            outline = GetComponent<Outline>();

            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }

            outline.enabled = false;
        }

        public static void SetGlobalOutlineProperties (Color color, float width, Outline.Mode mode)
        {
            var interactables = FindObjectsByType<InteractableBase>(FindObjectsSortMode.None);

            foreach (var interactable in interactables)
            {
                if (interactable.outline == null)
                {
                    interactable.outline = interactable.GetComponent<Outline>();
                }

                interactable.outline.OutlineColor = color;
                interactable.outline.OutlineWidth = width;
                interactable.outline.OutlineMode = mode;
            }
        }

        public abstract void Interact (InteractContext context);

        public abstract bool IsInteractable ();
    }
}
