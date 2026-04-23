using UnityEngine;

public class CustomerStateMachine
{
    public ICustomerState CurrentState { get; private set; }
    private readonly CustomerController controller;

    public CustomerStateMachine(CustomerController controller)
    {
        this.controller = controller;
    }

    public void Update()
    {
        CurrentState?.Tick();
    }

    public void SetState(ICustomerState nextState)
    {
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        CurrentState = nextState;

        if (CurrentState != null)
        {
            CurrentState.Enter();
        }
        Debug.Log($"CustomerStateMachine: {controller.name} transitioned to state {CurrentState?.GetType().Name}", controller);
    }
}
