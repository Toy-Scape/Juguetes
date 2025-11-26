using UnityEngine;

[CreateAssetMenu(menuName = "Limb/StrongArm")]
public class StrongArmSO : LimbSO
{
    private void OnEnable ()
    {
        LimbName = "Brazo Fuerte";

        secondaryAbility = new AimWithObjectAbility();
        activeAbility = new ThrowObjectAbility();
        passiveAbility = new SuperStrengthPassive();
    }
}

// Secundaria
public class AimWithObjectAbility : ISecondaryAbility
{
    public bool CanExecute (LimbContext context) => true;// context.HeldObject != null;
    public void Execute (LimbContext context)
    {
        Debug.Log($"Apuntando con el brazo fuerte!");
        context.IsAiming = true;
    }
}

public class ThrowObjectAbility : IActiveAbility
{
    public void Execute (LimbContext context)
    {
        //if (context.IsAiming && context.HeldObject != null)
        if (context.IsAiming)
        {
            // Revisar si esto funciona una vez tengamos bien el sistema de agarrar objetos...
            //var rb = context.HeldObject.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    rb.AddForce(Vector3.forward * 10f, ForceMode.Impulse);
            //}

            Debug.Log($"Objeto lanzado!");
            context.HeldObject = null;
            context.IsAiming = false;
        }
    }
}



public class SuperStrengthPassive : IPassiveAbility
{
    public void Apply (LimbContext context)
    {
        context.CanLiftHeavyObjects = true;
    }

    public void Remove (LimbContext context)
    {
        context.CanLiftHeavyObjects = false;
    }
}
