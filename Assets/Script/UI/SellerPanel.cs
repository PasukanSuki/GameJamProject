using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SellerPanel : MonoBehaviour
{
    private SellerManager seller;
    private Player player;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform itemRoot;
    [SerializeField] private TMP_Text selectedNameText;
    [SerializeField] private TMP_Text selectedQuantityText;
    [SerializeField] private TMP_Text selectedPriceText;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;
    private readonly List<SellerItemRow> itemRows = new List<SellerItemRow>();
    private ItemDefinition selectedItem;
    private int selectedQuantity = 1;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        BindSerializedUI();
        panelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (seller != null)
        {
            seller.ItemSold -= HandleItemSold;
        }
    }

    public void Open(SellerManager sellerManager, Player playerTarget)
    {
        if (seller != null)
        {
            seller.ItemSold -= HandleItemSold;
        }

        seller = sellerManager;
        player = playerTarget;
        seller.ItemSold += HandleItemSold;
        CloseOtherPanels();
        isOpen = true;
        panelRoot.SetActive(true);
        player.SetPaused(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        selectedItem = null;
        selectedQuantity = 1;
        Refresh();
    }

    public void Close()
    {
        isOpen = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        player?.SetPaused(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    private void BindSerializedUI()
    {
        if (panelRoot == null || itemRoot == null || selectedNameText == null || selectedQuantityText == null
            || selectedPriceText == null || totalText == null || decreaseButton == null || increaseButton == null
            || sellButton == null || closeButton == null)
        {
            return;
        }

        ClearItems();
        decreaseButton.onClick.RemoveAllListeners();
        increaseButton.onClick.RemoveAllListeners();
        sellButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        decreaseButton.onClick.AddListener(() => ChangeQuantity(-1));
        increaseButton.onClick.AddListener(() => ChangeQuantity(1));
        sellButton.onClick.AddListener(SellSelected);
        closeButton.onClick.AddListener(Close);
    }

    private void Refresh()
    {
        if (seller == null)
        {
            return;
        }

        ClearItems();
        IReadOnlyList<ItemDefinition> items = seller.GetSellableItems();
        foreach (ItemDefinition item in items)
        {
            CreateItemButton(item, item == selectedItem);
        }

        if (selectedItem == null || !ContainsItem(items, selectedItem))
        {
            selectedItem = items.Count > 0 ? items[0] : null;
            selectedQuantity = 1;
        }

        RefreshItemSelection();
        RefreshSelected();
    }

    private void CreateItemButton(ItemDefinition item, bool isSelected)
    {
        GameObject buttonObject = new GameObject(item.DisplayName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(itemRoot, false);
        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = 42f;
        layoutElement.preferredHeight = 42f;
        Image background = buttonObject.GetComponent<Image>();
        background.color = GetItemRowColor(isSelected);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() => SelectItem(item));

        TextMeshProUGUI text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(buttonObject.transform, false);
        text.text = $"{item.DisplayName}   {seller.GetItemCount(item)}  |  {item.SellValue:N0}$";
        text.fontSize = 18f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.raycastTarget = false;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.06f, 0f);
        textRect.anchorMax = new Vector2(0.94f, 1f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        itemRows.Add(new SellerItemRow(item, background));
    }

    private void SelectItem(ItemDefinition item)
    {
        selectedItem = item;
        selectedQuantity = 1;
        RefreshItemSelection();
        RefreshSelected();
    }

    private void ChangeQuantity(int amount)
    {
        if (selectedItem == null)
        {
            return;
        }

        int available = seller.GetItemCount(selectedItem);
        selectedQuantity = Mathf.Clamp(selectedQuantity + amount, 1, Mathf.Max(1, available));
        RefreshSelected();
    }

    private void SellSelected()
    {
        if (selectedItem != null && seller.TrySell(selectedItem, selectedQuantity))
        {
            selectedQuantity = 1;
            Refresh();
        }
    }

    private void HandleItemSold(ItemDefinition item, int quantity, int totalValue)
    {
        Refresh();
    }

    private void RefreshSelected()
    {
        bool hasSelection = selectedItem != null;
        int available = hasSelection ? seller.GetItemCount(selectedItem) : 0;
        if (hasSelection)
        {
            selectedQuantity = Mathf.Clamp(selectedQuantity, 1, Mathf.Max(1, available));
        }

        selectedNameText.text = hasSelection ? selectedItem.DisplayName : "No sellable items";
        selectedQuantityText.text = hasSelection ? $"Quantity: {selectedQuantity} / {available}" : string.Empty;
        selectedPriceText.text = hasSelection ? $"Each: {selectedItem.SellValue:N0}$" : string.Empty;
        totalText.text = hasSelection ? $"Total: {(selectedItem.SellValue * selectedQuantity):N0}$" : string.Empty;
        sellButton.interactable = hasSelection && seller.CanSell(selectedItem, selectedQuantity);
    }

    private void ClearItems()
    {
        itemRows.Clear();
        for (int index = itemRoot.childCount - 1; index >= 0; index--)
        {
            Destroy(itemRoot.GetChild(index).gameObject);
        }
    }

    private void RefreshItemSelection()
    {
        for (int index = 0; index < itemRows.Count; index++)
        {
            SellerItemRow row = itemRows[index];
            row.Background.color = GetItemRowColor(row.Item == selectedItem);
        }
    }

    private static Color GetItemRowColor(bool isSelected)
    {
        return isSelected
            ? new Color(0.22f, 0.52f, 0.48f, 1f)
            : new Color(0.12f, 0.18f, 0.2f, 1f);
    }

    private sealed class SellerItemRow
    {
        public readonly ItemDefinition Item;
        public readonly Image Background;

        public SellerItemRow(ItemDefinition item, Image background)
        {
            Item = item;
            Background = background;
        }
    }

    private static bool ContainsItem(IReadOnlyList<ItemDefinition> items, ItemDefinition item)
    {
        for (int index = 0; index < items.Count; index++)
        {
            if (items[index] == item)
            {
                return true;
            }
        }

        return false;
    }

    private void CloseOtherPanels()
    {
        foreach (InventoryPanel panel in FindObjectsByType<InventoryPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panel.SetOpen(false);
        }

        foreach (CraftingPanel panel in FindObjectsByType<CraftingPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panel.SetOpen(false);
        }

        foreach (UpgradePanel panel in FindObjectsByType<UpgradePanel>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            panel.SetOpen(false);
        }
    }

}
