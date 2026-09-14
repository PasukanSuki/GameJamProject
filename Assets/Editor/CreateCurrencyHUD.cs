#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreateCurrencyHUD
{
    private const string HudName = "WalletHUD";
    private const string SpritePath = "Assets/Sprites/Uang.png";

    [InitializeOnLoadMethod]
    private static void CreateForGameplayScene()
    {
        EditorApplication.delayCall += TryCreateForGameplayScene;
    }

    private static void TryCreateForGameplayScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.path.EndsWith("Assets/Scenes/SampleScene.unity"))
        {
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null && canvas.transform.Find(HudName) == null)
        {
            CreateInActiveScene();
        }
    }

    [MenuItem("Gedede/UI/Create Currency HUD")]
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

        Transform existing = canvas.transform.Find(HudName);
        GameObject hudObject = existing != null ? existing.gameObject : new GameObject(HudName, typeof(RectTransform));
        if (existing == null)
        {
            hudObject.transform.SetParent(canvas.transform, false);
        }

        RectTransform hudRect = hudObject.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = new Vector2(24f, -24f);
        hudRect.sizeDelta = new Vector2(260f, 64f);

        WalletCounter counter = hudObject.GetComponent<WalletCounter>();
        if (counter == null)
        {
            counter = hudObject.AddComponent<WalletCounter>();
        }

        Image icon = GetOrCreateIcon(hudObject.transform);
        TMP_Text amount = GetOrCreateAmount(hudObject.transform);
        Sprite currencySprite = FindCurrencySprite();
        icon.sprite = currencySprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        amount.raycastTarget = false;

        SerializedObject serializedCounter = new SerializedObject(counter);
        serializedCounter.FindProperty("counterText").objectReferenceValue = amount;
        serializedCounter.FindProperty("iconImage").objectReferenceValue = icon;
        serializedCounter.FindProperty("currencyIcon").objectReferenceValue = currencySprite;
        serializedCounter.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(hudObject);
        EditorUtility.SetDirty(counter);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = hudObject;
        Debug.Log("Currency HUD created under Canvas/WalletHUD.");
    }

    private static Image GetOrCreateIcon(Transform parent)
    {
        Transform child = parent.Find("CurrencyIcon");
        GameObject iconObject = child != null ? child.gameObject : new GameObject("CurrencyIcon", typeof(RectTransform), typeof(Image));
        if (child == null)
        {
            iconObject.transform.SetParent(parent, false);
        }

        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(72f, 48f);
        return iconObject.GetComponent<Image>();
    }

    private static TMP_Text GetOrCreateAmount(Transform parent)
    {
        Transform child = parent.Find("CurrencyAmount");
        GameObject amountObject = child != null ? child.gameObject : new GameObject("CurrencyAmount", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (child == null)
        {
            amountObject.transform.SetParent(parent, false);
        }

        RectTransform rect = amountObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(88f, 0f);
        rect.offsetMax = Vector2.zero;
        TextMeshProUGUI text = amountObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.fontSize = 28f;
        text.color = Color.white;
        return text;
    }

    private static Sprite FindCurrencySprite()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SpritePath);
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite && sprite.name == "Uang_0")
            {
                return sprite;
            }
        }

        Debug.LogError($"Sprite Uang_0 was not found at {SpritePath}.");
        return null;
    }
}
#endif
