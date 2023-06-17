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

    public override void FixedUpdateState() { }

    public override void OnAnimatorMoveState() { }

    public override void ExitState() { }
    public override void InitializeSubState() 
    {
        if (ctx.isHeavyLand)
        {
            SetSubState(factory.Land());
        }
        else if (!ctx.input.isMovementHeld)
        {
            SetSubState(factory.Idle());
        }
        else
        {
            SetSubState(factory.Walk());
        }
    }

    public override void CheckSwitchState()
    {
        if (ctx.jumpBufferCounter > 0f || !ctx.isGrounded && !ctx.isDashing)
        {
            if (ctx.isHeavyLand)
            {
                return;
            }

            SwitchState(factory.Jump());
        }
    }
}
