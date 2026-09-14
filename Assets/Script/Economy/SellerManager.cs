using System;
using System.Collections.Generic;
using UnityEngine;

public class SellerManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private CurrencyWallet wallet;

    public event Action<ItemDefinition, int, int> ItemSold;

    private void Awake()
    {
        ResolveReferences();
    }

    public void ResolveReferences()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventoryManager>();
        }

        if (wallet == null)
        {
            Player player = FindAnyObjectByType<Player>();
            wallet = player != null ? player.GetComponent<CurrencyWallet>() : FindAnyObjectByType<CurrencyWallet>();
        }
    }

    public IReadOnlyList<ItemDefinition> GetSellableItems()
    {
        List<ItemDefinition> items = new List<ItemDefinition>();
        if (inventory == null)
        {
            return items;
        }

        foreach (ItemStack slot in inventory.Slots)
        {
            if (slot == null || slot.IsEmpty || slot.item == null || slot.item.SellValue <= 0 || items.Contains(slot.item))
            {
                continue;
            }

            items.Add(slot.item);
        }

        return items;
    }

    public int GetItemCount(ItemDefinition item)
    {
        return inventory != null ? inventory.GetItemCount(item) : 0;
    }

    public bool CanSell(ItemDefinition item, int quantity)
    {
        if (item == null || item.SellValue <= 0 || quantity <= 0 || inventory == null || wallet == null)
        {
            return false;
        }

        long totalValue = (long)item.SellValue * quantity;
        return totalValue <= int.MaxValue
            && inventory.GetItemCount(item) >= quantity
            && wallet.Balance <= int.MaxValue - (int)totalValue;
    }

    public bool TrySell(ItemDefinition item, int quantity)
    {
        if (!CanSell(item, quantity) || !inventory.RemoveItem(item, quantity))
        {
            return false;
        }

        int totalValue = item.SellValue * quantity;
        if (wallet.TryAdd(totalValue, CurrencyTransactionSource.ItemSale))
        {
            ItemSold?.Invoke(item, quantity, totalValue);
            return true;
        }

        inventory.RestoreItem(item, quantity);
        return false;
    }
}