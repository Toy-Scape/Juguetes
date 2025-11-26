using UnityEngine;

[CreateAssetMenu(menuName = "Limb/NormalArm")]
public class NormalArmSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Brazo";
    }
}