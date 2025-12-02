using InteractionSystem.Core;

namespace InteractionSystem.Interfaces
{
    public interface IInteractable
    {
        void Interact(InteractContext context);

        bool IsInteractable();
    }
}
