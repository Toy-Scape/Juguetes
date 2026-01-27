using UnityEngine;

public abstract class AutoDestroyBase : MonoBehaviour
{
    public bool autoStart = true;

    protected virtual void Awake ()
    {
        if (autoStart)
            BeginDestroy();
    }

    public abstract void BeginDestroy ();
}
