using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    public PlayerAttackState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState()
    {
        Debug.Log("Attack State");

        ctx.rb.velocity = Vector3.zero;
        ctx.disableInputRotations = true;
        ctx.currentSpeed = 0f;
        ctx.animIsAttacking = true;
        ctx.animController.SetTrigger("Attack");

        Attack();
    }
    public override void UpdateState()
    {
        CheckSwitchState();

        AttackAnimation();
        Attack();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() 
    {
        if (!ctx.attackAgain)
            ctx.animController.ResetTrigger("Attack");

        ctx.attackAgain = false;
        ctx.disableInputRotations = false;
        ctx.animIsAttacking = false;
        ctx.comboCounter = 0;


        ctx.animController.SetBool("Attack1", false);
        ctx.animController.SetBool("Attack2", false);
        ctx.animController.SetBool("Attack3", false);
    }
    public override void CheckSwitchState()
    {
        if (ctx.input.isInputDashPressed && ctx.currentDashCooldown <= 0f)
        {
            SwitchState(factory.Dash());
        }
        else if (ctx.input.isMovementHeld && !ctx.animIsAttacking && !ctx.animIsAtkTriggered)
        {
            SwitchState(factory.Walk());
        }
        else if (!ctx.input.isMovementHeld && !ctx.animIsAttacking && !ctx.animIsAtkTriggered)
        {
            SwitchState(factory.Idle());
        }
    }
    public override void InitializeSubState() { }


    void AttackAnimation()
    {
        // Move after attacking
        if (ctx.animController.IsInTransition(0) && ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f )
        {
            ctx.animIsAttacking = false;
        }

        // End animation combo
        if (ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > ctx.animController.GetFloat("resetComboTime") && ctx.animIsTagAttack)
        {
            if (!ctx.animController.IsInTransition(0) && ctx.resetAttack) // Check if not in transiton, so it doesn't reset run during transition
            {
                ctx.animController.SetBool("Attack1", false);
                ctx.animIsAttacking = false;
                ctx.resetAttack = false;
                ctx.disableInputRotations = false;
                ctx.comboCounter = 0;
            }
        }
        else
        {
            ctx.resetAttack = true;
        }

        if (ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > ctx.animController.GetFloat("resetComboTime") && ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && !ctx.animController.IsInTransition(0))
        {
            ctx.animController.SetBool("Attack2", false);
            ctx.disableMovement = false;
            ctx.comboCounter = 0;
        }
        if (ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > ctx.animController.GetFloat("resetComboTime") && ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("Attack3") && !ctx.animController.IsInTransition(0))
        {
            ctx.animController.SetBool("Attack3", false);
            ctx.disableMovement = false;
            ctx.comboCounter = 0;
        }
    }


    void Attack()
    {
        if (ctx.input.isInputAttackPressed && !ctx.animController.IsInTransition(0) && ctx.currentAttackCooldown <= 0f)
        {
            if (ctx.comboCounter == 0)
            {
                ctx.disableInputRotations = true;
            }

            // Set time
            ctx.currentAttackCooldown = vso.attackCooldown;

            // Increase combo count
            ctx.comboCounter++;
            // Clamp combo
            ctx.comboCounter = Mathf.Clamp(ctx.comboCounter, 0, 2);


            float normTime = ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float allowAtkTime = ctx.animController.GetFloat("attackInputTime");

            if (!ctx.animIsAtkTriggered && ctx.animIsAttacking && normTime > allowAtkTime)
            {
                ctx.animController.SetTrigger("Attack");
                ctx.attackAgain = true;
            }


            // Detect which combo to trigger
            if (ctx.comboCounter == 1 && !ctx.animController.GetBool("Attack1"))
            {
                ctx.animController.SetBool("Attack1", true);
            }
            // Transitions to next combo animation
            if (ctx.comboCounter >= 2 && ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > ctx.animController.GetFloat("attackInputTime") && ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
            {
                ctx.animController.SetBool("Attack1", false);
                ctx.animController.SetBool("Attack2", true);
            }
            if (ctx.comboCounter >= 3 && ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > ctx.animController.GetFloat("attackInputTime") && ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
            {
                ctx.animController.SetBool("Attack2", false);
                ctx.animController.SetBool("Attack3", true);
            }
        }

    }
}
