public abstract class ShootAbility : IAbility, IAimable
{
    public abstract bool CanShoot (LimbContext context);
    public abstract void Shoot (LimbContext context);

    public void Execute (LimbContext context)
    {
        if (CanShoot(context))
            Shoot(context);
    }

    public virtual void StartAiming (LimbContext context) => context.IsAiming = true;
    public virtual void StopAiming (LimbContext context) => context.IsAiming = false;
}
