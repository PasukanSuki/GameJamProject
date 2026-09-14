using System;
using UnityEngine;

public enum CurrencyTransactionSource
{
    Unknown,
    Deposit,
    Upgrade,
    ItemPurchase,
    ItemSale,
    SaveLoad
}

public class CurrencyWallet : MonoBehaviour
{
    [SerializeField, Min(0)] private int startingBalance;

    private int balance;
    private bool hasInitializedBalance;

    public int Balance => balance;
    public event Action<int> BalanceChanged;
    public event Action<int, CurrencyTransactionSource> TransactionCompleted;

    private void Awake()
    {
        if (!hasInitializedBalance)
        {
            balance = Mathf.Max(0, startingBalance);
            hasInitializedBalance = true;
        }
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && balance >= amount;
    }

    public bool TrySpend(int amount, CurrencyTransactionSource source)
    {
        if (amount <= 0 || !CanAfford(amount))
        {
            return false;
        }

        balance -= amount;
        NotifyChanged(source);
        return true;
    }

    public bool TryAdd(int amount, CurrencyTransactionSource source)
    {
        if (amount <= 0 || balance > int.MaxValue - amount)
        {
            return false;
        }

        balance += amount;
        NotifyChanged(source);
        return true;
    }

    public void SetBalance(int value)
    {
        balance = Mathf.Max(0, value);
        hasInitializedBalance = true;
        BalanceChanged?.Invoke(balance);
    }

    private void NotifyChanged(CurrencyTransactionSource source)
    {
        BalanceChanged?.Invoke(balance);
        TransactionCompleted?.Invoke(balance, source);
    }
}
