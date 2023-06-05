using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso)
    {
        isRootState = true;
        InitializeSubState();
    }

    public override void EnterState() 
    {
        Debug.Log("Grounded State");
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
        
    }
    public override void ExitState() { }
    public override void InitializeSubState() 
    {
        if (!ctx.input.isMovementHeld)
        {
            SetSubState(factory.Idle());
        }
        else if (ctx.input.isInputDashPressed && ctx.currentDashCooldown <= 0f)
        {
            SetSubState(factory.Dash());
        }
        else
        {
            SetSubState(factory.Walk());
        }
    }
    public override void CheckSwitchState()
    {
        if (ctx.input.isInputJumpPressed || !ctx.isGrounded)
        {
            SwitchState(factory.Jump());
        }
    }
}
