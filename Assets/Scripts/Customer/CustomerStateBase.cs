using UnityEngine;

public abstract class CustomerStateBase : ICustomerState
{
    protected readonly CustomerController Controller;

    protected CustomerStateBase(CustomerController controller)
    {
        Controller = controller;
    }

    public abstract string StateName { get; }
    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();

    protected bool HasReachedDestination => Controller.HasReachedDestination;

    protected void SetDestination(Vector3 target)
    {
        Controller.SetDestination(target);
    }

    protected void StopMovement()
    {
        Controller.StopMovement();
    }
}
