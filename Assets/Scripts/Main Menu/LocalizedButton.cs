using Localization;
using TMPro;
using UnityEngine;

public class LocalizedButton : MonoBehaviour, ILocalizable
{
    public string labelKey;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TMP_Text>();
        Localize();
    }

    private void Start()
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

    public void Localize()
    {
        if (label == null) return;
        string newText = GetLocalizedText(labelKey);
        if (label.text != newText)
            label.text = string.IsNullOrEmpty(newText) ? "[Missing Translation]" : newText;
    }

    private string GetLocalizedText(string key)
    {
        if (LocalizationManager.Instance != null)
        {
            string value = LocalizationManager.Instance.GetLocalizedValue(key);
            if (!string.IsNullOrEmpty(value))
                return value;
        }
        var db = Resources.Load<LocalizationDatabase>("LocalizationDatabase");
        if (db != null)
        {
            var entry = db.GetEntry(key);
            if (entry != null)
                return entry.Get(Language.Spanish);
        }
        return key;
    }
}
