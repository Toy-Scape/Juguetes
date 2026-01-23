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

    [SerializeField] private ContextVariablesSO contextVariables;
    [SerializeField] private LimbSO DefaultLimb;
    [SerializeField] private ItemData DefaultLimbData; // For Localization Name
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private List<LimbSocketDefinition> limbSockets;

    private LimbContext context;
    private LimbSO equippedLimb;
    private Dictionary<LimbSlot, GameObject> spawnedLimbModels = new();

    private void Awake()
    {
        contextVariables.canLiftHeavyObjectsVar.Value = false;
        contextVariables.canClimbWallsVar.Value = false;
        contextVariables.canSwimVar.Value = false;
        contextVariables.isAimingVar.Value = false;

        context = new LimbContext
        {
            CanLiftHeavyObjectsVar = contextVariables.canLiftHeavyObjectsVar,
            CanClimbWallsVar = contextVariables.canClimbWallsVar,
            CanSwimVar = contextVariables.canSwimVar,
            IsAimingVar = contextVariables.isAimingVar
        };
        equippedLimb = DefaultLimb;
    }

    public void EquipLimb(LimbSO newLimb)
    {
        if (newLimb == null) return;

        if (equippedLimb != null)
        {
            equippedLimb.OnUnequip(context);
            this.transform.FindDeep(equippedLimb.LimbNameOnModel)?.gameObject.SetActive(false);
            context.Reset();
        }

        equippedLimb = newLimb;
        equippedLimb.OnEquip(context);
        this.transform.FindDeep(equippedLimb.LimbNameOnModel)?.gameObject.SetActive(true);

    }

    public void UseActive() => equippedLimb?.UseActive(context);
    public void UseSecondary() => equippedLimb?.UseSecondary(context);

    public LimbContext GetContext() => context;
    public LimbSO GetEquippedLimb() => equippedLimb;
    public ItemData GetDefaultLimbData() => DefaultLimbData;
    public List<LimbSO> GetAvailableLimbs() => inventory.GetAllLimbs().Select(p => p.LimbSO).Prepend(DefaultLimb).ToList();
}
