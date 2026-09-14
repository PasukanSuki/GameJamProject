#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreateUpgradeUI
{
    private const string PanelName = "UpgradePanel";

    [MenuItem("Gedede/UI/Create Upgrade Panel")]
    public static void CreateInActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("No valid active scene is open.");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        Transform existing = canvas.transform.Find(PanelName);
        GameObject panelObject = existing != null ? existing.gameObject : new GameObject(PanelName, typeof(RectTransform));
        if (existing == null)
        {
            panelObject.transform.SetParent(canvas.transform, false);
        }

        if (panelObject.GetComponent<UpgradePanel>() == null)
        {
            panelObject.AddComponent<UpgradePanel>();
        }

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Transform legacyWindow = panelObject.transform.Find("UpgradeWindow");
        if (legacyWindow != null)
        {
            Object.DestroyImmediate(legacyWindow.gameObject);
        }

        BuildVisualHierarchy(panelObject.transform);

        EditorUtility.SetDirty(panelObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = panelObject;
        Debug.Log("Upgrade panel created under Canvas/UpgradePanel.");
    }

    private static void BuildVisualHierarchy(Transform panelRoot)
    {
        Transform window = panelRoot;
        RectTransform windowRect = window.GetComponent<RectTransform>();
        if (windowRect == null) windowRect = window.gameObject.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(940f, 700f);
        Image background = window.GetComponent<Image>();
        if (background == null) background = window.gameObject.AddComponent<Image>();
        background.color = new Color(0.29f, 0.55f, 0.86f, 0.98f);

        CreateText("Title", window, "UPGRADES", new Vector2(0.06f, 0.92f), new Vector2(0.62f, 0.98f), 30f);
        CreateText("Balance", window, "MONEY 0$", new Vector2(0.64f, 0.92f), new Vector2(0.94f, 0.98f), 22f);

        Transform cards = GetOrCreate("UpgradeCards", window, typeof(RectTransform), typeof(VerticalLayoutGroup));
        RectTransform cardsRect = cards.GetComponent<RectTransform>();
        cardsRect.anchorMin = new Vector2(0.06f, 0.14f);
        cardsRect.anchorMax = new Vector2(0.94f, 0.9f);
        cardsRect.offsetMin = Vector2.zero;
        cardsRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = cards.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        CreateCard("BagCapacityCard", cards, "BAG CAPACITY");
        CreateCard("IncomeCard", cards, "INCOME");

        CreateText("Feedback", window, string.Empty, new Vector2(0.06f, 0.04f), new Vector2(0.72f, 0.1f), 16f);
        Transform exit = GetOrCreate("ExitButton", window, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform exitRect = exit.GetComponent<RectTransform>();
        exitRect.anchorMin = new Vector2(0.76f, 0.035f);
        exitRect.anchorMax = new Vector2(0.94f, 0.105f);
        exitRect.offsetMin = Vector2.zero;
        exitRect.offsetMax = Vector2.zero;
        CreateText("Label", exit, "EXIT", Vector2.zero, Vector2.one, 18f);
    }

    private static void CreateCard(string name, Transform parent, string title)
    {
        Transform card = GetOrCreate(name, parent, typeof(RectTransform), typeof(Image));
        card.GetComponent<Image>().color = new Color(0.22f, 0.48f, 0.78f, 0.92f);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(0f, 84f);
        CreateChildImage("Icon", card);
        CreateText("Title", card, title, new Vector2(0.18f, 0.54f), new Vector2(0.62f, 0.9f), 22f);
        CreateText("Description", card, "Upgrade description", new Vector2(0.18f, 0.1f), new Vector2(0.62f, 0.48f), 14f);
        CreateText("Level", card, "LEVEL 0/7", new Vector2(0.64f, 0.54f), new Vector2(0.81f, 0.9f), 14f);
        CreateText("Value", card, "0 > 1", new Vector2(0.64f, 0.1f), new Vector2(0.81f, 0.48f), 16f);
        Transform buy = GetOrCreate("BuyButton", card, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buyRect = buy.GetComponent<RectTransform>();
        buyRect.anchorMin = new Vector2(0.83f, 0.16f);
        buyRect.anchorMax = new Vector2(0.98f, 0.84f);
        buyRect.offsetMin = Vector2.zero;
        buyRect.offsetMax = Vector2.zero;
        CreateText("Price", buy, "250$", Vector2.zero, Vector2.one, 16f);
    }

    private static Transform GetOrCreate(string name, Transform parent, params System.Type[] components)
    {
        Transform existing = parent.Find(name);
        if (existing != null) return existing;

        GameObject child = new GameObject(name, components);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static Image CreateChildImage(string name, Transform parent)
    {
        Transform imageTransform = GetOrCreate(name, parent, typeof(RectTransform), typeof(Image));
        RectTransform rect = imageTransform.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.02f, 0.16f);
        rect.anchorMax = new Vector2(0.15f, 0.84f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = imageTransform.GetComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(0.85f, 0.8f, 0.35f, 1f);
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, Vector2 min, Vector2 max, float fontSize)
    {
        Transform textTransform = GetOrCreate(name, parent, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = textTransform.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = textTransform.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }
}
#endif
