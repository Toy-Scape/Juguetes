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
    
        void Start()
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        public override void Interact()
        {
            if (playerInventory.Contains(itemData))
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                base.Interact();
            }
        }
        
    }
}
