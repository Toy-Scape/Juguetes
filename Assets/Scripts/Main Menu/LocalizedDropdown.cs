using Localization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedDropdown : MonoBehaviour, ILocalizable
{
    public string labelKey;
    public List<string> optionKeys = new();
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Dropdown dropdown;

    private void Awake()
    {
        Localize();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.OnLanguageChanged += Localize;
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.OnLanguageChanged -= Localize;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.OnLanguageChanged -= Localize;
    }

    public void Localize()
    {
        if (label != null)
        {
            string newLabel = Get(labelKey);
            if (label.text != newLabel)
                label.text = newLabel;
        }

        if (dropdown == null) return;

        int currentIndex = dropdown.value;
        List<string> localizedOptions = new();
        foreach (var key in optionKeys)
            localizedOptions.Add(Get(key));

        if (dropdown.options.Count == localizedOptions.Count)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
                dropdown.options[i].text = localizedOptions[i];
            dropdown.RefreshShownValue();
        }
        else
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(localizedOptions);
            dropdown.value = Mathf.Clamp(currentIndex, 0, localizedOptions.Count - 1);
            dropdown.RefreshShownValue();
        }
    }

    private string Get(string key)
    {
        if (LocalizationManager.Instance != null)
            return LocalizationManager.Instance.GetLocalizedValue(key);
        var db = Resources.Load<Localization.LocalizationDatabase>("LocalizationDatabase");
        return db != null ? db.GetValue(key, Localization.Language.Spanish) : key;
    }
}
