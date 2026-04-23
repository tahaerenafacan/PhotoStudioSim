using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private QuestHUD questHUD;

    private readonly Dictionary<string, OrderData> registeredOrders = new();

    public event Action<OrderData> OnOrderRegistered;
    public event Action<OrderResult> OnOrderCompleted;

    public void RegisterOrder(OrderData order)
    {
        if (order == null)
        {
            Debug.LogWarning("Cannot register a null order.");
            return;
        }

        if (registeredOrders.ContainsKey(order.OrderId))
        {
            return;
        }

        registeredOrders.Add(order.OrderId, order);
        OnOrderRegistered?.Invoke(order);
        questHUD.SetQuestDisplay(order);
    }

    public void CompleteOrder(OrderResult result)
    {
        if (result == null)
        {
            Debug.LogWarning("Cannot complete a null order result.");
            return;
        }

        if (!registeredOrders.ContainsKey(result.OrderId))
        {
            Debug.LogWarning($"Order result received for unknown order id: {result.OrderId}");
        }

        OnOrderCompleted?.Invoke(result);
        questHUD.CloseQuestDisplay();
    }

    public OrderData GetOrder(string orderId)
    {
        registeredOrders.TryGetValue(orderId, out var order);
        return order;
    }
}
