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
    }

    public override void Tick()
    {
        if (!HasReachedDestination) return;
        

        if (Controller.QueueManager != null && Controller.QueueManager.HasQueuePositions)
        {
            Controller.QueueManager.RegisterQueueEntry(Controller);
            Controller.CustomerData.QueueEnterAt = UnityEngine.Time.time;
            Controller.CustomerData.QueueLeaveAt = 0f;
            Controller.StateMachine.SetState(new JoinQueueState(Controller));
        }
        else
        {
            Controller.StateMachine.SetState(new MoveToServiceState(Controller));
        }
    }

    public override void Exit()
    {
    }
}
