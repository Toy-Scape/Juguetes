using MissionSystem.Data;
using MissionSystem.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveEntryUI : MonoBehaviour
{
    [SerializeField] private Image statusIcon;
    [SerializeField] private Sprite pendingSprite;
    [SerializeField] private Sprite completedSprite;
    [SerializeField] private Sprite failedSprite;

    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Bind (Objective objective)
    {
        if (Localization.LocalizationManager.Instance != null)
            descriptionText.text = Localization.LocalizationManager.Instance.GetLocalizedValue(objective.Definition.description);
        else
            descriptionText.text = objective.Definition.description;

        switch (objective.State)
        {
            case ObjectiveState.Completed:
                statusIcon.sprite = completedSprite;
                break;

            case ObjectiveState.Failed:
                statusIcon.sprite = failedSprite;
                break;

            default:
                statusIcon.sprite = pendingSprite;
                break;
        }
    }
}
