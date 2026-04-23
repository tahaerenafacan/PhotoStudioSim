using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour, IQueueManager
{
    [SerializeField] private List<Transform> queuePositions = new();
    private readonly List<CustomerController> queuedCustomers = new();

    public bool HasQueuePositions => queuePositions.Count > 0;

    public void RegisterQueueEntry(CustomerController customer)
    {
        if (customer == null || queuedCustomers.Contains(customer))
        {
            return;
        }

        if (queuedCustomers.Count >= queuePositions.Count)
        {
            Debug.LogWarning("Queue is full. Customer will wait until a position opens.", customer);
            return;
        }

        queuedCustomers.Add(customer);
    }

    public void LeaveQueue(CustomerController customer)
    {
        if (customer == null)
        {
            return;
        }

        if (queuedCustomers.Remove(customer))
        {
            AssignQueuePositions();
        }
    }

    public bool IsFirstInQueue(CustomerController customer)
    {
        return queuedCustomers.Count > 0 && queuedCustomers[0] == customer;
    }

    public Transform GetQueuePosition(CustomerController customer)
    {
        var index = queuedCustomers.IndexOf(customer);
        if (index < 0 || index >= queuePositions.Count)
        {
            return null;
        }

        return queuePositions[index];
    }

    private void AssignQueuePositions()
    {
        for (var index = 0; index < queuedCustomers.Count; index++)
        {
            var customer = queuedCustomers[index];
            var target = queuePositions[Mathf.Min(index, queuePositions.Count - 1)];
            customer.SetDestination(target.position);
        }
    }
}
