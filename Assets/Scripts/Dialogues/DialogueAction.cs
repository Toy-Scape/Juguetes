using UnityEngine;

[System.Serializable]
public class DialogueAction
{
    [SerializeField] private TriggerTiming timing;
    [SerializeField] private ActionBase action;

    public TriggerTiming Timing => timing;
    public ActionBase Action => action;

    public void Execute (DialogueContext context)
    {
        action?.Execute(context);
    }
}