public class DespawnState : CustomerStateBase
{
    public override string StateName => nameof(DespawnState);

    public DespawnState(CustomerController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        Controller.NotifyDespawning();
        UnityEngine.Object.Destroy(Controller.gameObject);
    }

    public override void Tick()
    {
    }

    public override void Exit()
    {
    }
}
