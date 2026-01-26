using System.Collections.Generic;
using System.Linq;
using Inventory;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
    [System.Serializable]
    public struct LimbSocketDefinition
    {
        public LimbSlot Slot;
        public Transform SocketTransform;
        public LimbSO DefaultLimb;
    }

    [SerializeField] private LimbSO DefaultLimb;
    [SerializeField] private ItemData DefaultLimbData;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private List<LimbSocketDefinition> limbSockets;

    private LimbContext context;
    private LimbSO equippedLimb;
    private Dictionary<LimbSlot, GameObject> spawnedLimbModels = new();

    private void Awake()
    {
        // El contexto ya no depende de ScriptableObjects
        context = GetComponent<LimbContext>();

        // Equipamos la extremidad por defecto
        equippedLimb = DefaultLimb;

        // Activamos el modelo inicial si existe
        this.transform.FindDeep(DefaultLimb.LimbNameOnModel)?.gameObject.SetActive(true);
    }

    public void EquipLimb(LimbSO newLimb)
    {
        if (newLimb == null) return;

        // Desactivar la extremidad anterior
        if (equippedLimb != null)
        {
            equippedLimb.OnUnequip(context);
            this.transform.FindDeep(equippedLimb.LimbNameOnModel)?.gameObject.SetActive(false);
        }

        // Equipar la nueva
        equippedLimb = newLimb;
        equippedLimb.OnEquip(context);
        this.transform.FindDeep(equippedLimb.LimbNameOnModel)?.gameObject.SetActive(true);
    }

    public void UseActive() => equippedLimb?.UseActive(context);
    public void UseSecondary() => equippedLimb?.UseSecondary(context);

    public LimbContext GetContext() => context;
    public LimbSO GetEquippedLimb() => equippedLimb;
    public ItemData GetDefaultLimbData() => DefaultLimbData;

    public List<LimbSO> GetAvailableLimbs() =>
        inventory.GetAllLimbs()
                 .Select(p => p.LimbSO)
                 .Prepend(DefaultLimb)
                 .ToList();
}
