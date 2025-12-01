using InteractionSystem.Interactables;
using Inventory;
using UnityEngine;

public class TutorialNPC : NPCInteractableBase
{
    PlayerInventory playerInventory;
    [SerializeField] ItemData limbItemData;
    
    void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }
    public override void Interact()
    {
        base.Interact();

        AddLimbToInventory();
    }

    private void AddLimbToInventory()
    {
        Debug.Log("Adding limb");
        playerInventory.AddItem(limbItemData, 1);
    }
}
