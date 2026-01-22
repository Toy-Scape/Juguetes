using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(menuName = "Limb/Limb")]
public class LimbSO : ScriptableObject
{
    // public string LimbName; // Removed to use Localization from ItemData
    public LimbSlot Slot;
    public Sprite LimbIcon;
    public GameObject LimbModel;
    public string LimbNameOnModel => limbNameOnModel;

    [SerializeField] protected ActiveAbilitySO activeAbility;
    [SerializeField] protected SecondaryAbilitySO secondaryAbility;
    [SerializeField] protected PassiveAbilitySO passiveAbility;
    [SerializeField] protected string limbNameOnModel;

    public void OnEquip(LimbContext context)
    {
        passiveAbility?.Apply(context);
    }

    public void OnUnequip(LimbContext context)
    {
        passiveAbility?.Remove(context);
    }

    public void UseActive(LimbContext context)
    {
        Debug.Log($"Using active ability of limb: {name}");
        activeAbility?.Execute(context);
    }

    public void UseSecondary(LimbContext context)
    {
        Debug.Log($"Using active ability of limb: {name}");
        if (secondaryAbility?.CanExecute(context) ?? false)
            secondaryAbility.Execute(context);
    }

    public string SlotName
    {
        get => GetSlotName();
    }

    private string GetSlotName()
    {
        return this.Slot.GetType().GetField(Slot.ToString()).GetCustomAttribute<DescriptionAttribute>().Description;
    }
}
