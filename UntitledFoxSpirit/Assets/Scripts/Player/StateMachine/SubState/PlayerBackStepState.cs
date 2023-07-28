using System.Collections;
using UnityEngine;

public class PlayerBackStepState : PlayerBaseState
{
    public PlayerBackStepState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState()
    {
        Debug.Log("BackStep State");

        ctx.animController.SetBool("isMoving", false);
        ctx.animController.SetFloat("ForwardSpeed", 0f);

        DashInput();
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void OnAnimatorMoveState() 
    {
        float y = ctx.rb.velocity.y;

        ctx.rb.velocity = ctx.animController.deltaPosition * vso.rootMotionBackStepSpeed / Time.deltaTime;

        ctx.rb.velocity = new Vector3(ctx.rb.velocity.x, y, ctx.rb.velocity.z);
    }

    public override void ExitState()
    {
        ctx.disableInputRotations = false;
        ctx.isBackStep = false;
        ctx.animController.SetBool("BackStep", false);
    }
    public override void CheckSwitchState()
    {
        if (!ctx.input.isMovementHeld && !ctx.isBackStep)
        {
            SwitchState(factory.Idle());
        }
        else if (ctx.input.isMovementHeld && !ctx.isBackStep)
        {
            SwitchState(factory.Walk());
        }
    }
    public override void InitializeSubState() { }

    void DashInput()
    {
        ctx.isBackStep = true;
        ctx.animController.SetTrigger("DashTrigger");
        ctx.disableInputRotations = true;

        ctx.currentDashCooldown = vso.dashCooldown;

        if (!ctx.input.isMovementHeld)
        {
            ctx.animController.SetBool("BackStep", true);

            // Stamina
            ctx.currentStamina -= vso.staminaConsumption;
            ctx.currentStaminaCooldown = vso.staminaCooldown;
        }
    }
}
