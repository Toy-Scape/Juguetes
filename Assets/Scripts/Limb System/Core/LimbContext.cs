using System;
using UnityEngine;

public class LimbContext
{
    public bool IsAiming { get; set; }
    public GameObject HeldObject { get; set; } 

    public bool CanLiftHeavyObjects { get; set; }
    public bool CanClimbWalls { get; set; }
    public bool CanSwim { get; set; }

    internal void Reset ()
    {
        IsAiming = false;
        HeldObject = null;
        CanLiftHeavyObjects = false;
        CanClimbWalls = false;
        CanSwim = false;

    }
}
