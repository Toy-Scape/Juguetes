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
        [SerializeField] string NextLevelName = "Level_01_Office";

        PlayerInventory playerInventory;
        private SceneManager sceneManager;
    
        public override void Interact(InteractContext context)
        {
            if (context.PlayerInventory != null && context.PlayerInventory.Contains(itemData))
            {
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                SceneManager.LoadScene(NextLevelName);
            }
            else
            {
                base.Interact(context);
            }
        }
        
    }
}
