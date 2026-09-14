using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CraftingPanel : MonoBehaviour
{
    [SerializeField] private CraftingManager crafting;
    [SerializeField] private Player player;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform ingredientRoot;
    [SerializeField] private Transform recipeRoot;
    [SerializeField] private Transform inventoryRoot;
    [SerializeField] private Image resultIcon;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button resultButton;
    [SerializeField] private List<Image> ingredientIcons = new List<Image>();
    [SerializeField] private List<TMP_Text> ingredientQuantities = new List<TMP_Text>();
    [SerializeField] private List<InventorySlotView> inventorySlots = new List<InventorySlotView>();

    private CraftingRecipe selectedRecipe;
    private bool isOpen;
    private CanvasGroup panelCanvasGroup;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (crafting == null) crafting = FindAnyObjectByType<CraftingManager>();
        if (player == null) player = FindAnyObjectByType<Player>();
        if (crafting == null) crafting = gameObject.AddComponent<CraftingManager>();
        BuildRuntimeUIIfNeeded();
        BindButtons();
        crafting.CraftingChanged += Refresh;
        isOpen = false;
        panelCanvasGroup = panelRoot == gameObject
            ? gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>()
            : null;
        SelectRecipe(crafting.Recipes.Count > 0 ? crafting.Recipes[0] : null);
    }

    private void OnDestroy()
    {
        if (crafting != null) crafting.CraftingChanged -= Refresh;
    }

    private void Update()
    {
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = open ? 1f : 0f;
            panelCanvasGroup.interactable = open;
            panelCanvasGroup.blocksRaycasts = open;
        }
        else
        {
            panelRoot.SetActive(open);
        }
        if (open)
        {
            foreach (UpgradePanel upgradePanel in FindObjectsByType<UpgradePanel>(FindObjectsSortMode.None))
            {
                upgradePanel.SetOpen(false);
            }
            player?.SetPaused(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Refresh();
        }
        else
        {
            player?.SetPaused(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        Refresh();
    }

    private void Refresh()
    {
        if (crafting == null) return;
        for (int index = 0; index < ingredientIcons.Count; index++)
        {
            CraftingRecipe.IngredientCell ingredient = selectedRecipe != null && selectedRecipe.Ingredients != null && index < selectedRecipe.Ingredients.Length
                ? selectedRecipe.Ingredients[index] : null;
            bool hasIngredient = ingredient != null && ingredient.item != null;
            ingredientIcons[index].enabled = hasIngredient && ingredient.item.Icon != null;
            ingredientIcons[index].sprite = hasIngredient ? ingredient.item.Icon : null;
            ingredientQuantities[index].text = hasIngredient ? ingredient.quantity.ToString() : string.Empty;
        }

        resultIcon.enabled = selectedRecipe != null && selectedRecipe.Output != null && selectedRecipe.Output.Icon != null;
        resultIcon.sprite = selectedRecipe != null && selectedRecipe.Output != null ? selectedRecipe.Output.Icon : null;
        resultText.text = selectedRecipe != null && selectedRecipe.Output != null
            ? $"{selectedRecipe.Output.DisplayName} x{selectedRecipe.OutputQuantity}"
            : "Pilih resep";
        resultButton.interactable = selectedRecipe != null && crafting.CanCraft(selectedRecipe);

        for (int index = 0; index < crafting.Recipes.Count; index++)
        {
            if (index >= recipeRoot.childCount)
            {
                continue;
            }

            Transform recipeObject = recipeRoot.GetChild(index);
            Button button = recipeObject.GetComponent<Button>();
            Image image = recipeObject.GetComponent<Image>();
            bool available = crafting.CanCraft(crafting.Recipes[index]);
            button.interactable = true;
            image.color = crafting.Recipes[index] == selectedRecipe
                ? new Color(0.3f, 0.85f, 0.65f, 1f)
                : available ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }

        for (int index = 0; index < inventorySlots.Count; index++)
        {
            if (crafting.Inventory != null)
            {
                inventorySlots[index].Refresh(crafting.Inventory.GetSlot(index), false);
            }
        }
    }

    private void BindButtons()
    {
        if (resultButton != null)
        {
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() => { if (selectedRecipe != null) crafting.Craft(selectedRecipe); });
        }

        for (int index = 0; index < crafting.Recipes.Count && index < recipeRoot.childCount; index++)
        {
            CraftingRecipe recipe = crafting.Recipes[index];
            Button button = recipeRoot.GetChild(index).GetComponent<Button>();
            if (button == null)
            {
                continue;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectRecipe(recipe));
        }
    }

    private void BuildRuntimeUIIfNeeded()
    {
        if (panelRoot == null) panelRoot = CreatePanelRoot(transform);
        if (ingredientRoot == null) ingredientRoot = CreateGridPanel("IngredientGridPanel", panelRoot.transform, new Vector2(0.34f, 0.49f), new Vector2(0.65f, 0.91f), 3);
        if (recipeRoot == null) recipeRoot = CreateListPanel("CraftableListPanel", panelRoot.transform, new Vector2(0.68f, 0.08f), new Vector2(0.97f, 0.91f));
        if (inventoryRoot == null) inventoryRoot = CreateGridPanel("InventoryPanel", panelRoot.transform, new Vector2(0.04f, 0.06f), new Vector2(0.65f, 0.42f), 6);
        if (resultButton == null) CreateResultPanel();

        while (ingredientIcons.Count < 9)
        {
            GameObject cell = CreateCell(ingredientRoot, $"Ingredient_{ingredientIcons.Count + 1}");
            ingredientIcons.Add(cell.GetComponent<Image>());
            ingredientQuantities.Add(CreateQuantity(cell.transform));
        }

        int slotCapacity = crafting.Inventory != null ? crafting.Inventory.SlotCapacity : 0;
        while (inventorySlots.Count < slotCapacity)
        {
            inventorySlots.Add(InventorySlotView.Create(inventoryRoot, inventorySlots.Count, null));
        }

        for (int index = 0; index < crafting.Recipes.Count; index++)
        {
            if (recipeRoot.childCount > index) continue;
            CraftingRecipe recipe = crafting.Recipes[index];
            GameObject entry = CreateCell(recipeRoot, recipe.Output != null ? recipe.Output.DisplayName : "Recipe");
            Button button = entry.AddComponent<Button>();
            button.onClick.AddListener(() => SelectRecipe(recipe));
            Image icon = entry.GetComponent<Image>();
            icon.sprite = recipe.Output != null ? recipe.Output.Icon : null;
            icon.color = Color.white;
        }
    }

    private void CreateResultPanel()
    {
        GameObject result = CreatePanel("CraftResultPanel", panelRoot.transform, new Vector2(0.04f, 0.49f), new Vector2(0.30f, 0.91f), new Color(0.95f, 0.78f, 0.42f, 1f));
        resultButton = result.AddComponent<Button>();
        resultButton.onClick.AddListener(() => { if (selectedRecipe != null) crafting.Craft(selectedRecipe); });
        resultIcon = CreateChildImage(result.transform, "ResultIcon");
        resultIcon.rectTransform.anchorMin = new Vector2(0.2f, 0.3f);
        resultIcon.rectTransform.anchorMax = new Vector2(0.8f, 0.78f);
        resultIcon.preserveAspect = true;
        resultText = CreateText(result.transform, "ResultText", TextAlignmentOptions.Bottom | TextAlignmentOptions.Center);
        resultText.rectTransform.anchorMin = new Vector2(0.05f, 0.05f);
        resultText.rectTransform.anchorMax = new Vector2(0.95f, 0.25f);
    }

    private GameObject CreatePanelRoot(Transform parent)
    {
        return CreatePanel("CraftingWindow", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.14f, 0.09f, 0.05f, 0.98f), new Vector2(960f, 600f));
    }

    private Transform CreateGridPanel(string name, Transform parent, Vector2 min, Vector2 max, int columns)
    {
        GameObject panel = CreatePanel(name, parent, min, max, new Color(0.28f, 0.2f, 0.13f, 0.96f));
        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(62f, 62f);
        grid.spacing = new Vector2(8f, 8f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        return panel.transform;
    }

    private Transform CreateListPanel(string name, Transform parent, Vector2 min, Vector2 max)
    {
        GameObject panel = CreatePanel(name, parent, min, max, new Color(0.24f, 0.38f, 0.25f, 0.96f));
        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(62f, 62f);
        grid.spacing = new Vector2(8f, 8f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        return panel.transform;
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 min, Vector2 max, Color color, Vector2? size = null)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (size.HasValue)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size.Value;
        }
        else
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private GameObject CreateCell(Transform parent, string name)
    {
        GameObject cell = new GameObject(name, typeof(RectTransform), typeof(Image));
        cell.transform.SetParent(parent, false);
        cell.GetComponent<Image>().color = new Color(0.65f, 0.58f, 0.47f, 1f);
        return cell;
    }

    private Image CreateChildImage(Transform parent, string name)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        return child.GetComponent<Image>();
    }

    private TMP_Text CreateQuantity(Transform parent)
    {
        TMP_Text text = CreateText(parent, "Quantity", TextAlignmentOptions.BottomRight);
        text.rectTransform.anchorMin = new Vector2(0f, 0f); text.rectTransform.anchorMax = Vector2.one;
        return text;
    }

    private TMP_Text CreateText(Transform parent, string name, TextAlignmentOptions alignment)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        child.transform.SetParent(parent, false);
        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        text.alignment = alignment; text.fontSize = 16f; text.color = Color.white; text.raycastTarget = false;
        return text;
    }
}
