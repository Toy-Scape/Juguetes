using Localization;
using UnityEngine;

public class MenuLocalizationRoot : MonoBehaviour
{
    private void Start()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.OnLanguageChanged += LocalizeChildren;

        LocalizeChildren();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.OnLanguageChanged -= LocalizeChildren;
    }

    private void LocalizeChildren()
    {
        foreach (var l in GetComponentsInChildren<ILocalizable>(true))
            l.Localize();
    }
}
