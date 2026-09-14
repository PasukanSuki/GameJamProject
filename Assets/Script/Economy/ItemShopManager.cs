using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemShopManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private CurrencyWallet wallet;
    [SerializeField] private List<ItemShopDefinition> products = new List<ItemShopDefinition>();

    public IReadOnlyList<ItemShopDefinition> Products => products;
    public event Action<ItemShopDefinition> ItemPurchased;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventoryManager>();
        }

        if (wallet == null)
        {
            wallet = FindAnyObjectByType<CurrencyWallet>();
        }
    }

    public bool CanPurchase(ItemShopDefinition product)
    {
        return product != null
            && inventory != null
            && wallet != null
            && product.Item != null
            && wallet.CanAfford(product.Price)
            && inventory.CanAddItem(product.Item, product.Quantity);
    }

    public bool TryPurchase(ItemShopDefinition product)
    {
        if (!CanPurchase(product))
        {
            return false;
        }

        if (!wallet.TrySpend(product.Price, CurrencyTransactionSource.ItemPurchase))
        {
            return false;
        }

        if (!inventory.AddItem(product.Item, product.Quantity))
        {
            wallet.TryAdd(product.Price, CurrencyTransactionSource.ItemPurchase);
            return false;
        }

        ItemPurchased?.Invoke(product);
        return true;
    }
}
