using System;
using UnityEngine;

public interface IServiceTable
{
    bool IsAvailable { get; }
    Transform ServicePoint { get; }
    bool TryReserve(CustomerController customer);
    void Release();
    void BeginService(OrderData order, Action<OrderResult> onServiceComplete);
    void CompleteService();
}
