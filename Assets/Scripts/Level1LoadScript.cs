using Inventory;
using UnityEngine;

public class Level1LoadScript : MonoBehaviour
{
    [SerializeField] private ItemData StrongArm;
    [SerializeField] private ItemData Screwdriver;

    void Start()
    {
        var inventory = FindFirstObjectByType<PlayerInventory>();
        inventory.AddItemSilent(StrongArm);
        inventory.AddItemSilent(Screwdriver);

        var player = FindFirstObjectByType<PlayerController>();
        if (player)
            player.gameObject.GetComponent<Animator>().SetBool("SkipStandUp", true);

    }
}
