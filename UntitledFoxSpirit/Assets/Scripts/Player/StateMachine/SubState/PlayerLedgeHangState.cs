using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLedgeHangState : PlayerBaseState
{
    public PlayerLedgeHangState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) 
    {
        isRootState = true;
        InitializeSubState();
    }

    public override void EnterState()
    {
        Debug.Log("LedgeHang State");

        ctx.animController.SetBool("LedgeHang", true);
        ctx.animController.SetBool("isMoving", false);
        ctx.rb.useGravity = false;
        ctx.rb.velocity = Vector3.zero;
        ctx.disableInputRotations = true;
        ctx.tallCollider.enabled = false;
        ctx.currentSpeed = 0f;
    }
    public override void UpdateState()
    {
        CheckSwitchState();

        LedgeClimb();
    }

    public override void FixedUpdateState() { }
    public override void OnAnimatorMoveState() 
    {
        if (ctx.isClimbing && !ctx.animController.IsInTransition(0))
        {
            ctx.rb.velocity = ctx.animController.deltaPosition * vso.rootMotionAtkSpeed / Time.deltaTime;
        }
    }
    public override void ExitState() 
    {
        ctx.isClimbing = false;
        ctx.rb.useGravity = true;
        ctx.disableGravity = false;
        ctx.disableInputRotations = false;
        ctx.tallCollider.enabled = true;
        ctx.animController.SetBool("LedgeHang", false);


        ctx.currentLedgeHangCooldown = vso.ledgeHangCooldown;
    }
    public override void CheckSwitchState()
    {
        if (ctx.isGrounded && !ctx.canClimbLedge)
        {
            SwitchState(factory.Grounded());
        }
        else if (!ctx.isGrounded && !ctx.canClimbLedge)
        {
            SwitchState(factory.Jump());
        }
    }
    public override void InitializeSubState() { }

    void LedgeClimb()
    {
        if (ctx.input.isInputClimbPressed)
        {
            ctx.animController.SetTrigger("ClimbUp");
            ctx.isClimbing = true;
        }
        else if (ctx.input.isInputDropPressed)
        {
            ctx.animController.SetTrigger("Drop");
            ctx.canClimbLedge = false;
        }
    }
}
