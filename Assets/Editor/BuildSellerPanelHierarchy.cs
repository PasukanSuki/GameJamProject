using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildSellerPanelHierarchy
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    [MenuItem("Tools/Gedede/Build Seller Panel Hierarchy")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        SellerPanel panel = Object.FindAnyObjectByType<SellerPanel>(FindObjectsInactive.Include);
        if (panel == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                throw new System.InvalidOperationException("Canvas not found in SampleScene.");
            }

            panel = canvas.AddComponent<SellerPanel>();
        }

        Transform window = panel.transform.Find("SellerWindow");
        if (window == null)
        {
            GameObject windowObject = new GameObject("SellerWindow", typeof(RectTransform), typeof(Image), typeof(CanvasRenderer));
            windowObject.transform.SetParent(panel.transform, false);
            window = windowObject.transform;
        }

        RectTransform windowRect = window.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.2f, 0.12f);
        windowRect.anchorMax = new Vector2(0.8f, 0.88f);
        windowRect.offsetMin = Vector2.zero;
        windowRect.offsetMax = Vector2.zero;
        window.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.09f, 0.98f);
        window.gameObject.SetActive(false);

        Transform itemRoot = EnsureChild(window, "SellableItems");
        VerticalLayoutGroup layout = itemRoot.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = itemRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        SetRect(itemRoot, new Vector2(0.06f, 0.2f), new Vector2(0.5f, 0.9f));

        TMP_Text selectedName = EnsureText(window, "SelectedName", 28f, TextAlignmentOptions.TopLeft, new Vector2(0.56f, 0.8f), new Vector2(0.94f, 0.92f));
        TMP_Text selectedQuantity = EnsureText(window, "SelectedQuantity", 18f, TextAlignmentOptions.TopLeft, new Vector2(0.56f, 0.68f), new Vector2(0.94f, 0.78f));
        TMP_Text selectedPrice = EnsureText(window, "SelectedPrice", 18f, TextAlignmentOptions.TopLeft, new Vector2(0.56f, 0.58f), new Vector2(0.94f, 0.68f));
        TMP_Text total = EnsureText(window, "Total", 22f, TextAlignmentOptions.TopLeft, new Vector2(0.56f, 0.46f), new Vector2(0.94f, 0.56f));
        Button decrease = EnsureButton(window, "Decrease", "-", new Vector2(0.56f, 0.32f), new Vector2(0.66f, 0.42f));
        Button increase = EnsureButton(window, "Increase", "+", new Vector2(0.68f, 0.32f), new Vector2(0.78f, 0.42f));
        Button sell = EnsureButton(window, "Sell", "SELL", new Vector2(0.56f, 0.2f), new Vector2(0.78f, 0.3f));
        Button close = EnsureButton(window, "Close", "CLOSE", new Vector2(0.8f, 0.2f), new Vector2(0.94f, 0.3f));

        SerializedObject serializedPanel = new SerializedObject(panel);
        serializedPanel.FindProperty("panelRoot").objectReferenceValue = window.gameObject;
        serializedPanel.FindProperty("itemRoot").objectReferenceValue = itemRoot;
        serializedPanel.FindProperty("selectedNameText").objectReferenceValue = selectedName;
        serializedPanel.FindProperty("selectedQuantityText").objectReferenceValue = selectedQuantity;
        serializedPanel.FindProperty("selectedPriceText").objectReferenceValue = selectedPrice;
        serializedPanel.FindProperty("totalText").objectReferenceValue = total;
        serializedPanel.FindProperty("decreaseButton").objectReferenceValue = decrease;
        serializedPanel.FindProperty("increaseButton").objectReferenceValue = increase;
        serializedPanel.FindProperty("sellButton").objectReferenceValue = sell;
        serializedPanel.FindProperty("closeButton").objectReferenceValue = close;
        serializedPanel.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(panel);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = window.gameObject;
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child;
        GameObject childObject = new GameObject(name, typeof(RectTransform));
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private static TMP_Text EnsureText(Transform parent, string name, float size, TextAlignmentOptions alignment, Vector2 min, Vector2 max)
    {
        Transform child = EnsureChild(parent, name);
        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        if (text == null) text = child.gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        SetRect(child, min, max);
        return text;
    }

    private static Button EnsureButton(Transform parent, string name, string label, Vector2 min, Vector2 max)
    {
        Transform child = EnsureChild(parent, name);
        Image image = child.GetComponent<Image>();
        if (image == null) image = child.gameObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.42f, 0.5f, 1f);
        Button button = child.GetComponent<Button>();
        if (button == null) button = child.gameObject.AddComponent<Button>();
        Transform labelTransform = EnsureChild(child, "Label");
        TextMeshProUGUI text = labelTransform.GetComponent<TextMeshProUGUI>();
        if (text == null) text = labelTransform.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 16f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        SetRect(labelTransform, Vector2.zero, Vector2.one);
        SetRect(child, min, max);
        return button;
    }

    private static void SetRect(Transform target, Vector2 min, Vector2 max)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
