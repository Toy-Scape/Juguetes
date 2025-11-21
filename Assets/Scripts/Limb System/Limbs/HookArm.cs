using UnityEngine;

[CreateAssetMenu(menuName = "Limb/HookArm")]
public class HookArmSO : AimableLimbSO
{
    private void OnEnable ()
    {
        LimbName = "Brazo Gancho";

        ActiveAbility = new GrappleAbility();
        SecondaryAbility = new AimAbility(this);
    }
}
