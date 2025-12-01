using InteractionSystem.Core;
using InteractionSystem.Interactables;
using Inventory;
using UnityEngine;

namespace Interaction_System.Interactables
{
    public class VentilationGrateInteractable : NPCInteractableBase
    {
        [SerializeField] ItemData itemData;
        PlayerInventory playerInventory;
    
        void Start()
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        public override void Interact()
        {
            if (playerInventory.Contains(itemData))
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                base.Interact();
            }
        }
        
    }
}
