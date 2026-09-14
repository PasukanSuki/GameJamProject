#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateUpgradeDefinitionAsset
{
    private const string FolderPath = "Assets/Resources/Upgrades";

    [MenuItem("Gedede/Economy/Create Bag Capacity Definition")]
    public static void CreateBagCapacityDefinition()
    {
        CreateDefinition("BagCapacity", "bag_capacity", "Bag Capacity", "Increase inventory slot capacity.", PlayerUpgradeType.BagCapacity, 800, 5f);
    }

    [MenuItem("Gedede/Economy/Create Income Definition")]
    public static void CreateIncomeDefinition()
    {
        CreateDefinition("Income", "income", "Income", "Increase deposit income.", PlayerUpgradeType.Income, 650, 0.10f);
    }

    private static void CreateDefinition(string fileName, string id, string displayName, string description, PlayerUpgradeType type, int price, float amount)
    {
        EnsureFolder(FolderPath);
        string assetPath = FolderPath + "/" + fileName + ".asset";
        UpgradeDefinition definition = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(assetPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<UpgradeDefinition>();
            AssetDatabase.CreateAsset(definition, assetPath);
        }

        SerializedObject serializedDefinition = new SerializedObject(definition);
        serializedDefinition.FindProperty("upgradeId").stringValue = id;
        serializedDefinition.FindProperty("displayName").stringValue = displayName;
        serializedDefinition.FindProperty("description").stringValue = description;
        serializedDefinition.FindProperty("upgradeType").enumValueIndex = (int)type;
        serializedDefinition.FindProperty("maxLevel").intValue = 7;
        serializedDefinition.FindProperty("price").intValue = price;
        serializedDefinition.FindProperty("amountPerLevel").floatValue = amount;
        serializedDefinition.FindProperty("priceGrowth").floatValue = 1.35f;
        serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(definition);
        AssetDatabase.SaveAssets();
        Selection.activeObject = definition;
        Debug.Log($"{displayName} definition created at {assetPath}. Configure prices, amounts, and icons in the Inspector.", definition);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentPath = System.IO.Path.GetDirectoryName(folderPath).Replace('\\', '/');
        EnsureFolder(parentPath);
        string folderName = System.IO.Path.GetFileName(folderPath);
        AssetDatabase.CreateFolder(parentPath, folderName);
    }
}
#endif
