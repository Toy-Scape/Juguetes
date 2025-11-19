using UnityEngine;
using Inventory.Core;
using Inventory.Services;
using System.Collections;

public class InventoryHolder : MonoBehaviour
{
    public IInventory Inventory { get; private set; }
    public InventoryService Service { get; private set; }

    void Awake()
    {
        Inventory = new Inventory.Core.Inventory();
        Service = new InventoryService(Inventory);
    }

    /// <summary>
    /// Intenta añadir el item al inventario de este holder.
    /// Si se añade, gestiona el GameObject source:
    /// - respawnTime <= 0 => Destroy(source)
    /// - respawnTime > 0 => SetActive(false), esperar y SetActive(true)
    /// Devuelve true si el item fue añadido al inventario.
    /// </summary>
    public bool RequestPickup(GameObject source, Item item, float respawnTime = 0f)
    {
        if (source == null || item == null) return false;

        bool added = Service.PickUp(item);
        if (!added) return false;

        if (respawnTime <= 0f)
        {
            Destroy(source);
        }
        else
        {
            StartCoroutine(DisableAndRespawn(source, respawnTime));
        }

        return true;
    }

    private IEnumerator DisableAndRespawn(GameObject go, float seconds)
    {
        if (go == null) yield break;
        var initialActive = go.activeSelf;
        go.SetActive(false);
        yield return new WaitForSeconds(seconds);
        if (go != null)
            go.SetActive(initialActive);
    }
}
