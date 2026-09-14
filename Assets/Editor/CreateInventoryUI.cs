using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class CreateInventoryUI
{
    private const string WindowName = "InventoryWindow";
    private const string WindowBackgroundPath = "Assets/Sprites/Single Grid.png";

    static CreateInventoryUI()
    {
        EditorApplication.delayCall += CreateForOpenSampleScene;
    }

    [MenuItem("Tools/Gedede/Create Inventory UI Hierarchy")]
    public static void Create()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        InventoryPanel inventoryPanel = Object.FindAnyObjectByType<InventoryPanel>();
        InventoryManager inventory = Object.FindAnyObjectByType<InventoryManager>();

        if (canvas == null || inventoryPanel == null || inventory == null)
        {
            throw new System.InvalidOperationException("Canvas, InventoryPanel, and InventoryManager are required.");
        }

        Transform existingWindow = canvas.transform.Find(WindowName);
        if (existingWindow != null)
        {
            Object.DestroyImmediate(existingWindow.gameObject);
        }

        GameObject window = CreateUiObject(WindowName, canvas.transform);
        RectTransform windowRect = window.GetComponent<RectTransform>();
        SetRect(windowRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(960f, 600f));
        Image windowImage = window.GetComponent<Image>();
        windowImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WindowBackgroundPath);
        windowImage.color = Color.white;
        windowImage.preserveAspect = false;

        GameObject gridObject = CreateUiObject("ItemGrid", window.transform);
        RectTransform gridRect = gridObject.GetComponent<RectTransform>();
        SetAnchoredRect(gridRect, new Vector2(0.06f, 0.12f), new Vector2(0.56f, 0.88f));
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(76f, 76f);
        grid.spacing = new Vector2(10f, 10f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;

        List<InventorySlotView> slots = new List<InventorySlotView>();
        for (int index = 0; index < inventory.SlotCapacity; index++)
        {
            slots.Add(CreateSlot(gridObject.transform, index));
        }

        GameObject descriptionObject = CreateUiObject("DescriptionPanel", window.transform);
        RectTransform descriptionRect = descriptionObject.GetComponent<RectTransform>();
        SetAnchoredRect(descriptionRect, new Vector2(0.62f, 0.12f), new Vector2(0.94f, 0.88f));
        descriptionObject.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.13f, 1f);
        ItemDescriptionPanel descriptionPanel = CreateDescription(descriptionObject.transform);

        SerializedObject panelObject = new SerializedObject(inventoryPanel);
        panelObject.FindProperty("panelRoot").objectReferenceValue = window;
        panelObject.FindProperty("gridRoot").objectReferenceValue = gridObject.transform;
        panelObject.FindProperty("descriptionPanel").objectReferenceValue = descriptionPanel;
        SerializedProperty slotProperty = panelObject.FindProperty("slotViews");
        slotProperty.arraySize = slots.Count;
        for (int index = 0; index < slots.Count; index++)
        {
            slotProperty.GetArrayElementAtIndex(index).objectReferenceValue = slots[index];
        }
        panelObject.ApplyModifiedPropertiesWithoutUndo();

        window.SetActive(false);
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        Selection.activeGameObject = window;
        Debug.Log("Inventory UI hierarchy created under Canvas/InventoryWindow.");
    }

    private static void CreateForOpenSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
        {
            return;
        }

        if (EditorSceneManager.GetActiveScene().name != "SampleScene")
        {
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null && canvas.transform.Find(WindowName) == null && Object.FindAnyObjectByType<InventoryPanel>() != null)
        {
            Create();
        }
    }

    private static InventorySlotView CreateSlot(Transform parent, int index)
    {
        GameObject slot = CreateUiObject($"InventorySlot_{index + 1:00}", parent);
        Image background = slot.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0f);
        Button button = slot.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.25f, 0.42f, 0.45f, 1f);
        colors.pressedColor = new Color(0.35f, 0.55f, 0.5f, 1f);
        button.colors = colors;

        Image icon = CreateImage("Icon", slot.transform, new Vector2(0.12f, 0.25f), new Vector2(0.88f, 0.92f));
        icon.preserveAspect = true;
        TMP_Text quantity = CreateText("Quantity", slot.transform, 18f, TextAlignmentOptions.BottomRight);
        SetAnchoredRect(quantity.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.3f), new Vector2(4f, 2f), new Vector2(-4f, 0f));
        Image selection = CreateImage("Selection", slot.transform, Vector2.zero, Vector2.one);
        selection.color = new Color(0.3f, 0.85f, 0.65f, 0.3f);
        selection.raycastTarget = false;
        selection.transform.SetAsFirstSibling();
        selection.enabled = false;

        InventorySlotView view = slot.AddComponent<InventorySlotView>();
        SerializedObject slotObject = new SerializedObject(view);
        slotObject.FindProperty("button").objectReferenceValue = button;
        slotObject.FindProperty("iconImage").objectReferenceValue = icon;
        slotObject.FindProperty("quantityText").objectReferenceValue = quantity;
        slotObject.FindProperty("selectionImage").objectReferenceValue = selection;
        slotObject.ApplyModifiedPropertiesWithoutUndo();
        return view;
    }

    private static ItemDescriptionPanel CreateDescription(Transform parent)
    {
        Image icon = CreateImage("Icon", parent, new Vector2(0.08f, 0.6f), new Vector2(0.38f, 0.9f));
        icon.preserveAspect = true;
        TMP_Text nameText = CreateText("ItemName", parent, 26f, TextAlignmentOptions.TopLeft);
        SetAnchoredRect(nameText.rectTransform, new Vector2(0.42f, 0.76f), new Vector2(0.92f, 0.93f));
        TMP_Text quantityText = CreateText("Quantity", parent, 18f, TextAlignmentOptions.TopLeft);
        SetAnchoredRect(quantityText.rectTransform, new Vector2(0.42f, 0.62f), new Vector2(0.92f, 0.76f));
        TMP_Text descriptionText = CreateText("Description", parent, 18f, TextAlignmentOptions.TopLeft);
        descriptionText.textWrappingMode = TextWrappingModes.Normal;
        SetAnchoredRect(descriptionText.rectTransform, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.56f));

        ItemDescriptionPanel panel = parent.gameObject.AddComponent<ItemDescriptionPanel>();
        SerializedObject panelObject = new SerializedObject(panel);
        panelObject.FindProperty("iconImage").objectReferenceValue = icon;
        panelObject.FindProperty("nameText").objectReferenceValue = nameText;
        panelObject.FindProperty("quantityText").objectReferenceValue = quantityText;
        panelObject.FindProperty("descriptionText").objectReferenceValue = descriptionText;
        panelObject.ApplyModifiedPropertiesWithoutUndo();
        return panel;
    }

    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static Image CreateImage(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        SetAnchoredRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax);
        return imageObject.GetComponent<Image>();
    }

    private static TMP_Text CreateText(string objectName, Transform parent, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        SetAnchoredRect(rect, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}