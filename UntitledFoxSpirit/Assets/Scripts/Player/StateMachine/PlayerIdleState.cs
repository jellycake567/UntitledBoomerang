using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState() 
    {
        ctx.animController.SetBool("isMoving", false);
        ctx.animController.SetFloat("ForwardSpeed", 0f);
        ctx.animController.speed = 1f;
    }
    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }
    public override void CheckSwitchState() 
    {
        if (ctx.input.isMovementHeld && ctx.isRunning)
        {
            SwitchState(factory.Run());
        }
        else if (ctx.input.isMovementHeld)
        {
            SwitchState(factory.Walk());
        }
    }
    public override void InitializeSubState() { }
}
