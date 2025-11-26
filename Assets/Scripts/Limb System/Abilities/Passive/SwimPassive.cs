[System.Serializable]
public class SwimPassive : IPassiveAbility
{
    public void Apply (LimbContext context) => context.CanSwim = true;
    public void Remove (LimbContext context) => context.CanSwim = false;
}
