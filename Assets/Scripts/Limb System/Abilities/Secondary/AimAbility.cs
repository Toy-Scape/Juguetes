public class AimAbility : ISecondaryAbility
{
    private readonly IAimable aimable;

    public AimAbility (IAimable aimable)
    {
        this.aimable = aimable;
    }

    public void Perform (LimbContext context)
    {
        aimable.Aim(context);
    }

    public void Shoot (LimbContext context)
    {
        aimable.Shoot(context);
    }
}
