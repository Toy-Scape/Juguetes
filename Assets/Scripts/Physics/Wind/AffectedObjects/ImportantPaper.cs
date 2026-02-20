using UnityEngine;

public class ImportantPaper : WindAffected
{
    protected override void Awake()
    {
        base.Awake();


        //IA father = FindFirstObjectByType<IA>();
        //OnBlown += father.OnPaperBlown;
    }

    public override void ApplyWind(Vector3 direction, float force)
    {
        base.ApplyWind(direction, force);
    }
}
