public class StrongArmShootAbility : ShootAbility
{
    public override bool CanShoot (LimbContext context, AbilityState state)
        => context.CanGrabObjects && state.HasObjectInHand;

    public override void Shoot (LimbContext context, AbilityState state)
    {
        ThrowObject();
        state.HasObjectInHand = false;
    }

    private void ThrowObject ()
    {
        // lógica de lanzamiento
    }
}
