using UnityEngine;
using System.Collections.Generic;

public class LimbManager : MonoBehaviour
{
    //[SerializeField] private LimbContext limbContext;
    [SerializeField] private LimbContextSO limbContextSO;
    [SerializeField] private List<Limb> availableLimbs; 


    private Limb equippedLimb;

    public void EquipLimb (Limb newLimb)
    {
        if (equippedLimb != null)
            equippedLimb.OnUnequip(limbContextSO.context);

        equippedLimb = newLimb;
        equippedLimb.OnEquip(limbContextSO.context);

        Debug.Log($"Equipada extremidad: {equippedLimb.LimbName}");
    }

    public Limb GetEquippedLimb () => equippedLimb;
    public List<Limb> GetAvailableLimbs () => availableLimbs;
}
