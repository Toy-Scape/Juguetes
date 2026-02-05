using TMPro;
using UnityEngine;

public class LocalizedSlider : MonoBehaviour, ILocalizable
{
    [Header("Localization Keys")]
    public string labelKey;

    [Header("References")]
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        Localize();
    }

    public void Localize()
    {
        if (label == null) return;

        label.text = Get(labelKey);
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
