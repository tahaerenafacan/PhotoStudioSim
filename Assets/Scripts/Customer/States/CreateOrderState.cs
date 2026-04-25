using System;

public class CreateOrderState : CustomerStateBase
{
    public override string StateName => nameof(CreateOrderState);

    private readonly ServiceTable serviceTable;

    public CreateOrderState(CustomerController controller, ServiceTable serviceTable) : base(controller)
    {
        this.serviceTable = serviceTable;
    }

    public override void Enter()
    {
        if (serviceTable == null)
        {
            Controller.StateMachine.SetState(new ExitShopState(Controller));
            UnityEngine.Debug.LogWarning($"CreateOrderState: {Controller.name} service table is null, exiting shop", Controller);
            return;
        }

        if (Controller.CustomerData.AssignedOrder == null)
        {
            Controller.CreateOrder();
        }

        Controller.NotifyServiceStarted();
        serviceTable.BeginService(Controller.CustomerData.AssignedOrder, Controller.NotifyServiceCompleted);
        Controller.StateMachine.SetState(new WaitForServiceCompletionState(Controller, serviceTable));
    }

    public override void Tick()
    {
    }

    public override void Exit()
    {
    }
}
