using UnityEngine;

public class AutoDestroy : AutoDestroyBase
{
    public float lifetime = 2f;

    public override void BeginDestroy ()
    {
        Destroy(gameObject, lifetime);
    }
}
