
public abstract class PlayerBaseState
{
    protected PlayerStateMachine context;
    protected PlayerStateFactory factory;
    public PlayerBaseState(PlayerStateMachine context, PlayerStateFactory factory)
    {
        this.context = context;
        this.factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public abstract void InitializeSubState();
    
    void UpdateStates() { }
    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        newState.EnterState();

        context.currentState = newState;
    }
    protected void SetSuperState() { }
    protected void SetSubState() { }

}
