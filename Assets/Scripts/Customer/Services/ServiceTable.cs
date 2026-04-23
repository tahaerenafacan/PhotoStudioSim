using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

public class ServiceTable : MonoBehaviour, IServiceTable
{
    [SerializeField] private Transform servicePoint;

    private CustomerController reservedCustomer;
    private bool isBusy;
    private OrderData currentOrder;
    private Action<OrderResult> onServiceCompleteCallback;

    public bool IsAvailable => reservedCustomer == null && !isBusy;
    public Transform ServicePoint => servicePoint != null ? servicePoint : transform;

    public bool TryReserve(CustomerController customer)
    {
        if (!IsAvailable)
        {
            return false;
        }

        reservedCustomer = customer;
        return true;
    }

    public void Release()
    {
        reservedCustomer = null;
        isBusy = false;
        currentOrder = null;
        onServiceCompleteCallback = null;
    }

    public void BeginService(OrderData order, Action<OrderResult> onServiceComplete)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        if (reservedCustomer == null)
        {
            throw new InvalidOperationException("Service table must be reserved before beginning service.");
        }

        if (isBusy)
        {
            return;
        }

        isBusy = true;
        currentOrder = order;
        onServiceCompleteCallback = onServiceComplete;
    }

    public void CompleteService()
    {
        if (currentOrder == null || onServiceCompleteCallback == null)
        {
            return;
        }

        var result = new OrderResult
        {
            OrderId = currentOrder.OrderId,
            CompletedSuccessfully = true,
            AccuracyScore = UnityEngine.Random.Range(0.8f, 1f),
            MaterialQualityScore = UnityEngine.Random.Range(0.75f, 1f),
            CompletedAt = Time.time
        };

        onServiceCompleteCallback?.Invoke(result);
        Release();
    }
}
