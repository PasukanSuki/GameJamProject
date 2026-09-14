using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeCardView : MonoBehaviour
{
    private Image iconImage;
    private TMP_Text titleText;
    private TMP_Text valueText;
    private TMP_Text priceText;
    private TMP_Text levelText;
    private TMP_Text descriptionText;
    private Button purchaseButton;
    private UpgradeDefinition upgrade;
    private PlayerUpgradeManager manager;

    public static UpgradeCardView Create(Transform parent, UpgradeDefinition definition, PlayerUpgradeManager upgradeManager, UnityAction purchaseAction)
    {
        GameObject cardObject = new GameObject(definition.DisplayName, typeof(RectTransform), typeof(Image), typeof(UpgradeCardView));
        cardObject.transform.SetParent(parent, false);
        UpgradeCardView card = cardObject.GetComponent<UpgradeCardView>();
        card.upgrade = definition;
        card.manager = upgradeManager;
        card.BuildUI(purchaseAction);
        card.Refresh();
        return card;
    }

    public bool ConfigureExisting(UpgradeDefinition definition, PlayerUpgradeManager upgradeManager, UnityAction purchaseAction)
    {
        upgrade = definition;
        manager = upgradeManager;
        iconImage = FindChild<Image>("Icon");
        titleText = FindChild<TMP_Text>("Title");
        descriptionText = FindChild<TMP_Text>("Description");
        valueText = FindChild<TMP_Text>("Value");
        levelText = FindChild<TMP_Text>("Level");
        Transform priceTransform = FindChildTransform("BuyButton/Price");
        priceText = priceTransform != null ? priceTransform.GetComponent<TMP_Text>() : FindChild<TMP_Text>("Price");
        Transform buttonTransform = FindChildTransform("BuyButton");
        purchaseButton = buttonTransform != null ? buttonTransform.GetComponent<Button>() : null;

        if (iconImage == null || titleText == null || descriptionText == null || valueText == null
            || levelText == null || priceText == null || purchaseButton == null)
        {
            Debug.LogWarning(
                $"[UpgradeCardView] Authored card '{name}' is missing a required child. " +
                "Expected Icon, Title, Description, Level, Value, BuyButton, and BuyButton/Price.",
                this);
            return false;
        }

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(purchaseAction);
        titleText.text = definition.DisplayName.ToUpperInvariant();
        descriptionText.text = definition.Description;
        iconImage.preserveAspect = true;

        Refresh();
        return true;
    }

    public void Refresh()
    {
        if (upgrade == null || manager == null) return;

        int level = manager.GetLevel(upgrade);
        bool maxed = manager.IsMaxLevel(upgrade);
        bool canPurchase = manager.CanPurchase(upgrade);
        if (iconImage != null)
        {
            iconImage.sprite = upgrade.GetIconForLevel(level);
            iconImage.preserveAspect = true;
            iconImage.color = iconImage.sprite != null
                ? Color.white
                : GetPlaceholderColor(upgrade.UpgradeType);
        }

        levelText.text = $"LEVEL {level}/{upgrade.MaxLevel}";
        descriptionText.text = upgrade.Description;
        valueText.text = maxed
            ? $"{FormatValue(manager.GetCurrentAmount(upgrade))}  MAX"
            : $"{FormatValue(manager.GetCurrentAmount(upgrade))} > {FormatValue(manager.GetNextAmount(upgrade))}";
        priceText.text = maxed ? "MAX" : $"{manager.GetNextPrice(upgrade):N0}$";
        purchaseButton.interactable = canPurchase;
        purchaseButton.GetComponent<Image>().color = maxed
            ? new Color(0.25f, 0.3f, 0.34f, 1f)
            : canPurchase ? new Color(0.16f, 0.42f, 0.7f, 1f) : new Color(0.28f, 0.32f, 0.36f, 1f);
    }

    private void BuildUI(UnityAction purchaseAction)
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 84f);
        GetComponent<Image>().color = new Color(0.22f, 0.48f, 0.78f, 0.92f);

        iconImage = CreateImage("Icon", new Vector2(0.02f, 0.16f), new Vector2(0.15f, 0.84f));
        iconImage.sprite = upgrade.Icon;
        iconImage.preserveAspect = true;
    if (iconImage.sprite == null) iconImage.color = GetPlaceholderColor(upgrade.UpgradeType);

        titleText = CreateText("Title", new Vector2(0.18f, 0.54f), new Vector2(0.62f, 0.9f), 22f);
        titleText.text = upgrade.DisplayName.ToUpperInvariant();
        descriptionText = CreateText("Description", new Vector2(0.18f, 0.1f), new Vector2(0.62f, 0.48f), 14f);
        descriptionText.text = upgrade.Description;
        valueText = CreateText("Value", new Vector2(0.18f, 0.1f), new Vector2(0.62f, 0.48f), 17f);
        levelText = CreateText("Level", new Vector2(0.64f, 0.54f), new Vector2(0.81f, 0.9f), 14f);
        priceText = CreateText("Price", new Vector2(0.64f, 0.1f), new Vector2(0.81f, 0.48f), 18f);
        GameObject buttonObject = new GameObject("Buy", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.83f, 0.16f);
        buttonRect.anchorMax = new Vector2(0.98f, 0.84f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        purchaseButton = buttonObject.GetComponent<Button>();
        purchaseButton.onClick.AddListener(purchaseAction);
        TMP_Text buttonText = CreateText("ButtonText", buttonRect, Vector2.zero, Vector2.one, 16f);
        buttonText.text = "BUY";
        buttonText.alignment = TextAlignmentOptions.Center;
    }

    private Image CreateImage(string name, Vector2 min, Vector2 max)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = imageObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(string name, Vector2 min, Vector2 max, float fontSize)
    {
        return CreateText(name, transform, min, max, fontSize);
    }

    private TMP_Text CreateText(string name, Transform parent, Vector2 min, Vector2 max, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        return text;
    }

    private static string FormatValue(float value)
    {
        return value.ToString("0.##");
    }

    private static Color GetPlaceholderColor(PlayerUpgradeType type)
    {
        switch (type)
        {
            case PlayerUpgradeType.BagCapacity: return new Color(0.85f, 0.8f, 0.35f, 1f);
            case PlayerUpgradeType.PickupSpeed: return new Color(0.35f, 0.85f, 0.9f, 1f);
            case PlayerUpgradeType.FullbagBonus: return new Color(0.95f, 0.5f, 0.35f, 1f);
            default: return new Color(0.65f, 0.45f, 0.9f, 1f);
        }
    }

    private T FindChild<T>(string objectName) where T : Component
    {
        Transform child = FindChildTransform(objectName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private Transform FindChildTransform(string path)
    {
        Transform current = transform;
        foreach (string part in path.Split('/'))
        {
            current = current.Find(part);
            if (current == null) return null;
        }

        return current;
    }
}
