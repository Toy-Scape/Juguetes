using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AutoDestroyBase))]
public class ItemMessageTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text quantityText;

    public void SetData (ItemMessageType itemMessageType, Sprite iconSprite, string titleKey, int? quantity = null)
    {
        string descriptionKey = GetDescriptionKey(itemMessageType);
        messageText.text = Localize(descriptionKey);

        icon.sprite = iconSprite;
        itemName.text = Localize(titleKey);
        quantityText.text = quantity.HasValue ? $"x{quantity.Value}" : "";
    }

    private static string GetDescriptionKey (ItemMessageType type)
    {
        return type switch
        {
            ItemMessageType.Added => "inventory-item-added",
            ItemMessageType.Removed => "inventory-item-removed",
            _ => ""
        };
    }

    private static string Localize (string key)
    {
        if (Localization.LocalizationManager.Instance != null)
            return Localization.LocalizationManager.Instance.GetLocalizedValue(key);

        var db = Resources.Load<Localization.LocalizationDatabase>("LocalizationDatabase");
        if (db != null)
            return db.GetValue(key, Localization.Language.Spanish);

        return key; // fallback final
    }
}

public enum ItemMessageType
{
    Added,
    Removed
}
