using InteractionSystem.Core;
using InteractionSystem.Interactables;
using Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Interaction_System.Interactables
{
    public class VentilationGrateInteractable : NPCInteractableBase
    {
        [SerializeField] ItemData itemData;
        PlayerInventory playerInventory;
        private SceneManager sceneManager;
    
        public override void Interact(InteractContext context)
        {
            if (context.PlayerInventory != null && context.PlayerInventory.Contains(itemData))
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                base.Interact(context);
            }
        }
        
    }
}
