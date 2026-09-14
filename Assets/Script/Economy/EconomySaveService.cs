using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EconomySaveService : MonoBehaviour
{
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private PlayerUpgradeManager upgrades;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private List<ItemDefinition> itemCatalog = new List<ItemDefinition>();
    [SerializeField] private string saveFileName = "economy-save.json";

    private bool isLoading;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (wallet == null) wallet = GetComponent<CurrencyWallet>();
        if (upgrades == null) upgrades = GetComponent<PlayerUpgradeManager>();
        if (inventory == null) inventory = GetComponent<InventoryManager>();
        if (wallet == null) wallet = FindAnyObjectByType<CurrencyWallet>();
        if (upgrades == null) upgrades = FindAnyObjectByType<PlayerUpgradeManager>();
        if (inventory == null) inventory = FindAnyObjectByType<InventoryManager>();
    }

    private void OnEnable()
    {
        if (wallet != null) wallet.BalanceChanged += HandleStateChanged;
        if (upgrades != null) upgrades.UpgradePurchased += HandleUpgradePurchased;
        if (inventory != null) inventory.InventoryChanged += HandleStateChanged;
    }

    private void Start()
    {
        Load();
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.BalanceChanged -= HandleStateChanged;
        if (upgrades != null) upgrades.UpgradePurchased -= HandleUpgradePurchased;
        if (inventory != null) inventory.InventoryChanged -= HandleStateChanged;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Save()
    {
        if (isLoading || wallet == null)
        {
            return;
        }

        EconomySaveData data = new EconomySaveData
        {
            walletBalance = wallet.Balance
        };

        if (upgrades != null)
        {
            foreach (KeyValuePair<string, int> entry in upgrades.GetLevels())
            {
                data.upgradeLevels.Add(new UpgradeSaveEntry
                {
                    upgradeId = entry.Key,
                    level = entry.Value
                });
            }
        }

        if (inventory != null)
        {
            for (int index = 0; index < inventory.SlotCapacity; index++)
            {
                ItemStack slot = inventory.GetSlot(index);
                if (slot == null || slot.IsEmpty || slot.item == null || string.IsNullOrWhiteSpace(slot.item.ItemId))
                {
                    continue;
                }

                data.inventory.Add(new InventorySaveEntry
                {
                    itemId = slot.item.ItemId,
                    quantity = slot.quantity
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath) || wallet == null)
        {
            return;
        }

        try
        {
            isLoading = true;
            EconomySaveData data = JsonUtility.FromJson<EconomySaveData>(File.ReadAllText(SavePath));
            if (data == null)
            {
                return;
            }

            wallet.SetBalance(Mathf.Max(0, data.walletBalance));
            if (upgrades != null)
            {
                Dictionary<string, int> savedLevels = new Dictionary<string, int>();
                foreach (UpgradeSaveEntry entry in data.upgradeLevels ?? new List<UpgradeSaveEntry>())
                {
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.upgradeId))
                    {
                        savedLevels[entry.upgradeId] = entry.level;
                    }
                }

                if (savedLevels.Count > 0)
                {
                    upgrades.RestoreLevels(savedLevels);
                }
                else
                {
                    upgrades.RestorePurchased(data.purchasedUpgradeIds);
                }
            }

            if (inventory != null)
            {
                inventory.Clear();
                    foreach (InventorySaveEntry entry in data.inventory ?? new List<InventorySaveEntry>())
                {
                    ItemDefinition item = itemCatalog.Find(candidate => candidate != null && candidate.ItemId == entry.itemId);
                    if (item != null && entry.quantity > 0)
                    {
                        inventory.RestoreItem(item, entry.quantity);
                    }
                }
            }
        }
        catch (IOException)
        {
            Debug.LogWarning($"Unable to load economy save at {SavePath}.");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void HandleStateChanged(int balance)
    {
        Save();
    }

    private void HandleStateChanged()
    {
        Save();
    }

    private void HandleUpgradePurchased(UpgradeDefinition upgrade, int level)
    {
        Save();
    }
}