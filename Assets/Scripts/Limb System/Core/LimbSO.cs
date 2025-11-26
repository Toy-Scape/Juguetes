using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Toy/Limb")]
public class LimbSO : ScriptableObject
{
    [field: SerializeField] public string LimbName { get; private set; }
    [field: SerializeField] public IAbility ActiveAbility { get; private set; }
    [field: SerializeField] public ISecondaryAbility SecondaryAbility { get; private set; }
    [field: SerializeField] public List<IPassiveAbility> PassiveAbilities { get; private set; }

    public void OnEquip (LimbContext context)
    {
        foreach (var passive in PassiveAbilities)
            passive.Apply(context);
    }

    public void OnUnequip (LimbContext context)
    {
        foreach (var passive in PassiveAbilities)
            passive.Remove(context);
    }
}