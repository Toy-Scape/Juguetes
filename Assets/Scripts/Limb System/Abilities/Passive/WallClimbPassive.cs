[System.Serializable]
public class ClimbWallsPassive : IPassiveAbility
{
    public void Apply (LimbContext context) => context.CanClimbWalls = true;
    public void Remove (LimbContext context) => context.CanClimbWalls = false;
}
