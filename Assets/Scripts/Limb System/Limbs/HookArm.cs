using UnityEngine;

[CreateAssetMenu(menuName = "Limb/HookArm")]
public class HookArmSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Brazo Gancho";

        secondaryAbility = new AimAbility();
        activeAbility = new ShootHookAbility();
    }
}

public class AimAbility : ISecondaryAbility
{
    public bool CanExecute (LimbContext context) => true;
    public void Execute (LimbContext context)
    {
        context.IsAiming = true;
        Debug.Log("Apuntando gancho!");

    }
}

public class ShootHookAbility : IActiveAbility
{
    public void Execute (LimbContext context)
    {
        if (context.IsAiming)
        {
            Debug.Log("Disparando gancho!");
        }
    }
}
