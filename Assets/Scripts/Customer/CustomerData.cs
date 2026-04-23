using System;

[Serializable]
public class CustomerData
{
    public string CustomerId = Guid.NewGuid().ToString();
    public OrderData AssignedOrder;
    public OrderResult OrderResult;
    public float OrderCreatedAt;
    public float ServiceStartedAt;
    public float ServiceCompletedAt;
    public float TableReservedAt;
    public float QueueEnterAt;
    public float QueueLeaveAt;

    public float WaitDuration => QueueLeaveAt > QueueEnterAt ? QueueLeaveAt - QueueEnterAt : 0f;
    public float ServiceDuration => ServiceCompletedAt > ServiceStartedAt ? ServiceCompletedAt - ServiceStartedAt : 0f;
}
