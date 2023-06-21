﻿
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
    public abstract void OnAnimatorMoveState();
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

    public void OnAnimatorMoveStates()
    {
        OnAnimatorMoveState();
        if (currentSubState != null)
        {
            currentSubState.OnAnimatorMoveState();
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        newState.EnterState();

        if (isRootState)
        {
            // new root state, substate is null
            if (currentSubState != null && newState.currentSubState != null)
            {
                // Exit roots substates
                if (currentSubState.ToString() != newState.currentSubState.ToString())
                {
                    currentSubState.ExitState();
                }
            }

            ctx.currentState = newState;
        }
        else if (currentSuperState != null)
        {
            currentSuperState.SetSubState(newState);
        }
    }
    protected void SetSuperState(PlayerBaseState newSuperState) 
    {
        currentSuperState = newSuperState;
    }
    protected void SetSubState(PlayerBaseState newSubState)
    {
        // Sets the supers sub state
        currentSubState = newSubState;

        // Set the sub's super state
        newSubState.SetSuperState(this);
    }

}
