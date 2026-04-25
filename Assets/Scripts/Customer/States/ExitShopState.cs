public class ExitShopState : CustomerStateBase
{
    public override string StateName => nameof(ExitShopState);

    public ExitShopState(CustomerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        if (Controller.ExitTarget == null)
        {
            UnityEngine.Debug.LogError("Customer has no exit target assigned.", Controller);
            return;
        }

        SetDestination(Controller.ExitTarget.position);
    }

    public override void Tick()
    {
        if (!HasReachedDestination) return;
        
        Controller.StateMachine.SetState(new DespawnState(Controller));
    }

    public override void Exit()
    {
    }
}
