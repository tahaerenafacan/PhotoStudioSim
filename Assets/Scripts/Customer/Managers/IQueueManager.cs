using UnityEngine;

public interface IQueueManager
{
    bool HasQueuePositions { get; }
    void RegisterQueueEntry(CustomerController customer);
    void LeaveQueue(CustomerController customer);
    bool IsFirstInQueue(CustomerController customer);
    Transform GetQueuePosition(CustomerController customer);
}
