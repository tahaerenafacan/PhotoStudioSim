public interface ICustomerState
{
    void Enter();
    void Tick();
    void Exit();
    string StateName { get; }
}
