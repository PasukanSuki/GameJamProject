using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public static class CreateTrashPileScene
{
    private const string PrefabFolder = "Assets/Prefab";
    private const string PrefabPath = PrefabFolder + "/TrashPile.prefab";
    private const string VisualFolder = "Assets/TrashVisuals";
    private const string SpritePath = VisualFolder + "/TrashPickupSprite.asset";
    private const string HandTexturePath = "Assets/Sprites/Tangan.png";
    private const string PcbItemPath = "Assets/ScriptableObjects/PCB.asset";
    private const string IcItemPath = "Assets/ScriptableObjects/IC.asset";
    private const string WireItemPath = "Assets/ScriptableObjects/WIRE.asset";
    private const string CopperItemPath = "Assets/ScriptableObjects/COPPER.asset";

    [MenuItem("Tools/Gedede/Create Trash Pile")]
    public static void Create()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(VisualFolder);
        GameObject root = new GameObject("TrashPile");
        TrashPile trashPile = root.AddComponent<TrashPile>();
        SerializedObject serializedPile = new SerializedObject(trashPile);
        serializedPile.FindProperty("trashAmount").intValue = 100;

        GameObject pileShape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pileShape.name = "Tumpukan Sampah fase 1";
        pileShape.transform.SetParent(root.transform, false);
        pileShape.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        pileShape.transform.localScale = new Vector3(1.5f, 0.65f, 1.3f);
        Renderer pileRenderer = pileShape.GetComponent<Renderer>();
        pileRenderer.sharedMaterial = CreateMaterial("TrashPileMaterial", new Color(0.16f, 0.12f, 0.08f));
        MeshCollider stageOneCollider = pileShape.GetComponent<MeshCollider>();
        if (stageOneCollider == null)
        {
            Object.DestroyImmediate(pileShape.GetComponent<Collider>());
            stageOneCollider = pileShape.AddComponent<MeshCollider>();
        }

        GameObject stageTwo = Object.Instantiate(pileShape, root.transform);
        stageTwo.name = "Tumpukan Sampah fase 2";
        stageTwo.transform.localScale = pileShape.transform.localScale * 0.8f;
        MeshCollider stageTwoCollider = stageTwo.GetComponent<MeshCollider>();

        GameObject stageThree = Object.Instantiate(pileShape, root.transform);
        stageThree.name = "Tumpukan Sampah fase 3";
        stageThree.transform.localScale = pileShape.transform.localScale * 0.55f;
        MeshCollider stageThreeCollider = stageThree.GetComponent<MeshCollider>();

        serializedPile.FindProperty("stageOne").objectReferenceValue = pileShape.transform;
        serializedPile.FindProperty("stageTwo").objectReferenceValue = stageTwo.transform;
        serializedPile.FindProperty("stageThree").objectReferenceValue = stageThree.transform;
        serializedPile.FindProperty("stageOneCollider").objectReferenceValue = stageOneCollider;
        serializedPile.FindProperty("stageTwoCollider").objectReferenceValue = stageTwoCollider;
        serializedPile.FindProperty("stageThreeCollider").objectReferenceValue = stageThreeCollider;

        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.SetParent(root.transform, false);
        spawnPoint.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        serializedPile.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
        serializedPile.FindProperty("trashSprite").objectReferenceValue = CreateTrashSprite();
        SerializedProperty itemDrops = serializedPile.FindProperty("itemDrops");
        itemDrops.arraySize = 5;
        SetOrdinaryTrashDrop(itemDrops.GetArrayElementAtIndex(0));
        SetItemDrop(itemDrops.GetArrayElementAtIndex(1), PcbItemPath);
        SetItemDrop(itemDrops.GetArrayElementAtIndex(2), IcItemPath);
        SetItemDrop(itemDrops.GetArrayElementAtIndex(3), WireItemPath);
        SetItemDrop(itemDrops.GetArrayElementAtIndex(4), CopperItemPath);
        serializedPile.ApplyModifiedPropertiesWithoutUndo();

        stageTwo.SetActive(false);
        stageThree.SetActive(false);

        GameObject prefabRoot = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        if (prefabRoot == null)
        {
            throw new System.InvalidOperationException("Failed to create TrashPile prefab.");
        }

        GameObject scenePile = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
        scenePile.name = "TrashPile_Demo";
        scenePile.transform.position = new Vector3(0f, 0f, 4f);
        CreateTrashMachine();
        CreateBagCounter();
        CreateHandOverlay();
        Selection.activeGameObject = scenePile;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }

    private static void CreateTrashMachine()
    {
        GameObject existingMachine = GameObject.Find("TrashMachine_Demo");
        if (existingMachine != null)
        {
            Object.DestroyImmediate(existingMachine);
        }

        GameObject machine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        machine.name = "TrashMachine_Demo";
        machine.transform.position = new Vector3(2.5f, 0.75f, 2f);
        machine.transform.localScale = new Vector3(1.4f, 1.5f, 1.2f);
        machine.GetComponent<Renderer>().sharedMaterial = CreateMaterial(
            "TrashMachineMaterial",
            new Color(0.08f, 0.2f, 0.24f));
        machine.AddComponent<TrashMachine>();
    }

    private static void CreateBagCounter()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null || GameObject.Find("BagCounter") != null)
        {
            return;
        }

        GameObject counterObject = new GameObject("BagCounter");
        counterObject.transform.SetParent(canvas.transform, false);
        RectTransform rectTransform = counterObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(24f, -24f);
        rectTransform.sizeDelta = new Vector2(220f, 48f);

        TextMeshProUGUI text = counterObject.AddComponent<TextMeshProUGUI>();
        text.text = "Bag: 0/20";
        text.fontSize = 28f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        counterObject.AddComponent<BagCounter>();
    }

    private static void CreateHandOverlay()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        Texture2D handTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(HandTexturePath);
        if (canvas == null || handTexture == null || GameObject.Find("HandOverlay") != null)
        {
            return;
        }

        GameObject handObject = new GameObject("HandOverlay");
        handObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = handObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 0f);
        rectTransform.sizeDelta = new Vector2(720f, 360f);

        RawImage image = handObject.AddComponent<RawImage>();
        image.texture = handTexture;
        image.color = Color.white;
        image.raycastTarget = false;

        AspectRatioFitter aspectRatioFitter = handObject.AddComponent<AspectRatioFitter>();
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectRatioFitter.aspectRatio = (float)handTexture.width / handTexture.height;

        HandOverlay overlay = handObject.AddComponent<HandOverlay>();
        SerializedObject serializedOverlay = new SerializedObject(overlay);
        serializedOverlay.FindProperty("handImage").objectReferenceValue = image;
        serializedOverlay.FindProperty("handTexture").objectReferenceValue = handTexture;
        serializedOverlay.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureFolder(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string folderName = System.IO.Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder("Assets", folderName);
        }
    }

    private static void SetItemDrop(SerializedProperty drop, string itemPath)
    {
        drop.FindPropertyRelative("ordinaryTrash").boolValue = false;
        drop.FindPropertyRelative("item").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
        drop.FindPropertyRelative("obtainChance").floatValue = 5f;
    }

    private static void SetOrdinaryTrashDrop(SerializedProperty drop)
    {
        drop.FindPropertyRelative("ordinaryTrash").boolValue = true;
        drop.FindPropertyRelative("item").objectReferenceValue = null;
        drop.FindPropertyRelative("obtainChance").floatValue = 80f;
    }

    private static Sprite CreateTrashSprite()
    {
        Sprite existingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (existingSprite != null)
        {
            return existingSprite;
        }

        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        texture.name = "TrashPickupTexture";
        texture.filterMode = FilterMode.Point;
        Color[] pixels = new Color[texture.width * texture.height];
        for (int index = 0; index < pixels.Length; index++)
        {
            int x = index % texture.width;
            int y = index / texture.width;
            bool visible = (x - 16) * (x - 16) + (y - 16) * (y - 16) < 190;
            pixels[index] = visible
                ? new Color(0.2f + (x % 4) * 0.06f, 0.38f, 0.12f, 1f)
                : Color.clear;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        AssetDatabase.CreateAsset(texture, VisualFolder + "/TrashPickupTexture.asset");
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
        sprite.name = "TrashPickupSprite";
        AssetDatabase.CreateAsset(sprite, SpritePath);
        AssetDatabase.SaveAssets();
        return sprite;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = name;
        material.color = color;
        return material;
    }

}
