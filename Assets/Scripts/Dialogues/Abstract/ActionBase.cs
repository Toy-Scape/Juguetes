using UnityEngine;

public abstract class ActionBase : ScriptableObject
{
    public abstract void Execute (DialogueContext context);
}