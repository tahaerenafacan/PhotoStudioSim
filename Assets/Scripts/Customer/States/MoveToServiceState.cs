public class MoveToServiceState : CustomerStateBase
{
    public override string StateName => nameof(MoveToServiceState);

    private ServiceTable reservedServiceTable;

    public MoveToServiceState(CustomerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        TryReserveTable();
    }

    public override void Tick()
    {
        if (reservedServiceTable == null)
        {
            TryReserveTable();
            return;
        }

        if (!HasReachedDestination)
        {
            return;
        }

        // Stop walking animation when waiting at service table
        StopMovement();
        
        Controller.StateMachine.SetState(new CreateOrderState(Controller, reservedServiceTable));
    }

    public override void Exit()
    {
    }

    private void TryReserveTable()
    {
        reservedServiceTable = Controller.ServiceTableManager.TryReserveTable(Controller);
        if (reservedServiceTable == null)
        {
            return;
        }

        Controller.ReserveServiceTable(reservedServiceTable);
        SetDestination(reservedServiceTable.ServicePoint.position);
    }
}
