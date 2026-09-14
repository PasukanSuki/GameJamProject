using System;
using System.Collections.Generic;

[Serializable]
public class EconomySaveData
{
    public int walletBalance;
    public List<string> purchasedUpgradeIds = new List<string>();
    public List<UpgradeSaveEntry> upgradeLevels = new List<UpgradeSaveEntry>();
    public List<InventorySaveEntry> inventory = new List<InventorySaveEntry>();
}

[Serializable]
public class UpgradeSaveEntry
{
    public string upgradeId;
    public int level;
}

[Serializable]
public class InventorySaveEntry
{
    public string itemId;
    public int quantity;
}
