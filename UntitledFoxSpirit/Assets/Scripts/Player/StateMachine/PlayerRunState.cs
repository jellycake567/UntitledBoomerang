using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState() { }
    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }
    public override void CheckSwitchState() 
    {
        if (!ctx.input.isMovementHeld)
        {
            SwitchState(factory.Idle());
        }
        else if (ctx.input.isMovementHeld && !ctx.isRunning)
        {
            SwitchState(factory.Walk());
        }
    }
    public override void InitializeSubState() { }
}
