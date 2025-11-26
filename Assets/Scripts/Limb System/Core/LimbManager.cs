using UnityEngine;
using System.Collections.Generic;

public class LimbManager : MonoBehaviour
{
    private LimbContext context = new();
    [SerializeField] private List<LimbSO> availableLimbs;

    private LimbSO equippedLimb;

    public void EquipLimb (LimbSO newLimb)
    {
        context.Reset();
        if (equippedLimb != null)
        {
            equippedLimb.OnUnequip(context);
        }

        equippedLimb = newLimb;
        equippedLimb.OnEquip(context);

        Debug.Log($"Equipada extremidad: {equippedLimb.LimbName}");
    }

    public void UseActive () => equippedLimb?.UseActive(context);
    public void UseSecondary () => equippedLimb?.UseSecondary(context);

    public LimbContext GetContext () => context;
    public LimbSO GetEquippedLimb () => equippedLimb;
    public List<LimbSO> GetAvailableLimbs () => availableLimbs;
}
