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


    public void SetData(ItemMessageType itemMessageType, Sprite iconSprite, string titleText, int? quantity = null)
    {
        // TODO:: Localize these strings
        messageText.text = GetDescription(itemMessageType);
        icon.sprite = iconSprite;
        itemName.text = titleText;
        quantityText.text = quantity.HasValue ? $"x{quantity.Value}" : "";
        Debug.Log($"ItemMessageTooltip SetData: {messageText.text}, {titleText}, Quantity: {quantityText.text}");
    }

    private static string GetDescription(ItemMessageType type)
    {
        return type switch
        {
            ItemMessageType.Added => "New item added",
            ItemMessageType.Removed => "Item removed",
            _ => ""
        };
    }

}

public enum ItemMessageType
{
    Added,
    Removed
}