using UnityEngine;

public class CubeInteractable : AbstractInteractable
{
    public override void Interact()
    {
        Debug.Log("Interact");
    }

    public override bool IsInteractable()
    {
        return true;
    }
}
