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
        // The service table begins processing the order immediately.
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
