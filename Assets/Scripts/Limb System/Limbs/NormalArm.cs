using UnityEngine;

[CreateAssetMenu(menuName = "Limb/NormalArm")]
public class NormalArmSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Brazo";
    }

    public override void OnEquip (LimbContext context)
    {
        base.OnEquip(context);
        Debug.Log("Equipado brazo normal");
    }

    public override void OnUnequip (LimbContext context)
    {
        base.OnUnequip(context);
        Debug.Log("Desequipado brazo normal");
    }
}