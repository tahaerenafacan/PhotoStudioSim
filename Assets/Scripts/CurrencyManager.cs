using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int startingBalance = 0;

    public event Action<int> OnBalanceChanged;
    public event Action<int> OnMoneyAdded;
    public event Action<int> OnMoneySpent;

    public int GetMoney() => money;
    public bool CanBuy(int amount) => money >= amount;

    private int money;
    
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        money = startingBalance;
    }

    public void AddCurrency(int amount)
    {
        money += amount;
        OnMoneyAdded?.Invoke(amount);
        OnBalanceChanged?.Invoke(money);
    }

    public void SpendCurrency(int amount)
    {
        if (!CanBuy(amount))
        {
            Debug.Log($"Attempted to spend {amount} currency, but only {money} available.");
            return;
        }

        money -= amount;
        OnMoneySpent?.Invoke(amount);
        OnBalanceChanged?.Invoke(money);
    }
}