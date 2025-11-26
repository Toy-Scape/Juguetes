using UnityEngine;
using System.Collections.Generic;

public class LimbManager : MonoBehaviour
{
    [SerializeField] private LimbContextSO limbContextSO;
    [SerializeField] private List<LimbSO> availableLimbs;   // ahora son ScriptableObjects

    private LimbSO equippedLimb;

    public void EquipLimb (LimbSO newLimb)
    {
        if (equippedLimb != null)
            equippedLimb.OnUnequip(limbContextSO.context);

        equippedLimb = newLimb;
        equippedLimb.OnEquip(limbContextSO.context);

        Debug.Log($"Equipada extremidad: {equippedLimb.LimbName}");
    }

    public LimbSO GetEquippedLimb () => equippedLimb;
    public List<LimbSO> GetAvailableLimbs () => availableLimbs;
}
