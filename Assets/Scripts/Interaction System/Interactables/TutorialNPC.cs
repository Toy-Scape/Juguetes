using InteractionSystem.Interactables;
using InteractionSystem.Core;
using Inventory;
using UnityEngine;

public class TutorialNPC : NPCInteractableBase
{
    PlayerInventory playerInventory;
    [SerializeField] ItemData limbItemData;

    [SerializeField] GameObject LeftArm;
    [SerializeField] GameObject RightArm;

    public override void Interact(InteractContext context)
    {
        base.Interact(context);

        if (context.PlayerInventory != null && !context.PlayerInventory.Contains(limbItemData))
        {
            AddLimbToInventory(context.PlayerInventory);
            LeftArm.SetActive(false);
            RightArm.SetActive(false);
        }
    }

    private void AddLimbToInventory(PlayerInventory playerInventory)
    {
        Debug.Log("Adding limb");
        playerInventory.AddItem(limbItemData, 1);
    }
}
