public class ApproachEntranceState : CustomerStateBase
{
    public override string StateName => nameof(ApproachEntranceState);

    public ApproachEntranceState(CustomerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        if (Controller.EntranceTarget == null)
        {
            UnityEngine.Debug.LogError("Customer has no entrance target assigned.", Controller);
            return;
        }

        SetDestination(Controller.EntranceTarget.position);
        UnityEngine.Debug.Log($"ApproachEntranceState: {Controller.name} entering approach entrance state", Controller);
    }

    public override void Tick()
    {
        if (!HasReachedDestination)
        {
            return;
        }

        if (Controller.QueueManager != null && Controller.QueueManager.HasQueuePositions)
        {
            Controller.QueueManager.RegisterQueueEntry(Controller);
            Controller.CustomerData.QueueEnterAt = UnityEngine.Time.time;
            Controller.CustomerData.QueueLeaveAt = 0f;
            Controller.StateMachine.SetState(new JoinQueueState(Controller));
            UnityEngine.Debug.Log($"ApproachEntranceState: {Controller.name} joining queue", Controller);
        }
        else
        {
            Controller.StateMachine.SetState(new MoveToServiceState(Controller));
            UnityEngine.Debug.Log($"ApproachEntranceState: {Controller.name} moving to service (no queue)", Controller);
        }
    }

    public override void Exit()
    {
    }
}
