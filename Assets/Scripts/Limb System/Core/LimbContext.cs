public class LimbContext
{
    public bool StrongArmActive { get; set; }
    public bool HookArmActive { get; set; }
    public bool TentaclesActive { get; set; }

    public bool CanClimbWalls { get; set; }
    public bool CanSwim { get; set; }


    public void ResetContext ()
    {
        StrongArmActive = false;
        HookArmActive = false;
        TentaclesActive = false;
        CanClimbWalls = false;
        CanSwim = false;
    }
}
