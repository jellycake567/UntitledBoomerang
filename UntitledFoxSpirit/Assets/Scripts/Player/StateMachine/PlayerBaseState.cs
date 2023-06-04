
public abstract class PlayerBaseState
{
    protected PlayerStateMachine ctx;
    protected PlayerStateFactory factory;
    protected VariableScriptObject vso;

    public PlayerBaseState(PlayerStateMachine context, PlayerStateFactory factory, VariableScriptObject vso)
    {
        this.ctx = context;
        this.factory = factory;
        this.vso = vso;
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

        ctx.currentState = newState;
    }
    protected void SetSuperState() { }
    protected void SetSubState() { }

}
