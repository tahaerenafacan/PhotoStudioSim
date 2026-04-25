public class JoinQueueState : CustomerStateBase
{
    public override string StateName => nameof(JoinQueueState);

    public JoinQueueState(CustomerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        var queuePosition = Controller.QueueManager.GetQueuePosition(Controller);
        if (queuePosition == null)
        {
            Controller.StateMachine.SetState(new MoveToServiceState(Controller));
            UnityEngine.Debug.Log($"JoinQueueState: {Controller.name} no queue position available, moving to service", Controller);
            return;
        }

        SetDestination(queuePosition.position);
    }

    public override void Tick()
    {
        if (!HasReachedDestination) return;
        
        // Stop walking animation when waiting in queue
        StopMovement();

        if (!Controller.QueueManager.IsFirstInQueue(Controller))
        {
            var queuePosition = Controller.QueueManager.GetQueuePosition(Controller);
            if (queuePosition != null)
            {
                SetDestination(queuePosition.position);
            }

            return;
        }

        Controller.CustomerData.QueueLeaveAt = UnityEngine.Time.time;
        Controller.QueueManager.LeaveQueue(Controller);
        Controller.StateMachine.SetState(new MoveToServiceState(Controller));
    }

    public override void Exit()
    {
    }
}
