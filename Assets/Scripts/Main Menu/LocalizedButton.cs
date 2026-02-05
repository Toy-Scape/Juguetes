using Localization;
using TMPro;
using UnityEngine;

public class LocalizedButton : MonoBehaviour, ILocalizable
{
    [Header("Localization")]
    public string labelKey;

    [Header("References")]
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

    public void Localize()
    {
        if (label == null) return;

        label.text = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetLocalizedValue(labelKey)
            : labelKey;
    }
}
