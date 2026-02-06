using Localization;
using TMPro;
using UnityEngine;

public class LocalizedSlider : MonoBehaviour, ILocalizable
{
    public string labelKey;
    [SerializeField] private TMP_Text label;

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
        if (label == null) return;
        string newText = Get(labelKey);
        if (label.text != newText)
            label.text = newText;
    }

    private string Get(string key)
    {
        if (LocalizationManager.Instance != null)
            return LocalizationManager.Instance.GetLocalizedValue(key);
        var db = Resources.Load<Localization.LocalizationDatabase>("LocalizationDatabase");
        return db != null ? db.GetValue(key, Localization.Language.Spanish) : key;
    }
}
