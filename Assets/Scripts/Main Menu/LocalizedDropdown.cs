using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class LocalizedDropdown : MonoBehaviour, ILocalizable
{
    [Header("Localization Keys")]
    public string labelKey;
    public List<string> optionKeys = new();

    [Header("References")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Dropdown dropdown;

    private void Awake()
    {
        Localize();
    }

    public void Localize()
    {
        if (label != null)
            label.text = Get(labelKey);

        if (dropdown == null) return;

        dropdown.ClearOptions();

        List<string> localizedOptions = new();
        foreach (var key in optionKeys)
            localizedOptions.Add(Get(key));

        dropdown.AddOptions(localizedOptions);
    }

    private string Get(string key)
    {
        if (Localization.LocalizationManager.Instance != null)
            return Localization.LocalizationManager.Instance.GetLocalizedValue(key);

        var db = Resources.Load<Localization.LocalizationDatabase>("LocalizationDatabase");
        return db != null
            ? db.GetValue(key, Localization.Language.Spanish)
            : key;
    }
}
