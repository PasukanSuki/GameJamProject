using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image selectionImage;

    private int slotIndex;
    private System.Action<int> clickHandler;

    public void Configure(int index, System.Action<int> onClicked)
    {
        slotIndex = index;
        clickHandler = onClicked;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);
    }

    public void Refresh(ItemStack stack, bool isSelected)
    {
        bool hasItem = stack != null && !stack.IsEmpty;
        iconImage.enabled = hasItem && stack.item.Icon != null;
        iconImage.sprite = hasItem ? stack.item.Icon : null;
        quantityText.text = hasItem ? stack.quantity.ToString() : string.Empty;
        selectionImage.enabled = hasItem && isSelected;
        button.interactable = hasItem;
    }

    private void HandleClick()
    {
        clickHandler?.Invoke(slotIndex);
    }

    public static InventorySlotView Create(Transform parent, int index, System.Action<int> onClicked)
    {
        GameObject slotObject = new GameObject($"InventorySlot_{index + 1}", typeof(RectTransform), typeof(Image), typeof(Button));
        slotObject.transform.SetParent(parent, false);

        Image background = slotObject.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0f);

        Button slotButton = slotObject.GetComponent<Button>();
        ColorBlock colors = slotButton.colors;
        colors.highlightedColor = new Color(0.25f, 0.42f, 0.45f, 1f);
        colors.pressedColor = new Color(0.35f, 0.55f, 0.5f, 1f);
        slotButton.colors = colors;

        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(slotObject.transform, false);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.12f, 0.25f);
        iconRect.anchorMax = new Vector2(0.88f, 0.92f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        iconObject.GetComponent<Image>().preserveAspect = true;

        GameObject quantityObject = new GameObject("Quantity", typeof(RectTransform), typeof(TextMeshProUGUI));
        quantityObject.transform.SetParent(slotObject.transform, false);
        RectTransform quantityRect = quantityObject.GetComponent<RectTransform>();
        quantityRect.anchorMin = new Vector2(0f, 0f);
        quantityRect.anchorMax = new Vector2(1f, 0.3f);
        quantityRect.offsetMin = new Vector2(4f, 2f);
        quantityRect.offsetMax = new Vector2(-4f, 0f);
        TextMeshProUGUI quantityText = quantityObject.GetComponent<TextMeshProUGUI>();
        quantityText.alignment = TextAlignmentOptions.BottomRight;
        quantityText.fontSize = 18f;
        quantityText.color = Color.white;
        quantityText.raycastTarget = false;

        GameObject selectionObject = new GameObject("Selection", typeof(RectTransform), typeof(Image));
        selectionObject.transform.SetParent(slotObject.transform, false);
        selectionObject.transform.SetAsFirstSibling();
        RectTransform selectionRect = selectionObject.GetComponent<RectTransform>();
        selectionRect.anchorMin = Vector2.zero;
        selectionRect.anchorMax = Vector2.one;
        selectionRect.offsetMin = Vector2.zero;
        selectionRect.offsetMax = Vector2.zero;
        Image selectionImage = selectionObject.GetComponent<Image>();
        selectionImage.color = new Color(0.3f, 0.85f, 0.65f, 0.3f);
        selectionImage.raycastTarget = false;

        InventorySlotView view = slotObject.AddComponent<InventorySlotView>();
        view.button = slotButton;
        view.iconImage = iconObject.GetComponent<Image>();
        view.quantityText = quantityText;
        view.selectionImage = selectionImage;
        view.Configure(index, onClicked);
        return view;
    }
}