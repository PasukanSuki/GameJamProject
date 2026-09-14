using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeManager : MonoBehaviour
{
    private const string BagCapacityResourcePath = "Upgrades/BagCapacity";
    private const string IncomeResourcePath = "Upgrades/Income";

    [SerializeField] private Player player;
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();

    private readonly Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();

    public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;
    public event Action<UpgradeDefinition, int> UpgradePurchased;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
        if (wallet == null) wallet = GetComponent<CurrencyWallet>();
        if (inventory == null) inventory = GetComponent<InventoryManager>();
        if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();
        RemoveDisabledUpgrades();
        EnsureDefaultUpgrades();
    }

    public int GetLevel(UpgradeDefinition upgrade)
    {
        return upgrade != null && upgradeLevels.TryGetValue(upgrade.UpgradeId, out int level) ? level : 0;
    }

    public bool IsMaxLevel(UpgradeDefinition upgrade)
    {
        return upgrade != null && GetLevel(upgrade) >= upgrade.MaxLevel;
    }

    public int GetNextPrice(UpgradeDefinition upgrade)
    {
        return upgrade == null ? 0 : upgrade.GetPriceForLevel(GetLevel(upgrade) + 1);
    }

    public float GetCurrentAmount(UpgradeDefinition upgrade)
    {
        return upgrade == null ? 0f : upgrade.GetCumulativeAmountForLevel(GetLevel(upgrade));
    }

    public float GetNextAmount(UpgradeDefinition upgrade)
    {
        return upgrade == null
            ? 0f
            : upgrade.GetCumulativeAmountForLevel(GetLevel(upgrade) + 1);
    }

    public bool CanPurchase(UpgradeDefinition upgrade)
    {
        return upgrade != null && player != null && wallet != null
            && !IsMaxLevel(upgrade) && wallet.CanAfford(GetNextPrice(upgrade));
    }

    public bool TryPurchase(UpgradeDefinition upgrade)
    {
        if (!CanPurchase(upgrade)) return false;

        int nextLevel = GetLevel(upgrade) + 1;
        if (!wallet.TrySpend(upgrade.GetPriceForLevel(nextLevel), CurrencyTransactionSource.Upgrade)) return false;

        upgradeLevels[upgrade.UpgradeId] = nextLevel;
        ApplyUpgradeLevel(upgrade, nextLevel);
        UpgradePurchased?.Invoke(upgrade, nextLevel);
        return true;
    }

    public void RestorePurchased(IEnumerable<string> legacyIds)
    {
        Dictionary<string, int> levels = new Dictionary<string, int>();
        if (legacyIds != null)
        {
            foreach (string upgradeId in legacyIds) levels[upgradeId] = 1;
        }

        RestoreLevels(levels);
    }

    public void RestoreLevels(Dictionary<string, int> savedLevels)
    {
        upgradeLevels.Clear();
        player?.ResetUpgradeStats();
        inventory?.ResetSlotCapacity();
        if (savedLevels == null) return;

        foreach (UpgradeDefinition upgrade in upgrades)
        {
            if (upgrade == null || !savedLevels.TryGetValue(upgrade.UpgradeId, out int level)) continue;

            int clampedLevel = Mathf.Clamp(level, 0, upgrade.MaxLevel);
            upgradeLevels[upgrade.UpgradeId] = clampedLevel;
            ApplyUpgradeLevel(upgrade, clampedLevel);
        }
    }

    public IReadOnlyDictionary<string, int> GetLevels()
    {
        return upgradeLevels;
    }

    private void ApplyUpgradeLevel(UpgradeDefinition upgrade, int level)
    {
        float totalAmount = upgrade.GetCumulativeAmountForLevel(level);
        switch (upgrade.UpgradeType)
        {
            case PlayerUpgradeType.BagCapacity:
                if (inventory == null)
                {
                    inventory = FindAnyObjectByType<InventoryManager>();
                }

                inventory?.SetSlotCapacityBonus(Mathf.RoundToInt(totalAmount));
                break;
            case PlayerUpgradeType.PickupSpeed:
                player.SetPickupSpeedBonus(totalAmount);
                break;
            case PlayerUpgradeType.FullbagBonus:
                player.SetFullBagBonusLevel(totalAmount);
                break;
            case PlayerUpgradeType.Income:
                player.SetIncomeLevel(totalAmount);
                break;
        }
    }

    private void EnsureDefaultUpgrades()
    {
        UpgradeDefinition bagCapacity = Resources.Load<UpgradeDefinition>(BagCapacityResourcePath);
        UpgradeDefinition income = Resources.Load<UpgradeDefinition>(IncomeResourcePath);

        AddIfMissing(
            bagCapacity != null
                ? bagCapacity
                : UpgradeDefinition.CreateRuntime("bag_capacity", "Bag Capacity", PlayerUpgradeType.BagCapacity, 800, 5f));
        AddIfMissing(
            income != null
                ? income
                : UpgradeDefinition.CreateRuntime("income", "Income", PlayerUpgradeType.Income, 650, 0.10f));

        if (bagCapacity == null)
        {
            Debug.LogWarning(
                "[PlayerUpgradeManager] BagCapacity definition was not found at Resources/Upgrades/BagCapacity. " +
                "Using runtime fallback price 800. Recreate the asset with Gedede/Economy/Create Bag Capacity Definition.",
                this);
        }

        if (income == null)
        {
            Debug.LogWarning(
                "[PlayerUpgradeManager] Income definition was not found at Resources/Upgrades/Income. " +
                "Using runtime fallback. Create the asset with Gedede/Economy/Create Income Definition.",
                this);
        }
    }

    private void AddIfMissing(UpgradeDefinition definition)
    {
        if (definition == null || upgrades.Contains(definition))
        {
            return;
        }

        foreach (UpgradeDefinition existing in upgrades)
        {
            if (existing != null && existing.UpgradeId == definition.UpgradeId)
            {
                return;
            }
        }

        upgrades.Add(definition);
    }

    private void RemoveDisabledUpgrades()
    {
        upgrades.RemoveAll(upgrade => upgrade == null
            || upgrade.UpgradeType == PlayerUpgradeType.PickupSpeed
            || upgrade.UpgradeType == PlayerUpgradeType.FullbagBonus);
    }
}
