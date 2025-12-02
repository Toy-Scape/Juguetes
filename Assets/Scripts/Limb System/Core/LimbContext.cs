using UnityEngine;

public class LimbContext
{
    public BoolVariableSO CanLiftHeavyObjectsVar { get; set; }
    public BoolVariableSO CanClimbWallsVar { get; set; }
    public BoolVariableSO CanSwimVar { get; set; }
    public BoolVariableSO IsAimingVar { get; set; }

    // Campos internos
    private bool canLiftHeavyObjects;
    private bool canClimbWalls;
    private bool canSwim;
    private bool isAiming;
    private GameObject heldObject;

    public bool CanLiftHeavyObjects
    {
        get => canLiftHeavyObjects;
        set
        {
            canLiftHeavyObjects = value;
            if (CanLiftHeavyObjectsVar != null)
                CanLiftHeavyObjectsVar.Value = value;
        }
    }

    public bool CanClimbWalls
    {
        get => canClimbWalls;
        set
        {
            canClimbWalls = value;
            if (CanClimbWallsVar != null)
                CanClimbWallsVar.Value = value;
        }
    }

    public bool CanSwim
    {
        get => canSwim;
        set
        {
            canSwim = value;
            if (CanSwimVar != null)
                CanSwimVar.Value = value;
        }
    }

    public bool IsAiming
    {
        get => isAiming;
        set
        {
            isAiming = value;
            if (IsAimingVar != null)
                IsAimingVar.Value = value;
        }
    }

    public GameObject HeldObject
    {
        get => heldObject;
        set => heldObject = value;
    }

    public void Reset()
    {
        IsAiming = false;
        HeldObject = null;
        CanLiftHeavyObjects = false;
        CanClimbWalls = false;
        CanSwim = false;
    }
}
