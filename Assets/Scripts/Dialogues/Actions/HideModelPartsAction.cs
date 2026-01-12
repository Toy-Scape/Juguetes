using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue System/Actions/Hide Model Parts")]
public class HideModelPartsAction : ActionBase
{
    public string[] partsToHide;

    public override void Execute (DialogueContext context)
    {
        if (context.Speaker == null)
        {
            Debug.LogWarning("HideModelPartsAction: Speaker is null.");
            return;
        }

        foreach (var partName in partsToHide)
        {
            Transform part = FindDeep(context.Speaker.transform, partName);

            if (part != null)
            {
                part.gameObject.SetActive(false);
                Debug.Log($"HideModelPartsAction: Part '{partName}' hidden on '{context.Speaker.name}'.");
            }
            else
            {
                Debug.LogWarning($"HideModelPartsAction: Part '{partName}' not found on speaker '{context.Speaker.name}'.");
            }
        }
    }

    private Transform FindDeep (Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            var result = FindDeep(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
