using UnityEngine;

[CreateAssetMenu(menuName = "Limb/Context Variables")]
public class ContextVariablesSO : ScriptableObject
{
    public BoolVariableSO canLiftHeavyObjectsVar;
    public BoolVariableSO canClimbWallsVar;
    public BoolVariableSO canSwimVar;
    public BoolVariableSO isAimingVar;
}
