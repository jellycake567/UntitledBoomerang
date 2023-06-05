
using System;
using UnityEngine;

public abstract class PlayerBaseState
{
    protected bool isRootState = false;
    protected PlayerStateMachine ctx;
    protected PlayerStateFactory factory;
    protected VariableScriptObject vso;
    protected PlayerBaseState currentSuperState;
    public PlayerBaseState currentSubState; // CHANGE TO PROTECTED LATER

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

    public void UpdateStates() 
    {
        UpdateState();
        if (currentSubState != null)
        {
            currentSubState.UpdateState();
        }
    }

    public void FixedUpdateStates()
    {
        FixedUpdateState();
        if (currentSubState != null)
        {
            currentSubState.FixedUpdateState();
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        newState.EnterState();

        if (isRootState)
        {
            ctx.currentState = newState;
        }
        else if (currentSuperState != null)
        {
            currentSuperState.SetSubState(newState);
        }
    }
    protected void SetSuperState(PlayerBaseState newSuperState) 
    {
        // Set parent state
        currentSuperState = newSuperState;
    }
    protected void SetSubState(PlayerBaseState newSubState) 
    {
        if (currentSubState != null && currentSubState != newSubState)
        {
            currentSubState.ExitState();
        }

        // Set child state
        currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }

}
