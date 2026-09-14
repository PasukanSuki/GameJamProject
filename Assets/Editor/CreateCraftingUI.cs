using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreateCraftingUI
{
    private const string RecipePath = "Assets/ScriptableObjects/RadioRecipe.asset";
    private const string ManagerName = "CraftingManager";
    private const string PanelName = "CraftingPanel";

    [MenuItem("Tools/Gedede/Create Crafting UI")]
    public static void Create()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        Player player = Object.FindAnyObjectByType<Player>();
        InventoryManager inventory = Object.FindAnyObjectByType<InventoryManager>();
        CraftingRecipe recipe = AssetDatabase.LoadAssetAtPath<CraftingRecipe>(RecipePath);

        if (canvas == null || player == null || inventory == null || recipe == null)
        {
            throw new System.InvalidOperationException("Canvas, Player, InventoryManager, and RadioRecipe are required.");
        }

        CraftingManager manager = Object.FindAnyObjectByType<CraftingManager>();
        if (manager == null)
        {
            manager = new GameObject(ManagerName).AddComponent<CraftingManager>();
        }

        SerializedObject managerData = new SerializedObject(manager);
        managerData.FindProperty("inventory").objectReferenceValue = inventory;
        SerializedProperty recipes = managerData.FindProperty("recipes");
        recipes.arraySize = 1;
        recipes.GetArrayElementAtIndex(0).objectReferenceValue = recipe;
        managerData.ApplyModifiedPropertiesWithoutUndo();

        CraftingPanel panel = Object.FindAnyObjectByType<CraftingPanel>();
        if (panel == null)
        {
            GameObject panelObject = new GameObject(PanelName, typeof(RectTransform));
            panelObject.transform.SetParent(canvas.transform, false);
            panel = panelObject.AddComponent<CraftingPanel>();
        }

        Transform oldWindow = panel.transform.Find("CraftingWindow");
        if (oldWindow != null)
        {
            Object.DestroyImmediate(oldWindow.gameObject);
        }

        GameObject window = CreatePanel("CraftingWindow", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.14f, 0.09f, 0.05f, 0.98f));
        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.sizeDelta = new Vector2(960f, 600f);

        GameObject resultPanel = CreatePanel("CraftResultPanel", window.transform, new Vector2(0.04f, 0.49f), new Vector2(0.30f, 0.91f), new Color(0.95f, 0.78f, 0.42f, 1f));
        Button resultButton = resultPanel.AddComponent<Button>();
        Image resultIcon = CreateImage("ResultIcon", resultPanel.transform, new Vector2(0.2f, 0.3f), new Vector2(0.8f, 0.78f));
        resultIcon.preserveAspect = true;
        TMP_Text resultText = CreateText("ResultText", resultPanel.transform, TextAlignmentOptions.Bottom | TextAlignmentOptions.Center, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.25f));

        GameObject ingredientPanel = CreatePanel("IngredientGridPanel", window.transform, new Vector2(0.34f, 0.49f), new Vector2(0.65f, 0.91f), new Color(0.28f, 0.2f, 0.13f, 0.96f));
        GridLayoutGroup ingredientGrid = ingredientPanel.AddComponent<GridLayoutGroup>();
        ingredientGrid.cellSize = new Vector2(62f, 62f);
        ingredientGrid.spacing = new Vector2(8f, 8f);
        ingredientGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        ingredientGrid.constraintCount = 3;
        List<Image> ingredientIcons = new List<Image>();
        List<TMP_Text> ingredientQuantities = new List<TMP_Text>();
        for (int index = 0; index < 9; index++)
        {
            GameObject cell = CreateCell(ingredientPanel.transform, $"Ingredient_{index + 1}");
            ingredientIcons.Add(cell.GetComponent<Image>());
            ingredientQuantities.Add(CreateText("Quantity", cell.transform, TextAlignmentOptions.Bottom | TextAlignmentOptions.Right, Vector2.zero, Vector2.one));
        }

        GameObject recipePanel = CreatePanel("CraftableListPanel", window.transform, new Vector2(0.68f, 0.08f), new Vector2(0.97f, 0.91f), new Color(0.24f, 0.38f, 0.25f, 0.96f));
        GridLayoutGroup recipeGrid = recipePanel.AddComponent<GridLayoutGroup>();
        recipeGrid.cellSize = new Vector2(62f, 62f);
        recipeGrid.spacing = new Vector2(8f, 8f);
        recipeGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        recipeGrid.constraintCount = 4;
        foreach (CraftingRecipe currentRecipe in manager.Recipes)
        {
            GameObject entry = CreateCell(recipePanel.transform, currentRecipe.Output != null ? currentRecipe.Output.DisplayName : "Recipe");
            entry.AddComponent<Button>();
            Image icon = entry.GetComponent<Image>();
            icon.sprite = currentRecipe.Output != null ? currentRecipe.Output.Icon : null;
            icon.preserveAspect = true;
        }

        GameObject inventoryPanel = CreatePanel("InventoryPanel", window.transform, new Vector2(0.04f, 0.06f), new Vector2(0.65f, 0.42f), new Color(0.28f, 0.2f, 0.13f, 0.96f));
        GridLayoutGroup inventoryGrid = inventoryPanel.AddComponent<GridLayoutGroup>();
        inventoryGrid.cellSize = new Vector2(62f, 62f);
        inventoryGrid.spacing = new Vector2(8f, 8f);
        inventoryGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        inventoryGrid.constraintCount = 6;
        List<InventorySlotView> inventorySlots = new List<InventorySlotView>();
        for (int index = 0; index < inventory.SlotCapacity; index++)
        {
            inventorySlots.Add(InventorySlotView.Create(inventoryPanel.transform, index, null));
        }

        SerializedObject panelData = new SerializedObject(panel);
        panelData.FindProperty("crafting").objectReferenceValue = manager;
        panelData.FindProperty("player").objectReferenceValue = player;
        panelData.FindProperty("panelRoot").objectReferenceValue = window;
        panelData.FindProperty("ingredientRoot").objectReferenceValue = ingredientPanel.transform;
        panelData.FindProperty("recipeRoot").objectReferenceValue = recipePanel.transform;
        panelData.FindProperty("inventoryRoot").objectReferenceValue = inventoryPanel.transform;
        panelData.FindProperty("resultIcon").objectReferenceValue = resultIcon;
        panelData.FindProperty("resultText").objectReferenceValue = resultText;
        panelData.FindProperty("resultButton").objectReferenceValue = resultButton;
        SetObjectList(panelData.FindProperty("ingredientIcons"), ingredientIcons);
        SetObjectList(panelData.FindProperty("ingredientQuantities"), ingredientQuantities);
        SetObjectList(panelData.FindProperty("inventorySlots"), inventorySlots);
        panelData.ApplyModifiedPropertiesWithoutUndo();

        window.SetActive(false);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = panel.gameObject;
        Debug.Log("Crafting hierarchy created under Canvas/CraftingPanel/CraftingWindow. Press Q during play mode.");
    }

    private static void SetObjectList<T>(SerializedProperty property, List<T> objects) where T : Object
    {
        property.arraySize = objects.Count;
        for (int index = 0; index < objects.Count; index++)
        {
            property.GetArrayElementAtIndex(index).objectReferenceValue = objects[index];
        }
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static GameObject CreateCell(Transform parent, string name)
    {
        return CreatePanel(name, parent, Vector2.zero, Vector2.one, new Color(0.65f, 0.58f, 0.47f, 1f));
    }

    private static Image CreateImage(string name, Transform parent, Vector2 min, Vector2 max)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return imageObject.GetComponent<Image>();
    }

    private static TMP_Text CreateText(string name, Transform parent, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = alignment;
        text.fontSize = 16f;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
