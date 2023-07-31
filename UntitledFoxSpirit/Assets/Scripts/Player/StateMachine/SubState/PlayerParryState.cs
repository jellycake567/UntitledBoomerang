using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParryState : PlayerBaseState
{
    public PlayerParryState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState()
    {
        Debug.Log("Parry State");

        ctx.rb.velocity = Vector3.zero;
        ctx.disableInputRotations = true;
        ctx.currentSpeed = 0f;
        ctx.isParrying = true;
        ctx.animController.SetTrigger("Parry");
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void OnAnimatorMoveState() { }
    public override void ExitState() 
    {
        ctx.disableInputRotations = false;
        Debug.Log("exit");
    }
    public override void CheckSwitchState()
    {
        if (ctx.input.isInputDashPressed && ctx.currentDashCooldown <= 0f)
        {
            //ctx.animController.ResetTrigger("Parry");
            SwitchState(factory.Dash());
        }
        else if (ctx.input.isMovementHeld && !ctx.isParrying && !ctx.animIsParryTriggered)
        {
            SwitchState(factory.Walk());
        }
        else if (!ctx.input.isMovementHeld && !ctx.isParrying && !ctx.animIsParryTriggered)
        {
            SwitchState(factory.Idle());
        }
    }
    public override void InitializeSubState() { }
}
