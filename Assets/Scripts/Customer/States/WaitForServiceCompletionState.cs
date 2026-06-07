public class WaitForServiceCompletionState : CustomerStateBase
{
    public override string StateName => nameof(WaitForServiceCompletionState);

    private readonly ServiceTable serviceTable;

    public WaitForServiceCompletionState(CustomerController controller, ServiceTable serviceTable) : base(controller)
    {
        this.serviceTable = serviceTable;
    }

    public override void Enter()
    {
        // Stop walking animation when waiting for service
        StopMovement();
    }

    public override void Tick()
    {
        if (Controller.CustomerData.OrderResult == null)
        {
            return;
        }

        serviceTable.Release();
        Controller.BeginExit();
    }

    public override void Exit()
    {
    }
}
