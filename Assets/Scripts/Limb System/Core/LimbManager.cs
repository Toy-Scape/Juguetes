using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inventory;

public class LimbManager : MonoBehaviour
{
    [System.Serializable]
    public struct LimbSocketDefinition
    {
        public LimbSlot Slot;
        public Transform SocketTransform;
        public LimbSO DefaultLimb;
    }

    private LimbContext context = new();
    
    [SerializeField] private LimbSO DefaultLimb;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private List<LimbSocketDefinition> limbSockets;

    private LimbSO equippedLimb;
    private Dictionary<LimbSlot, GameObject> spawnedLimbModels = new();

    private void Awake()
    {
        RefreshVisuals();
    }

    public void EquipLimb (LimbSO newLimb)
    {
        if (newLimb == null) return;

        // Unequip previous if exists
        if (equippedLimb != null)
        {
            equippedLimb.OnUnequip(context);
            context.Reset();
        }

        equippedLimb = newLimb;
        equippedLimb.OnEquip(context);

        RefreshVisuals();

        Debug.Log($"Equipada extremidad: {equippedLimb.LimbName} en {equippedLimb.SlotName}");
    }

    private void RefreshVisuals()
    {
        foreach (var socket in limbSockets)
        {
            LimbSO limbForSocket = socket.DefaultLimb;

            if (equippedLimb != null && equippedLimb.Slot == socket.Slot)
            {
                limbForSocket = equippedLimb;
            }

            UpdateSocketModel(socket, limbForSocket);
        }
    }

    private void UpdateSocketModel(LimbSocketDefinition socket, LimbSO limb)
    {
        if (spawnedLimbModels.ContainsKey(socket.Slot))
        {
            if (spawnedLimbModels[socket.Slot] != null)
            {
                Destroy(spawnedLimbModels[socket.Slot]);
            }
            spawnedLimbModels.Remove(socket.Slot);
        }

        LimbSO limbToSpawn = limb != null ? limb : socket.DefaultLimb;

        if (limbToSpawn != null && limbToSpawn.LimbModel != null && socket.SocketTransform != null)
        {
            var model = Instantiate(limbToSpawn.LimbModel, socket.SocketTransform);
            spawnedLimbModels[socket.Slot] = model;
        }
    }

    public void UseActive () => equippedLimb?.UseActive(context);
    public void UseSecondary () => equippedLimb?.UseSecondary(context);

    public LimbContext GetContext () => context;
    public LimbSO GetEquippedLimb () => equippedLimb;
    public List<LimbSO> GetAvailableLimbs () => inventory.GetAllLimbs().Select(p => p.LimbSO).Prepend(DefaultLimb).ToList();
}
