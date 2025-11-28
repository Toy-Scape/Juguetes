using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(menuName = "Limb/Limb")]
public class LimbSO : ScriptableObject
{
    public string LimbName;
    public LimbSlot Slot;
    public Sprite LimbIcon;
    public GameObject LimbModel;

    [SerializeField] protected ActiveAbilitySO activeAbility;
    [SerializeField] protected SecondaryAbilitySO secondaryAbility;
    [SerializeField] protected PassiveAbilitySO passiveAbility;

    public void OnEquip (LimbContext context)
    {
        passiveAbility?.Apply(context);
    }

    public void OnUnequip (LimbContext context)
    {
        passiveAbility?.Remove(context);
    }

    public void UseActive (LimbContext context)
    {
        activeAbility?.Execute(context);
    }

    public void UseSecondary (LimbContext context)
    {
        if (secondaryAbility?.CanExecute(context) ?? false)
            secondaryAbility.Execute(context);
    }

    public string SlotName {
        get => GetSlotName();
    }

    private string GetSlotName()
    {
        return this.Slot.GetType().GetField(Slot.ToString()).GetCustomAttribute<DescriptionAttribute>().Description;
    }
}
