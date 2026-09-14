using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionPanel : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text descriptionText;

    public void Show(ItemStack stack)
    {
        bool hasItem = stack != null && !stack.IsEmpty;
        iconImage.enabled = hasItem && stack.item.Icon != null;
        iconImage.sprite = hasItem ? stack.item.Icon : null;
        nameText.text = hasItem ? stack.item.DisplayName : "Select an item";
        quantityText.text = hasItem ? $"Quantity: {stack.quantity}" : string.Empty;
        descriptionText.text = hasItem ? stack.item.Description : "Choose an item from the inventory to inspect it.";
    }

    public static ItemDescriptionPanel Create(Transform parent)
    {
        GameObject panelObject = new GameObject("ItemDescription", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.11f, 0.13f, 1f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject iconObject = CreateChild(panelObject.transform, "Icon", typeof(Image));
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.08f, 0.6f);
        iconRect.anchorMax = new Vector2(0.38f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        iconObject.GetComponent<Image>().preserveAspect = true;

        TMP_Text nameText = CreateText(panelObject.transform, "ItemName", 26f, TextAlignmentOptions.TopLeft);
        SetRect(nameText.rectTransform, new Vector2(0.42f, 0.76f), new Vector2(0.92f, 0.93f), new Vector2(0f, 0f), new Vector2(0f, 0f));

        TMP_Text quantityText = CreateText(panelObject.transform, "Quantity", 18f, TextAlignmentOptions.TopLeft);
        SetRect(quantityText.rectTransform, new Vector2(0.42f, 0.62f), new Vector2(0.92f, 0.76f), Vector2.zero, Vector2.zero);

        TMP_Text descriptionText = CreateText(panelObject.transform, "Description", 18f, TextAlignmentOptions.TopLeft);
        descriptionText.textWrappingMode = TextWrappingModes.Normal;
        SetRect(descriptionText.rectTransform, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.56f), Vector2.zero, Vector2.zero);

        ItemDescriptionPanel panel = panelObject.AddComponent<ItemDescriptionPanel>();
        panel.iconImage = iconObject.GetComponent<Image>();
        panel.nameText = nameText;
        panel.quantityText = quantityText;
        panel.descriptionText = descriptionText;
        panel.Show(null);
        return panel;
    }

    private static GameObject CreateChild(Transform parent, string objectName, params System.Type[] components)
    {
        GameObject child = new GameObject(objectName, components);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = CreateChild(parent, objectName, typeof(TextMeshProUGUI));
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}