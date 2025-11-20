using UnityEngine;

public class HookArm : Limb, IAimable
{
	public HookArm ()
	{
		LimbName = "Brazo Gancho";
		ActiveAbility = new GrappleAbility();
		SecondaryAbility = new AimAbility(this);
	}

	public void Aim (LimbContext context)
	{
		Debug.Log("Apuntando con el gancho...");
	}

	public void Shoot (LimbContext context)
	{
		Debug.Log("Disparando el gancho!");
		ActiveAbility.Activate(context);
	}
}