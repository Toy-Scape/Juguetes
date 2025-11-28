using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    
    [SerializeField] private List<LimbSO> availableLimbs;
    [SerializeField] private List<LimbSocketDefinition> limbSockets;

    private LimbSO equippedLimb;
    private Dictionary<LimbSlot, GameObject> spawnedLimbModels = new();

    private void Awake()
    {
        // Initialize with defaults, but we don't really "equip" a specific one as active yet?
        // Or maybe we treat the first default as active?
        // Actually, if nothing is equipped, we just show defaults.
        // Let's just spawn defaults visually.
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
            // Determine which limb goes in this socket
            LimbSO limbForSocket = socket.DefaultLimb;

            // If the equipped limb belongs to this socket, use it instead
            if (equippedLimb != null && equippedLimb.Slot == socket.Slot)
            {
                limbForSocket = equippedLimb;
            }

            // Spawn the model
            UpdateSocketModel(socket, limbForSocket);
        }
    }

    private void UpdateSocketModel(LimbSocketDefinition socket, LimbSO limb)
    {
        // Remove existing model for this slot
        if (spawnedLimbModels.ContainsKey(socket.Slot))
        {
            if (spawnedLimbModels[socket.Slot] != null)
            {
                Destroy(spawnedLimbModels[socket.Slot]);
            }
            spawnedLimbModels.Remove(socket.Slot);
        }

        // Instantiate new model
        // If limb is null (shouldn't happen if defaults are set), use default
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
    public List<LimbSO> GetAvailableLimbs () => availableLimbs;
}
