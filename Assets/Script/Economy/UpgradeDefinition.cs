using System.Collections.Generic;
using UnityEngine;

public enum PlayerUpgradeType
{
    BagCapacity,
    PickupSpeed,
    FullbagBonus,
    Income
}

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Gedede/Economy/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    [SerializeField] private string upgradeId;
    [SerializeField] private string displayName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private PlayerUpgradeType upgradeType;
    [HideInInspector]
    [SerializeField] private Sprite icon;
    [HideInInspector]
    [SerializeField, Min(1)] private int price = 100;
    [SerializeField, Min(1)] private int maxLevel = 7;
    [HideInInspector]
    [SerializeField, Min(0.01f)] private float amountPerLevel = 1f;
    [HideInInspector]
    [SerializeField, Min(1f)] private float priceGrowth = 1.35f;
    [Tooltip("Optional explicit price for each purchased level, starting at level 1.")]
    [SerializeField] private List<int> pricesByLevel = new List<int>();
    [Tooltip("Optional slot/value increase for each purchased level, starting at level 1.")]
    [SerializeField] private List<float> amountsByLevel = new List<float>();
    [Tooltip("Icon at index 0 is the unpurchased state; index N is shown at level N.")]
    [SerializeField] private List<Sprite> iconsByLevel = new List<Sprite>();
    [SerializeField] private int sortOrder;

    public string UpgradeId => string.IsNullOrWhiteSpace(upgradeId) ? name : upgradeId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public string Description => description;
    public PlayerUpgradeType UpgradeType => upgradeType;
    public Sprite Icon => icon;
    public int Price => Mathf.Max(1, price);
    public int MaxLevel => Mathf.Max(1, maxLevel);
    public float AmountPerLevel => Mathf.Max(0.01f, amountPerLevel);
    public int SortOrder => sortOrder;

    public float GetAmountForLevel(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 1, MaxLevel);
        int index = clampedLevel - 1;
        if (index >= 0 && index < amountsByLevel.Count && amountsByLevel[index] > 0f)
        {
            return amountsByLevel[index];
        }

        return AmountPerLevel;
    }

    public float GetCumulativeAmountForLevel(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 0, MaxLevel);
        float total = 0f;
        for (int currentLevel = 1; currentLevel <= clampedLevel; currentLevel++)
        {
            total += GetAmountForLevel(currentLevel);
        }

        return total;
    }

    public Sprite GetIconForLevel(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 0, MaxLevel);
        if (clampedLevel < iconsByLevel.Count && iconsByLevel[clampedLevel] != null)
        {
            return iconsByLevel[clampedLevel];
        }

        return icon;
    }

    public int GetPriceForLevel(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 1, MaxLevel);
        int index = clampedLevel - 1;
        if (index >= 0 && index < pricesByLevel.Count && pricesByLevel[index] > 0)
        {
            return pricesByLevel[index];
        }

        return Mathf.Max(1, Mathf.RoundToInt(Price * Mathf.Pow(Mathf.Max(1f, priceGrowth), clampedLevel - 1)));
    }

    public static UpgradeDefinition CreateRuntime(string id, string label, PlayerUpgradeType type, int basePrice, float amount)
    {
        UpgradeDefinition definition = CreateInstance<UpgradeDefinition>();
        definition.name = id;
        definition.upgradeId = id;
        definition.displayName = label;
        definition.upgradeType = type;
        definition.price = basePrice;
        definition.amountPerLevel = amount;
        definition.maxLevel = 7;
        definition.priceGrowth = 1.35f;
        return definition;
    }
}
