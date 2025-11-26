using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Limb")]
public class LimbSO : ScriptableObject
{
    public string LimbName;

    [SerializeField] protected IActiveAbility activeAbility;
    [SerializeField] protected ISecondaryAbility secondaryAbility;
    [SerializeField] protected IPassiveAbility passiveAbility;

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
}
