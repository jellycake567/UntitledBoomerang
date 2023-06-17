using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState() 
    {
        Debug.Log("Idle State");
        ctx.animController.SetBool("isMoving", false);
        ctx.animController.SetFloat("ForwardSpeed", 0f);
    }
    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }
    public override void CheckSwitchState() 
    {
        if (ctx.input.isInputDashPressed && ctx.currentDashCooldown <= 0f)
        {
            SwitchState(factory.Dash());
        }
        else if (ctx.input.isMovementHeld)
        {
            SwitchState(factory.Walk());
        }
        else if (ctx.input.isInputAttackPressed && ctx.isGrounded && ctx.currentAttackCooldown <= 0f)
        {
            SwitchState(factory.Attack());
        }

    }
    public override void InitializeSubState() { }
}
